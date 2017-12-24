using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework
{
    public abstract class Service
    {
        public Service()
        {
            this.MethodsByRoutingId = new Dictionary<int, Analytics.Contracts.Method>();
        }
        public abstract Service GetCopy();

        public abstract void Dispatch(int methodId, object request, Connection user);

        public abstract int RoutingId
        {
            get;
        }
        public abstract bool IsShared
        {
            get;
        }

        public Connection Connection
        {
            get;
            set;
        }

        public abstract  String Name
	    {
		    get;
	    }

        public virtual String Description
        {
            get
            {
                return string.Empty;
            }
        }

        internal Dictionary<int, Analytics.Contracts.Method> MethodsByRoutingId
        {
            get;
            set;
        }

        public void RegisterMethod(int routingId, string name)
        {
            this.RegisterMethod(routingId, this.RoutingId * 1000 + routingId, name);
        }

        public void RegisterMethod(int routingId, int guid, string name)
        {
            Analytics.Contracts.Method method = new Analytics.Contracts.Method();
            method.Name = name;
            method.Guid = guid;
            method.RoutingId = routingId;
            method.ServiceRoutingId = this.RoutingId;

            this.MethodsByRoutingId.Add(routingId, method);
        }

        internal virtual int GetMethodGuid(int routingId)
        {
            return this.RoutingId * 1000 + routingId;
        }
    }
}
