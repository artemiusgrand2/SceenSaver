using System;
using System.Collections.Generic;
using System.Text;
using NewScreenSaver.Interface;
using NewScreenSaver.Messages;

namespace NewScreenSaver.OtherScreens
{
    class ProtocolCommand : IConverter
    {

        public Message FromBytes(byte[] data, int offset)
        {
            if (data.Length == (offset + 1))
                return new Message(Enums.MessageView.commad, (data[offset] == 1) ? true : false);
            //
            return null;
        }

        public byte[] ToBytes(Message message, int offset)
        {
            byte[] result = new byte[offset + 2];
            result[offset] = (byte)message.MessageType;
            result[offset + 1] = (byte)((message.Locked) ? 1 : 0);
            //
            return result;
        }
    }
}