using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PushFramework
{
    public abstract class ScheduledTask
    {
        public ScheduledTask(int id, int periodicity)
        {
            this.InternalId = id;
            this.Periodicity = periodicity;
        }

        private Task internalTask;

        private bool isStopped;

        private ManualResetEvent oPeriodicEvent = new ManualResetEvent(false);

        public int Periodicity
        {
            get;
            internal set;
        }

        internal int InternalId
        {
            get;
            private set;
        }

        public Server Server
        {
            get;
            set;
        }

        private void ExecutionFunc()
        {
            this.Initialize();

            while (!isStopped)
            {
                if (this.oPeriodicEvent.WaitOne(this.Periodicity * 1000))
                {
                    return;
                }


                var watch = System.Diagnostics.Stopwatch.StartNew();
                this.Run();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                this.Server.MeasurementMgr.IncrementAvgDistributionValue(Analytics.MeasurementMgr.Metrics.PerformanceAvgTimePerTask, this.InternalId, elapsedMs);
            }
        }

        public abstract string Name
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        protected virtual void Initialize()
        {
            //
        }

        protected virtual void Run()
        {
            //
        }

        internal void Start()
        {
            this.internalTask = Task.Factory.StartNew(() => { this.ExecutionFunc(); }, TaskCreationOptions.LongRunning);
        }

        internal void Stop()
        {
            isStopped = true;
            this.oPeriodicEvent.Set();

            this.internalTask.Wait();
        }
    }
}
