using System;
using System.Linq;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Tamatochi.Interfaces;
using Tamatochi.PetEnums;
using Tamatochi.Statics;
using System.Collections.Generic;

namespace Tamatochi.Classes
{
    /*Pet class uses Async way to communicate with other threads such as Player Class, Console or WIN32 UI(WPF, winForm if needed)
    and has its own thread clocking for Tamatochi logic calculations which will not be blocked by any other thread*/
    public class Pet : ICallBackServed
    {
        public delegate void AsyncPlayerFunctionPointer(); // define function pointer type
        public delegate void AsyncPlayerMessagingPointer(string m); // can be any self-defined type
        private readonly AsyncPlayerFunctionPointer requirefeed, requirebed, requireclean; // define function pointers
        private readonly AsyncPlayerMessagingPointer playerMessaging;
        private readonly Random rnd;
        private readonly IPetPersistence petdata;
        private List<PetEvents> eventList;

        public Pet(ICallBackRequired player, IPetPersistence petdataobj) // Inject messages channel to Player thread and pet data object
        {
            requirefeed = new AsyncPlayerFunctionPointer(player.FeedRequired); // Set function pointers to Player methods
            requirebed = new AsyncPlayerFunctionPointer(player.BedRequired);
            requireclean = new AsyncPlayerFunctionPointer(player.CleanRequired);
            playerMessaging = new AsyncPlayerMessagingPointer(player.MessageReceived);
            petdata = petdataobj;
            eventList = new List<PetEvents>();
            rnd = new Random();
        }

        public void Start()
        {
            try
            {
                while (!AppStatic.isExiting)
                {
                    Thread.Sleep(MagicNumbers.RoundInterval_ms); //Pet system clock, each RoundInterval_ms means ONE round.
                    ExePetEvents();
                }
            }
            catch (Exception e) // May need to specify Exception types in the furture
            {
                //Console.WriteLine(e.Message);
                AppStatic.log.Error(e.Message, e); //Log4net
            }
            finally
            {
                AppStatic.isExiting = true; // Notify Game Over
            }
        }

        private void ExePetEvents()
        {
            eventList.Clear();

            if (petdata.StroyLine.Count == 0)
            {
                eventList.Add(new RoundStart(petdata, AsyncFunctionCallBack, playerMessaging));
            }
            else
            {
                petdata.CurrentRoundSeed = petdata.StroyLine.Dequeue();
                eventList.Add(new RoundStart(petdata, AsyncFunctionCallBack, playerMessaging));
                eventList.Add(new PetAged(petdata, AsyncFunctionCallBack, playerMessaging));
                eventList.Add(new PetEat(petdata, AsyncFunctionCallBack, playerMessaging));
                eventList.Add(new PetSleeping(petdata, AsyncFunctionCallBack, playerMessaging));
                eventList.Add(new PetAskFood(petdata, requirefeed, AsyncFunctionCallBack, playerMessaging));
                eventList.Add(new PetAskBed(petdata, requirebed, AsyncFunctionCallBack, playerMessaging));
                eventList.Add(new PetAskClean(petdata, requireclean, AsyncFunctionCallBack, playerMessaging));
            }

            foreach (PetEvents pe in eventList)
            {
                pe.Execute();
                if (pe is RoundStart && pe.IsEndofRound())
                {
                    AppStatic.isExiting = true;
                    return;
                }

                if (pe.IsEndofRound())
                {
                    break;
                }
            }
        }

        private void AsyncFunctionCallBack(IAsyncResult r) // Async Call Completes
        {
            AsyncResult result = (AsyncResult)r;
            if(result.AsyncDelegate is AsyncPlayerFunctionPointer)
            {
                AsyncPlayerFunctionPointer caller = (AsyncPlayerFunctionPointer)result.AsyncDelegate;
                caller.EndInvoke(r);
                return;
            }
            if (result.AsyncDelegate is AsyncPlayerMessagingPointer)
            {
                AsyncPlayerMessagingPointer caller = (AsyncPlayerMessagingPointer)result.AsyncDelegate;
                caller.EndInvoke(r);
                return;
            }
            throw new Exception("Invalid AsyncDelegate Type");
        }

        public void FeedGiven() //Event
        {
            petdata.foodCnt++; //got a food from Player thread
        }

        public void BedGiven() //Event
        {
            try
            {
                petdata.NegativeFactors.RemoveAll(p => p == NegativeFactors.NoBed); // Bed Given from Player thread
            }
            catch (Exception e)  //May need to specify exception types in the future
            {
                //Console.WriteLine(e.Message);
                AppStatic.log.Error(e.Message, e);
            }
        }

        public void CleanGiven() //Event
        {
            try
            {
                petdata.NegativeFactors.RemoveAll(p => p == NegativeFactors.Dirty); // Popo Cleaned from Player thread
            }
            catch (Exception e)  //May need to specify exception types in the future
            {
                //Console.WriteLine(e.Message);
                AppStatic.log.Error(e.Message, e);
            }
        }

        public void ControlKeyGiven(string keyValue)
        {
            try
            {
                // Manual Key Command: 1 for Food, 2 for Bed, 3 for Clean
                switch (keyValue)
                {
                    case "D1":
                        petdata.foodCnt++; // 1 Food added from manual key command
                        break;
                    case "D2":
                        petdata.NegativeFactors.RemoveAll(p => p == NegativeFactors.NoBed); //Bed given from manual key command
                        break;
                    case "D3":
                        petdata.NegativeFactors.RemoveAll(p => p == NegativeFactors.Dirty); //Popo Cleaned from manual key command
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e) //May need to specify exception types in the future
            {
                //Console.WriteLine(e.Message);
                AppStatic.log.Error(e.Message, e);
            }
        }
    }
}
