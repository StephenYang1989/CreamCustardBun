using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Model
{
    public class ExchangeOption
    {
        public string ExchangeName { set; get; }

        public string ExchangeType { set; get; }

        public string RoutingKey { set; get; }


        public bool IsDurable { set; get; }

        public bool IsAutoDeleted { set; get; }
    }
}
