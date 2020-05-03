using System;
using System.Collections.Generic;
using System.Linq;

namespace covidSim.Services
{
    public class Game
    {
        public List<Person> People;
        public CityMap Map;
        private DateTime _lastUpdate;

        private static Game _gameInstance;
        private static Random _random = new Random();
        public const float InfectedPeoplePossibility = 0.03f;
        public const int PeopleCount = 320;
        public const int FieldWidth = 1000;
        public const int FieldHeight = 500;
        public const int MaxPeopleInHouse = 10;

        private Game()
        {
            Map = new CityMap();
            People = CreatePopulation();
            _lastUpdate = DateTime.Now;
        }

        public static Game Instance => _gameInstance ?? (_gameInstance = new Game());

        public static Game Reset()
        {
            _gameInstance = new Game();
            return _gameInstance;
        }

        private List<Person> CreatePopulation()
        {
            var population = Enumerable
                .Repeat(0, PeopleCount)
                .Select((_, index) => new Person(index, FindHome(), Map))
                .ToList();
            InfectPopulation(population);
            return population;
        }
     
        private void InfectPopulation(List<Person> population)
        {
            var peopleToInfect = (int)(population.Count * InfectedPeoplePossibility);
            foreach (var person in population.Take(peopleToInfect))
                person.IsSick = true;
        }

        private int FindHome()
        {
            while (true)
            {
                var homeId = _random.Next(CityMap.HouseAmount);

                if (Map.Houses[homeId].ResidentCount < MaxPeopleInHouse)
                {
                    Map.Houses[homeId].ResidentCount++;
                    return homeId;
                }
            }
            
        }

        public Game GetNextState()
        {
            var diff = (DateTime.Now - _lastUpdate).TotalMilliseconds;
            if (diff >= 1000)
            {
                CalcNextStep();
            }

            return this;
        }

        private void CalcNextStep()
        {
            _lastUpdate = DateTime.Now;
            var toDelete = new List<Person>();
            foreach (var person in People)
            {
                person.CalcNextStep();
                if (person.isDead && person.DeadStepsCount >= 10)
                    toDelete.Add(person);
            }

            foreach (var person in toDelete)
            {
                People.Remove(person);
            }
        }

        public void InfectNeighbors()
        {
            var houses = People
                        .Where(p => p.IsSick && p.State == PersonState.AtHome)
                        .Select(p => p.HomeId);

            foreach (var homeId in houses)
            {
                foreach (var person in People.Where(p => p.State == PersonState.AtHome && p.HomeId == homeId))
                    person.GetInfected();

            }
        }
    }
}