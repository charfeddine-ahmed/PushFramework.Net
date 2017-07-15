using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtocolFramework;

namespace PushFramework
{
    public class EndPoint
    {
        public EndPoint()
        {
            //
        }

        public EndPoint(int port, Protocol protocol)
        {
            this.Port = port;
            this.ProtocolList = new List<Protocol>() { protocol };
        }

        public int Port
        {
            get;
            set;
        }

        public List<Protocol> ProtocolList
        {
            get;
            set;
        }
    }
}
