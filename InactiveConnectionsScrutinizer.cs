using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework
{
    public class InactiveConnectionsScrutinizer : ScheduledTask
    {
        public InactiveConnectionsScrutinizer() : base(-3, 1 * 60)
        {
        }

        public override string Description
        {
            get
            {
                return "Scrutinizes connections that stay inactive for too long";
            }
        }

        public override string Name
        {
            get
            {
                return "InactiveConnectionsScrutinizer";
            }
        }

        protected override void Run()
        {
            DateTime now = DateTime.Now;

            foreach (var pair in this.Server.Connections)
            {
                var c = pair.Value;

                if (!c.PhysicalConnection.IsAuthenticated)
                {
                    continue;
                }

                if (c.IsInactive())
                {
                    c.PhysicalConnection.Close(PhysicalConnection.CloseReason.Inactive);
                }
            }
        }
    }
}
