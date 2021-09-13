using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using PCSC.Utils;
using PCSC.Iso7816;

using Authentificator;
using Authentificator.Enums;

using NewScreenSaver.Enums;

namespace NewScreenSaver.RFIDModul
{
    public class RFIDScanSmartCard : RFIDScanBase
    {
        ISCardMonitor _monitor = null;
        SCardReader _reader;
        SCardContext _context;
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

        private void Initialization()
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

        private string[] GetReaderNames()
        {
            try
            {
                return _context.GetReaders();
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
            AnalisStateReader(e.NewState, e.ReaderName);
        }

        private void CardEvent(StatusSmartCard statusCard, CardStatusEventArgs cardEvent)
        {
            AnalisStateReader(cardEvent.State, cardEvent.ReaderName);
        }

        private void AnalisStateReader(SCRState state, string readerName)
        {
            CurrStat = state;
            if ((SCRState)((int)state & 63) == SCRState.Present)
            {
                string login;
                var uId = GetUID(readerName);
                if (_auth.Authenticate(Encoding.Unicode.GetString(uId), out login, _viewReader) == Authentificators.UserAuthentResult.OK)
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
                    MainWindow.SaveMessInFile($"{DateTime.Now.ToString()} Неизвестный пользовтель: {ConvertorDataRfid.ConvertFromBytesToStr(uId, _viewReader)}", "", "");

                }
            }
            else
                IsAuthorization = false;
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
