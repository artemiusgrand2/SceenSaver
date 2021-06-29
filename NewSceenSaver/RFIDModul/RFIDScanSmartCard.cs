using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using PCSC.Utils;

using Authentificator;
using Authentificator.Enums;

using NewScreenSaver.Enums;

namespace NewScreenSaver.RFIDModul
{
    public class RFIDScanSmartCard : RFIDScanBase
    {
        ISCardMonitor _monitor = null;
        SCRState _currStat;

        public SCRState CurrStat
        {
            get
            {
                return _currStat;
            }
            private set
            {
                _currStat = value;
                IsOpen = !((_currStat == (SCRState.Ignore | SCRState.Unavailable) || _currStat == SCRState.Ignore || _currStat == SCRState.Unaware));
            }
        }

        public RFIDScanSmartCard(Authentificators auth) : base(auth)
        {
            _viewReader = ViewReader.smartCard;
            Initialization();
        }

        public new void Start()
        {
            base.Start();
        }

        public new void Stop()
        {
            base.Stop();
            Close();
        }

        private void Close()
        {
            if (_monitor != null)
            {
                _monitor.Cancel();
                _monitor.Dispose();
            }
        }

        private void Initialization()
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

        private string[] GetReaderNames()
        {
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                {
                    return context.GetReaders();
                }
            }
            catch(Exception error)
            {
                MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} {error.Message} ", "Scan", "107");
            }
            //
            return null;
        }

        private bool IsEmpty(ICollection<string> readerNames) => readerNames == null || readerNames.Count < 1;

        private void AttachToAllEvents(ISCardMonitor monitor)
        {
            monitor.CardInserted += (sender, args) => CardEvent(StatusSmartCard.inserted, args);
            monitor.CardRemoved += (sender, args) => CardEvent(StatusSmartCard.removed, args);
            monitor.Initialized += (sender, args) => CardEvent(StatusSmartCard.initialized, args);
            monitor.StatusChanged += Monitor_StatusChanged;
        }

        private void Monitor_StatusChanged(object sender, StatusChangeEventArgs e)
        {
            AnalisStateReader(e.NewState, e.Atr);
        }

        private void CardEvent(StatusSmartCard statusCard, CardStatusEventArgs cardEvent)
        {
            AnalisStateReader(cardEvent.State, cardEvent.Atr);
        }

        private void AnalisStateReader(SCRState state, byte[] atr)
        {
            CurrStat = state;
            if ((SCRState)((int)state & 63) == SCRState.Present)
            {
                string login;
                if (_auth.Authenticate(Encoding.Unicode.GetString(atr ?? new byte[0]), out login, _viewReader) == Authentificators.UserAuthentResult.OK)
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
                    MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} Неизвестный пользовтель: {ConvertorDataRfid.ConvertFromBytesToStr(atr ?? new byte[0], _viewReader)}", "", "");

                }
            }
            else
                IsAuthorization = false;
        }

        protected override void Scan()
        {
            try
            {
                if (!IsOpen)
                {
                    Initialization();
                }
            }
            catch (Exception error)
            {
                MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} {error.Message} ", "Scan", "129");
            }
        }
    }
}
