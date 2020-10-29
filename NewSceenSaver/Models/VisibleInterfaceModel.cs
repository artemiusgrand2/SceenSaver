using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;

using NewScreenSaver.RFIDModul;

namespace NewScreenSaver.Models
{
    public class VisibleInterfaceModel :ViewModelBase
    {

        RFIDScanBase rfid;

        public bool IsOpen
        {
            get
            {
                if (rfid != null)
                    return rfid.IsOpen;
                else
                    return false;
            }
        }

        //private string 

        //public string Login
        //{
        //    get
        //    {
        //        return rfid.IsOpen;
        //    }
        //}


        public void Initialization(RFIDScanBase rfid, Dispatcher dispatcher)
        {
            this.rfid = rfid;
            rfid.ConnectComPort +=
                (status) => { OnPropertyChanged("IsOpen"); };
            //dispatcher.Invoke(DispatcherPriority.Render,
            //                                          new Action(() =>
            //                                          {

            //                                          }));
        }
    }
}
