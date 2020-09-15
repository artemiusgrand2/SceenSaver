using NewScreenSaver.Enums;

namespace NewScreenSaver.Messages
{
    public class Message
    {
        #region Variable

        private MessageView messageType;
        /// <summary>
        /// тип протокола (запрос или ответ)
        /// </summary>
        public MessageView MessageType
        {
            get
            {
                return messageType;
            }
        }

         private bool locked;
         /// <summary>
         /// блоктровать ли экран
         /// </summary>
         public bool Locked
         {
             get
             {
                 return locked;
             }
         }

        #endregion

         public Message(MessageView messageType, bool locked)
         {
             this.messageType = messageType;
             this.locked = locked;
         }

         public Message(MessageView messageType)
         {
             this.messageType = messageType;
         }
    }
}
