using System;
using Tamatochi.Interfaces;
using System.Threading;
using Tamatochi.Statics;
using System.Runtime.Remoting.Messaging;

namespace Tamatochi.Classes
{
    /*According to Pet Class's design, Player Class uses Async way to communicate with Pet thread so that Player thread will not block Pet thread
     Player Class provides key command processing and forwarding mechanism so that Pet thread can interact with human user inputs via UI (console, or any GUI after extension)
     Player Class also provides Auto Care mode for the testing of Pet class implementation, when Auto Care mode is activated, Player Class will automatically process
     all messages sent from Pet thread and responding accordingly to try to keep the pet alive as long as possile
     Player Class has its own thread clocking to simulate WINDOWS GUI message pump and process incoming messages from Pet thread in Auto Care mode*/
    public class Player : ICallBackRequired
    {
        private delegate void AsyncPetFunctionPointer();  // define function pointer type
        private delegate void AsyncSendKeyFunctionPointer(string value); // define function pointers
        private readonly AsyncPetFunctionPointer feed, bed, clean;
        private readonly AsyncSendKeyFunctionPointer sendkey;
        private readonly Thread petThread;
        private readonly Pet pet;

        private volatile int feedReq = 0; //Store how many food request(s) pending
        private volatile bool needBed, needClean, isAutoCare = true;

        public Player()
        {
            // Generate pet data object and init Pet instance with 500 random number
            PetPersistence p = new PetPersistence("Ninja", BuildStory()); //or assign a known int array so that it can be UNIT tested easily as the output would be exactly same.
            pet = new Pet(this, p);

            //Set function pointers for Async Calls to Pet thread
            feed = new AsyncPetFunctionPointer(pet.FeedGiven);
            bed = new AsyncPetFunctionPointer(pet.BedGiven);
            clean = new AsyncPetFunctionPointer(pet.CleanGiven);
            sendkey = new AsyncSendKeyFunctionPointer(pet.ControlKeyGiven);

            // Start Pet thread
            petThread = new Thread(new ThreadStart(pet.Start));
        }

        public void Start()
        {
            try
            {
                petThread.Start();

                while (!AppStatic.isExiting)
                {
                    if (isAutoCare == true) // If in Auto Care mode, this thread will take care of the Pet（Player simulation and auto testing Pet Class）
                    {
                        lock (AppStatic.objLock)
                        {
                            for (int i = 0; i < feedReq; i++)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("#Auto Care Mode# Food Provided at " + DateTime.Now);
                                Console.ResetColor();
                                feed.BeginInvoke(new AsyncCallback(AsyncFunctionCallBack), null); // Async call for adding a food
                            }
                            feedReq = 0;
                        }

                        if (needBed)
                        {
                            needBed = false;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("#Auto Care Mode# Bed Provided at " + DateTime.Now);
                            Console.ResetColor();
                            bed.BeginInvoke(new AsyncCallback(AsyncFunctionCallBack), null); // Async call for providing a bed
                        }

                        if (needClean)
                        {
                            needClean = false;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("#Auto Care Mode# Cleaned Popo at " + DateTime.Now);
                            Console.ResetColor();
                            clean.BeginInvoke(new AsyncCallback(AsyncFunctionCallBack), null); // Async call for cleaning
                        }
                    }
                    Thread.Sleep(MagicNumbers.PlayerInterval_ms); // Player simulation clock, each round period is ONE second
                }
                petThread.Join(); // wait till pet thread stopped
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Game Over, press ESC to exit"); // ready for exiting this app
                Console.ResetColor();
            }
            catch (Exception e) // May need to specify Exception types in the furture
            {
                Console.WriteLine(e.Message);
                AppStatic.log.Error(e.Message, e); //log4net
            }
        }

        public void BedRequired() //Event
        {
            needBed = true; // Bed require message from Pet thread
        }

        public void CleanRequired() //Event
        {
            needClean = true; // Clean require message from Pet thread
        }

        public void FeedRequired() //Event
        {
            lock (AppStatic.objLock)
            {
                feedReq++; // Food require message from Pet thread
            }
        }

        public void SendConsoleKey(string key)
        {
            try
            {
                // Manual Key Command: 1 for Food, 2 for Bed, 3 for Clean, 4 for Auto Care
                switch (key)
                {
                    case "D1":
                        lock (AppStatic.objLock)
                        {
                            if (feedReq > 0)
                            {
                                feedReq--; // 1 Food given from manual key command
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine("#Manual# Food Provided at " + DateTime.Now);
                        Console.ResetColor();
                        break;
                    case "D2":
                        needBed = false; // Bed given from manual key command
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine("#Manual# Bed Provided at " + DateTime.Now);
                        Console.ResetColor();
                        break;
                    case "D3":
                        needClean = false; //Popo Cleaned from manual key command
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine("#Manual# Cleaned Popo at " + DateTime.Now);
                        Console.ResetColor();
                        break;
                    case "D4":
                        isAutoCare = !isAutoCare; // Toggle Auto Care mode
                        break;
                    default:
                        break;
                }
                sendkey.BeginInvoke(key, new AsyncCallback(AsyncSendKeyCallBack), null); // Maintain key message chain, Send key value to Pet thread
            }
            catch (Exception e) //May need to specify exception types in the future
            {
                Console.WriteLine(e.Message);
                AppStatic.log.Error(e.Message, e);
            }
        }

        private void AsyncFunctionCallBack(IAsyncResult r) // Async Call Completes
        {
            AsyncResult result = (AsyncResult)r;
            AsyncPetFunctionPointer caller = (AsyncPetFunctionPointer)result.AsyncDelegate;
            caller.EndInvoke(r);
        }

        private void AsyncSendKeyCallBack(IAsyncResult r) // Async Key Command Completes
        {
            AsyncResult result = (AsyncResult)r;
            AsyncSendKeyFunctionPointer caller = (AsyncSendKeyFunctionPointer)result.AsyncDelegate;
            caller.EndInvoke(r);
        }

        private int[] BuildStory()
        {
            int[] Seeds = new int[MagicNumbers.StoryLength];
            Random randNum = new Random();
            for (int i = 0; i < Seeds.Length; i++)
            {
                Seeds[i] = randNum.Next(MagicNumbers.SeedMin, MagicNumbers.SeedMax);
            }
            return Seeds;
        }

        public void MessageReceived(string m) // Messages from Pet thread, m can be any self-defined type in the future and can be used for simple UNIT tests at this moment
        {
            Console.WriteLine(m); 
            //Output to console, Async messaging may not in correct order, to solve this issue, 
            //need store the messages with some kinds of ID or timestamp system and update entire UI according to the sequence when necessary(TBC)
            //or use invoke from the Pet thread(may block Pet thread.
            Console.WriteLine("");
        }
    }
}
