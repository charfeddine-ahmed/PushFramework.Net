using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework
{
    public class ProfilingTask : ScheduledTask
    {
        public ProfilingTask(int periodicity) : base(-1, periodicity)
        {
        }

        public override string Description
        {
            get
            {
                return "Collects profiling measures and sends them to Analytics server";
            }
        }

        public override string Name
        {
            get
            {
                return "ProfilingTask";
            }
        }

        protected override void Run()
        {
            this.Server.AnalyticsServer.CollectAndBroadcastMeasures();
        }
    }
}
