using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Authentificator;
using Authentificator.Enums;

namespace NewScreenSaver.RFIDModul
{
    public class RFIDScanCrem : RFIDScanSerialBase
    {

        private DateTime _lastReadData;

        private readonly TimeSpan _waitRead = new TimeSpan(0, 0, 0, 0, 1150);

        public RFIDScanCrem(int baudRate, Authentificators auth) : base(baudRate, auth)
        {
            _viewReader = ViewReader.crem;
        }

        protected override void Scan()
        {
            try
            {
                if (CheckNameComPorts(_serialPort.PortName))
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
                MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} {error.Message} ", "Scan", "45");
                Close();
            }
        }

        private void Close()
        {
            _serialPort.Close();
            IsOpen = false;
            _serialPort.DataReceived -= SerialPort_DataReceived;
            IsAuthorization = false;
            RenameComPort();
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (IsOpen)
            {
                try
                {
                    _lastReadData = DateTime.Now;
                    var dataRead = new byte[_serialPort.BytesToRead];
                    int countByte = _serialPort.Read(dataRead, 0, dataRead.Length);
                    string login;
                    if (_auth.Authenticate(Encoding.Unicode.GetString(dataRead), out login, _viewReader) == Authentificators.UserAuthentResult.OK)
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
                        MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} Неизвестный пользовтель: {ConvertorDataRfid.ConvertFromBytesToStr(dataRead, _viewReader)}", "", "");
                    }
                }
                catch (Exception error)
                {
                    MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} {error.Message} ", "SerialPort_DataReceived", "90");
                }
            }
        }
    }
}
