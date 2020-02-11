using System;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp.CrestronWebSocketClient;

namespace WS_Client
{
    public class WsClient
    {
        private WebSocketClient _client;
        private bool connectionRequested = false;
        private bool autoReconnect;
        private bool debugMode;
        private bool isConnected;

        public delegate void ConnectionStatus(ushort status, SimplSharpString resultCode);
        public delegate void DataReceived(SimplSharpString data);
        public ConnectionStatus onConnectionStatus { get; set; }
        public DataReceived onDataReceived { get; set; }

        public event EventHandler<ConnectionEventArgs> ConnectionStatusChange;
        public event EventHandler<ReceiveDataEventArgs> ReceiveDataChange;

        public string ID { get; set; }
        public ushort IsConnected
        {
            get { return Convert.ToUInt16(isConnected); }
        }
        public ushort DebugMode
        {
            get { return Convert.ToUInt16(debugMode); }
            set { debugMode = Convert.ToBoolean(value); }
        }
        public ushort AutoReconnect
        {
            get { return Convert.ToUInt16(autoReconnect); }
            set { autoReconnect = Convert.ToBoolean(value); }
        }

        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public WsClient()
        {
            _client = new WebSocketClient();

            _client.ConnectionCallBack = ClientConnectCallBack;
            _client.DisconnectCallBack = ClientDisconnectCallBack;
            _client.ReceiveCallBack = ClientReceiveCallBack;
            _client.SendCallBack = ClientSendCallBack;

            _client.KeepAlive = true;
        }

