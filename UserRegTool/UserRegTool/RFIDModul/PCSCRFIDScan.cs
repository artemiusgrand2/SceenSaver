using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using PCSC.Utils;
using PCSC.Iso7816;

using Authentificator;
using Authentificator.Enums;

namespace UserRegTool.RFIDModul
{
    public class PCSCRFIDScan : IRFIDScan
    {
        ISCardMonitor _monitor = null;

        public event CardInsertedDelegate EventCardInserted;
        System.Threading.Thread _threadScan;
        bool _isStop = false;
        SCardReader _reader;
        SCardContext _context;

        public bool IsOpen
        {
            get
            {
                return !((_currStat == (SCRState.Ignore | SCRState.Unavailable) || _currStat == SCRState.Ignore || _currStat == SCRState.Unaware));
            }
        }

        SCRState _currStat;

        public PCSCRFIDScan()
        {
            _threadScan = new System.Threading.Thread(Scan);
            _threadScan.Start();
        }

        public void Stop()
        {
            Close();
            _isStop = true;
            _threadScan.Join();
        }

        private void Close()
        {
            try
            {
                if (_reader != null)
                    _reader.Dispose();
                //
                if (_monitor != null)
                {
                    _monitor.Cancel();
                    _monitor.Dispose();
                }
                //
                if (_context != null)
                    _context.Dispose();
            }
            catch (Exception) { }
        }

        private void Init()
        {
            Close();

            _context = new SCardContext();
            _context.Establish(SCardScope.System);
            _reader = new SCardReader(_context);
            var readerNames = GetReaderNames();
            //
            if (!IsEmpty(readerNames))
            {
                _monitor = MonitorFactory.Instance.Create(SCardScope.System);
                AttachToAllEvents(_monitor);
                _monitor.Start(readerNames);
            }
        }

        private void Scan()
        {
            while (!_isStop)
            {
                if (!IsOpen)
                {
                    Init();
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private string[] GetReaderNames()
        {
            try
            {
                return _context.GetReaders();
            }
            catch { }
            //
            return null;
        }

        private bool IsEmpty(ICollection<string> readerNames) => readerNames == null || readerNames.Count < 1;

        private void AttachToAllEvents(ISCardMonitor monitor)
        {
            monitor.CardInserted += (sender, args) => DisplayEvent("CardInserted", args);
            monitor.CardRemoved += (sender, args) => DisplayEvent("CardRemoved", args);
            monitor.Initialized += (sender, args) => DisplayEvent("Initialized", args);
            monitor.StatusChanged += Monitor_StatusChanged;
        }

        private void Monitor_StatusChanged(object sender, StatusChangeEventArgs e)
        {
            _currStat = e.NewState;
        }

        private void DisplayEvent(string eventName, CardStatusEventArgs unknown)
        {
            _currStat = unknown.State;

            if (unknown.Atr != null && unknown.Atr.Length > 0)
            {
                if (EventCardInserted != null)
                    EventCardInserted(ConvertorDataRfid.ConvertFromBytesToStr(GetUID(unknown.ReaderName), ViewReader.smartCard));
            }

        }
    

        private byte[] GetUID(String readerName)
        {
            byte[] uid = new byte[0];
            try
            {
                if (_reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any) == SCardError.Success)
                {
                    var apdu = new CommandApdu(IsoCase.Case2Short, _reader.ActiveProtocol)
                    {
                        CLA = 0xFF,
                        Instruction = InstructionCode.GetData,
                        P1 = 0x00,
                        P2 = 0x00,
                        Le = 0x00
                    };
                    //
                    _reader.BeginTransaction();
                    var receiveBuffer = new byte[6];
                    var answeCom = _reader.Transmit(apdu.ToArray(), ref receiveBuffer);
                    if (answeCom == SCardError.Success)
                        uid = receiveBuffer;
                    _reader.EndTransaction(SCardReaderDisposition.Leave);
                    _reader.Disconnect(SCardReaderDisposition.Reset);
                    
                }
            }
            catch { }
            return uid;
        }
    }
}
