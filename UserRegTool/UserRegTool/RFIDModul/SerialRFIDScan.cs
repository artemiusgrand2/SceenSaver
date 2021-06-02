using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;

using Authentificator;
using Authentificator.Enums;

namespace UserRegTool.RFIDModul
{
    public class SerialRFIDScan : IRFIDScan
    {
        SerialPort _serialPort = null;
        ViewReader _viewReadCard;
        public event CardInsertedDelegate EventCardInserted;

        public SerialRFIDScan(string serialName, int baudRate, ViewReader viewReadCard)
        {
            _viewReadCard = viewReadCard;
            if (SerialPort.GetPortNames().Contains(serialName))
            {
                _serialPort = new SerialPort(serialName, baudRate);
                _serialPort.RtsEnable = true;
                if (viewReadCard == ViewReader.ironlogic)
                    _serialPort.Encoding = Encoding.UTF8;
                _serialPort.DataReceived += SerialPortDataReceived;
                _serialPort.Open();
            }
            else
            {
                var ports = new StringBuilder();
                SerialPort.GetPortNames().ToList().ForEach(x => ports.Append($"{x} "));
                throw new Exception($"ComPort- {serialName} не существует. Список портов: {ports.ToString()}");
            }
        }
        public void Stop()
        {
            if(_serialPort != null)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            var bytes = new byte[serialPort.BytesToRead];
            serialPort.Read(bytes, 0, bytes.Length);
            if (EventCardInserted != null)
                EventCardInserted(ConvertorDataRfid.ConvertFromBytesToStr(bytes, _viewReadCard));
        }
    }
}
