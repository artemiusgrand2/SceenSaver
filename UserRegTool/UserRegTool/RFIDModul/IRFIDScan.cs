
namespace UserRegTool.RFIDModul
{
    public delegate void CardInsertedDelegate(string cardText);
    public interface IRFIDScan
    {
        void Stop();

        event CardInsertedDelegate EventCardInserted;
    }
}
