using NewScreenSaver.Messages;

namespace NewScreenSaver.Interface
{
    public interface IConverter
    {
        Message FromBytes(byte[] data, int offset);
        byte[] ToBytes(Message message, int offset);
    }
}
