using System;

using Authentificator;
using Authentificator.Enums;

namespace NewScreenSaver.RFIDModul
{
    public class RFIDScanFactory
    {
        public static RFIDScanBase Create(ViewReader viewReader, int baudRate, Authentificators auth, bool cardOn)
        {
            switch (viewReader)
            {
                case ViewReader.ironlogic:
                    {
                        return new RFIDScanIronLogic(baudRate, auth, cardOn);
                    }
                case ViewReader.crem:
                    {
                        return new RFIDScanCrem(baudRate, auth);
                    }
                case ViewReader.smartCard:
                    {
                        return new RFIDScanSmartCard(auth);
                    }
                default:
                    return new RFIDScanIronLogic(baudRate, auth, cardOn);
            }
        }
    }
}
