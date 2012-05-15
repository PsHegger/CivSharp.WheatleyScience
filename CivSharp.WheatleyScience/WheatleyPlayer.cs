using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CivSharp.Common;
using System.Windows.Forms;

namespace CivSharp.WheatleyScience
{
    class WheatleyPlayer : IPlayer
    {
        private Form mainForm = Application.OpenForms[0];
        private WorldInfo world;
        private List<UnitInfo> myUnits, enemyUnits;
        private List<CityInfo> enemyCities, myCities;
        private PlayerInfo myInfo;
        private int playerCount;
        private int termeltEgysegekSzama;

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

        private bool isTown(int x, int y)
        {
            bool town = false;
            for (int i = 0; i < myCities.Count; i++)
            {
                if (myCities.ElementAt(i).PositionX == x && myCities.ElementAt(i).PositionY == y)
                {
                    town = true;
                    break;
                }
            }

            return town;
        }

        public BuildingData OnBuilding()
        {
            if (playerCount > 2)
                return null;

            if (myInfo.Money >= 140)
            {
                for (int i = 0; i < myUnits.Count; i++)
                {
                    if (!isTown(myUnits.ElementAt(i).PositionX, myUnits.ElementAt(i).PositionY))
                    {
                        BuildingData cmd = new BuildingData();
                        cmd.PositionX = myUnits.ElementAt(i).PositionX;
                        cmd.PositionY = myUnits.ElementAt(i).PositionY;
                        myInfo.Money -= 140;
                        return cmd;
                    }
                }
            }

            return null;
        }

        public MovementData OnMovement()
        {
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
            CityInfo enemy = null;
            List<CityInfo> gyengek = new List<CityInfo>();
            for (int i = 0; i < enemyCities.Count; i++)
            {
                if (vedelem(enemyCities.ElementAt(i)) < 2)
                {
                    gyengek.Add(enemyCities.ElementAt(i));
                }
            }
            enemy = getBest(gyengek, unit);
            if (enemy == null)
                enemy = enemyCities.ElementAt(0);
            MovementData cmd = new MovementData();
            cmd.FromX = unit.PositionX; cmd.FromY = unit.PositionY;
            cmd.UnitID = unit.UnitID;
            if (enemy.PositionX == unit.PositionX)
            {
                int direction = (enemy.PositionY < unit.PositionY) ? -1 : 1;
                cmd.ToX = unit.PositionX;
                cmd.ToY = unit.PositionY + direction;
            }
            else if (enemy.PositionY == unit.PositionY)
            {
                int direction = (enemy.PositionX < unit.PositionX) ? -1 : 1;
                cmd.ToY = unit.PositionY;
                cmd.ToX = unit.PositionX + direction;
            }
            else
            {
                int directionX = (enemy.PositionX < unit.PositionX) ? -1 : 1;
                int directionY = (enemy.PositionY < unit.PositionY) ? -1 : 1;
                cmd.ToX = unit.PositionX + directionX;
                cmd.ToY = unit.PositionY + directionY;
            }

            myUnits.ElementAt(unitNum).MovementPoints--;
            myUnits.ElementAt(unitNum).PositionX = cmd.ToX;
            myUnits.ElementAt(unitNum).PositionY = cmd.ToY;

            return cmd;
        }

        private int vedelem(CityInfo cityInfo)
        {
            int def = 0;

            for (int i = 0; i < enemyUnits.Count; i++)
            {
                if (enemyUnits.ElementAt(i).PositionX == cityInfo.PositionX && enemyUnits.ElementAt(i).PositionY == cityInfo.PositionY)
                    def++;
            }

            return def;
        }

        public ResearchData OnResearch()
        {
            if (playerCount > 3)
                return null;
            if (myInfo.Money >= 100 && !myInfo.Researched.Contains("írás"))
            {
                ResearchData cmd = new ResearchData() { WhatToResearch = "írás" };
                myInfo.Money -= 100;
                return cmd;
            }

            return null;
        }

        private CityInfo getBest(List<CityInfo> cities, UnitInfo unit)
        {
            if (cities.Count == 0)
                return null;
            int minDistance = 15;
            int cityNum = -1;
            for (int i = 0; i < cities.Count; i++)
            {
                int distance = Math.Max(Math.Abs(cities.ElementAt(i).PositionX - unit.PositionX), Math.Abs(cities.ElementAt(i).PositionY - unit.PositionY));
                if (distance < minDistance)
                {
                    cityNum = i;
                    minDistance = distance;
                }
            }

            return cities.ElementAt(cityNum);
        }

        public TrainingData OnTraining()
        {
            if (termeltEgysegekSzama == 4 && playerCount == 2) return null;
            if (playerCount <= 3 && myInfo.Money < 100 && !myInfo.Researched.Contains("írás")) return null;
            if (myUnits.Count > 20) return null;
            if (myInfo.Money >= 50)
            {
                TrainingData cmd = new TrainingData();
                cmd.UnitTypeName = "talpas";
                cmd.PositionX = myCities.ElementAt(0).PositionX;
                cmd.PositionY = myCities.ElementAt(0).PositionY;
                myInfo.Money -= 50;
                return cmd;
            }

            return null;
        }

        public string PlayerName
        {
            get { return "PsHegger2"; }
        }

        public void RefreshWorldInfo(int turn, WorldInfo world)
        {
            this.world = world;

            playerCount = 0;
            for (int i = 0; i < world.Players.Length; i++)
            {
                if (world.Players[i].InGame) playerCount++;
            }
            this.myInfo = world.Players.Where(player => player.Name == this.PlayerName).Single();
            this.myCities = world.Cities.Where(city => city.Owner == this.PlayerName).ToList();
            this.enemyCities = world.Cities.Where(city => city.Owner != this.PlayerName).ToList();
            this.myUnits = world.Units.Where(unit => unit.Owner == this.PlayerName).ToList();
            this.enemyUnits = world.Units.Where(unit => unit.Owner != this.PlayerName).ToList();
            termeltEgysegekSzama = 0;

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
        }
    }
}
