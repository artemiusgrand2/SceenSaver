using System;
using System.Text;
using NewScreenSaver.Interface;
using NewScreenSaver.Messages;

namespace NewScreenSaver.OtherScreens
{
    class ProtocolRequest : IConverter
    {

        public Message FromBytes(byte[] data, int offset)
        {
            return new Message(Enums.MessageView.request);
        }

        public byte[] ToBytes(Message message, int offset)
        {
            byte[] result = new byte[offset + 1];
            result[offset] = (byte)message.MessageType;
            //
            return result;
        }
    }
}
