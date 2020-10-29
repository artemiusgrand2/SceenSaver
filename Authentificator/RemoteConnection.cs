using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Authentificator
{
    public class RemoteConnection
    {

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
         string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };

        public enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }

        public static void Connect(string login, string password, string connectionString)
        {
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                var credentials = new NetworkCredential(login, password);
                var netResource = new NetResource()
                {
                    Scope = ResourceScope.GlobalNetwork,
                    ResourceType = ResourceType.Any,
                    DisplayType = ResourceDisplaytype.Share,
                    RemoteName = connectionString
                };
                var userName = string.IsNullOrEmpty(credentials.Domain)
                           ? credentials.UserName
                           : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

                var result = WNetAddConnection2(
                    netResource,
                    credentials.Password,
                    userName,
                    0);

                if (result == 1219)
                {
                    result = WNetCancelConnection2(netResource.RemoteName, 0, true);
                    result = WNetAddConnection2(netResource, credentials.Password, credentials.UserName, 0);
                }
            }
        }

    }
}
