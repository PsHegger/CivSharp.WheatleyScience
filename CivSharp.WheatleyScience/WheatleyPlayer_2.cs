using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CivSharp.Common;
using System.Windows.Forms;

namespace CivSharp.WheatleyScience
{
    class WheatleyPlayer_2 : IPlayer
    {

        private Form mainForm = Application.OpenForms[0];
        private int turn;
        private WorldInfo world;
        private List<CityInfo> myCities, enemyCities;
        private List<UnitInfo> myUnits, enemyUnits;
        private PlayerInfo myInfo;

        public void ActionResult(bool succeeded)
        {
        }

        public void CityLost(int positionX, int positionY)
        {
        }

        public void EnemyDestroyed(string playerName)
        {
        }

        public void GameOver(bool winner, string message)
        {
        }

        public BuildingData OnBuilding()
        {
            if (myInfo.Money >= 140)
            {
                int epito = -1;
                for (int i = 0; i < myUnits.Count; i++)
                {
                    bool epithet = true;
                    for (int j = 0; j < myCities.Count; j++)
                    {
                        if (myCities.ElementAt(j).PositionX == myUnits.ElementAt(i).PositionX && myCities.ElementAt(j).PositionY == myUnits.ElementAt(i).PositionY)
                            epithet = false;
                    }
                    for (int j = 0; j < enemyCities.Count; j++)
                    {
                        if (enemyCities.ElementAt(j).PositionX == myUnits.ElementAt(i).PositionX && enemyCities.ElementAt(j).PositionY == myUnits.ElementAt(i).PositionY)
                            epithet = false;
                    }

                    if (epithet)
                    {
                        epito = i;
                        break;
                    }
                }

                if (epito == -1)
                    return null;

                UnitInfo unit = myUnits.ElementAt(epito);
                BuildingData cmd = new BuildingData();
                cmd.PositionX = unit.PositionX;
                cmd.PositionY = unit.PositionY;
                myInfo.Money -= 140;
                myCities.Add(new CityInfo() { PositionX = cmd.PositionX, PositionY = cmd.PositionY, Owner = PlayerName });
                return cmd;
            }

            return null;
        }

