using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework
{
    public class IllegitemateConnectionScrutinizer : ScheduledTask
    {
        public IllegitemateConnectionScrutinizer() : base(-2, 30)
        {
        }

        public override string Description
        {
            get
            {
                return "Scrutinizes connections that were not authorized in time";
            }
        }

        public override string Name
        {
            get
            {
                return "IllegitemateConnectionScrutinizer";
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
                    TimeSpan duration = now - c.PhysicalConnection.CreationTime;

                    if (duration.TotalSeconds > this.Server.ServerOptions.MaximumLoginDuration)
                    {
                        c.PhysicalConnection.Close(PhysicalConnection.CloseReason.UnAuthorized);
                    }
                }                
            }
        }
    }
}
