using System;
using ProtocolFramework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace PushFramework
{
    public abstract class Server
    {
        public Server()
        {
            this.Listeners = new List<Listener>();
            this.Services = new Dictionary<int, Service>();
            this.Queues = new Dictionary<string, Queue>();
            this.MeasurementMgr = new Analytics.MeasurementMgr();
            this.ServerOptions = new ServerOptions();
            this.ScheduledTasks = new HashSet<ScheduledTask>();
            this.Connections = new ConcurrentDictionary<int, Connection>();

            this.AddScheduledTask(new InactiveConnectionsScrutinizer());
            this.AddScheduledTask(new IllegitemateConnectionScrutinizer());
        }

        internal HashSet<ScheduledTask> ScheduledTasks;

        private int _connectionIdGen;

        internal int AllocateUniqueConnectionId()
        {
            int ret;
            lock(this.Connections)
            {
                _connectionIdGen++;
                if (_connectionIdGen == int.MaxValue)
                {
                    _connectionIdGen = 1;
                }

                ret = _connectionIdGen;
            }

            return ret;
        }

        internal ConcurrentDictionary<int, Connection> Connections
        {
            get;
            set;
        }

        public void AddEndPoint(EndPoint endPoint)
        {
            this.Listeners.Add(new Listener(this, endPoint));
        }

        public void AddScheduledTask(ScheduledTask task)
        {
            task.Server = this;
            this.ScheduledTasks.Add(task);
        }

        public Serializer Serializer
        {
            get;
            set;
        }

        public ServerOptions ServerOptions
        {
            get;
            set;
        }

        internal Dictionary<string, Queue> Queues
        {
            get;
            set;
        }

        internal List<Listener> Listeners
        {
            get;
            set;
        }

        internal Analytics.MeasurementMgr MeasurementMgr
        {
            get;
            set;
        }

        public void EnableMonitoring(int Port, string Password, int SamplingRate)
        {
            this.AnalyticsServer = new Analytics.Server(this);
            this.AnalyticsServer.MonitoringPort = Port;
            this.AnalyticsServer.MonitoringPassword = Password;
            this.AnalyticsServer.SamplingRate = SamplingRate;

            this.AddScheduledTask(new ProfilingTask(SamplingRate));
        }

        internal Analytics.Server AnalyticsServer
        {
            get;
            set;
        }
        
        internal Dictionary<int, Service> Services
        {
            get;
            set;
        }

        public void RegisterService(Service service)
        {
            this.Services[service.RoutingId] = service;
        }

        public void Handle(ProtocolFramework.Buffer data, Connection user)
        {
            //TODO.
            if (!user.IsAuthenticated)
            {
                bool isIntercepted = user.HandleMessage(data);

                if (isIntercepted)
                {
                    return;
                }
            }            

            object message;
            int serviceRoutingId;
            int methodRoutingId;            
            this.Serializer.Deserialize(data, out serviceRoutingId, out methodRoutingId, out message);

            Service service;
            if (!this.Services.TryGetValue(serviceRoutingId, out service))
            {
                return;
            }

            int methodGuid = service.GetMethodGuid(methodRoutingId);

            this.MeasurementMgr.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.PerformanceRequestVol);
            this.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.BandwidthInboundVolPerService, serviceRoutingId, data.Size);
            this.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.PerformanceRequestVolPerService, serviceRoutingId, data.Size);
            

            if (methodRoutingId != -1)
            {
                this.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.BandwidthInboundVolPerMethod, methodGuid, data.Size);
                this.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.PerformanceRequestVolPerMethod, methodGuid, data.Size);
            }


            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (service.IsShared)
            {
                service.Dispatch(methodRoutingId, message, user);
            }
            else
            {
                Service instanceService;
                if (!user.Services.TryGetValue(serviceRoutingId, out instanceService))
                {
                    instanceService = service.GetCopy();
                    instanceService.Connection = user;
                    user.Services.Add(serviceRoutingId, instanceService);
                }

                //
                instanceService.Dispatch(methodRoutingId, message, user);
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;


            this.MeasurementMgr.IncrementAveragedValue(Analytics.MeasurementMgr.Metrics.PerformanceProcessingTime, elapsedMs);
            this.MeasurementMgr.IncrementAvgDistributionValue(Analytics.MeasurementMgr.Metrics.PerformanceProcessingTimePerService, serviceRoutingId, elapsedMs);

            if (methodRoutingId != -1)
            {
                this.MeasurementMgr.IncrementAvgDistributionValue(Analytics.MeasurementMgr.Metrics.PerformanceProcessingTimePerMethod, methodGuid, elapsedMs);
            }

        }


        public void Start()
        {
            foreach (var listener in this.Listeners)
            {
                listener.Start();
            }

            if (this.AnalyticsServer != null)
            {
                this.AnalyticsServer.Start();
            }

            foreach (var t in this.ScheduledTasks)
            {
                t.Start();
            }
        }

        public void Stop()
        {
            foreach (var t in this.ScheduledTasks)
            {
                t.Stop();
            }

            foreach (var listener in this.Listeners)
            {
                listener.Stop();
            }

            if (this.AnalyticsServer != null)
            {
                this.AnalyticsServer.Stop();
            }
        }

        public void CreateQueue(int id, string name, QueueOptions options)
        {
            this.Queues.Add(name, new Queue(id, name, options));
        }

        public void RemoveQueue(string name)
        {
            //
        }

        public void PushToQueue(string name, object message)
        {
            Queue queue;
            if (!this.Queues.TryGetValue(name, out queue))
            {
                return;
            }

            this.MeasurementMgr.IncrementDistributionValue(Analytics.MeasurementMgr.Metrics.BroadcastPushedMessageVolPerQueue, queue.Id, 1);
            queue.PushMessage(message);
        }

        public abstract  Connection CreateConnection();

        public virtual string Name
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string Description
        {
            get
            {
                return string.Empty;
            }
        }
    }
}