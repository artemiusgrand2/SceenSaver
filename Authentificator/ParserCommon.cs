using System;
using Authentificator.Enums;

namespace Authentificator
{
    public class ParserCommon
    {
        public static ViewReader GetViewCard(string nameViewCard)
        {
            if (ViewReader.crem.ToString() == nameViewCard)
                return ViewReader.crem;
            else if(ViewReader.crem.ToString() == nameViewCard)
                    return ViewReader.ironlogic;
            //
            return ViewReader.ironlogic;
        }

        public static bool GetNameAndSpeedComPort(string serialPortRead, out string nameSerial, out int baudRate)
        {
            nameSerial = string.Empty;
            baudRate = 0;
            var parserStr = serialPortRead.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parserStr.Length == 2 && int.TryParse(parserStr[1], out baudRate))
            {
                nameSerial = parserStr[0];
                return true;
            }
            //
            return false;
        }
    }
}
