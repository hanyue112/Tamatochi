namespace Tamatochi.Interfaces
{
    public interface ICallBackServed
    {
        void FeedGiven();
        void BedGiven();
        void CleanGiven();
        void ControlKeyGiven(string keyValue);
    }
}
