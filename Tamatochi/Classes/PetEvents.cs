using System;
using System.Linq;
using Tamatochi.Interfaces;
using Tamatochi.PetEnums;
using Tamatochi.Statics;

namespace Tamatochi.Classes
{
    public class PetEvents
    {
        protected IPetPersistence petData;
        protected Pet.AsyncPlayerFunctionPointer fx_p;
        protected Pet.AsyncPlayerMessagingPointer fx_m;
        protected fx_CB fx_cb;
        protected bool isExecuted;
        protected int seed;

        public delegate void fx_CB(IAsyncResult ar);
        public PetEvents(IPetPersistence p, Pet.AsyncPlayerFunctionPointer fx_pointer, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m)
        {
            this.petData = p;
            this.fx_p = fx_pointer;
            this.fx_cb = fx_cb;
            this.seed = p.CurrentRoundSeed;
            this.fx_m = fx_m;
        }

        public virtual void Execute()
        {
            this.isExecuted = true;
        }

        public virtual bool IsEndofRound()
        {
            return false;
        }
    }

    public class RoundStart : PetEvents
    {
        public RoundStart(IPetPersistence p, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m) : base(p, null, fx_cb, fx_m)
        {

        }

        public override bool IsEndofRound()
        {
            if (petData.StroyLine.Count == 0)
            {
                fx_m.BeginInvoke(petData.Name + "'s story is over", new AsyncCallback(fx_cb), null);
                petData.PetStatus = PetStatus.Dead;
                return true; // Exit thread loop
            }

            if (petData.Health <= MagicNumbers.PetMinLivingHealth) // If Health is reached min living health, pet will die, Game Over.
            {
                fx_m.BeginInvoke(petData.Name + " is dead", new AsyncCallback(fx_cb), null);
                petData.PetStatus = PetStatus.Dead;
                return true; // Exit thread loop
            }
            return false;
        }

        public override void Execute()
        {
            base.Execute();
            fx_m.BeginInvoke("\n=====Round " + petData.Age + "=====\n", new AsyncCallback(fx_cb), null);

            int deductHunger, deductDirty, deductBed;
            deductBed = petData.NegativeFactors.Count(n => n == NegativeFactors.NoBed); //count health point deductions
            deductDirty = petData.NegativeFactors.Count(n => n == NegativeFactors.Dirty);
            deductHunger = petData.NegativeFactors.Count(n => n == NegativeFactors.Hunger);

            petData.Health -= deductBed; // deduct point(s)
            if (deductBed > 0)
            {
                fx_m.BeginInvoke(petData.Name + "'s Health Points decucted by " + deductBed + " due to NoBed", new AsyncCallback(fx_cb), null);
            }

            petData.Health -= deductDirty;  // deduct point(s)
            if (deductDirty > 0)
            {
                fx_m.BeginInvoke(petData.Name + "'s Health Points decucted by " + deductDirty + " due to Dirty", new AsyncCallback(fx_cb), null);
            }

            petData.Health -= deductHunger;  // deduct point(s)
            if (deductHunger > 0)
            {
                fx_m.BeginInvoke(petData.Name + "'s Health Points decucted by " + deductHunger + " due to Hunger", new AsyncCallback(fx_cb), null);
            }

            fx_m.BeginInvoke(petData.Name + "'s Health Points is " + petData.Health, new AsyncCallback(fx_cb), null);
        }
    }

    public class PetAged : PetEvents
    {
        public PetAged(IPetPersistence p, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m) : base(p, null, fx_cb, fx_m)
        {

        }

        public override void Execute()
        {
            base.Execute();
            if (++petData.Age >= MagicNumbers.PetAgedRange) // If age greater than PetAgedRange
            {
                if (seed < MagicNumbers.PetAgedDeathRate) //May die due to the age, Game Over
                {
                    fx_m.BeginInvoke(petData.Name + " dead at age of " + petData.Age, new AsyncCallback(fx_cb), null);
                    petData.PetStatus = PetStatus.Dead;
                    petData.Health = MagicNumbers.PetMinLivingHealth;
                }
            }
        }
    }

    public class PetEat : PetEvents
    {
        public PetEat(IPetPersistence p, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m) : base(p, null, fx_cb, fx_m)
        {

        }