        private bool shouldMove(UnitInfo unit, int x, int y)
        {
            string unitType = unit.UnitTypeName;
            if (x < 0 || x > 14 || y < 0 || y > 14)
                return false;
            int unitCount = 0;
            for (int i = 0; i < myUnits.Count; i++)
            {
                if (myUnits.ElementAt(i).PositionX == unit.PositionX && myUnits.ElementAt(i).PositionY == unit.PositionY) unitCount++;
            }
            bool isTown = false;
            for (int i = 0; i < myCities.Count; i++)
            {
                if (myCities.ElementAt(i).PositionX == unit.PositionX && myCities.ElementAt(i).PositionY == unit.PositionY)
                {
                    isTown = true;
                    break;
                }
            }
            if (isTown && unitCount < 2) return false;

            int attack = getAttack(unit.PositionX, unit.PositionY);
            float enemyDefense = getDefense(x, y);

            for (int i = 0; i < enemyCities.Count; i++)
            {
                CityInfo enemyCity = enemyCities.ElementAt(i);
                if (enemyCity.PositionX == x && enemyCity.PositionY == y)
                {
                    float szorzo = 1.0f;
                    PlayerInfo enemy = world.Players.Where(player => player.Name == enemyCity.Owner).Single();
                    if (enemy.Researched.Contains("cölöpkerítés")) szorzo = 1.2f;
                    if (enemy.Researched.Contains("kőfal")) szorzo = 1.4f;
                    enemyDefense *= szorzo;
                    break;
                }
            }

            if (attack > enemyDefense)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int getAttack(int x, int y)
        {
            int attack = 0;
            for (int i = 0; i < myUnits.Count; i++)
            {
                if (myUnits.ElementAt(i).PositionX == x && myUnits.ElementAt(i).PositionY == y)
                {
                    switch (myUnits.ElementAt(i).UnitTypeName)
                    {
                        case "talpas":
                        case "íjász":
                            attack += 1;
                            break;
                        case "lándzsás":
                            attack += 2;
                            break;
                        case "lovag":
                            attack += 4;
                            break;
                        case "katapult":
                            attack += 8;
                            break;
                    }
                }
            }

            return attack;
        }

        private float getDefense(int x, int y)
        {
            float def = 0.0f;

            for (int i = 0; i < myUnits.Count; i++)
            {
                if (myUnits.ElementAt(i).PositionX == x && myUnits.ElementAt(i).PositionY == y)
                {
                    switch (myUnits.ElementAt(i).UnitTypeName)
                    {
                        case "talpas":
                            def += 1;
                            break;
                        case "íjász":
                            def += 3;
                            break;
                        case "lándzsás":
                            def += 2;
                            break;
                        case "lovag":
                            def += 4;
                            break;
                        case "katapult":
                            def += 3;
                            break;
                    }
                }
            }

            return def;
        }

        public MovementData OnMovement()
        {
            if (myUnits.Count == 0)
                return null;

            int unitNum = -1;
            for (int i = 0; i < myUnits.Count; i++)
            {
                if (myUnits.ElementAt(i).MovementPoints > 0)
                {
                    unitNum = i;
                    break;
                }
            }

            if (unitNum == -1)
                return null;

            UnitInfo unit = myUnits.ElementAt(unitNum);

            Random rand = new Random((int)DateTime.Now.Ticks);
            List<CityInfo> enemies = getClosestTowns(unit);
            CityInfo enemy = enemies.ElementAt(rand.Next(enemies.Count));

            MovementData cmd = new MovementData();
            cmd.UnitID = unit.UnitID;
            cmd.FromX = unit.PositionX; cmd.FromY = unit.PositionY;
            if (enemy.PositionY == unit.PositionY)      // Az x tengely mentén kell mozogni
            {
                int direction = (enemy.PositionX > unit.PositionX) ? 1 : -1;
                cmd.ToX = unit.PositionX + direction;
                cmd.ToY = unit.PositionY;
            }
            else if (enemy.PositionX == unit.PositionX) // Az y tengely mentén kell mozogni
            {
                int direction = (enemy.PositionY > unit.PositionY) ? 1 : -1;
                cmd.ToY = unit.PositionY + direction;
                cmd.ToX = unit.PositionX;
            }
            else
            {
                int directionX = (enemy.PositionX > unit.PositionX) ? 1 : -1;
                int directionY = (enemy.PositionY > unit.PositionY) ? 1 : -1;
                cmd.ToX = unit.PositionX + directionX;
                cmd.ToY = unit.PositionY + directionY;
            }

            if (!shouldMove(unit, cmd.ToX, cmd.ToY))
            {
                myUnits.ElementAt(unitNum).MovementPoints = 0;
                return new MovementData() { FromX = unit.PositionX, FromY = unit.PositionY, ToY = unit.PositionY, ToX = unit.PositionX, UnitID = unit.UnitID };
            }

            myUnits.ElementAt(unitNum).MovementPoints--;
            myUnits.ElementAt(unitNum).PositionX = cmd.ToX;
            myUnits.ElementAt(unitNum).PositionY = cmd.ToY;
            return cmd;
        }

        private List<CityInfo> getClosestTowns(UnitInfo unit)
        {
            int minDistance = 15;
            for (int i = 0; i < enemyCities.Count; i++)
            {
                int distance = Math.Max(Math.Abs(unit.PositionX - enemyCities.ElementAt(i).PositionX), Math.Abs(unit.PositionY - enemyCities.ElementAt(i).PositionY));
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            List<CityInfo> towns = new List<CityInfo>();
            for (int i = 0; i < enemyCities.Count; i++)
            {
                int distance = Math.Max(Math.Abs(unit.PositionX - enemyCities.ElementAt(i).PositionX), Math.Abs(unit.PositionY - enemyCities.ElementAt(i).PositionY));
                if (distance == minDistance)
                    towns.Add(enemyCities.ElementAt(i));
            }

            return towns;
        }

        private ResearchData researchIfPossible(string name, int cost, string depedency = null)
        {
            if (myInfo.Money >= cost && !myInfo.Researched.Contains(name))
            {
                if (depedency == null)
                {
                    ResearchData cmd = new ResearchData() { WhatToResearch = name };
                    return cmd;
                }
                else
                {
                    if (myInfo.Researched.Contains(depedency))
                    {
                        ResearchData cmd = new ResearchData() { WhatToResearch = name };
                        return cmd;
                    }
                }
            }

            return null;
        }

        public ResearchData OnResearch()
        {
            if (researchIfPossible("famegmunkálás", 50) != null)
            {
                var cmd = researchIfPossible("famegmunkálás", 50);
                myInfo.Money -= 50;
                return cmd;
            }
            if (researchIfPossible("írás", 100) != null)
            {
                var cmd = researchIfPossible("írás", 100);
                myInfo.Money -= 100;
                return cmd;
            }
            if (researchIfPossible("bíróság", 300, "írás") != null)
            {
                var cmd = researchIfPossible("bíróság", 300, "írás");
                myInfo.Money -= 300;
                return cmd;
            }
            if (researchIfPossible("íjászat", 50, "famegmunkálás") != null)
            {
                var cmd = researchIfPossible("íjászat", 50, "famegmunkálás");
                myInfo.Money -= 50;
                return cmd;
            }
            if (researchIfPossible("lótenyésztés", 100) != null)
            {
                var cmd = researchIfPossible("lótenyésztés", 100);
                myInfo.Money -= 100;
                return cmd;
            }
            if (researchIfPossible("lovag", 150, "lótenyésztés") != null)
            {
                var cmd = researchIfPossible("lovag", 150, "lótenyésztés");
                myInfo.Money -= 150;
                return cmd;
            }
            if (researchIfPossible("cölöpkerítés", 100) != null)
            {
                var cmd = researchIfPossible("cölöpkerítés", 100);
                myInfo.Money -= 100;
                return cmd;
            }
            if (researchIfPossible("kőfal", 300, "cölöpkerítés") != null)
            {
                var cmd = researchIfPossible("kőfal", 300, "cölöpkerítés");
                myInfo.Money -= 300;
                return cmd;
            }

            return null;
        }

        public TrainingData OnTraining()
        {
            if (turn > 70) return null;
            if (myUnits.Count > myCities.Count * 2 && turn > 20) return null;
            if (myInfo.Money >= 50 && myUnits.Count < 1)        // train first unit
            {
                CityInfo city = myCities.ElementAt(0);
                TrainingData cmd = new TrainingData();
                cmd.PositionX = city.PositionX;
                cmd.PositionY = city.PositionY;
                cmd.UnitTypeName = (myInfo.Researched.Contains("íjászat")) ? "íjász" : "talpas";
                myInfo.Money -= 50;
                myUnits.Add(new UnitInfo() { MovementPoints = 2, Owner = PlayerName, PositionX = city.PositionX, PositionY = city.PositionY, UnitTypeName = "talpas" });
                return cmd;
            }

            if (myInfo.Money >= 100)
            {
                int varos = -1;
                for (int i = 0; i < myCities.Count; i++)
                {
                    int unitCount = 0;
                    CityInfo myCity = myCities.ElementAt(i);
                    for (int j = 0; j < myUnits.Count; j++)
                    {
                        if (myUnits.ElementAt(j).PositionX == myCity.PositionX && myUnits.ElementAt(j).PositionY == myCity.PositionY) unitCount++;
                    }

                    int surrounding = 0;
                    for (int x = Math.Max(0, myCity.PositionX - 3); x < Math.Min(14, myCity.PositionX + 3); x++)
                    {
                        for (int y = Math.Max(0, myCity.PositionY - 3); y < Math.Min(14, myCity.PositionY); y++)
                        {
                            int enemyCount = 0;
                            for (int j = 0; j < enemyUnits.Count; j++)
                            {
                                UnitInfo enemy = enemyUnits.ElementAt(j);
                                if (enemy.PositionX == x && enemy.PositionY == y) enemyCount++;
                            }
                            surrounding += enemyCount;
                        }
                    }

                    if (unitCount < Math.Max(2, surrounding))
                    {
                        varos = i;
                        break;
                    }
                }

                if (varos == -1)
                    return null;

                CityInfo city = myCities.ElementAt(varos);
                TrainingData cmd = new TrainingData();
                cmd.PositionX = city.PositionX;
                cmd.PositionY = city.PositionY;
                cmd.UnitTypeName = "lovag";
                myInfo.Money -= 100;
                myUnits.Add(new UnitInfo() { MovementPoints = 2, Owner = PlayerName, PositionX = city.PositionX, PositionY = city.PositionY, UnitTypeName = "lovag" });
                int lovagCtr = 0;
                for (int i = 0; i < myUnits.Count; i++)
                {
                    if (myUnits.ElementAt(i).UnitTypeName == "lovag") lovagCtr++;
                }
                return cmd;
            }

            if (myInfo.Money >= 50)
            {
                int varos = -1;
                for (int i = 0; i < myCities.Count; i++)
                {
                    int unitCount = 0;
                    CityInfo myCity = myCities.ElementAt(i);
                    for (int j = 0; j < myUnits.Count; j++)
                    {
                        if (myUnits.ElementAt(j).PositionX == myCity.PositionX && myUnits.ElementAt(j).PositionY == myCity.PositionY) unitCount++;
                    }

                    int surrounding = 0;
                    for (int x = Math.Max(0, myCity.PositionX - 3); x < Math.Min(14, myCity.PositionX + 3); x++)
                    {
                        for (int y = Math.Max(0, myCity.PositionY-3); y < Math.Min(14, myCity.PositionY); y++)
                        {
                            int enemyCount = 0;
                            for (int j = 0; j < enemyUnits.Count; j++)
                            {
                                UnitInfo enemy = enemyUnits.ElementAt(j);
                                if (enemy.PositionX == x && enemy.PositionY == y) enemyCount++;
                            }
                            surrounding += enemyCount;
                        }
                    }

                    if (unitCount < Math.Max(2,surrounding))
                    {
                        varos = i;
                        break;
                    }
                }

                if (varos == -1)
                    return null;

                CityInfo city = myCities.ElementAt(varos);
                TrainingData cmd = new TrainingData();
                cmd.PositionX = city.PositionX;
                cmd.PositionY = city.PositionY;
                cmd.UnitTypeName = (myInfo.Researched.Contains("íjászat")) ? "íjász" : "talpas";
                myInfo.Money -= 50;
                myUnits.Add(new UnitInfo() { MovementPoints = 2, Owner = PlayerName, PositionX = city.PositionX, PositionY = city.PositionY, UnitTypeName = "talpas" });
                return cmd;
            }
            return null;
        }

        public string PlayerName
        {
            get { return "WheatleyScience"; }
        }

        public void RefreshWorldInfo(int turn, WorldInfo world)
        {
            this.turn = turn;
            this.world = world;

            for (int i = 0; i < world.Players.Length; i++)
            {
                if (world.Players[i].Name == PlayerName)
                    myInfo = world.Players[i];
            }

            myUnits = new List<UnitInfo>();
            enemyUnits = new List<UnitInfo>();
            for (int i = 0; i < world.Units.Length; i++)
            {
                if (world.Units[i].Owner == PlayerName)
                    myUnits.Add(world.Units[i]);
                else
                    enemyUnits.Add(world.Units[i]);
            }

            myCities = new List<CityInfo>();
            enemyCities = new List<CityInfo>();
            for (int i = 0; i < world.Cities.Length; i++)
            {
                if (world.Cities[i].Owner == PlayerName)
                    myCities.Add(world.Cities[i]);
                else
                    enemyCities.Add(world.Cities[i]);
            }
            // Funky
            if (turn == 1)
            {
                mainForm.Text = "X+X+X Hacked by WheatleyScience +X+X+";
            }
            if (turn > 1)
            {
                string newHead = mainForm.Text;
                char tmpChar = newHead[36];
                newHead = newHead.Remove(37, (newHead.Length - 37));
                newHead = newHead.Insert(0, tmpChar.ToString());
                mainForm.Text = newHead;
            }
            // //Funky
        }

        public void UnitLost(string unitID)
        {
            for (int i = 0; i < myUnits.Count; i++)
            {
                if (myUnits.ElementAt(i).UnitID == unitID)
                {
                    myUnits.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
