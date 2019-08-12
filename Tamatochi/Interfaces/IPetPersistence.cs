using System;
using System.Collections.Generic;
using Tamatochi.PetEnums;

namespace Tamatochi.Interfaces
{
    public interface IPetPersistence
    {
        string GUID
        {
            get;
        }
        string Name
        {
            get;
        }
        int Health
        {
            get;
            set;
        }
        int Age
        {
            get;
            set;
        }
        int CurrentRoundSeed
        {
            get;
            set;
        }
        int foodCnt
        {
            get;
            set;
        }
        List<NegativeFactors> NegativeFactors
        {
            get;
        }
        PetStatus PetStatus
        {
            get;
            set;
        }
        PetAction PetAction
        {
            get;
            set;
        }
        Queue<int> StroyLine
        {
            get;
        }
        void Save();
        void Load(Guid guid);
    }
}
