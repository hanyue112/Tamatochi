using System;
using System.Collections.Generic;
using Tamatochi.Interfaces;
using Tamatochi.PetEnums;
using Tamatochi.Statics;

namespace Tamatochi.Classes
{
    class PetPersistence : IPetPersistence
    {
        //fields
        private readonly string guid, name;
        private int health, age, foodcnt;
        private readonly List<NegativeFactors> negativefactors;
        private PetStatus petstatus;
        private PetAction petactions;
        private readonly Queue<int> storyline;
        private int currentSeed;

        //properties
        public string GUID => guid; //readonly
        public string Name { get => name; } //readonly
        public int Health { get => health; set => health = value; }
        public int Age { get => age; set => age = value; }
        public List<NegativeFactors> NegativeFactors { get => negativefactors; } //ref readonly
        public PetStatus PetStatus { get => petstatus; set => petstatus = value; }
        public PetAction PetAction { get => petactions; set => petactions = value; }
        public int foodCnt { get => foodcnt; set => foodcnt = value; }
        public Queue<int> StroyLine { get => storyline; }
        public int CurrentRoundSeed { get => currentSeed; set => currentSeed = value; }

        public PetPersistence(string petname, int[] storyLineSeed)
        {
            guid = Guid.NewGuid().ToString();
            health = MagicNumbers.StartHealth;
            age = MagicNumbers.StartAge;
            negativefactors = new List<NegativeFactors>();
            petstatus = PetStatus.Hatching;
            name = petname;
            PetStatus = PetStatus.Live; // init values
            PetAction = PetAction.None;
            storyline = new Queue<int>(storyLineSeed);
        }

        public void Load(Guid guid)
        {
            throw new NotImplementedException(); //TO-DO: load pet data from any resource such as file, database, web-api, web-service etc.
        }

        public void Save()
        {
            throw new NotImplementedException(); //TO-DO: save pet data object to any resource such as file, database, web-api, web-service etc.
        }
    }
}
