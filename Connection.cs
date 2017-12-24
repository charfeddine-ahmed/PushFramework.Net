using System.Collections.Generic;

namespace PushFramework
{
    public abstract class Connection
    {
        public Connection()
        {
            this.Services = new Dictionary<int, Service>();
        }

        internal PhysicalConnection PhysicalConnection
        {
            get;
            set;
        }

        internal Dictionary<int, Service> Services
        {
            get;
            set;
        }

        public bool IsAuthenticated
        {
            get
            {
                return this.PhysicalConnection.IsAuthenticated;
            }
        }

        public void MarkAsAuthenticated()
        {
            this.PhysicalConnection.MarkAsAuthenticated();
        }

        public virtual bool HandleMessage(ProtocolFramework.Buffer data)
        {
            return false;
        }

        public void SendMessage(object message)
        {
            this.PhysicalConnection.SendMessage(message);
        }

        public void SendMessage(object message, int serviceRoutingId, int methodId)
        {
            this.PhysicalConnection.SendMessage(message, serviceRoutingId, methodId);
        }

        public void SendSerializedMessage(ProtocolFramework.Buffer data)
        {
            this.PhysicalConnection.SendSerializedMessage(data);
        }

        public void SubscribeToQueue(string queueName)
        {
            this.PhysicalConnection.SubscribeToQueue(queueName);
        }

        public void UnSubscribeFromQueue(string queueName)
        {
            this.PhysicalConnection.UnsubscribeFromQueue(queueName);
        }

        public void UnSubscribeFromAll()
        {
            this.PhysicalConnection.UnsubscribeFromAll();
        }

        public void Disconnect()
        {
            this.PhysicalConnection.Close(PhysicalConnection.CloseReason.Requested);
        }

        public virtual void OnDisconnected()
        {
            //
        }

        public virtual bool IsInactive()
        {
            return this.PhysicalConnection.IsInactive();
        }

    }
}