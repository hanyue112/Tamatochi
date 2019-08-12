using System;
using Tamatochi.Classes;
using System.Threading;
using Tamatochi.Statics;

namespace Tamatochi
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AppStatic.log.Info("App Start "+DateTime.Now);
                Player player = new Player();
                Thread playerThread = new Thread(new ThreadStart(player.Start));
                playerThread.Start();

                ConsoleKeyInfo cki;
                Console.TreatControlCAsInput = true;
                Console.Title = "Tamatochi, 1 for Food, 2 for Bed, 3 for Clean, 4 for Auto Care, ESC for Exit";
                do
                {
                    cki = Console.ReadKey(true);
                    //Console.Write("--- You pressed ");
                    //if ((cki.Modifiers & ConsoleModifiers.Alt) != 0) Console.Write("ALT+");
                    //if ((cki.Modifiers & ConsoleModifiers.Shift) != 0) Console.Write("SHIFT+");
                    //if ((cki.Modifiers & ConsoleModifiers.Control) != 0) Console.Write("CTL+");
                    //Console.WriteLine(cki.Key.ToString());
                    player.SendConsoleKey(cki.Key.ToString()); // Send Key value to player
                } while (cki.Key != ConsoleKey.Escape);

                AppStatic.isExiting = true; // Set exiting flag
                playerThread.Join(); // Wait till plyaer thread stopped
                AppStatic.log.Info("App Exit " + DateTime.Now + "\n");
            }
            catch (Exception e) // May need to specify Exception types in the furture
            {
                Console.WriteLine(e.Message);
                AppStatic.log.Error(e.Message, e); //log4net
            }
        }
    }
}