        public void Connect(string ipAddress, ushort port)
        {
            try
            {
                if (connectionRequested == false && !isConnected)
                {
                    connectionRequested = true;

                    _client.URL = "ws://" + ipAddress;
                    _client.Port = port;
                    

                    _client.ConnectAsync();
                }
            }
            catch (SocketException se)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} SocketException Occured in Connect", ID), se);
                }
            }
            catch (Exception e)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} Exception Occured in Connect", ID), e);
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                if (connectionRequested && isConnected)
                {
                    connectionRequested = false;
                    _client.DisconnectAsync(this);
                }
            }
            catch (SocketException se)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} SocketException Occured in Disconnect", ID), se);
                }
            }
            catch (Exception e)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} Exception Occured in Disconnect", ID), e);
                }
            }
        }

        public void SendData(string data)
        {
            try
            {
                var bytes = Encoding.ASCII.GetBytes(data);

                _client.SendAsync(bytes, Convert.ToUInt16(bytes.Length), WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__TEXT_FRAME);
            }
            catch (SocketException se)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} SocketException Occured in SendData", ID), se);
                }
            }
            catch (Exception e)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} Exception Occured in SendData", ID), e);
                }
            }
        }

        private void OnConnectionStatusChange(ConnectionEventArgs e)
        {
            EventHandler<ConnectionEventArgs> handler = ConnectionStatusChange;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnReceiveDataChange(ReceiveDataEventArgs e)
        {
            EventHandler<ReceiveDataEventArgs> handler = ReceiveDataChange;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private int ClientConnectCallBack(WebSocketClient.WEBSOCKET_RESULT_CODES resultCode)
        {
            try
            {
                if (resultCode == WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
                {
                    isConnected = true;

                    if (onConnectionStatus != null)
                    {
                        onConnectionStatus(1, resultCode.ToString());
                    }

                    ConnectionEventArgs e = new ConnectionEventArgs();
                    e.ID = ID;
                    e.IsConnected = isConnected;
                    e.ResultCode = resultCode;

                    OnConnectionStatusChange(e);

                    _client.ReceiveAsync();

                    return 1;
                }
                else
                {
                    isConnected = false;

                    if (onConnectionStatus != null)
                    {
                        onConnectionStatus(1, resultCode.ToString());
                    }

                    ConnectionEventArgs e = new ConnectionEventArgs();
                    e.ID = ID;
                    e.IsConnected = isConnected;
                    e.ResultCode = resultCode;

                    OnConnectionStatusChange(e);

                    return 0;
                }
            }
            catch (SocketException se)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} SocketException Occured in ClientConnectCallBack", ID), se);
                }

                return 0;
            }
            catch (Exception e)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} Exception Occured in ClientConnectCallBack", ID), e);
                }

                return 0;
            }
            finally
            {
                if (connectionRequested && !_client.Connected && autoReconnect)
                {
                    _client.ConnectAsync();
                }
            }
        }

        private int ClientDisconnectCallBack(WebSocketClient.WEBSOCKET_RESULT_CODES resultCode, object obj)
        {
            try
            {
                if (resultCode == WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
                {
                    isConnected = false;

                    if (onConnectionStatus != null)
                    {
                        onConnectionStatus(0, resultCode.ToString());
                    }

                    ConnectionEventArgs e = new ConnectionEventArgs();
                    e.ID = ID;
                    e.IsConnected = isConnected;
                    e.ResultCode = resultCode;

                    OnConnectionStatusChange(e);

                    return 1;
                }
                else
                {
                    if (onConnectionStatus != null)
                    {
                        onConnectionStatus(0, resultCode.ToString());
                    }

                    ConnectionEventArgs e = new ConnectionEventArgs();
                    e.ID = ID;
                    e.IsConnected = isConnected;
                    e.ResultCode = resultCode;

                    OnConnectionStatusChange(e);
                    return 0;
                }
            }
            catch (SocketException se)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} SocketException Occured in ClientDisconnectCallBack", ID), se);
                }

                return 0;
            }
            catch (Exception e)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} Exception Occured in ClientDisconnectCallBack", ID), e);
                }

                return 0;
            }
            finally
            {
                if (connectionRequested && !_client.Connected && autoReconnect)
                {
                    _client.ConnectAsync();
                }
            }
        }

        private int ClientReceiveCallBack(byte[] data, uint length, WebSocketClient.WEBSOCKET_PACKET_TYPES opCode, WebSocketClient.WEBSOCKET_RESULT_CODES resultCode)
        {
            try
            {
                if (opCode == WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__CLOSE)
                {
                    _client.DisconnectAsync(this);

                    return 0;
                }
                else if (resultCode == WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
                {
                    var sData = Encoding.ASCII.GetString(data, 0, data.Length);

                    ReceiveDataEventArgs e = new ReceiveDataEventArgs();
                    e.ID = ID;
                    e.Data = sData;

                    OnReceiveDataChange(e);

                    if (onDataReceived != null)
                    {
                        onDataReceived(sData);
                    }

                    return 1;
                }
                else if (resultCode == WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_INVALID_HANDLE)
                {
                    _client.DisconnectAsync(this);

                    return 0;
                }
                else
                {
                    return 0;
                }
            }
            catch (SocketException se)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} SocketException Occured in ClientReceiveCallBack", ID), se);
                }

                return 0;
            }
            catch (Exception e)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} Exception Occured in ClientReceiveCallBack", ID), e);
                }

                return 0;
            }
            finally
            {
                if (_client.Connected)
                {
                    _client.ReceiveAsync();
                }
            }
        }

        private int ClientSendCallBack(WebSocketClient.WEBSOCKET_RESULT_CODES resultCode)
        {
            try
            {
                if (resultCode == WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
                {
                    /*byte[] data;
                    WebSocketClient.WEBSOCKET_PACKET_TYPES opCode;

                    _client.Receive(out data, out opCode);

                    var sData = Encoding.ASCII.GetString(data, 0, data.Length);

                    ReceiveDataEventArgs e = new ReceiveDataEventArgs();
                    e.ID = ID;
                    e.Data = sData;

                    OnReceiveDataChange(e);

                    if (onDataReceived != null)
                    {
                        onDataReceived(sData);
                    }*/
                }

                return 1;
            }
            catch (SocketException se)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} SocketException Occured in ClientSendCallBack", ID), se);
                }

                return 0;
            }
            catch (Exception e)
            {
                if (debugMode)
                {
                    ErrorLog.Exception(string.Format("WebSocketCleint ID {0} Exception Occured in ClientSendCallBack", ID), e);
                }

                return 0;
            }
        }
    }
}