        public override bool IsEndofRound()
        {
            if (petData.PetAction == PetAction.Eating)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Execute()
        {
            base.Execute();
            if (seed >= MagicNumbers.PetEatRate && petData.foodCnt > MagicNumbers.PetEatMinFoodRetain && petData.PetAction == PetAction.None)//Each round, if the pet is idle, pet may eat a food
            {
                petData.foodCnt--;
                petData.PetAction = PetAction.Eating;
                fx_m.BeginInvoke(petData.Name + " is eating", new AsyncCallback(fx_cb), null);
            }

            if (petData.PetAction == PetAction.Eating)
            {
                petData.Health += seed / MagicNumbers.PetEatRecoverRate + MagicNumbers.PetEatExtraPoints; // Each food may provides Health Point(s)
                fx_m.BeginInvoke(petData.Name + "'s Health Points recoved to " + petData.Health, new AsyncCallback(fx_cb), null);
                petData.NegativeFactors.Remove(NegativeFactors.Hunger); // Each food will reduce one Hunger effects
                petData.PetAction = PetAction.None;
            }
        }
    }

    public class PetSleeping : PetEvents
    {
        public PetSleeping(IPetPersistence p, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m) : base(p, null, fx_cb, fx_m)
        {

        }

        public override bool IsEndofRound()
        {
            if (petData.PetAction == PetAction.Sleeping && seed < MagicNumbers.PetWeakUpRate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Execute()
        {
            base.Execute();
            if (petData.PetAction == PetAction.Sleeping) // If the pet is Sleeping
            {
                if (seed >= MagicNumbers.PetSleeRate) // chances to waek up or keep sleeping
                {
                    fx_m.BeginInvoke(petData.Name + " is weak up", new AsyncCallback(fx_cb), null);
                    petData.PetAction = PetAction.None;
                    petData.NegativeFactors.RemoveAll(P => P == NegativeFactors.NoBed);
                }
                else
                {
                    fx_m.BeginInvoke(petData.Name + " is sleeping", new AsyncCallback(fx_cb), null);
                    Console.ResetColor();
                }
            }
        }
    }

    public class PetAskFood : PetEvents
    {
        public PetAskFood(IPetPersistence p, Pet.AsyncPlayerFunctionPointer fx_pointer, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m) : base(p, fx_pointer, fx_cb, fx_m)
        {

        }

        public override void Execute()
        {
            base.Execute();
            if (seed >= MagicNumbers.PetAskFoodSeedMin && seed < MagicNumbers.PetAskFoodSeedMax) // Each round Pet may needs one more food
            {
                if (petData.PetAction == PetAction.None)
                {
                    fx_p.BeginInvoke(new AsyncCallback(fx_cb), null);
                    fx_m.BeginInvoke(petData.Name + " needs a Food", new AsyncCallback(fx_cb), null);
                    petData.NegativeFactors.Add(NegativeFactors.Hunger);
                }
            }
        }
    }

    public class PetAskBed : PetEvents
    {
        public PetAskBed(IPetPersistence p, Pet.AsyncPlayerFunctionPointer fx_pointer, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m) : base(p, fx_pointer, fx_cb, fx_m)
        {

        }

        public override void Execute()
        {
            base.Execute();
            if (seed >= MagicNumbers.PetAskBedSeedMin && seed < MagicNumbers.PetAskBedSeedMax)  // Each round Pet may sleep
            {
                if (petData.PetAction == PetAction.None)
                {
                    fx_m.BeginInvoke(petData.Name + " is Sleeping without Bed", new AsyncCallback(fx_cb), null);
                    petData.PetAction = PetAction.Sleeping;
                    fx_p.BeginInvoke(new AsyncCallback(fx_cb), null);
                    petData.NegativeFactors.Add(NegativeFactors.NoBed);
                }
            }
        }
    }

    public class PetAskClean : PetEvents
    {
        public PetAskClean(IPetPersistence p, Pet.AsyncPlayerFunctionPointer fx_pointer, fx_CB fx_cb, Pet.AsyncPlayerMessagingPointer fx_m) : base(p, fx_pointer, fx_cb, fx_m)
        {

        }

        public override void Execute()
        {
            base.Execute();
            if (seed >= MagicNumbers.PetAskCleanSeedMin && seed < MagicNumbers.PetAskCleanSeedMax)  // Each round Pet may needs clean popo
            {
                fx_p.BeginInvoke(new AsyncCallback(fx_cb), null);
                fx_m.BeginInvoke(petData.Name + " needs Clean Popo", new AsyncCallback(fx_cb), null);
                petData.NegativeFactors.Add(NegativeFactors.Dirty);
            }
        }
    }
}