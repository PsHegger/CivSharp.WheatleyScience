using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CivSharp.Common;

namespace CivSharp.WheatleyScience
{
    public class WheatleyPlayer_1 : IPlayer
    {
        private class Position
        {
            public int X,Y;
        }

        private int turn;
        private List<CityInfo> enemyCities, myCities;
        private List<UnitInfo> myUnits;
        private List<Position> builtCities = new List<Position>();
        private PlayerInfo myPlayer;
        private bool firstMove;
        private bool trained = false;
        private int citiesCount;
        

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
            if (myPlayer.Money >= 140 && turn <= 17)
            {
                UnitInfo unit = myUnits.ElementAt(0);
                for (int i = 0; i < builtCities.Count; i++)
                {
                    if (builtCities[i].X == unit.PositionX && builtCities[i].Y == unit.PositionY)
                        return null;
                }
                BuildingData cmd = new BuildingData();
                cmd.PositionX = unit.PositionX;
                cmd.PositionY = unit.PositionY;
                citiesCount++;
                builtCities.Add(new Position { X=cmd.PositionX, Y=cmd.PositionY });
                myPlayer.Money -= 140;
                return cmd;
            }

            return null;
        }

        public MovementData OnMovement()
        {
            if (myUnits.Count == 0 || !firstMove)
                return null;

            UnitInfo unit = myUnits.ElementAt(0);
            MovementData cmd = new MovementData();
            cmd.UnitID = unit.UnitID;
            cmd.FromX = unit.PositionX; cmd.FromY = unit.PositionY;
            if (enemyCities.ElementAt(0).PositionY == myCities.ElementAt(0).PositionY)      // Az x tengely mentén kell mozogni
            {
                int direction = (enemyCities.ElementAt(0).PositionX > myCities.ElementAt(0).PositionX) ? 1 : -1;
                cmd.ToX = unit.PositionX + direction;
                cmd.ToY = unit.PositionY;
            }
            else if (enemyCities.ElementAt(0).PositionX == myCities.ElementAt(0).PositionX) // Az y tengely mentén kell mozogni
            {
                int direction = (enemyCities.ElementAt(0).PositionY > myCities.ElementAt(0).PositionY) ? 1 : -1;
                cmd.ToY = unit.PositionY + direction;
                cmd.ToX = unit.PositionX;
            }

            if (cmd.ToX == enemyCities.ElementAt(0).PositionX && cmd.ToY == enemyCities.ElementAt(0).PositionY && turn != 25)
                return null;

            myUnits.ElementAt(0).PositionX = cmd.ToX;
            myUnits.ElementAt(0).PositionY = cmd.ToY;
            firstMove = false;
            return cmd;
        }

        public ResearchData OnResearch()
        {
            if (myPlayer.Money >= 100 && !myPlayer.Researched.Contains("írás"))
            {
                ResearchData cmd = new ResearchData();
                cmd.WhatToResearch = "írás";
                myPlayer.Money -= 100;
                return cmd;
            }
            if (myPlayer.Money >= 300 && !myPlayer.Researched.Contains("bíróság") && myPlayer.Researched.Contains("írás"))
            {
                ResearchData cmd = new ResearchData();
                cmd.WhatToResearch = "bíróság";
                myPlayer.Money -= 300;
                return cmd;
            }

            return null;
        }

        public TrainingData OnTraining()
        {
            if (trained)
                return null;
            if (myUnits.Count >= 1)
                return null;
            if (myPlayer.Money < 50)
                return null;

            CityInfo myCity = myCities.ElementAt(0);
            TrainingData cmd = new TrainingData();
            cmd.PositionX = myCity.PositionX;
            cmd.PositionY = myCity.PositionY;
            cmd.UnitTypeName = "talpas";
            trained = true;
            myPlayer.Money -= 50;
            return cmd;
        }

        public string PlayerName
        {
            get { return "WheatleyScience"; }
        }

        public void RefreshWorldInfo(int turn, WorldInfo world)
        {
            // update turn counter
            this.turn = turn;
            this.firstMove = true;

            // get my info
            for (int i = 0; i < world.Players.Length; i++)
            {
                if (world.Players[i].Name == PlayerName)
                    myPlayer = world.Players[i];
            }

            // get the cities list
            enemyCities = new List<CityInfo>();
            myCities = new List<CityInfo>();
            for (int i = 0; i < world.Cities.Length; i++)
            {
                if (world.Cities[i].Owner != PlayerName)
                    enemyCities.Add(world.Cities[i]);
                else
                    myCities.Add(world.Cities[i]);
            }
            citiesCount = myCities.Count;

            // get the list of my units
            myUnits = new List<UnitInfo>() ;
            for (int i = 0; i < world.Units.Length; i++)
            {
                if (world.Units[i].Owner == PlayerName)
                    myUnits.Add(world.Units[i]);
            }
        }

        public void UnitLost(string unitID)
        {
        }
    }
}
