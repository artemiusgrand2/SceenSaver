using System;
using System.Collections.Generic;
using System.Text;
using NewScreenSaver.Enums;
using NewScreenSaver.Interface;
using NewScreenSaver.Messages;

namespace NewScreenSaver.OtherScreens
{
    public class Protokol
    {
        private const int HeaderSize = 1;

        private readonly IDictionary<MessageView, IConverter> converters = new Dictionary<MessageView, IConverter>()
            {
                { MessageView.answer, new ProtocolAnswer()},
                { MessageView.request, new ProtocolRequest()},
                { MessageView.commad, new ProtocolCommand()},
            };
        /// <summary>
        /// первый байт
        /// </summary>
        byte firstByte = 126;

        public Message FromBytes(byte[] data)
        {
            try
            {
                IConverter converter;
                if (data.Length > 1 && data[0] == firstByte)
                {
                    if (converters.TryGetValue((MessageView)data[1], out converter))
                    {
                        return converter.FromBytes(data, 2);
                    }
                }
            }
            catch { }
            //
            return null;
        }

        public byte[] ToBytes(Message message)
        {
            byte[] result = null;
            IConverter converter;
            if (converters.TryGetValue(message.MessageType, out converter))
            {
                result = converter.ToBytes(message, 1);
                if (result != null && result.Length > 0)
                    result[0] = firstByte;
            }
            return result;
        }
    }
}
