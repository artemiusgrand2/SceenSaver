using System;
using System.Linq;
using System.Text;
using System.IO.Ports;

using Authentificator;
using Authentificator.Enums;

namespace NewScreenSaver.RFIDModul
{
    public class RFIDScanIronLogic : RFIDScanBase
    {
        /// <summary>
        /// флаг изъятия карточки
        /// </summary>
        string noCard = "NO CARD";

        string infoRequest = "i";

        /// <summary>
        /// Контроль наличия карточки на считывателе
        /// </summary>
        bool _cardOn = true;

        /// <summary>
        /// строка считывания
        /// </summary>
        private string _readString = string.Empty;

        public RFIDScanIronLogic(int baudRate, Authentificators auth, bool cardOn) : base(baudRate, auth)
        {
            _cardOn = cardOn;
            _viewReader = ViewReader.ironlogic;
        }

        protected override void Scan()
        {
            try
            {
                if (SerialPort.GetPortNames().Contains(_serialPort.PortName))
                {
                    if (!IsOpen)
                    {
                        System.Threading.Thread.Sleep(1000);
                        _serialPort.Open();
                        //
                        try
                        {
                            _serialPort.Write(infoRequest);
                            var data = new byte[1024];
                            _serialPort.Read(data, 0, data.Length);
                            IsOpen = true;
                        }
                        catch (Exception error)
                        {
                            _serialPort.Close();
                            IsOpen = false;
                            RenameComPort();
                        }

                    }
                    //
                    if (IsOpen)
                    {
                        byte[] data = new byte[1024];
                        int countByte = _serialPort.Read(data, 0, data.Length);
                        _readString += Encoding.ASCII.GetString(data, 0, countByte);
                        string login;
                        //
                        if (_readString.ToUpper().StartsWith(noCard))
                        {
                            if (_cardOn)
                                IsAuthorization = false;
                            //
                            _readString = string.Empty;
                        }
                        else if (_auth.Authenticate(_readString.Trim(), out login, _viewReader) == Authentificators.UserAuthentResult.OK)
                        {
                            MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} Аутентификация пользователя: {login}", "", "");
                            IsAuthorization = true;
                            _readString = string.Empty;
                        }
                    }
                }
                else
                {
                    if (IsOpen || _isfirst_connect)
                    {
                        if (_serialPort.IsOpen)
                            _serialPort.Close();
                        IsAuthorization = false;
                        IsOpen = false;
                        _isfirst_connect = false;
                    }
                    //
                    RenameComPort();
                }
            }
            catch (TimeoutException error)
            {
                if (_readString != string.Empty)
                {
                    MainWindow.SaveMessInFile(string.Format("{0} Неизвестный пользовтель: {1}", DateTime.Now.ToString(), _readString.Trim()), "", "");
                    _readString = string.Empty;
                }
            }
            catch (Exception error)
            {
                MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} {error.Message} ", "Scan", "107");
                if (_readString != string.Empty)
                    _readString = string.Empty;
                _serialPort.Close();
                IsOpen = false;
                RenameComPort();
            }
        }
    }
}
