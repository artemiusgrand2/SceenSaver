using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSerialPort
{
    class Program
    {

        public enum ViewReadCard
        {
            ironlogic = 0,
            crem
        }

        static void Main(string[] args)
        {
            SerialPort mySerialPort = new SerialPort("COM13");
            var f = SerialPort.GetPortNames();
            var d =  ViewReadCard.ironlogic.ToString();
            mySerialPort.BaudRate = 38400;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.OnePointFive;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            try
            {
                mySerialPort.DataReceived += SerialPortDataReceived;
                mySerialPort.Open();
                var bytes = new byte[1] {1 };
                mySerialPort.Write(bytes, 0, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
            mySerialPort.Close();
        }
        private static void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            var data = serialPort.ReadExisting();
            Console.WriteLine("Data Received:");
            Console.WriteLine(data);
        }
    }
}
