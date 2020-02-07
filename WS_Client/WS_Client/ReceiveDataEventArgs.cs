using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace WS_Client
{
    public class ReceiveDataEventArgs : EventArgs
    {
        public string ID { get; set; }
        public string Data { get; set; }
    }
}