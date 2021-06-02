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

        public PCSCRFIDScan()
        {
            var readerNames = GetReaderNames();
            //
            if (IsEmpty(readerNames))
                throw new Exception($"Не найдено ни одного считывателя !!!");
            //
            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
            AttachToAllEvents(_monitor); 
            _monitor.Start(readerNames);
        }

        public void Stop()
        {
            if (_monitor != null)
            {
                _monitor.Cancel();
                _monitor.Dispose();
            }
        }

        private string[] GetReaderNames()
        {
            using (var context = ContextFactory.Instance.Establish(SCardScope.System))
            {
                return context.GetReaders();
            }
        }

        private bool IsEmpty(ICollection<string> readerNames) => readerNames == null || readerNames.Count < 1;

        private void AttachToAllEvents(ISCardMonitor monitor)
        {
            monitor.CardInserted += (sender, args) => DisplayEvent("CardInserted", args);
            monitor.CardRemoved += (sender, args) => DisplayEvent("CardRemoved", args);
            monitor.Initialized += (sender, args) => DisplayEvent("Initialized", args);
        }

        private void DisplayEvent(string eventName, CardStatusEventArgs unknown)
        {
            if (EventCardInserted != null)
                EventCardInserted(BitConverter.ToString(unknown.Atr ?? new byte[0]));
        }
    }
}
