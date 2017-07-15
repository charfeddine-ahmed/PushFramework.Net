using System.Collections.Generic;
using System.Net.Sockets;
using ProtocolFramework;
using System.Threading;
using System.Threading.Tasks;

namespace PushFramework
{
    internal class PhysicalConnection
    {
        internal enum CloseReason : int
        {
            Internal = 1,
            Requested,
            Inactive,
            UnAuthorized,
        }

        private const int SocketBufferSize = 8192;
        public PhysicalConnection()
        {
            this.readIoBuffer = new Buffer(SocketBufferSize);
            this.outgoingBytes = new List<Buffer>();
            System.Console.WriteLine("object created");

            Interlocked.Increment(ref count);
        }

        public void Initialize(Socket socket, EndPoint endPoint, Server server)
        {
            this.socket = socket;

            this.CreationTime = System.DateTime.Now;
            this.EndPoint = endPoint;
            this.Server = server;
            this.InternalId = this.Server.AllocateUniqueConnectionId();

            this.ReceivArg = new SocketAsyncEventArgs();
            this.ReceivArg.AcceptSocket = socket;
            this.ReceivArg.Completed += (sender, e) => CompleteReceiveIO(); 
            this.ReceivArg.SetBuffer(this.readIoBuffer.Data, 0, SocketBufferSize);

            this.SendArg = new SocketAsyncEventArgs();
            this.SendArg.AcceptSocket = socket;
            this.SendArg.Completed += (sender, e) => CompleteSendIO();

            this.InitializeProtocolStackLayers();
            this.InitiateReceiveIO(true);
           
        }

        ~PhysicalConnection()
        {
            
            Interlocked.Decrement(ref count);
            System.Console.WriteLine("destroying object. remaining: " + count);
        }

        internal Connection LogicalConnection
        {
            get;
            set;
        }


        #region Data Members

        private Socket socket;

        private Buffer readIoBuffer;

        private List<Buffer> outgoingBytes;

        private object antiParallelWriteLock = new object();

        private object recvArgsLock = new object();

        private System.DateTime lastReceivedDataTime;

        public static int count = 0;


        #endregion


        bool isShutdown;

        public void Close(CloseReason closeReason)
        {
            isShutdown = true;

            try
            {
                this.socket.Shutdown(SocketShutdown.Both);
            }
            catch (System.Exception)
            {
                //
            }

            lock (this.antiParallelWriteLock)
            {
                this.SendArg.Dispose();
            }

            lock (this.recvArgsLock)
            {
                this.ReceivArg.Dispose();
            }

            this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsHitsOut);
            
            if (this.IsAuthenticated)
            {
                this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsOnline, -1);

                //Calculate stay duration:
                System.TimeSpan stayDuration = System.DateTime.Now - this.CreationTime;
                this.Server.MeasurementMgr.IncrementAveragedValue(Analytics.MeasurementMgr.Metrics.VisitorsDuration, stayDuration.TotalSeconds);

                this.LogicalConnection.OnDisconnected();

                if (closeReason == CloseReason.Inactive)
                {
                    this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsInactive);
                }

