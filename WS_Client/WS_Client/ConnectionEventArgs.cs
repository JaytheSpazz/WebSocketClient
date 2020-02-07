using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp.CrestronWebSocketClient;

namespace WS_Client
{
    public class ConnectionEventArgs : EventArgs
    {
        public string ID { get; set; }
        public bool IsConnected { get; set; }
        public WebSocketClient.WEBSOCKET_RESULT_CODES ResultCode { get; set; }
    }
}