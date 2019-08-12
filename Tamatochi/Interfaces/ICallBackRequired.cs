namespace Tamatochi.Interfaces
{
    public interface ICallBackRequired
    {
        void FeedRequired();
        void BedRequired();
        void CleanRequired();
        void MessageReceived(string m);
    }
}
