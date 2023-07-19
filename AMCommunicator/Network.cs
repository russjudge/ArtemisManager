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
using System;
using AMCommunicator.Messages;

namespace AMCommunicator
{
    public class Network
    {
        const int udpPort = 2012;
        const int tcpPort = 2011;

        //TODO: Master/slave relationship?  --later.
        public static string Password { get; set; }
        private readonly ManualResetEvent mreSender = new(false);
        
        private Network()
        {
            
        }

        public static Network? Current { get; private set; }
        public static Network GetNetwork(string password)
        {
            Current ??= new Network();
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
        
        public void SendCommand(IPAddress target )
        {

        }
        public static int ConnectionPort { get; set; }

        private void Connect(IPAddress target, int port)
        {
            RaiseStatusUpdate("(UDP broadcast received) Starting TCP connection to: {0} on port: {1}", target, port);
            TcpClient? Liveclient = null;
            try
            {
                Liveclient = new TcpClient(AddressFamily.InterNetwork);
                
                Liveclient.Connect(target, port);
                if (Liveclient.Connected)
                {
                    RaiseStatusUpdate("Successful connection to {0}", target);
                    QueueThread(Liveclient, false);
                    Liveclient = null;
                }
            }
            catch (SocketException ex)
            {
                RaiseStatusUpdate("SocketException connecting to {0}: \r\n{1}", target, ex.Message);
            }
            finally
            {
                Liveclient?.Dispose();
            }
            RaiseStatusUpdate("(from UDP broadcast receipt) Ending Connect to {0}", target);
        }
        public void Halt(IPAddress address)
        {
            if (activeConnections.TryGetValue(address, out var tracker))
            {
                RaiseStatusUpdate("Shutting down connection to host {0} - {1}", address, tracker.Hostname);
                tracker.Socket.Shutdown(SocketShutdown.Both);
                tracker.Socket.Close();
                tracker.Thread?.Interrupt();
                activeConnections.Remove(address);
            }
        }
        public void HaltAll()
        {
            RaiseStatusUpdate("Halting all connections and processes");
            abort = true;
            try
            {
                server?.Stop();

                foreach (var address in activeConnections.Keys)
                {
                    RaiseStatusUpdate("Shutting down connection to host {0} - {1}", address, activeConnections[address].Hostname);
                    try
                    {
                        activeConnections[address].Socket.Shutdown(SocketShutdown.Both);
                        activeConnections[address].Socket.Close();
                        activeConnections[address].Thread?.Interrupt();
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }
            activeConnections.Clear();
            RaiseStatusUpdate("All connections halted");
        }
        
        public void SendPing(IPAddress target)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                PingMessage msg = new()
                {
                    Acknowledge = true
                };
                Transmit(connection.Stream, msg);
            }

        }
        public void SendPCAction(IPAddress target, PCActions action, bool force)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                PCActionMessage msg = new(action, force);
                Transmit(connection.Stream, msg);
            }
        }
        public void SendArtemisAction(IPAddress target, ArtemisActions action, Guid identifier, string modJSON)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                ArtemisActionMessage msg = new(action, identifier, modJSON);
                Transmit(connection.Stream, msg);
            }
        }
        public void SendMessage(IPAddress target, string message)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                CommunicationMessage msg = new(message);

                Transmit(connection.Stream, msg);
            }
        }
        public void SendChangeSetting(IPAddress target, string settingName, string settingTarget)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                ChangeAppSettingMessage msg = new(settingName, settingTarget);

                Transmit(connection.Stream, msg);
            }
        }
        public void SendChangePassword(IPAddress target, string newPassword)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                ChangePasswordMessage msg = new(newPassword);

                Transmit(connection.Stream, msg);
            }
        }
        public void SendAlert(IPAddress target, AlertItems alert, string relatedData)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                AlertMessage msg = new(alert, relatedData);

                Transmit(connection.Stream, msg);
            }
        }
        public void SendClientInfo(IPAddress target, bool isMaster, bool connectOnStart, string[] installedMods,
            string[] installedMissions, bool artemisIsRunning, bool isUsingThisAppControlledArtemis, bool appInStartFolder)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                ClientInfoMessage msg = new(isMaster, connectOnStart, installedMods, installedMissions, artemisIsRunning, isUsingThisAppControlledArtemis, appInStartFolder);

                Transmit(connection.Stream, msg);
            }
        }
        public void SendModPackageRequest(IPAddress target, string modItem, string itemRequestedIdentifier)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                RequestModPackageMessage msg = new(modItem, itemRequestedIdentifier);
                Transmit(connection.Stream, msg);
            }
        }
        public void SendItem(IPAddress target, byte[] data, string? modItem)
        {
            if (activeConnections.TryGetValue(target, out var connection))
            {
                string mod = string.Empty;
                if (modItem != null)
                {
                    mod = modItem;
                }
                ModPackageMessage msg = new(mod, data);
                Transmit(connection.Stream, msg);
            }
        }
        //For tracking and processing on active connections.
        private readonly Dictionary<IPAddress, ConnectionTracker> activeConnections = new();

        //ONLY for starting connections.
        private readonly Queue<ConnectionQueueItem> queuedClients = new();
        /// <summary>
        /// This starts the TCP connection linking the command structure together.
        /// A TCP connection has been received here.
        /// </summary>
        void StartConnection()
        {
            IPAddress? remoteAddress = null;
            string? hostname = "";
            bool disconnect = false;
            var queuedItem = queuedClients.Dequeue();
            try
            {
                
                var generalEndPoint = queuedItem.Client.Client.RemoteEndPoint;
                if (generalEndPoint != null)
                {
                    var endPoint = (IPEndPoint)generalEndPoint;
                    remoteAddress = endPoint?.Address;
                    remoteAddress ??= IPAddress.None;
                    if (remoteAddress != null && remoteAddress != MyIP && remoteAddress != IPAddress.None && remoteAddress != IPAddress.Any)  //Don't want to connect to self or broadcast.
                    {
                        if (!activeConnections.ContainsKey(remoteAddress))
                        {
                            hostname = Dns.GetHostEntry(remoteAddress).HostName;
                            ConnectionTracker trackItem = new(hostname, remoteAddress, queuedItem.Client.GetStream(), queuedItem.Client.Client, queuedItem.Thread);
                            if (trackItem.Stream != null)
                            {
                                activeConnections.Add(remoteAddress, trackItem);
                                TCPStarter.Set();
                                RaiseConnectionReceived(remoteAddress, hostname);
                                byte[] buff;
                                List<byte> buffer;
                                int bytesRead = 0;
                                if (!queuedItem.FromListener) // (from UDP broadcast receipt)
                                {
                                    RaiseStatusUpdate("(from UDP broadcast receipt) Sending Handshake to {0}:{1}", remoteAddress.ToString(), hostname);
                                    //MUST SEND handshake item first here.
                                    HandshakeMessage msg = new();
                                    Transmit(trackItem.Stream, msg);
                                    
                                }
                                SendPCAction(remoteAddress, PCActions.SendClientInformation, true);
                                do
                                {
                                    RaiseStatusUpdate("Beginning read of stream loop for {0}", hostname);
                                    do
                                    {
                                        buff = new byte[NetworkMessageHeader.JSONDefinitionLengthPosition + NetworkMessageHeader.JSONDefinitionLength];
                                        buffer = new List<byte>();
                                        bytesRead = trackItem.Stream.Read(buff, 0, buff.Length);
                                        RaiseStatusUpdate("Read packet length: {0} bytes from {1}", bytesRead, hostname);
                                        if (bytesRead > 0)
                                        {
                                            byte[] wrkByte = new byte[bytesRead];
                                            Array.Copy(buff, 0, wrkByte, 0, bytesRead);
                                            buffer.AddRange(wrkByte);
                                        }

                                    } while (buffer.Count < (NetworkMessageHeader.JSONDefinitionLengthPosition + NetworkMessageHeader.JSONDefinitionLength) && bytesRead > 0);



                                    if (bytesRead > 0)
                                    {

                                        int msgLength = BitConverter.ToInt32(buffer.ToArray(), NetworkMessageHeader.JSONDefinitionLengthPosition) + NetworkMessageHeader.HeaderLength;

                                        RaiseStatusUpdate("Packet Length: {0}", msgLength);
                                        List<byte> newBuffer = new(buffer.ToArray());

                                        while (newBuffer.Count < msgLength && bytesRead > 0)
                                        {
                                            buff = new byte[msgLength - newBuffer.Count];
                                            bytesRead = trackItem.Stream.Read(buff, 0, buff.Length);

                                            if (bytesRead > 0)
                                            {
                                                byte[] wrkByte = new byte[bytesRead];
                                                Array.Copy(buff, 0, wrkByte, 0, bytesRead);
                                                newBuffer.AddRange(wrkByte);
                                            }
                                        }
                                        RaiseStatusUpdate("Finished reading Packet for host {0}.  Full packet size: {1} (includes Length property)", hostname, newBuffer.Count);
                                        NetworkMessageHeader header = new(newBuffer.ToArray());

                                        if (!string.IsNullOrEmpty(header.JSON))
                                        {
                                            RaiseStatusUpdate("{0} message received from {1}.  Processing...", header.Command, hostname);
                                            disconnect = ProcessMessage(trackItem.Stream, header, hostname);
                                            RaiseStatusUpdate("Done processing {0} from {1}...Disconnect required = {2}", header.Command, hostname, disconnect);
                                        }
                                        else
                                        {
                                            RaiseStatusUpdate("Unable to convert packet to a network message for packet form host {0}.  Disconnecting from host.", hostname);
                                            disconnect = true;
                                        }
                                    }
                                    else
                                    {
                                        disconnect = true;

                                        mreSender.Set();
                                    }

                                } while (!abort && !disconnect);
                            }
                        }
                        else
                        {
                            RaiseStatusUpdate("Duplicate connection request--not connection to host: {0} - {1}", remoteAddress, hostname);
                        }
                    }
                    else
                    {
                        if (remoteAddress != null)
                        {
                            RaiseStatusUpdate("Invalid connection request: request to connect to IP: {0}", remoteAddress);
                        }
                        else
                        {
                            RaiseStatusUpdate("Invalid connection request: request to connect to IP: unknown");
                        }
                    }
                }
                else
                {
                    RaiseStatusUpdate("Invalid endpoint connection request.  Endpoint is null");
                }
            }
            catch (ThreadInterruptedException)
            {

            }
            catch (ThreadAbortException)
            {

            }
            catch (System.IO.IOException e)
            {
                RaiseStatusUpdate("IOException in StartConnection on Host {0}:\r\n{1}", hostname, e.Message);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                RaiseStatusUpdate("SocketException in StartConnection on Host {0}:\r\n{1}", hostname, e.Message);
            }
            catch (Exception e)
            {
                RaiseFatal(e);
            }
            finally
            {
                RaiseStatusUpdate("Doing final cleanup of TCP connection to {0}", hostname);
                if (remoteAddress != null)
                {
                    RaiseStatusUpdate("Removing {0} from dictionary", remoteAddress);
                    activeConnections.Remove(remoteAddress);
                }
                if (queuedItem.Client != null && queuedItem.Client.Connected)
                {
                    queuedItem.Client.Client.Shutdown(SocketShutdown.Both);
                    queuedItem.Client.Close();
                }
            }
            ConnectionClosed?.Invoke(this, new ConnectionRequestEventArgs(remoteAddress, hostname));

        }
        void RaiseFatal(Exception e)
        {
            FatalExceptionEncountered?.Invoke(this, new FatalExceptionEncounteredEventArgs(e));
        }
        void RaiseMessageVersionMismatch(short expected, short actual, IPAddress source)
        {
            MessageVersionMismatch?.Invoke(this, new VersionMismatchEventArgs(expected, actual, source));
        }
        bool ProcessMessage(NetworkStream stream, NetworkMessageHeader? message, string hostname)
        {
            bool disconnect = false;
            if (message != null)
            {
                RaiseStatusUpdate("Processing command: {0} for host {1}", message.Command.ToString(), hostname);
            }
            
            switch (message?.Command)
            {
                case MessageCommand.Handshake:
                    disconnect = ProcessHandshake(stream, message?.GetItem<HandshakeMessage>());
                    break;
                case MessageCommand.ClientInfo:
                    disconnect= ProcessClientInfoMessage(stream, message?.GetItem<ClientInfoMessage>());
                    break;
                case MessageCommand.RequestModPackage:
                    disconnect = ProcessRequestModPackageItem(stream, message?.GetItem<RequestModPackageMessage>());
                    break;
                case MessageCommand.ModPackage:
                    disconnect = ProcessModPackage(stream, message?.GetItem<ModPackageMessage>());
                    break;
                case MessageCommand.ChangePassword:
                    ProcessPasswordChange(stream, message?.GetItem<ChangePasswordMessage>());
                    break;
                case MessageCommand.PCAction:
                    disconnect = ProcessPCAction(stream, message?.GetItem<PCActionMessage>(), hostname);
                    break;
                case MessageCommand.UpdateCheck:
                    break;
                case MessageCommand.AretmisAction:
                    ProcessArtemisAction(stream, message?.GetItem<ArtemisActionMessage>());
                    break;
                case MessageCommand.Communication:
                    disconnect = ProcessCommunication(stream, message?.GetItem<CommunicationMessage>());
                    break;
                case MessageCommand.Ping:
                    disconnect = ProcessPing(stream, message?.GetItem<PingMessage>(), hostname);
                    break;
                case MessageCommand.ChangeAppSetting:
                    disconnect = ProcessChangeSetting(stream, message?.GetItem<ChangeAppSettingMessage>());
                    break;
                case MessageCommand.Alert:
                    disconnect = ProcessAlert(stream, message?.GetItem<AlertMessage>());
                    break;
                default:
                    RaiseStatusUpdate("Invalid Message command.  Ignored.  Check for software update to Artemis Manager.");
                    return false;
            }
           
            return disconnect;
        }
        public event EventHandler<ChangeSettingEventArgs>? ChangeSetting;
        public event EventHandler<StatusUpdateEventArgs>? StatusUpdated;
        public event EventHandler<StatusUpdateEventArgs>? PopupMessageEvent;
        public event EventHandler<ModPackageRequestEventArgs>? ModPackageRequested;
        public event EventHandler<ModPackageEventArgs>? ModPackageReceived;
        public event EventHandler<ConnectionRequestEventArgs>? ConnectionReceived;
        public event EventHandler<ConnectionRequestEventArgs>? ConnectionRequested;
        public event EventHandler<FatalExceptionEncounteredEventArgs>? FatalExceptionEncountered;
        public event EventHandler<ConnectionRequestEventArgs>? ConnectionClosed;

        public event EventHandler<ActionCommandEventArgs>? ActionCommand;
        public event EventHandler<CommunicationMessageEventArgs>? CommunicationMessageReceived;
        public event EventHandler<CommunicationMessageEventArgs>? PasswordChanged;
        public event EventHandler<VersionMismatchEventArgs>? MessageVersionMismatch;
        public event EventHandler<AlertEventArgs>? AlertReceived;
        public event EventHandler<ClientInfoEventArgs>? ClientInfoReceived;
        public event EventHandler<ArtemisActionEventArgs>? ArtemisActionReceived;
        void Transmit(NetworkStream? stream, NetworkMessage msg)
        {
            if (stream == null)
            {
                RaiseStatusUpdate("in Transmit, but stream is null.  Cannot transmit {0}", msg.GetType().Name);
            }
            else
            {
                RaiseStatusUpdate("Transmitting {0}", msg.GetType().Name);
                var data = new NetworkMessageHeader(msg);

                var bytes = data.GetBytes();
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        private bool ProcessModPackage(NetworkStream? stream, ModPackageMessage? msg)
        {
            if (msg != null)
            {
                if (msg.MessageVersion != ModPackageMessage.ThisVersion)
                {
                    if (msg.Source == null)
                    {
                        return true;
                    }
                    else
                    {
                        //We might be unattended here, so we need to alert the sender of an issue.
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                            string.Format("ModPackageMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. Settings cannot be changed.",
                            ModPackageMessage.ThisVersion, msg.MessageVersion));
                    }
                }
                else
                {
                    ModPackageReceived?.Invoke(this, new ModPackageEventArgs(msg.Data, msg.ModItem));
                }
            }
            return false;
        }
        private bool ProcessArtemisAction(NetworkStream? stream, ArtemisActionMessage? msg)
        {
            if (msg != null)
            {
                if (msg.MessageVersion != ArtemisActionMessage.ThisVersion)
                {
                    if (msg.Source == null)
                    {
                        return true;
                    }
                    else
                    {
                        //We might be unattended here, so we need to alert the sender of an issue.
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                            string.Format("ArtemisActionMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. Settings cannot be changed.",
                            ArtemisActionMessage.ThisVersion, msg.MessageVersion));
                    }
                }
                else
                {
                    if (msg.Source != null)
                    {
                        ArtemisActionReceived?.Invoke(this, new ArtemisActionEventArgs(IPAddress.Parse(msg.Source), msg.Action, msg.ItemIdentifier, msg.Mod));
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool ProcessAlert(NetworkStream? stream, AlertMessage? msg)
        {
            if (msg != null)
            {
                IPAddress? IP = null;
                if (!string.IsNullOrEmpty(msg.Source))
                {
                    IP = IPAddress.Parse(msg.Source);
                }
                AlertReceived?.Invoke(this, new AlertEventArgs(IP, msg.AlertItem, msg.RelatedData));
            }
            return false;
        }
        private bool ProcessChangeSetting(NetworkStream? stream, ChangeAppSettingMessage? msg)
        {
            if (msg != null)
            {
                if (msg.MessageVersion != ChangeAppSettingMessage.ThisVersion)
                {
                    if (msg.Source == null)
                    {
                        return true;
                    }
                    else
                    {
                        //We might be unattended here, so we need to alert the sender of an issue.
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                            string.Format("ChangeAppSettingsMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. Settings cannot be changed.",
                            ChangeAppSettingMessage.ThisVersion, msg.MessageVersion));
                    }
                }
                else
                {
                    ChangeSetting?.Invoke(this, new ChangeSettingEventArgs(msg.SettingName, msg.SettingValue));

                }
            }
            return false;
        }
        private bool ProcessPasswordChange(NetworkStream? stream, ChangePasswordMessage? msg)
        {
            if (msg != null)
            {
                if (msg.MessageVersion != ChangePasswordMessage.ThisVersion)
                {
                    if (msg.Source == null)
                    {
                        return true;
                    }
                    else
                    {
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                          string.Format("ChangePasswordMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. Password cannot be changed.\r\nIf this peer is restarted, it will not be able to reconnect to the peer-to-peer network until the password is manually changed.",
                          ChangePasswordMessage.ThisVersion, msg.MessageVersion));
                    }
                }
                else
                {
                    PasswordChanged?.Invoke(this, new CommunicationMessageEventArgs(msg.Source, msg.NewPassword));
                }
                return false;
            }
            else
            {
                return true;
            }
            
        }
        private bool ProcessCommunication(NetworkStream? stream, CommunicationMessage? msg)
        {
            bool disconnect = false;
            if (msg != null)
            {
                if (msg.MessageVersion != CommunicationMessage.ThisVersion)
                {
                    if (msg.Source == null)
                    {
                        return true;
                    }
                    else
                    {
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                          string.Format("CommunicationMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. .",
                          CommunicationMessage.ThisVersion, msg.MessageVersion));
                    }
                }
                CommunicationMessageReceived?.Invoke(this, new CommunicationMessageEventArgs(msg.Source?.ToString(), msg.Message));
            }
            return disconnect;
        }
        
        private bool ProcessPing(NetworkStream stream, PingMessage? msg, string hostname)
        {
            if (msg != null)
            {
                if (msg.MessageVersion != PingMessage.ThisVersion)
                {
                    if (msg.Source != null)
                    {
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                          string.Format("PingMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended.",
                          PingMessage.ThisVersion, msg.MessageVersion));
                    }
                    else
                    {
                        return true;
                    }
                }
                PopupMessageEvent?.Invoke(this, new StatusUpdateEventArgs("Ping from {0} received.", hostname));
                if (msg.Acknowledge)
                {
                    msg.Acknowledge = false;
                    Transmit(stream, msg);
                }
            }
            return false;
        }
        private void RaiseActionCommand(ActionCommands action, bool force, IPAddress? source)
        {
            ActionCommand?.Invoke(this, new ActionCommandEventArgs(action, force, source));
        }
        private bool ProcessPCAction(NetworkStream stream, PCActionMessage? msg, string hostname)
        {
            if (msg != null)
            {
                if ((PCActions)msg.Action != PCActions.CheckForUpdate && msg.MessageVersion != PCActionMessage.ThisVersion)
                {
                    if (msg.Source != null)
                    {
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                          string.Format("PCActionMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. Unable to process PC Action until update is performed.",
                          PCActionMessage.ThisVersion, msg.MessageVersion));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    IPAddress? ip = null;
                    if (!string.IsNullOrEmpty(msg.Source))
                    {
                        ip = IPAddress.Parse(msg.Source);
                    }
                    switch ((PCActions)msg.Action)
                    {
                       
                        case PCActions.DisconnectThisConnection:
                            RaiseStatusUpdate("Disconnect requested from host {0}.", hostname);
                            return true;
                        case PCActions.CheckForUpdate:
                            RaiseStatusUpdate("Software Update check requested from {0}", hostname);
                            RaiseActionCommand(ActionCommands.UpdateCheck, msg.Force, ip);
                            return false;
                        case PCActions.ShutdownPC:
                            RaiseStatusUpdate("PC Shutdown requested from {0}", hostname);
                            RaiseActionCommand(ActionCommands.ShutdownPC, msg.Force, ip);
                            return true;
                        case PCActions.RestartPC:
                            RaiseStatusUpdate("PC Restart requested from {0}", hostname);
                            RaiseActionCommand(ActionCommands.RestartPC, msg.Force, ip);
                            return true;
                        case PCActions.CloseApp:
                            RaiseStatusUpdate("Application Close requested from {0}", hostname);
                            RaiseActionCommand(ActionCommands.CloseApp, msg.Force, ip);
                            return true;
                        case PCActions.SendClientInformation:
                            RaiseStatusUpdate("Client Information requested from {0}", hostname);
                            RaiseActionCommand(ActionCommands.ClientInformationRequested, msg.Force, ip);
                            return false;
                        case PCActions.RemoveAppFromStartup:
                            RaiseStatusUpdate("Remove App from startup requested from {0}", hostname);
                            RaiseActionCommand(ActionCommands.RemoveAppFromStartup, msg.Force, ip);
                            return false; ;
                        case PCActions.AddAppToStartup:
                            RaiseStatusUpdate("Add App to startup requested from {0}", hostname);
                            RaiseActionCommand(ActionCommands.AddAppToStartup, msg.Force, ip);
                            return false; ;
                        default:
                            RaiseStatusUpdate("Invalid PCAction requested from host {0}", hostname);
                            return false;
                    }
                }
            }
            return false;
        }

        private bool ProcessRequestModPackageItem(NetworkStream stream, RequestModPackageMessage? msg)
        {
            if (msg != null)
            {
                if (msg.MessageVersion != RequestModPackageMessage.ThisVersion)
                {
                    if (msg.Source != null)
                    {
                        SendAlert(IPAddress.Parse(msg.Source), AlertItems.MessageVersionMismatch,
                          string.Format("RequestItemMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. Unable to process request until update is performed.",
                          RequestModPackageMessage.ThisVersion, msg.MessageVersion));
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (msg.Source != null)
                    {
                        ModPackageRequested?.Invoke(this, new ModPackageRequestEventArgs(IPAddress.Parse(msg.Source), msg.ItemRequestedIdentifier, msg.Mod));
                    }
                }
            }
            return false;
        }

        private bool ProcessHandshake(NetworkStream stream, HandshakeMessage? message)
        {
            bool retVal = false;
            if (message != null)
            {
                if (message.IsValid() && message.MessageVersion != HandshakeMessage.ThisVersion)
                {
                    if (message.Source == null)
                    {
                        return true;
                    }
                    else
                    {
                        SendAlert(IPAddress.Parse(message.Source), AlertItems.MessageVersionMismatch,
                          string.Format("HandshakeMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended.",
                          HandshakeMessage.ThisVersion, message.MessageVersion));
                    }
                }
                retVal = !message.IsValid();
                if (retVal)
                {
                    if (message.Source == null)
                    {
                        retVal = true;
                    }
                    else
                    {
                        SendPCAction(IPAddress.Parse(message.Source), PCActions.SendClientInformation, true);
                    }
                }
            }
            return retVal;
        }
        private bool ProcessClientInfoMessage(NetworkStream stream, ClientInfoMessage? message)
        {
            if (message != null)
            {

                if (message.MessageVersion != ClientInfoMessage.ThisVersion)
                {
                    if (message.Source == null)
                    {
                        return true;
                    }
                    else
                    {
                        SendAlert(IPAddress.Parse(message.Source), AlertItems.MessageVersionMismatch,
                          string.Format("ClientInfoMessage: Expected version={0}, Actual version={1}.\r\nUpdate recommended. Unable to process Information until update is performed.",
                          ClientInfoMessage.ThisVersion, message.MessageVersion));
                    }
                }
                else
                {
                    ClientInfoReceived?.Invoke(this, new ClientInfoEventArgs(message));
                }
            }
            return false;
        }

        class ConnectionQueueItem
        {
            public ConnectionQueueItem(TcpClient client, Thread thread, bool fromListener)
            {
                this.Client = client;
                FromListener = fromListener;
                this.Thread = thread;
                Stream = null;
            }
            public TcpClient Client { get; set; }
            public Thread Thread { get; set; }
            public bool FromListener { get; set; }
            public NetworkStream? Stream { get; set; }
        }
        void QueueThread(TcpClient client, bool fromListener)
        {
            ThreadStart start = new(StartConnection);
            Thread thd = new(start);
            ConnectionQueueItem itemToQueue = new(client, thd, fromListener);
            queuedClients.Enqueue(itemToQueue);
            thd.Start();
        }
        TcpListener? server = null;
        private readonly ManualResetEvent TCPStarter = new(true);
        private void ListenTCP()
        {
            try
            {
                if (MyIP == null)
                {
                    RaiseStatusUpdate("Setting listener for IPAddress: NULL, on port: {0}", ConnectionPort);
                }
                else
                {
                    RaiseStatusUpdate("Setting listener for IPAddress: {0}, on port: {1}", MyIP, ConnectionPort);
                    server = new TcpListener(MyIP, ConnectionPort);

                    // Start listening for client requests.
                    server.Start();
                    RaiseStatusUpdate("TCP Listener server started.");
                    // Enter the listening loop.
                    while (!abort)
                    {
                        TCPStarter.WaitOne();
                        RaiseStatusUpdate("Waiting for remote TCP Client to connect...");
                        TcpClient client = server.AcceptTcpClient();
                        RaiseStatusUpdate("TCP client has connected...");
                        QueueThread(client, true);
                        TCPStarter.Reset();
                    }
                }
            }
            catch (ThreadInterruptedException) 
            {
            }
            catch (SocketException e)
            {
                RaiseStatusUpdate("SocketException on TCPListener server:\r\n{0}", e.Message);
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server?.Stop();
            }
            RaiseStatusUpdate("TCP Listener server HALTED.");
        }
        
        private void ListenUDP()
        {
            using UdpClient client = new();
            IPEndPoint RemoteIpEndPoint = new(IPAddress.Any, udpPort);
            activeConnections.Add(IPAddress.Any, new ConnectionTracker("UDP Listener Service", IPAddress.Any, null, client.Client, null));
            
            client.Client.Bind(RemoteIpEndPoint);
            try
            {
                while (!abort)
                {
                    RaiseStatusUpdate("Starting listen for UDP Broadcast");
                    var received = client.Receive(ref RemoteIpEndPoint);
                    RaiseStatusUpdate("Broadcast message received...Processing...");
                    var msg = System.Text.ASCIIEncoding.ASCII.GetString(received);
                    if (msg.StartsWith("-AM-"))
                    {
                        var data = msg.Split('|');
                        if (data.Length > 2)
                        {
                            RaiseStatusUpdate("Broadcast source: {0}", data[1]);
                            IPAddress address = IPAddress.Parse(data[1]);
                            if (address?.ToString() == MyIP?.ToString())
                            {
                                RaiseStatusUpdate("Invalid connection IP--received self. rejecting.");
                            }
                            else
                            {
                                if (address != null && address != IPAddress.Any && address != IPAddress.None)
                                {
                                    if (activeConnections.ContainsKey(address))
                                    {
                                        RaiseStatusUpdate("Duplicate connection request for {0}--rejected", address);
                                    }
                                    else
                                    {

                                        if (int.TryParse(data[2], out int port))
                                        {
                                            RaiseConnectionRequested(address, "unknown");
                                            Connect(address, port);
                                        }
                                        else
                                        {
                                            RaiseStatusUpdate("Malformed broadcast message received--bad port specified.");
                                        }
                                    }
                                }
                                else
                                {
                                    if (address == null)
                                    {
                                        RaiseStatusUpdate("Invalid connection IP: NULL");
                                    }
                                    else
                                    {
                                        RaiseStatusUpdate("Invalid connection IP: {0}", address);
                                    }
                                }
                            }
                        }
                        else
                        {
                            RaiseStatusUpdate("Malformed broadcast message received (good prefix): {0}", msg);
                        }
                    }
                    else
                    {
                        RaiseStatusUpdate("Malformed broadcast message received: {0}", msg);
                    }
                    RaiseStatusUpdate("Restarting UDP Listener Service Loop...");
                }
                RaiseStatusUpdate("Abort = true--quitting UDP Listener.");
                
            }
            catch (SocketException e)
            {
                RaiseStatusUpdate("SocketException on UDPListener server:\r\n{0}", e.Message);
            }
            catch (ThreadInterruptedException) 
            {
                RaiseStatusUpdate("ThreadInterruptedException received on UDP Listener");
            }
            RaiseStatusUpdate("UDP Listener server HALTED");
        }
        
        private void RaiseConnectionReceived(IPAddress? address, string host)
        {
            ConnectionReceived?.Invoke(this, new ConnectionRequestEventArgs(address, host));
        }
        private void RaiseConnectionRequested(IPAddress? address, string host)
        {
            ConnectionRequested?.Invoke(this, new ConnectionRequestEventArgs(address, host));
        }
        private void RaiseStatusUpdate(string message, params object[] parameters)
        {
            StatusUpdated?.Invoke(this, new StatusUpdateEventArgs(message, parameters));
        }
        public void Connect()
        {
            RaiseStatusUpdate("Starting Connection Services...");
            ThreadStart start = new(ListenTCP);
            Thread thd = new(start);
            thd.Start();
           
            BroadcastMe();
            start = new ThreadStart(ListenUDP);
            thd = new(start);
            thd.Start();
        }
        public static IPAddress? MyIP { get;private set; }
        public static string MyHostname { get;private set; }
        public void BroadcastMe()
        {
            if (MyIP != null)
            {
                RaiseStatusUpdate("Broadcasting to Peers");
                using UdpClient client = new();
                
                string msg = string.Format("-AM-|{0}|{1}|{2}", MyIP?.ToString(), ConnectionPort, MyHostname );
                client.EnableBroadcast=true;
                client.ExclusiveAddressUse = false;
                client.MulticastLoopback = false;
                var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
                client.Send(bytes, bytes.Length, "255.255.255.255", udpPort);
                
            }
            else
            {
                RaiseStatusUpdate("There is a problem.  I don't know my own IP (MyIP is null) and so can't broadcast.");
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
                    break;
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
            throw new ArgumentException(string.Format("Can't find subnet mask for IP address '{0}'", address));
        }
        
    }
}