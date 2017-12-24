using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PushFramework.Analytics
{
    public class Server : PushFramework.Server
    {
        public Server(PushFramework.Server mainServer)
        {
            this.MainServer = mainServer;

            this.Serializer = new Contracts.JsonSerializer();

            this.RegisterService(new MonitorService(this));
        }

        public PushFramework.Server MainServer
        {
            get;
            internal set;
        }


        public override Connection CreateConnection()
        {
            return new AnalyticsClient(this);
        }

        public int MonitoringPort
        {
            get;
            set;
        }

        public string MonitoringPassword
        {
            get;
            set;
        }

        public int SamplingRate
        {
            get;
            set;
        }

        internal void CollectAndBroadcastMeasures()
        {
            var sample = this.MainServer.MeasurementMgr.Collect();

            Contracts.JsonResponse response = new Contracts.JsonResponse();
            response.IsSynchronous = false;
            response.Data = sample;

            this.PushToQueue("statsQueue", response);            
        }

        public new void Start()
        {
            QueueOptions options = new QueueOptions();
            options.ForgetHistory = false;
            options.MaxSize = 100; //TODO.
            options.Priority = 1;
            options.Quota = 1;

            this.CreateQueue(1, "statsQueue", options);


            this.AddEndPoint(new EndPoint(this.MonitoringPort, new WebsocketProtocol.WebsocketProtocol(true)));
            base.Start();
        }
    }
}
