using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using NewScreenSaver.Messages;
using NewScreenSaver.Enums;
using NewScreenSaver.RFIDModul;

namespace NewScreenSaver.OtherScreens
{
    public  class ListenScreen
    {
        #region Переменные и свойства

        /// <summary>
        /// Сервер
        /// </summary>
        TcpListener server = null;
        /// <summary>
        /// таймер опроса источника информации
        /// </summary>
        Timer timer_listen;
        /// <summary>
        /// интервал через который происходит слушание
        /// </summary>
        int interval = 100;
        /// <summary>
        /// событие авторизации
        /// </summary>
        public event StatusAuthorization IsAuthorization;

        public static DateTime LastCommand { get; private set; }

        #endregion

        public ListenScreen(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            timer_listen = new Timer(interval);
            timer_listen.Elapsed += ListenClient;
        }

        public void Start()
        {
            try
            {
                if (server != null)
                {
                    server.Start();
                    timer_listen.Start();
                }
            }
            catch { }
        }

        public void Stop()
        {
            try
            {
                if (server != null)
                {
                    server.Stop();
                    timer_listen.Stop();
                }
            }
            catch { }
        }

        private void ListenClient(object sender, ElapsedEventArgs e)
        {
            timer_listen.Stop();
            try
            {
                if (server.Pending())
                {
                    using (TcpClient client = server.AcceptTcpClient())
                    {
                        // Получаем объект потока для чтения и записи
                        using (NetworkStream stream = client.GetStream())
                        {
                            //получаем запрос от клиента
                            byte[] data = new byte[3];
                            int readbytes = 0;
                            while ((readbytes = stream.Read(data, 0, data.Length)) != 0)
                            {
                                data = OtherScreen.DataTrim(data, readbytes);
                                //анализируем полученную информацию
                                Message message = (new Protokol()).FromBytes(data);
                                if (message != null)
                                {
                                    switch (message.MessageType)
                                    {
                                        case MessageView.request:
                                            {
                                                //if (MainWindow.ViewServer == ViewServer.haveRFID)
                                                //{
                                                    byte[] answer = new Protokol().ToBytes(new Message(MessageView.answer, MainWindow.IsAuth));
                                                    stream.Write(answer, 0, answer.Length);
                                                //}
                                                //else
                                                //    stream.Write(new byte[1], 0, 1);
                                            }
                                            break;
                                        case MessageView.commad:
                                            {
                                                //if (MainWindow.ViewServer == ViewServer.noRFID)
                                                //{
                                                LastCommand = DateTime.Now;
                                                if (IsAuthorization != null)
                                                    IsAuthorization(message.Locked, true);
                                                //}
                                            }
                                            break;
                                    }
                                }
                            }
                            stream.Close();
                        }
                        // закрываем соединение с клиентом
                        client.Close();
                    }
                }
            }
            catch { }
            timer_listen.Start();
        }
    }
}
