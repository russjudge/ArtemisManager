using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.WebSockets;

namespace AMCommunicator
{
    public class Network
    {
        const int udpPort = 2012;
        const int tcpPort = 2011;

        //TODO: Master/slave relationship?  --later.
        public static string Password { get; set; }
        private ManualResetEvent mreSender = new(false);
        Dictionary<string, connectionQueueItem> ConnectionThreads = new();
        private Network()
        {
            
        }

        public static Network? Current { get; private set; }
        public static Network GetNetwork(string password)
        {
            
            if (Current == null)
            {
                Current = new Network();
            }
            Password = password;
            return Current;
        }
        static Network()
        {
            MyHostname = Dns.GetHostName();
            MyIP = GetMyIP();
            ConnectionPort = tcpPort;
            Password = "";
        }
        private static bool abort = false;
        public void Shutdown()
        {
            
            HaltAll();
            
        }
        public void SendCommand(IPAddress target )
        {

        }
        public static int ConnectionPort { get; set; }

        private void Connect(string host, int port)
        {
            TcpClient? Liveclient = null;
            try
            {
                Liveclient = new TcpClient();
                //DebugNetwork(NetworkDebugEventArgs.TcpUdp.TCP, NetworkDebugEventArgs.TheDirection.Connecting, null);
                Liveclient.Connect(host, port);
                if (Liveclient.Connected)
                {
                    QueueThread(Liveclient, false);
                    Liveclient = null;
                }
            }
            catch (SocketException ex)
            {

            }
            finally
            {
                Liveclient?.Dispose();
            }

        }
        List<Thread> Threads = new List<Thread>();
        List<Socket> sockets = new();
        Dictionary<IPAddress, NetworkStream> activeStreams = new();
        public void HaltAll()
        {
            abort = true;
            server?.Stop();

            
            foreach (var socket in sockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            foreach (var thread in Threads)
            {
                thread.Interrupt();
                //thread.Abort();
            }
        }
        Queue<connectionQueueItem> queuedClients = new();
        /// <summary>
        /// This starts the TCP connection linking the command structure together.
        /// A TCP connection has been received here.
        /// </summary>
        void StartConnection()
        {
            IPAddress? remoteAddress = null;
            string? hostname = null;
            bool disconnect = false;
            var queuedItem = queuedClients.Dequeue();
            try
            {
                
                var generalEndPoint = queuedItem.client.Client.RemoteEndPoint;
                if (generalEndPoint != null)
                {

                    var endPoint = (IPEndPoint)generalEndPoint;

                    
                        remoteAddress = endPoint?.Address;
                        if (remoteAddress == null)
                        {
                            remoteAddress = IPAddress.None;
                        }
                        if (remoteAddress != MyIP)
                        {
                        bool OkaytoAdd = true;
                        foreach (var socket in sockets)
                        {
                            if (((IPEndPoint)socket.RemoteEndPoint).Address == remoteAddress)
                            {
                                OkaytoAdd = false;
                                break;
                            }
                        }
                        if (OkaytoAdd && remoteAddress != null)
                        {
                            sockets.Add(queuedItem.client.Client);

                            Threads.Add(queuedItem.thread);


                            var hostEntry = Dns.GetHostEntry(remoteAddress);
                            hostname = hostEntry.HostName;
                            ConnectionReceived?.Invoke(this, new ConnectionRequestEventArgs(remoteAddress, hostname));

                            //bool trackingset = false;
                            queuedItem.stream = queuedItem.client.GetStream();
                            activeStreams.Add(remoteAddress, queuedItem.stream);
                            byte[] buff;
                            List<byte> buffer;
                            int bytesRead = 0;
                            if (!queuedItem.FromListener)
                            {
                                StatusUpdated?.Invoke(this, new StatusUpdate("Sending Handshake to {0}:{1}", remoteAddress.ToString(), hostname));
                                //MUST SEND handshake item first here.
                                HandshakeMessage msg = new HandshakeMessage();
                                var bytes = msg.GetBytes();
                                StringBuilder sb = new();
                                sb.Append("{");
                                foreach (byte b in bytes)
                                {
                                    sb.Append(b.ToString() + " ");
                                }
                                sb.Append("}");
                                StatusUpdated?.Invoke(this, new StatusUpdate("Sending to {0}: {1}", hostname, sb.ToString()));
                                queuedItem.stream.Write(bytes, 0, bytes.Length);//@@@@
                            }
                            do
                            {
                                StatusUpdated?.Invoke(this, new StatusUpdate("Beginning read loop for {0}", hostname));
                                do
                                {




                                    buff = new byte[NetworkMessage.LengthPosition + 4];
                                    buffer = new List<byte>();
                                    bytesRead = queuedItem.stream.Read(buff, 0, buff.Length);
                                    StatusUpdated?.Invoke(this, new StatusUpdate("In message header, read {0} bytes from {1}", bytesRead, hostname));
                                    if (bytesRead > 0)
                                    {

                                        byte[] wrkByte = new byte[bytesRead];
                                        Array.Copy(buff, 0, wrkByte, 0, bytesRead);

                                        buffer.AddRange(wrkByte);
                                    }

                                } while (buffer.Count < (NetworkMessage.LengthPosition + 4) && bytesRead > 0);


                                if (bytesRead > 0)
                                {
                                    int msgLength = BitConverter.ToInt32(buffer.ToArray(), NetworkMessage.LengthPosition);


                                    List<byte> newBuffer = new List<byte>(buffer.ToArray());

                                    while (newBuffer.Count < msgLength && bytesRead > 0)
                                    {
                                        buff = new byte[msgLength - newBuffer.Count];
                                        bytesRead = queuedItem.stream.Read(buff, 0, buff.Length);
                                        //StatusUpdated?.Invoke(this, new StatusUpdate("In message body, read {0} bytes from {1}", bytesRead, hostname));
                                        if (bytesRead > 0)
                                        {
                                            byte[] wrkByte = new byte[bytesRead];
                                            Array.Copy(buff, 0, wrkByte, 0, bytesRead);

                                            newBuffer.AddRange(wrkByte);
                                        }
                                    }

                                    var theMessage = NetworkMessage.GetMessage(newBuffer.ToArray());
                                    if (theMessage != null)
                                    {
                                        theMessage.Source = remoteAddress;
                                        //if (!trackingset)
                                        //{
                                        //    ConnectionThreads.Add(theMessage.Source.ToString(), queuedItem);
                                        //    trackingset = true;
                                        //}
                                        //DebugNetwork(NetworkDebugEventArgs.TcpUdp.TCP, NetworkDebugEventArgs.TheDirection.Receiving, theMessage);
                                        disconnect = ProcessMessage(queuedItem.stream, theMessage);
                                    }
                                    else
                                    {
                                        disconnect = true;
                                    }
                                }
                                else
                                {
                                    abort = true;

                                    mreSender.Set();
                                }

                            } while (!abort && !disconnect);
                        }
                    }
                }
            }
            catch(ThreadInterruptedException)
            {

            }
            catch (ThreadAbortException)
            {

            }
            catch (System.IO.IOException e)
            {
                //RaiseExceptionEncountered(e);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                //RaiseExceptionEncountered(e);
            }
            catch (Exception e)
            {
                FatalExceptionEncountered?.Invoke(this, new FatalExceptionEncounteredEventArgs(e));   
            }
            finally
            {
                if (queuedItem.client!= null)
                {
                    sockets.Remove(queuedItem.client.Client);
                }
                if (queuedItem.client != null && queuedItem.client.Connected)
                {
                    queuedItem.client.Client.Shutdown(SocketShutdown.Both);
                    queuedItem.client.Close();
                }

            }
            ConnectionClosed?.Invoke(this, new ConnectionRequestEventArgs(remoteAddress, hostname));
          
        }
        bool ProcessMessage(NetworkStream stream, NetworkMessage? message)
        {
            bool disconnect = false;
            if (message != null)
            {
                StatusUpdated?.Invoke(this, new StatusUpdate("Processing command: {0}", message.Command.ToString()));
            }
            
            switch (message?.Command)
            {
                case MessageCommand.Handshake:
                    disconnect = ProcessHandshake(stream, (HandshakeMessage)message);
                    break;
                case MessageCommand.ClientInfo:
                    disconnect= ProcessClientInfoMessage(stream, (ClientInfoMessage)message);
                    break;
                case MessageCommand.RequestItem:
                    disconnect = ProcessRequestItem(stream, (RequestItemMessage)message);
                    break;
                case MessageCommand.Item:
                    break;
                case MessageCommand.ChangePassword:
                    break;
                case MessageCommand.PCAction:
                    disconnect = ProcessPCAction(stream, (PCActionMessage)message);
                    break;
                case MessageCommand.UpdateCheck:
                    break;
                case MessageCommand.AretmisAction:
                    break;
               // case MessageCommand.SetClientInfo:
                  //  break;
                default:
                    return false;
            }
           
            return disconnect;
        }
        public event EventHandler<StatusUpdate>? StatusUpdated;
        public event EventHandler<ItemRequestEventArgs>? ItemRequested;
        public event EventHandler<ConnectionRequestEventArgs>? ConnectionReceived;
        public event EventHandler<ConnectionRequestEventArgs>? ConnectionRequested;
        public event EventHandler<FatalExceptionEncounteredEventArgs>? FatalExceptionEncountered;
        public event EventHandler<ConnectionRequestEventArgs>? ConnectionClosed;
        private bool ProcessPCAction(NetworkStream stream, PCActionMessage msg)
        {
            switch ((PCActionMessage.Actions)msg.Action)
            {
                case PCActionMessage.Actions.DisconnectThisConnection:
                    return true;
                default:
                    return false;
            }
        }

        private bool ProcessRequestItem(NetworkStream stream, RequestItemMessage msg)
        {
            ItemRequested?.Invoke(this, new ItemRequestEventArgs(msg.Source, msg.ItemIdentifier.Message));
            return false;
        }
        public void SendItem(Stream streamSource, string target)
        {
            if (ConnectionThreads.ContainsKey(target))
            {
                using MemoryStream ms = new();
                streamSource.CopyTo(ms);
                ms.Position = 0;
                ItemMessage message = new ItemMessage();
                message.Unspecified = ms.ToArray();

            }
        }
        private bool ProcessHandshake(NetworkStream stream, HandshakeMessage message)
        {
            return !message.IsValid();   
        }


        private bool ProcessClientInfoMessage(NetworkStream stream, ClientInfoMessage message)
        {
            return false;
        }

        class connectionQueueItem
        {
            public connectionQueueItem(TcpClient client, Thread thread, bool fromListener)
            {
                this.client = client;
                FromListener = fromListener;
                this.thread = thread;
                stream = null;
            }
            public TcpClient client { get; set; }
            public Thread thread { get; set; }
            public bool FromListener { get; set; }
            public NetworkStream? stream { get; set; }
        }
        void QueueThread(TcpClient client, bool fromListener)
        {
            ThreadStart start = new ThreadStart(StartConnection);
            Thread thd = new(start);
            connectionQueueItem itemToQueue = new connectionQueueItem(client, thd, fromListener);
            queuedClients.Enqueue(itemToQueue);
            thd.Start();
        }
        TcpListener? server = null;

        private void ListenTCP()
        {
            try
            {
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(IPAddress.Any, ConnectionPort);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (!abort)
                {
                    TcpClient client = server.AcceptTcpClient();
                    
                    QueueThread(client, true);
                }
            }
            catch (ThreadInterruptedException) 
            {
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server?.Stop();
            }

        }
        
        private void ListenUDP()
        {
            using UdpClient client = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
            sockets.Add(client.Client);
            client.Client.Bind(RemoteIpEndPoint);
            try
            {
                while (!abort)
                {
                    var received = client.Receive(ref RemoteIpEndPoint);

                    var msg = System.Text.ASCIIEncoding.ASCII.GetString(received);
                    if (msg.StartsWith("-AM-"))
                    {
                        var data = msg.Split('|');
                        if (data.Length > 2)
                        {
                            IPAddress address = IPAddress.Parse(data[1]);

                            if (address != MyIP && address != null)
                            {
                                bool IsOkay = true;
                                foreach (var socket in sockets)
                                {
                                    if (socket.RemoteEndPoint != null && ((IPEndPoint)socket.RemoteEndPoint).Address == address)
                                    {
                                        IsOkay = false;
                                        break;
                                    }
                                }
                                if (IsOkay)
                                {
                                    int port;

                                    if (int.TryParse(data[2], out port))
                                    {
                                        ConnectionRequested?.DynamicInvoke(this, new ConnectionRequestEventArgs(address, "unknown"));
                                        Connect(data[1], port);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (SocketException)
            {

            }
            catch (ThreadInterruptedException) 
            {
            }
        }
    
        public void Connect()
        {
            //System.Threading.ThreadPool.QueueUserWorkItem(DoNetworkDebug);
            ThreadStart start = new ThreadStart(ListenTCP);
            Thread thd = new(start);
            Threads.Add(thd);
            thd.Start();
            
           
            BroadcastMe();
            start = new ThreadStart(ListenUDP);
            thd = new(start);
            Threads.Add(thd);
            thd.Start();
            
        }
        public static IPAddress? MyIP { get;private set; }
        public static string MyHostname { get;private set; }
        public void BroadcastMe()
        {
            
            if (MyIP != null)
            {
                StatusUpdated?.Invoke(this, new StatusUpdate("Broadcasting to Peers"));
                using UdpClient client = new UdpClient();
                
                string msg = string.Format("-AM-|{0}|{1}|{2}", MyIP.ToString(), ConnectionPort, MyHostname );
                client.EnableBroadcast=true;
                client.ExclusiveAddressUse = false;
                client.MulticastLoopback = false;
                var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
                client.Send(bytes, bytes.Length, "255.255.255.255", udpPort);
                
            }
            

        }

        static Queue<NetworkDebugEventArgs> NetworkDebugEvents = new Queue<NetworkDebugEventArgs>();

        public static EventHandler<NetworkDebugEventArgs>? OnNetworkDebug;


        //static void DebugNetwork(NetworkDebugEventArgs.TcpUdp type,
        //    NetworkDebugEventArgs.TheDirection direction, NetworkMessage message )
        //{
        //    NetworkDebugEvents.Enqueue(new NetworkDebugEventArgs(type, direction, message));
        //    debugReset.Set();
            
        //}
        static ManualResetEvent debugReset = new ManualResetEvent(false);
        static void DoNetworkDebug(object? state)
        {
            while (!abort)
            {
                debugReset.WaitOne();
                if (OnNetworkDebug != null)
                {
                    while (NetworkDebugEvents.Count > 0)
                    {
                        OnNetworkDebug.Invoke(null, NetworkDebugEvents.Dequeue());
                    }
                }
                else
                {
                    if (NetworkDebugEvents.Count > 100)
                    {
                        NetworkDebugEvents.Dequeue();
                    }
                }
                debugReset.Reset();
            }
        }
        public static IPAddress? GetMyIP()
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            // Get the IP
            var MyHost = Dns.GetHostEntry(hostName);  //Yes--this works for getting the IP of a machine by hostname, assuming the hostname exists.
            IPAddress? retVal = null;
            foreach (var MyIP in MyHost.AddressList)
            {
                if (MyIP.AddressFamily == AddressFamily.InterNetwork)
                {
                    retVal = MyIP;
                }
            }
            return retVal;
            
        }
        
        public static IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            throw new ArgumentException(string.Format("Can't find subnetmask for IP address '{0}'", address));
        }
        
    }
}