using System;
using System.Collections.Generic;
using System.Text;

using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using PCSC.Utils;

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
            if (_monitor != null)
            {
                _monitor.Cancel();
                _monitor.Dispose();
            }
        }

        private void Init()
        {
            Close();
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
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                {
                    return context.GetReaders();
                }
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
            if (EventCardInserted != null)
                EventCardInserted(ConvertorDataRfid.ConvertFromBytesToStr((unknown.Atr ?? new byte[0]), ViewReader.smartCard));
        }
    }
}
