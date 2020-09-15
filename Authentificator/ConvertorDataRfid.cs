using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Authentificator.Enums;
namespace Authentificator
{
    public class ConvertorDataRfid
    {

        public static string ConvertFromBytesToStr(byte[] data, ViewReader viewReader)
        {
            switch (viewReader)
            {
                case ViewReader.crem:
                    {
                        var strBuilder = new StringBuilder();
                        for (var index = 0; index < data.Length; index++)
                            strBuilder.Append((index != data.Length - 1) ? string.Format("{0:X2}", data[index]) + " " : string.Format("{0:X2}", data[index]));
                        //
                        return strBuilder.ToString();
                    }
                case ViewReader.ironlogic:
                    {
                        return Encoding.UTF8.GetString(data);
                    }
                default:
                    return Encoding.UTF8.GetString(data);
            }
        }

        public static Tuple<string, bool> ConvertFromStrToStr(string data, ViewReader viewReader)
        {
            switch (viewReader)
            {
                case ViewReader.crem:
                    {
                        var bytes = new List<byte>();
                        foreach (var byteHex in data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            byte bufferByte;
                            if (byte.TryParse(byteHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bufferByte))
                                bytes.Add(bufferByte);
                            else
                                return new Tuple<string, bool>(string.Empty, false);
                        }
                        //
                        return new Tuple<string, bool>(Encoding.Unicode.GetString(bytes.ToArray()), true);
                    }
                case ViewReader.ironlogic:
                    return new Tuple<string, bool>(data, true);
                default:
                    return new Tuple<string, bool>(data, true);
            }
        }
    }
}