                if (stayDuration.TotalSeconds < this.Server.ServerOptions.BounceDuration)
                {
                    this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsBounce);
                }
            }
            else
            {
                if (closeReason == CloseReason.UnAuthorized)
                {
                    this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsUnAuthorized);
                }
            }

            //TODO Check if it guarantees that element is removed.
            Connection temp;
            this.Server.Connections.TryRemove(this.InternalId, out temp);

            System.Console.WriteLine("Terminated.");
        }
        

        public SocketAsyncEventArgs ReceivArg
        {
            get;
            internal set;
        }

        public SocketAsyncEventArgs SendArg
        {
            get;
            internal set;
        }

        internal bool IsAuthenticated
        {
            get;
            set;
        }

        internal int InternalId
        {
            get;
            private set;
        }

        public System.DateTime CreationTime
        {
            get;
            internal set;
        }

        private bool isWriteInProgress = false;

        public void MarkAsAuthenticated()
        {
            this.IsAuthenticated = true;
            this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsOnline, 1);
        }

        internal bool IsInactive()
        {
            if (this.Server.ServerOptions.MaximumInactivityDuration > 0)
            {
                System.TimeSpan duration = System.DateTime.Now - this.lastReceivedDataTime;
                return (duration.TotalSeconds > this.Server.ServerOptions.MaximumInactivityDuration);
            }

            return false;
        }
        public void InitiateReceiveIO(bool forceAsync = false)
        {
            bool isPending;
            lock (this.recvArgsLock)
            {
                if (this.isShutdown)
                {
                    return;
                }

                isPending = this.socket.ReceiveAsync(this.ReceivArg);
            }

            if (!isPending)
            {
                if (forceAsync)
                {
                    new Task(() => { CompleteReceiveIO(); }).Start();
                }
                else
                {
                    CompleteReceiveIO();
                }
            }
        }

        private void CompleteReceiveIO()
        {
            bool performClose = false;

            lock(this.recvArgsLock)
            {
                if (this.isShutdown)
                {
                    return;
                }

                if (this.ReceivArg.SocketError != SocketError.Success || this.ReceivArg.BytesTransferred == 0)
                {
                    System.Console.WriteLine("CompleteReceiveIO Error" + this.ReceivArg.SocketError);
                    performClose = true;
                }
                else
                {
                    this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.BandwidthInbound, this.ReceivArg.BytesTransferred);
                    this.Server.MeasurementMgr.IncrementKeyedAveragedValue(Analytics.MeasurementMgr.Metrics.BandwidthInboundPerConnection, this.InternalId, this.ReceivArg.BytesTransferred);

                    this.readIoBuffer.Size = this.ReceivArg.BytesTransferred; 
                }                
            } 

            // We need to get out of the lock to do Close.
            if (performClose)
            {
                this.Close(CloseReason.Internal);
                return;
            }

            this.lastReceivedDataTime = System.DateTime.Now;

            System.Console.WriteLine("Received bytes: " + this.readIoBuffer.Size);

            // decode and processs incoming messages:
            bool result = this.DecodeAndProcessLoop();

            if (result)
            {
                // Read more data.
                this.InitiateReceiveIO(false);
                return;
            }       
            else
            {
                this.Close(CloseReason.Internal);
            }
        }

        private bool DecodeAndProcessLoop()
        {
            if (!this.LowestProtocolContext.Protocol.ReadBytes(this.readIoBuffer, this.LowestProtocolContext.ConnectionToken))
            {
                return false;
            }

            DecodeState decodeState;
            decodeState.protocol = this.LowestProtocolContext;

            while (true)
            {
                Buffer decodedBytes;
                decodeState = decodeState.protocol.TryDecode(out decodedBytes);

                if (decodeState.decodeResult == DecodeResult.Failure)
                {
                    return false;
                }
                else if (decodeState.decodeResult == DecodeResult.WantMoreData)
                {
                    return true;
                }
                else
                {
                    if (decodedBytes != null)
                    {
                        this.Server.Handle(decodedBytes, this.LogicalConnection);
                    }
                }
            }
        }

        private void CompleteSendIO()
        {
           // System.Console.WriteLine("CompleteSendIO.");

            bool performClose = false;

            lock(this.antiParallelWriteLock)
            {
                if (this.isShutdown)
                {
                    return;
                }

                if (this.SendArg.SocketError != SocketError.Success ||
                    this.SendArg.BytesTransferred == 0)
                {
                    performClose = true;
                }
                else
                {
                    this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.BandwidthOutbound, this.SendArg.BytesTransferred);
                    this.Server.MeasurementMgr.IncrementKeyedAveragedValue(Analytics.MeasurementMgr.Metrics.BandwidthOutboundPerConnection, this.InternalId, this.SendArg.BytesTransferred);

                    isWriteInProgress = false;
                    Buffer buffer = this.outgoingBytes[0];

                    buffer.Pop(this.SendArg.BytesTransferred);

                    if (buffer.Empty())
                    {
                        this.outgoingBytes.RemoveAt(0);
                    }

                    if (this.outgoingBytes.Count == 0)
                    {
                        this.CheckAndProcessPendingBroadcast(false);
                    }
                    else
                    {
                        this.InitiateSendIO();
                    } 
                }                               
            }

            if (performClose)
            {
                this.Close(CloseReason.Internal);
            }
        } 

        public void InitiateSendIO()
        {
            if (this.outgoingBytes.Count == 0)
                return;

            Buffer buffer = this.outgoingBytes[0];

            this.isWriteInProgress = true;
            this.SendArg.SetBuffer(buffer.Data, buffer.Offset, buffer.Size);
            if (!this.socket.SendAsync(this.SendArg))
            {
                this.CompleteSendIO();
            }            
        }

        private void SendBuffer(Buffer bytes)
        {
            if (bytes.Empty())
                return;

            this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.BandwidthOutstanding, bytes.Size);

            lock(this.antiParallelWriteLock)
            {
                if (this.isShutdown)
                {
                    return;
                }

                this.outgoingBytes.Add(bytes);

                if (!this.isWriteInProgress)
                {
                    this.InitiateSendIO();
                }
            }
        }

        public void SendMessage(object message)
        {
            Buffer bytes;
            this.Server.Serializer.Serialize(message, out bytes);

            this.SendSerializedMessage(bytes);
        }

        public void SendMessage(object message, int serviceRoutingId, int methodId)
        {
            Buffer bytes;
            this.Server.Serializer.Serialize(message, out bytes);

            this.SendSerializedMessage(bytes);

            this.Server.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.BandwidthOutboundVolPerService, serviceRoutingId, bytes.Size);
            this.Server.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.BandwidthOutboundVolPerMethod, methodId, bytes.Size);
        }

        public void SendSerializedMessage(Buffer bytes)
        {
            Buffer encodedBytes;
            var result = this.HighestProtocolContext.Encode(bytes, out encodedBytes);

            if (result != EncodeResult.Success)
            {
                return;
            }

            //
            this.SendBuffer(encodedBytes);
        }

        public void SendProtocolBytes(Buffer bytes, ProtocolContext protocol)
        {
            if (protocol.LowerProtocol == null)
            {
                this.SendBuffer(bytes);
            }
            else
            {
                Buffer encodedBytes;
                var result = protocol.Encode(bytes, out encodedBytes);

                if (result !=EncodeResult.Success)
                {
                    return;
                }

                this.SendBuffer(encodedBytes);
            } 
        }

        #region Protocols/Serializer/Contexts

        private ProtocolContext HighestProtocolContext;

        private ProtocolContext LowestProtocolContext;

        public EndPoint EndPoint
        {
            get;
            internal set;
        }

        public Server Server
        {
            get;
            internal set;
        }

        private void InitializeProtocolStackLayers()
        {
            // Create the contexts:
            ProtocolContext lastContext = null;
            for(int i = 0; i< this.EndPoint.ProtocolList.Count; i++)
            {
                Protocol protocol = this.EndPoint.ProtocolList[i];
                ProtocolContext context = new ProtocolContext(protocol);
                context.LowerProtocol = lastContext;
                context.PhysicalConnection = this;

                if (lastContext == null)
                {
                    this.LowestProtocolContext = context;
                }
                else
                {
                    lastContext.UpperProtocol = context;
                }

                lastContext = context;
            }
            this.HighestProtocolContext = lastContext;

            this.LowestProtocolContext.AdvanceNegociation();          
        }
        #endregion

        #region Pub/Sub 

        private QueueGroupContext RootContext;

        private ReaderWriterLockSlim subscriptionDataLock = new ReaderWriterLockSlim();

        private bool CanPushBroadcast
        {
            get
            {
                return this.outgoingBytes.Count < 5;
            }
        }

        private bool skipCheckNextMessage;
        private int broadcastRunFlag;


        public void CheckAndProcessPendingBroadcast(bool somethingHappened)
        {
            if (somethingHappened)
            {
                this.skipCheckNextMessage = false;
            }

            if (Interlocked.CompareExchange(ref broadcastRunFlag, 1, 0) == 1)
            {
                return;
            }

            if (this.skipCheckNextMessage)
            {
                return;
            }

            if (this.isWriteInProgress)
            {
                return;
            }

            this.subscriptionDataLock.EnterReadLock();

            QueueGroupContext context = this.RootContext;
            while (context != null)
            {
                if (!this.CanPushBroadcast)
                {
                    break;
                }

                Queue originQueue;
                object message = context.GetNextMessage(out originQueue);

                if (message != null)
                {
                    Buffer bytes;
                    this.Server.Serializer.Serialize(message, out bytes);
                    this.SendSerializedMessage(bytes);

                    this.Server.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.BroadcastSentMessageVolPerQueue, originQueue.Id, 1);
                    this.Server.MeasurementMgr.IncrementAvgDistributionValue(Analytics.MeasurementMgr.Metrics.BroadcastAvgMessageSizePerQueue, originQueue.Id, bytes.Size);

                    this.Server.MeasurementMgr.IncrementKeyedAveragedValue(Analytics.MeasurementMgr.Metrics.BroadcastSentMessagePerConnection, this.InternalId, 1);
                }
                else
                {
                    context = context.Next;
                }
            }

            broadcastRunFlag = 0;
            this.subscriptionDataLock.ExitReadLock();
        }
        
        public void SubscribeToQueue(string queueName)
        {
            Queue queue;
            if (!this.Server.Queues.TryGetValue(queueName, out queue))
            {
                return;
            }
            
            this.subscriptionDataLock.EnterWriteLock();

            if (this.RootContext == null || !this.RootContext.AddQueue(queue))
            {
                QueueGroupContext group = new QueueGroupContext(queue);
                group.Next = this.RootContext;
                this.RootContext = group;
            }

            this.subscriptionDataLock.ExitWriteLock();

            queue.AddSubscriber(this.LogicalConnection);//TODO. Check locks.

            this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.BroadcastSubscriptionCount);
            this.Server.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.BroadcastSubscriptionCountPerQueue, queue.Id);
        }

        public void UnsubscribeFromAll()
        {
            this.subscriptionDataLock.EnterWriteLock();

            this.RootContext = null;

            this.subscriptionDataLock.ExitWriteLock();
        }

        public void UnsubscribeFromQueue(string queueName)
        {
            this.subscriptionDataLock.EnterWriteLock();

            if (this.RootContext != null)
            {
                this.RootContext.RemoveQueue(queueName);
                QueueGroupContext nextGroup = this.RootContext;

                if (nextGroup.Head == null)
                {
                    this.RootContext = nextGroup.Next;
                    nextGroup.Next = null;
                }
            }
            
            this.subscriptionDataLock.ExitWriteLock();
        }

        #endregion
    }
}
