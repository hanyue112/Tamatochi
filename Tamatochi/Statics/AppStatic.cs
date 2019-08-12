namespace Tamatochi.Statics
{
    public static class AppStatic
    {
        public static volatile bool isExiting = false; //App exiting flag for all threads
        public static readonly object objLock = new object();
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }

    public static class MagicNumbers // May read from App.config or other data sources
    {
        public static readonly int PetMinLivingHealth = 0;
        public static readonly int PetAgedRange = 90;
        public static readonly int PetAgedDeathRate = 3;
        public static readonly int PetEatRate = 50;
        public static readonly int PetEatRecoverRate = 33;
        public static readonly int PetEatExtraPoints = 1;
        public static readonly int PetEatMinFoodRetain = 0;
        public static readonly int PetSleeRate = 50;
        public static readonly int PetAskFoodSeedMax = 20;
        public static readonly int PetAskFoodSeedMin = 10;
        public static readonly int PetAskBedSeedMax = 30;
        public static readonly int PetAskBedSeedMin = 20;
        public static readonly int PetAskCleanSeedMax = 40;
        public static readonly int PetAskCleanSeedMin = 30;
        public static readonly int PetWeakUpRate = 50;
        public static readonly int SeedMin= 0;
        public static readonly int SeedMax = 100;
        public static readonly int RoundInterval_ms = 2000;
        public static readonly int PlayerInterval_ms = 1000;
        public static readonly int StoryLength = 200;
        public static readonly int StartHealth = 100;
        public static readonly int StartAge= 0;
    }
}
