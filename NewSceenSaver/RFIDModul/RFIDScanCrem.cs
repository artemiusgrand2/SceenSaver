using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO.Ports;

using Authentificator;

namespace NewScreenSaver.RFIDModul
{
    public class RFIDScanCrem : RFIDScanBase
    {

        private DateTime _lastReadData;

        private readonly TimeSpan _waitRead = new TimeSpan(0, 0, 0, 0, 1100);

        public RFIDScanCrem(int baudRate, Authentificators auth) : base(baudRate, auth) { }

        protected override void Scan()
        {
            try
            {
                if (SerialPort.GetPortNames().Contains(_serialPort.PortName))
                {
                    if (!IsOpen)
                    {
                        System.Threading.Thread.Sleep(1);
                        _lastReadData = DateTime.Now;
                        _serialPort.DataReceived += SerialPort_DataReceived;
                        _serialPort.Open();
                        IsOpen = true;
                    }
                    else
                    {
                        if ((DateTime.Now - _lastReadData) > _waitRead)
                            Close();
                    }
                }
                else
                    Close();
            }
            catch (Exception error)
            {
                MainWindow.SaveMessInFile(error.Message, "timerCom_Elapsed", "200");
                Close();
            }
        }

        private void Close()
        {
            lock (_serialPort)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.Close();
                IsAuthorization = false;
                IsOpen = false;
                RenameComPort();
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_serialPort)
            {
                if (IsOpen)
                {
                    try
                    {
                        _lastReadData = DateTime.Now;
                        var dataRead = new byte[_serialPort.BytesToRead];
                        int countByte = _serialPort.Read(dataRead, 0, dataRead.Length);
                        string login;
                        if (_auth.Authenticate(dataRead, out login) == Authentificators.UserAuthentResult.OK)
                        {
                            if (!IsAuthorization)
                            {
                                MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} Аутентификация пользователя: {login}", "", "");
                                IsAuthorization = true;
                            }
                        }
                        else
                        {
                            IsAuthorization = false;
                            MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} Неизвестный пользовтель: {Encoding.UTF8.GetString(dataRead, 0, dataRead.Length)}", "", "");
                        }
                    }
                    catch(Exception error)
                    {
                        MainWindow.SaveMessInFile(error.Message, "_serialPort_DataReceived", "92");
                    }
                }
            }
        }
    }
}
