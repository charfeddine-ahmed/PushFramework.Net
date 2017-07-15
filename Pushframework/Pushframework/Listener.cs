using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PushFramework
{
    internal class Listener
    {
        public Listener(Server server, EndPoint endPoint)
        {
            this.EndPoint = endPoint;
            this.Server = server;
        }

        public Server Server
        {
            get;
            internal set;
        }

        private Socket socket;

        private Task thread;

        public EndPoint EndPoint
        {
            get;
            internal set;
        }
 
        public void Start()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket .Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), this.EndPoint.Port));
            this.socket .Listen(100);

            this.thread = Task.Factory.StartNew(() => { this.listenProc(); }, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            this.socket.Dispose();
            this.thread.Wait();
        }

        private void listenProc()
        {
            try
            {
                while (true)
                {
                    Socket clientSocket = this.socket.Accept();
                    this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsHitsIn);
                    this.ProcessConnection(clientSocket);
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                return;
            }
        }

        private void ProcessConnection(Socket clientSocket)
        {
            // Check remote address.
            // Check maximum number of clients.
            Connection logicalConnection = this.Server.CreateConnection();        
            
            PhysicalConnection physicalConnection = new PhysicalConnection();
            
            logicalConnection.PhysicalConnection = physicalConnection;
            physicalConnection.LogicalConnection = logicalConnection;

            this.Server.Connections.TryAdd(physicalConnection.InternalId, logicalConnection);

            physicalConnection.Initialize(clientSocket, this.EndPoint, this.Server);
            this.Server.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.VisitorsOnline);

            IPEndPoint remoteIpEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;




            this.Server.MeasurementMgr.AddIP(/*remoteIpEndPoint.Address.ToString()*/"196.203.102.37");
        }
    }
}