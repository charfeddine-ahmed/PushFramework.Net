using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework
{
    public class ServerOptions
    {
        public ServerOptions()
        {
            this.MaximumConnections = 100;
            this.MaximumInactivityDuration = 0;
            this.MaximumLoginDuration = 40;
            this.BounceDuration = 60;

        }
        public int MaximumInactivityDuration
        {
            get;
            set;
        }

        public int MaximumConnections
        {
            get;
            set;
        }

        public int MaximumLoginDuration
        {
            get;
            set;
        }
        public int BounceDuration
        {
            get;
            set;
        }

    }
}
