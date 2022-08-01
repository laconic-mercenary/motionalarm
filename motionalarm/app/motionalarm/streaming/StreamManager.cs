
namespace app.motionalarm.streaming
{
    using library;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using System.Threading;

    sealed public class StreamManager
    {

        public StreamManager(int localPort, IPAddress localAddress = null, int maxQueueSize = 10)
        {
            if (localAddress == null)
            {
                localAddress = ReliableSocket.getNonLocalIPAddress();
            }
            _localAddr = localAddress;
            // bind here
            _connector = new ReliableSocket(new SocketBindingEventArgs
            {
                localAddress = localAddress,
                portNumber = localPort
            });
            _connector.onRxEvent += new IOPort.RxEvent(handleConnection);
            _connector.acceptConnections(15);
            // instantiate the rest
            _streams = new List<ReliableSocket>();
            _sendQueue = new Queue<byte[]>(maxQueueSize);
            //
            _thread = new Thread(new ThreadStart(handleSendLoop));
            _threadFlag = true;
            _thread.Start();
            _isAccepting = false;
            _isSendingPaused = true;
            _autoResetEvent = new AutoResetEvent(false);
            _autoResetEventBlocking = false;
        }

        ~StreamManager()
        {
            close();
        }

        /// <summary>
        /// Enqueues some data to send.
        /// </summary>
        /// <param name="data"></param>
        public void queueData(byte[] data)
        {
            if (_sendQueue != null)
            {
                lock (_lock)
                {
                    _sendQueue.Enqueue(data);
                }
                if (_autoResetEventBlocking == true)
                {
                    // tells the thread to release it's block
                    _autoResetEventBlocking = false;
                    _autoResetEvent.Set();
                }
            }            
        }

        /// <summary>
        /// Starts accepting connections
        /// </summary>
        public void startAccepting()
        {
            lock (_lock)
            {
                _isAccepting = true;
            }
        }

        /// <summary>
        /// Pauses from accepting connections.
        /// </summary>
        public void stopAccepting()
        {
            lock (_lock)
            {
                _isAccepting = false;
            }
        }

        /// <summary>
        /// Sets the password if any.
        /// </summary>
        /// <param name="password"></param>
        public void setPassword(string password)
        {
            if (password != null && password.Length != 0)
            {
                _password = password;
            }
        }

