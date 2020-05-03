using System;
using System.Drawing;
using covidSim.Models;
using System.Linq;

namespace covidSim.Services
{
    public class Person
    {
        private const int MaxDistancePerTurn = 30;
        private static Random random = new Random();
        private PersonState state = PersonState.AtHome;
        internal PersonState State => state;
        private Rectangle home;
        private int sickStepsCount = 0;

        public PersonMood Mood { get; private set; } = PersonMood.Normal;
        private int atHomeCount;

        public int DeadStepsCount = 0;


        private const int StepsToRecoveryCount = 35;

        public Person(int id, int homeId, CityMap map)
        {
            Id = id;
            HomeId = homeId;
            atHomeCount = 0;

            var homeCoords = map.Houses[homeId].Coordinates.LeftTopCorner;
            home = new Rectangle(homeCoords.X, homeCoords.Y, HouseCoordinates.Width, HouseCoordinates.Height);
            var x = homeCoords.X + random.Next(HouseCoordinates.Width);
            var y = homeCoords.Y + random.Next(HouseCoordinates.Height);
            Position = new Vec(x, y);
        }

        public string Status;
        public int Id;
        public int HomeId;
        public Vec Position;
        public bool IsSick;
        public bool isDead;
        

        public void CalcNextStep()
        {
            if (isDead)
            {
                DeadStepsCount++;
                return;
            }
            switch (state)
            {
                case PersonState.AtHome:
                    CalcNextStepForPersonAtHome();
                    break;
                case PersonState.Walking:
                    CalcNextPositionForWalkingPerson();
                    break;
                case PersonState.GoingHome:
                    CalcNextPositionForGoingHomePerson();
                    break;
            }
            if (IsSick)
            {
                isDead = random.NextDouble() < 0.000003;
                if (isDead)
                    return;
                sickStepsCount++;
                if (sickStepsCount >= StepsToRecoveryCount) {
                    sickStepsCount = 0;
                    IsSick = false;
                }
            }
        }

        private void CalcNextStepForPersonAtHome()
        {
            var goingWalk = random.NextDouble() < 0.005;
            if (goingWalk)
            {
                state = PersonState.Walking;
                atHomeCount = 0;
                Mood = PersonMood.Normal;
                CalcNextPositionForWalkingPerson();
            }
            else
            {
                atHomeCount++;
                if (atHomeCount == 5)
                    Mood = PersonMood.Bored;
                var nextPosition = CalculateHomeMovement();
                while (!home.Contains(nextPosition.X, nextPosition.Y))
                    nextPosition = CalculateHomeMovement();
                Position = nextPosition;
            }
        }

        private Vec CalculateHomeMovement()
        {
            var xLength = random.Next(MaxDistancePerTurn);
            var yLength = MaxDistancePerTurn - xLength;
            var direction = ChooseDirection();
            var delta = new Vec(xLength * direction.X, yLength * direction.Y);
            var nextPosition = new Vec(Position.X + delta.X, Position.Y + delta.Y);
            return nextPosition;
        }

        private void CalcNextPositionForWalkingPerson()
        {
            var xLength = random.Next(MaxDistancePerTurn);
            var yLength = MaxDistancePerTurn - xLength;
            var direction = ChooseDirection();
            var delta = new Vec(xLength * direction.X, yLength * direction.Y);
            var nextPosition = new Vec(Position.X + delta.X, Position.Y + delta.Y);

            if (isCoordInField(nextPosition) && !IsPersonComeInOtherHome(nextPosition))
            {
                Position = nextPosition;
            }
            else
            {
                CalcNextPositionForWalkingPerson();
            }
        }

        private void CalcNextPositionForGoingHomePerson()
        {
            var game = Game.Instance;
            var homeCoord = game.Map.Houses[HomeId].Coordinates.LeftTopCorner;
            var homeCenter = new Vec(homeCoord.X + HouseCoordinates.Width / 2, homeCoord.Y + HouseCoordinates.Height / 2);

            var xDiff = homeCenter.X - Position.X;
            var yDiff = homeCenter.Y - Position.Y;
            var xDistance = Math.Abs(xDiff);
            var yDistance = Math.Abs(yDiff);

            var distance = xDistance + yDistance;
            if (distance <= MaxDistancePerTurn)
            {
                Position = homeCenter;
                state = PersonState.AtHome;
                return;
            }

            var direction = new Vec(Math.Sign(xDiff), Math.Sign(yDiff));

            var xLength = Math.Min(xDistance, MaxDistancePerTurn);
            var newX = Position.X + xLength * direction.X;
            var yLength = MaxDistancePerTurn - xLength;
            var newY = Position.Y + yLength * direction.Y;
            Position = new Vec(newX, newY);
        }

		private bool IsPersonComeInOtherHome(Vec vec)
		{
			var game = Game.Instance;
			var home = game.Map.Houses.Where(house => house.IsPersonInHouse(vec.X, vec.Y)).FirstOrDefault();
			if (home == null || home.Id == HomeId)
				return false;
			return true;
		}

        public void GoHome()
        {
            if (state != PersonState.Walking) return;

            state = PersonState.GoingHome;
            CalcNextPositionForGoingHomePerson();
        }

        private Vec ChooseDirection()
        {
            var directions = new Vec[]
            {
                new Vec(-1, -1),
                new Vec(-1, 1),
                new Vec(1, -1),
                new Vec(1, 1),
            };
            var index = random.Next(directions.Length);
            return directions[index];
        }

        private bool isCoordInField(Vec vec)
        {
            var belowZero = vec.X < 0 || vec.Y < 0;
            var beyondField = vec.X > Game.FieldWidth || vec.Y > Game.FieldHeight;

            return !(belowZero || beyondField);
        }
        
        public void GetInfected()
        {
            if (random.NextDouble() > 0.5)
            {
                IsSick = true;
            }
        }
    }
}