using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Model
{
    public class QueueOption
    {
        public string QueueName { set; get; }

        public bool IsExclusive { set; get; }

        public bool IsDurable { set; get; }

        public bool IsAutoDeleted { set; get; }

        public string ConsumerTag { set; get; }

        public bool AutoAck { set; get; }
    }
}