        /// <summary>
        /// Gets rid of all items waiting in the queue.
        /// </summary>
        public void flush()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
            }
        }

        /// <summary>
        /// Closes up the stream manager.
        /// </summary>
        public void close()
        {
            if (_connector != null)
            {
                _connector.close();
            }            
            if (_autoResetEvent != null)
            {
                // release the resources used by it
                if (_autoResetEventBlocking == true)
                {
                    _autoResetEvent.Set();
                    //_thread.Abort();
                }
                _threadFlag = false; // this releases the thread
            }
            if (_sendQueue != null)
            {
                _sendQueue.Clear();
            }
            if (_streams != null)
            {
                foreach (ReliableSocket socket in _streams)
                {
                    socket.close();
                }
                _streams.Clear();
                _streams = null;
            }
            if (onStreamCountChanged != null)
            {
                onStreamCountChanged();
            }
        }

        /// <summary>
        /// Gets or sets if we want to delay sending images to everyone.
        /// </summary>
        public bool sendingPaused
        {
            get { return _isSendingPaused; }
            set
            {
                lock (_lock)
                {
                    _isSendingPaused = value;
                }
            }
        }

        /// <summary>
        /// Gets the number of clients currently connected to the manager.
        /// </summary>
        public int streamCount
        {
            get 
            {
                if (this._streams == null)
                {
                    return 0;
                }
                else
                {
                    return this._streams.Count;
                }
            }
        }

        // Private

        private bool validateConnector(string connectionString, out IPEndPoint clientDataEndPoint)
        {
            clientDataEndPoint = null;
            if (connectionString != null && connectionString.Length != 0)
            {
                string[] splitConnectionString = connectionString.Split(new string[] { _delim }, System.StringSplitOptions.RemoveEmptyEntries);
                if (splitConnectionString.Length >= 3)
                {
                    if (_password != null)
                    {
                        if (string.Compare(_password, splitConnectionString[0]) != 0)
                        {
                            logging.Logger.log("invalid password specified by: " + clientDataEndPoint.ToString());
                            return false;
                        }
                    }
                    IPAddress address = null;
                    int port = 0;
                    if (IPAddress.TryParse(splitConnectionString[1], out address))
                    {
                        if (int.TryParse(splitConnectionString[2], out port))
                        {
                            clientDataEndPoint = new IPEndPoint(address, port);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void handleConnection(RxEventArgs e)
        {
            RxSocketEventArgs socke = (RxSocketEventArgs)e;
            string connectionString = System.Text.Encoding.ASCII.GetString(socke.data);
            IPEndPoint endPoint = null;
            if (validateConnector(connectionString, out endPoint))
            {
                ReliableSocket newSocket = createStream(endPoint);
                if (newSocket != null)
                {
                    lock (_lock)
                    {
                        _streams.Add(newSocket);
                    }
                    if (onStreamCountChanged != null)
                    {
                        onStreamCountChanged();
                    }
                }
            }
        }

        private ReliableSocket createStream(IPEndPoint endPoint)
        {
            ReliableSocket socket = new ReliableSocket(
                new SocketBindingEventArgs
                {
                    portNumber = _streamPortCounter,
                    localAddress = _localAddr
                });
            try
            {
                socket.connect(new ConnectionEventArgs
                {
                    remoteAddress = endPoint.Address,
                    portNumber = endPoint.Port
                });
            }
            catch (SocketException)
            {
                // just close it and send back null
                socket.close();
                socket = null;
            }
            return socket;
        }

        private void handleSendLoop()
        {
            TxSocketEventArgs tx = new TxSocketEventArgs();
            while (_threadFlag == true)
            {
                if (_sendQueue != null && _sendQueue.Count != 0)
                {
                    if (_isSendingPaused == true)
                    {
                        continue;
                    }
                    // send it
                    byte[] data = _sendQueue.Dequeue();
                    lock (_lock)
                    {
                        tx.data = data;
                        tx.offset = 0;
                        tx.sendLength = data.Length;
                        int len = _streams.Count;
                        for (int i = 0; i != len; i++)
                        {
                            try
                            {
                                _streams[i].send(tx);
                            }
                            catch
                            {
                                if (_onDisconnect != null)
                                {
                                    _onDisconnect();
                                }
                                _streams[i].close();
                                _streams.Remove(_streams[i]);
                                if (_streams.Count == 0)
                                {   // if we ran out of streams, then exit the thread
                                    _threadFlag = false;
                                }
                                if (onStreamCountChanged != null)
                                {
                                    onStreamCountChanged();
                                }
                            }
                        } // send to all clients
                    } // mutex lock
                } // if anything to send
                else
                {
                    // this will block our thread until we have 
                    // more data to send, signal() is called in enqueue
                    _autoResetEventBlocking = true;
                    _autoResetEvent.WaitOne();
                }
            } // loop
        }

        /// <summary>
        /// Subscribe to this to know when the stream count has changed.
        /// </summary>
        public event System.Action onStreamCountChanged = null;
        
        private IPAddress _localAddr = null;
        private AutoResetEvent _autoResetEvent = null;
        private Queue<byte[]> _sendQueue = null;
        private event System.Action _onDisconnect = null;
        private List<ReliableSocket> _streams = null;
        private ReliableSocket _connector = null;
        private Thread _thread = null;
        private bool _threadFlag = false;
        private bool _isAccepting = false;
        private bool _isSendingPaused = false;
        private object _lock = new object();
        private int _streamPortCounter = _streamBasePort;
        private string _password = null;
        private static readonly int _streamBasePort = 34433;
        private static readonly string _delim = "[|]";
        private bool _autoResetEventBlocking = false;
    }
}
