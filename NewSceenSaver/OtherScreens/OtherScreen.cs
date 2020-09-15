using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using NewScreenSaver.Enums;
using NewScreenSaver.Messages;
using NewScreenSaver.RFIDModul;

namespace NewScreenSaver.OtherScreens
{
    public class OtherScreen
    {

        #region Переменные и свойства

        Timer timer = null;
        /// <summary>
        /// ip - адрес другово экрана
        /// </summary>
        string hostname = "127.0.0.1";
        /// <summary>
        /// порт соединения
        /// </summary>
        int port = 2002;
        /// <summary>
        /// запрос
        /// </summary>
        byte [] request = null;
        bool isConnect = false;
        /// <summary>
        /// событие авторизации
        /// </summary>
        public event StatusAuthorization IsAuthorization;

        #endregion

        public OtherScreen(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
            timer = new Timer(1000);
            timer.Elapsed +=timer_Elapsed;
            request = new Protokol().ToBytes(new Message(MessageView.request));
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            try
            {
                if (!isConnect)
                    CheckLink();
            }
            catch { }
            timer.Start();
        }

        public static byte[] DataTrim(byte[] data, int lenght)
        {
            byte[] newdata = new byte[lenght];
            for (int i = 0; i < lenght; i++)
                newdata[i] = data[i];
            //
            return newdata;
        }

        public void CheckLink()
        {
            try
            {
                if((DateTime.Now - ListenScreen.LastCommand).TotalMilliseconds > 1000)
                {
                    using (TcpClient source = new TcpClient(hostname, port))
                    {
                        using (NetworkStream stream = source.GetStream())
                        {
                            //запрашиваем 
                            isConnect = true;
                            stream.Write(request, 0, request.Length);
                            byte[] data = new byte[3];
                            int readbyte = stream.Read(data, 0, data.Length);
                            //анализирую данные
                            Message message = new Protokol().FromBytes(DataTrim(data, readbyte));
                            if (message != null)
                            {
                                if (message.MessageType == MessageView.answer)
                                {
                                    if (IsAuthorization != null)
                                        IsAuthorization(message.Locked);
                                }
                            }
                            //
                            stream.Close();
                        }
                        source.Close();
                    }
                }
            }
            catch (SocketException error)
            {
                if (isConnect)
                {
                    isConnect = false;
                    //если произошла ошибка связи блокируем экран
                    //блокируем экран
                    if (IsAuthorization != null)
                        IsAuthorization(true);
                }
            }
        }

        public void SendCommand(bool locked)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((object e) =>
            {
                try
                {
                    if (isConnect)
                    {
                        using (TcpClient source = new TcpClient(hostname, port) { SendTimeout = 100, ReceiveTimeout = 100 })
                        {
                            using (NetworkStream stream = source.GetStream())
                            {
                                //анализирую данные
                                byte[] data = new Protokol().ToBytes(new Message(MessageView.commad, locked));
                                //запрашиваем 
                                if (data != null)
                                    stream.Write(data, 0, data.Length);
                                //
                                stream.Close();
                            }
                            source.Close();
                        }
                    }
                }
                catch
                {
                    isConnect = false;
                }
            }, locked);

        }

    }
}
