using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework.Analytics
{
    public class MonitorService : Contracts.MonitorServiceAbs
    {
        public MonitorService(Server monitoringServer)
        {
            this.MonitoringServer = monitoringServer;
        }

        public override bool IsShared
        {
            get
            {
                return true;
            }
        }

        public Server MonitoringServer
        {
            get;
            set;
        }

        public override Service GetCopy()
        {
            return new MonitorService(this.MonitoringServer);
        }

        public override Contracts.Info GetServerInfo()
        {
            Contracts.Info info = new Contracts.Info();

            info.Name = this.MonitoringServer.MainServer.Name;
            info.Description = this.MonitoringServer.MainServer.Description;
            info.SamplingRate = this.MonitoringServer.SamplingRate;

            info.Services = new List<Contracts.Service>();
            foreach (var s in this.MonitoringServer.MainServer.Services)
            {
                var service = new Contracts.Service();
                service.Name = s.Value.Name;
                service.RoutingId = s.Value.RoutingId;
                service.Description = s.Value.Description;
                service.IsShared = s.Value.IsShared;

                info.Services.Add(service);
            }

            info.Methods = new List<Contracts.Method>();
            foreach (var s in this.MonitoringServer.MainServer.Services)
            {
                foreach (var m in s.Value.MethodsByRoutingId)
                {
                    info.Methods.Add(m.Value);
                }
            }

            info.Queues = new List<Contracts.Queue>();
            foreach (var q in this.MonitoringServer.MainServer.Queues)
            {
                var queue = new Contracts.Queue();
                queue.Name = q.Value.Name;
                queue.MaxSize = q.Value.QueueOptions.MaxSize;
                queue.Priority = q.Value.QueueOptions.Priority;
                queue.Quota = q.Value.QueueOptions.Quota;
                queue.ForgetHistory = q.Value.QueueOptions.ForgetHistory;
                queue.ExpireDuration = q.Value.QueueOptions.ExpireDuration;
                queue.Guid = q.Value.Id;

                info.Queues.Add(queue);
            }

            info.Listeners = new List<Contracts.Listener>();
            foreach (var l in this.MonitoringServer.MainServer.Listeners)
            {
                var listener = new Contracts.Listener();
                listener.Port = l.EndPoint.Port;

                listener.Protocols = new List<Contracts.Protocol>();
                foreach (var p in l.EndPoint.ProtocolList)
                {
                    var protocol = new Contracts.Protocol();
                    protocol.Name = p.Name;
                    protocol.Author = p.Author;
                    protocol.Version = p.Version;

                    listener.Protocols.Add(protocol);                    
                }

                info.Listeners.Add(listener);
            }

            info.ScheduledTasks = new List<Contracts.ScheduledTask>();
            foreach (var t in this.MonitoringServer.MainServer.ScheduledTasks)
            {
                var task = new Contracts.ScheduledTask();
                task.Name = t.Name;
                task.Description = t.Description;
                task.Periodicity = t.Periodicity;
                task.InternalId = t.InternalId;

                info.ScheduledTasks.Add(task);
            }

            return info;
        }

        public override void StartProfiling()
        {
            throw new NotImplementedException();
        }

        public override void StopProfiling()
        {
            throw new NotImplementedException();
        }

        public override void SubscribeToFeeds(Connection user)
        {
            user.SubscribeToQueue("statsQueue");
        }

        public override void UnsubscribeFromFeeds()
        {
            throw new NotImplementedException();
        }
    }
}
