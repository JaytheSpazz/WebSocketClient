using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace WebSocketClient
{
    public class ConnectionEventArgs : EventArgs
    {
        public string ID { get; set; }
        public bool IsConnected { get; set; }
        public Crestron.SimplSharp.CrestronWebSocketClient.WebSocketClient.WEBSOCKET_RESULT_CODES ResultCode { get; set; }
    }
}