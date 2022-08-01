
//
// this has been tested and appears to be good to go.
//

namespace library
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Provides a wrapper of a tcp socket that is finely controlled.  It provides sending
    /// and receiving data events to subscribe to.  Receiving and accepting is done asynchronously
    /// where as sending and connecting are done synchronously.
    /// </summary>
    public sealed class ReliableSocket : IOPort
    {
        /// <summary>
        /// Call bind after using this constructor.
        /// </summary>
        public ReliableSocket()
        {
            useIPv6 = false;
        }

        /// <summary>
        /// Binds the socket using the information.
        /// </summary>
        /// <param name="bindingInformation"></param>
        public ReliableSocket(SocketBindingEventArgs bindingInformation)
        {
            useIPv6 = false;
            bind(bindingInformation);
        }

        /// <summary>
        /// Automatically clean up if haven't done so.
        /// </summary>
        ~ReliableSocket()
        {
            close();
        }

        //
        // Public methods

        /// <summary>
        /// Gets a non-local ip address that can be used for binding locally.
        /// </summary>
        /// <param name="useIPv4"></param>
        /// <returns></returns>
        public static IPAddress getNonLocalIPAddress(bool useIPv4 = true)
        {
            IPAddress address = null;
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            if (useIPv4 == true)
            {
                foreach (IPAddress addr in addresses)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (IPAddress.IsLoopback(addr) == false)
                        {
                            address = addr;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (IPAddress addr in addresses)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        if (IPAddress.IPv6Loopback != addr)
                        {
                            address = addr;
                            break;
                        }
                    }
                }
            }
            return address;
        }

        /// <summary>
        /// Creates and binds the tcp socket.  The BindEventArgs should be of type SocketBindingEventArgs.
        /// </summary>
        /// <param name="bindingInformation">The SocketBindingEventArgs object with the binding info.</param>
        public override void bind(BindEventArgs bindingInformation)
        {
            if (_isBound == false)
            {
                // get the actual binding info
                SocketBindingEventArgs bindInfo = bindingInformation as SocketBindingEventArgs;
                // create the socket
                _sock = new Socket(useIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                _isDisposed = false; // set dispose flag to false
                // bind the socket
                _sock.Bind(new IPEndPoint(bindInfo.localAddress, bindInfo.portNumber));
                _isBound = true; // set bound flag to true
            }
            else
            {
                throw new Exception("Cannot bind an already bound socket. @ReliableSocket.bind()");
            }
        } //

        /// <summary>
        /// Performs a blocking connect to the specified remote location.
        /// </summary>
        /// <param name="connectionInformation"></param>
        public void connect(ConnectionEventArgs connectionInformation)
        {
            if (_isBound == false)
            {
                throw new Exception("Call bind() before connect().");
            }

            if (_isConnected == true)
            {
                throw new Exception("The socket is already connected.");
            }

            // do a blocking connect
            _sock.Connect(new IPEndPoint(connectionInformation.remoteAddress, connectionInformation.portNumber));
            // fire out success event
            if (onConnectSuccess != null)
            {
                onConnectSuccess(new ConnectionEventArgs
                {
                    remoteAddress = connectionInformation.remoteAddress,
                    portNumber = connectionInformation.portNumber
                });
            }
            // update our flags
            _isConnected = true; // update our flag
            _isServer = false;
        }

        /// <summary>
        /// Places the socket in a listening state and begins accepting connections.
        /// </summary>
        /// <param name="backlog">The max length of the pending connection queue.</param>
        public void acceptConnections(int backlog)
        {
            if (_isBound == false)
            {
                throw new Exception("Call bind() before accepting connections");
            }
            lock (_lock)
            {
                _sock.Listen(backlog);
                _sock.BeginAccept(new AsyncCallback(acceptCallback), _sock);
                _isServer = true;
            }
        }

        /// <summary>
        /// Disconnects a currently connected socket, and allows re-use of it.
        /// </summary>
        /// <param name="reuse"></param>
        public void disconnect(bool reuse = false)
        {
            if (_isConnected)
            {
                _sock.Disconnect(reuse);
                _isConnected = false;
            }
            else
            {
                fireInformationEvent("disconnect() called when a socket wasn't connected.");
            }
        }

        /// <summary>
        /// WARNING: This is implemented as a result of the IOPort base class and is not meant to 
        /// be used in a finely tuned context - however, it can be used if needed.  
        /// This simply calls acceptConnections(backlog = 10).  Make sure to call bind() or the overloaded
        /// ctor before this.
        /// </summary>
        public override void receive()
        {
            acceptConnections(10);
        }

        /// <summary>
        /// Attempts to send data reliably.  If the data length is larger than the 
        /// sendPacketSize, then it will be broken up automatically of packets
        /// in this size.  This is a blocking send so it will pause for the 
        /// duration it takes to send this data.
        /// </summary>
        /// <param name="sendInformation"></param>
        /// <returns></returns>
        public override int send(TxEventArgs sendInformation)
        {
            if (_isBound == false)
            {
                throw new Exception("Call bind(), then connect() before sending.");
            }
            TxSocketEventArgs se = sendInformation as TxSocketEventArgs;

            if (se.sendLength == 0)
            {
                se.sendLength = se.data.Length;
            }

            int dataSent = 0;
            int dataLength = se.sendLength;
            int dataSendLength = sendPacketSize;

            // mutex lock the sending
            lock (_lock)
            {
                // it's a sequence that is larger than our specified data send sequence length
                // so break it up and send in packets
                if (sendPacketSize < se.sendLength)
                {
                    // send in packets
                    while (dataSent < dataLength)
                    {
                        // send our packet
                        _sock.Send(se.data, dataSent, dataSendLength, SocketFlags.None);
                        // add to our data already sent
                        dataSent += dataSendLength;
                        // check if we're towards the end for our next send
                        if ((dataSent + dataSendLength) > dataLength)
                        {
                            dataSendLength = dataLength - dataSent;
                        }
                    }
                    return dataSent;
                }
                else
                {
                    // the data to send is less than our _dataSequenceLength maximum, so just send
                    return _sock.Send(se.data, se.offset, se.sendLength, SocketFlags.None);
                }
            }
        }

        /// <summary>
        /// Closes the socket only if it's bound, then disposes it.  Can re-use with a call to bind.
        /// </summary>
        public override void close()
        {
            if (_isBound == true)
            {
                _sock.Close();
                _isBound = false;
                _sock.Dispose();
                _isDisposed = true;
                _sock = null;
            }
        }

        //
        // Public properties

        /// <summary>
        /// Gets or sets the length of the packet to send.
        /// If a call to send() specifies data that is larger
        /// than this value, then the data will be broken up 
        /// into packets of this length.
        /// </summary>
        public int sendPacketSize
        {
            get { return _dataSequenceLength; }
            set
            {
                if (value > 0)
                {
                    _dataSequenceLength = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets if we should use IPv6.  This should be set before
        /// a call to bind() is made.  The default will be IPv4.
        /// </summary>
        public bool useIPv6
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value determining if the socket called connect() and is currently connected.
        /// </summary>
        public bool isConnected
        {
            get
            {
                return _isConnected;
            }
        }

        /// <summary>
        /// Gets a value that determines if the socket is running as a server (accepting connections
        /// and receiving data from clients) - as opposed to a client (connector).
        /// </summary>
        public bool isServer
        {
            get
            {
                return _isServer;
            }
        }

        //
        // Private methods

        private void acceptCallback(IAsyncResult result)
        {
            lock (_lock)
            {
                if (_isDisposed == true)
                {
                    return;
                }
                Socket sock = result.AsyncState as Socket;

                // end accept, and extract the socket that we will use to communicate with the client
                Socket csock = sock.EndAccept(result);

                // fire the event if subscribe to
                if (onClientConnection != null)
                {
                    onClientConnection(new ConnectionEventArgs
                    {
                        remoteAddress = (csock.RemoteEndPoint as IPEndPoint).Address,
                        portNumber = (csock.RemoteEndPoint as IPEndPoint).Port
                    });
                }

                // start accepting again (depends on our back log queue)
                sock.BeginAccept(new AsyncCallback(acceptCallback), sock);

                // place new socket in receiving state
                csock.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(receiveCallback), csock);
            }
        }

        private void receiveCallback(IAsyncResult result)
        {
            Socket csock = result.AsyncState as Socket;
            int rx = -1;
            byte[] _data = null;
            lock (_lock)
            {
                if (_isDisposed == true)
                {	//
                    // because this is async, EndReceive and EndAccept will get called
                    // if we called BeginReceive / BeginAccept and we close it()
                    return;
                }
                rx = csock.EndReceive(result);
                if (rx != 0)
                {
                    _data = new byte[rx];
                    Array.Copy(_buf, _data, rx);
                    if (onRxEvent != null)
                    {
                        onRxEvent(new RxSocketEventArgs { data = _data, dataLength = rx });
                    }
                }
                else
                {
                    // likely a disconnect
                    disconnect(true);
                    _isConnected = false;
                }
            } // lock
            csock.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, new AsyncCallback(receiveCallback), csock);
        }

        private void fireInformationEvent(string info)
        {
            if (onInformationEvent != null)
            {
                onInformationEvent(info);
            }
        }

        public delegate void InformationEvent(string info);
        public delegate void ConnectionEvent(ConnectionEventArgs e);

        /// <summary>
        /// Subscribe to this to get an event that fires when data is received over the socket.
        /// </summary>
        public event RxEvent onRxEvent = null;
        /// <summary>
        /// Subscribe to this to get an event that fires when data is sent successfully by this socket.
        /// </summary>
        public event TxEvent onTxEvent = null;
        /// <summary>
        /// Subscribe to this to get warnings and information about occurences in the socket.
        /// </summary>
        public event InformationEvent onInformationEvent = null;
        /// <summary>
        /// Subscribe to this when a connection attempt was made by this socket and succeeded.
        /// </summary>
        public event ConnectionEvent onConnectSuccess = null;
        /// <summary>
        /// Subscribe to this to know when a client connected to this socket.
        /// </summary>
        public event ConnectionEvent onClientConnection = null;

        private bool _isDisposed = true,
            _isServer = false,
            _isBound = false,
            _isConnected = false;

        private object _lock = new object();
        private byte[] _buf = new byte[_bufferLength];
        private Socket _sock = null;
        private int _dataSequenceLength = 550;
        private static readonly int _bufferLength = 4500;
    }
}


