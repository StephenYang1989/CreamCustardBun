using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Model
{
    public class HostOption
    {
        public string ClientName { set; get; }
        public string Host { set; get; }

        public int Port { set; get; }

        public string VirtualHost { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public int ConnectionTimeout { set; get; } = 30;

        public int HeartBeat { set; get; } = 10;
    }
}
