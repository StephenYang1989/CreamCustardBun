using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Model
{
    public class UnknownExceptionEventArgs:EventArgs
    {
        public string Message { set; get; }
    }
}