/*
 THis shows an example of how to use this:
 this creates two sockets, has them subscribe to the appropriate events.  It then opens a file, converts it to bytes
 * then sends the entire file over a port, where the other one will receive it, buffer it, and then save the file again.
 * Works good as a file transfer protocol (FTP) implementation.
 
 static void test()
		{
			SocketBindingEventArgs e = new SocketBindingEventArgs { portNumber = 44444, localAddress = IPAddress.Parse("127.0.0.1") };
			SocketBindingEventArgs e2 = new SocketBindingEventArgs{ portNumber = 44445, localAddress = IPAddress.Parse("127.0.0.1") };

			ReliableSocket sock = new ReliableSocket(e);
			ReliableSocket sock2 = new ReliableSocket(e2);

			sock2.onConnectSuccess += new ReliableSocket.ConnectionEvent(sock2_onConnectSuccess);
			sock.onRxEvent += new ReliableSocket.RxEvent(sock_onRxEvent);
			sock.onClientConnection += new ReliableSocket.ConnectionEvent(sock_onClientConnection);

			Console.WriteLine("sock1 accepting...");
			sock.acceptConnections(5);

			Console.WriteLine("pausing...");
			System.Threading.Thread.Sleep(3000);

			Console.WriteLine("sock2 connecting...");
			sock2.connect(new ConnectionEventArgs { portNumber = 44444, remoteAddress = IPAddress.Parse("127.0.0.1") });

			byte[] data = FileManipulator.fileToRaw(FILE_SEND);
			Console.WriteLine("sock2 sending...");
			sock2.send(new TxSocketEventArgs { data = data, offset = 0, sendLength = data.Length });

			Console.WriteLine("Press any key to finish...");
			Console.ReadKey();

			sock.close();
			sock2.close();
		}

		static void sock_onClientConnection(ConnectionEventArgs e)
		{
			Console.WriteLine("sock1 >> Client connected from: " + e.remoteAddress.ToString() + ":" + e.portNumber.ToString());
		}

		static void sock2_onConnectSuccess(ConnectionEventArgs e)
		{
			Console.WriteLine("sock2 >> Connected successfully: " + e.remoteAddress.ToString() + ":" + e.portNumber.ToString());
		}
		
		static void sock_onRxEvent(RxEventArgs e)
		{
			RxSocketEventArgs se = e as RxSocketEventArgs;

			receivedJpegBuffer.AddRange(se.data);
			int lastIndex = -1;

			Console.WriteLine("sock1 >> received " + se.dataLength.ToString() + " bytes");

			if (isJpegEnd(se.data, out lastIndex))
			{
				Console.WriteLine("EOF!!");
				FileManipulator.saveFromRaw(receivedJpegBuffer.ToArray(), FILE_RX, false);
				receivedJpegBuffer.Clear();
			}
*/