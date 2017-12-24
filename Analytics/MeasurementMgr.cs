using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework.Analytics
{
    internal class MeasurementMgr
    {
        [DllImport("SystemPerformanceSupport.dll")]
        public static extern int GetTotalMemoryUsage();

        [DllImport("SystemPerformanceSupport.dll")]
        public static extern int GetProcessMemoryUsage();

        [DllImport("SystemPerformanceSupport.dll")]
        public static extern bool CalculateProcessUsages();

        [DllImport("SystemPerformanceSupport.dll")]
        public static extern int GetTotalCpuUsage();

        [DllImport("SystemPerformanceSupport.dll")]
        public static extern int GetProcessCpuUsage();

        public enum Metrics : int
        {
            VisitorsOnline = 1, 
            VisitorsHitsIn,
            VisitorsHitsOut, 
            VisitorsDuration, 
            VisitorsBounce,
            VisitorsUnAuthorized,
            VisitorsInactive,

            BandwidthInbound,
            BandwidthOutbound,
            BandwidthRejection,
            BandwidthOutstanding,

            BandwidthInboundVolPerService, 
            BandwidthOutboundVolPerService,

            BandwidthInboundVolPerMethod,
            BandwidthOutboundVolPerMethod,

            BandwidthInboundPerConnection, 
            BandwidthOutboundPerConnection,

            PerformanceRequestVol,
            PerformanceRequestVolPerService,
            PerformanceRequestVolPerMethod,

            PerformanceProcessingTime,	
            PerformanceProcessingTimePerService,
            PerformanceProcessingTimePerMethod,

            BroadcastPushedMessageVolPerQueue,
            BroadcastSentMessageVolPerQueue,
            BroadcastAvgMessageSizePerQueue,

            BroadcastSentMessagePerConnection,

            BroadcastSubscriptionCountPerQueue,
            BroadcastSubscriptionCount,

            PerformanceTotalCpuUsagePercent,
            PerformanceProcessCpuUsagePercent,
            PerformanceTotalMemoryUsagePercent,
            PerformanceProcessMemoryUsagePercent,

            PerformanceAvgTimePerTask,

        }
        public MeasurementMgr()
        {
            this.cumulativeMetrics = new Dictionary<Metrics, double>();
            this.averagedMetrics = new Dictionary<Metrics, List<double>>();
            this.keyedAveragedMetrics = new Dictionary<Metrics, Dictionary<int, double>>();

            this.distributionMetrics = new Dictionary<Metrics, Dictionary<int, double>>();
            this.avgDistributionMetrics = new Dictionary<Metrics, Dictionary<int, List<double>>>();

            this.ContinuousMetrics = new HashSet<Metrics>();

            this.InitializeMetrics();

        }

        private object lockObj = new object();

        private void InitializeMetrics()
        {
            //TODO Complete.
            this.cumulativeMetrics[Metrics.VisitorsOnline] = 0;
            this.cumulativeMetrics[Metrics.VisitorsHitsIn] = 0;
            this.cumulativeMetrics[Metrics.VisitorsHitsOut] = 0;
            this.cumulativeMetrics[Metrics.VisitorsBounce] = 0;
            this.cumulativeMetrics[Metrics.VisitorsUnAuthorized] = 0;
            this.cumulativeMetrics[Metrics.VisitorsInactive] = 0;

            this.cumulativeMetrics[Metrics.BandwidthInbound] = 0;
            this.cumulativeMetrics[Metrics.BandwidthOutbound] = 0;

            this.cumulativeMetrics[Metrics.BandwidthRejection] = 0;//
            this.cumulativeMetrics[Metrics.BandwidthOutstanding] = 0;

            this.cumulativeMetrics[Metrics.PerformanceRequestVol] = 0;

            this.averagedMetrics[Metrics.VisitorsDuration] = new List<double>();

            this.keyedAveragedMetrics[Metrics.BandwidthInboundPerConnection] = new Dictionary<int, double>();
            this.keyedAveragedMetrics[Metrics.BandwidthOutboundPerConnection] = new Dictionary<int, double>();

            
            this.distributionMetrics[Metrics.BandwidthInboundVolPerService] = new Dictionary<int, double>();
            this.distributionMetrics[Metrics.BandwidthOutboundVolPerService] = new Dictionary<int, double>();//
            this.distributionMetrics[Metrics.BandwidthInboundVolPerMethod] = new Dictionary<int, double>();
            this.distributionMetrics[Metrics.BandwidthOutboundVolPerMethod] = new Dictionary<int, double>();//

            this.distributionMetrics[Metrics.PerformanceRequestVolPerService] = new Dictionary<int, double>();
            this.distributionMetrics[Metrics.PerformanceRequestVolPerMethod] = new Dictionary<int, double>();

            this.avgDistributionMetrics[Metrics.PerformanceProcessingTimePerService] = new Dictionary<int, List<double>>();
            this.avgDistributionMetrics[Metrics.PerformanceProcessingTimePerMethod] = new Dictionary<int, List<double>>();
            this.avgDistributionMetrics[Metrics.PerformanceAvgTimePerTask] = new Dictionary<int, List<double>>();


            this.averagedMetrics[Metrics.PerformanceProcessingTime] = new List<double>();

            this.distributionMetrics[Metrics.BroadcastPushedMessageVolPerQueue] = new Dictionary<int, double>();
            this.distributionMetrics[Metrics.BroadcastSentMessageVolPerQueue] = new Dictionary<int, double>();
            this.avgDistributionMetrics[Metrics.BroadcastAvgMessageSizePerQueue] = new Dictionary<int, List<double>>();


            this.keyedAveragedMetrics[Metrics.BroadcastSentMessagePerConnection] = new Dictionary<int, double>();

            this.distributionMetrics[Metrics.BroadcastSubscriptionCountPerQueue] = new Dictionary<int, double>();
            this.cumulativeMetrics[Metrics.BroadcastSubscriptionCount] = 0;

            this.cumulativeMetrics[Metrics.PerformanceTotalCpuUsagePercent] = 0;
            this.cumulativeMetrics[Metrics.PerformanceProcessCpuUsagePercent] = 0;
            this.cumulativeMetrics[Metrics.PerformanceTotalMemoryUsagePercent] = 0;
            this.cumulativeMetrics[Metrics.PerformanceProcessMemoryUsagePercent] = 0;


            this.ContinuousMetrics.Add(Metrics.VisitorsOnline);
            this.ContinuousMetrics.Add(Metrics.BroadcastSubscriptionCountPerQueue);
            this.ContinuousMetrics.Add(Metrics.BroadcastSubscriptionCount);

            this.ips = new List<string>();
        }

        private Dictionary<Metrics, double> cumulativeMetrics;
        private Dictionary<Metrics, List<double>> averagedMetrics;
        private Dictionary<Metrics, Dictionary<int, double>> keyedAveragedMetrics;
        private Dictionary<Metrics, Dictionary<int, double>> distributionMetrics;
        private Dictionary<Metrics, Dictionary<int, List<double>>> avgDistributionMetrics;
        private HashSet<Metrics> ContinuousMetrics;
        private List<string> ips;

        public void IncrementCumulativeValue(Metrics metricId, double value = 1)
        {
            lock(this.lockObj)
            {
                this.cumulativeMetrics[metricId] += value;
            }            
        }

        public void IncrementAveragedValue(Metrics metricId, double value = 1)
        {
            lock (this.lockObj)
            {
                this.averagedMetrics[metricId].Add(value);
            }            
        }

        public void IncrementKeyedAveragedValue(Metrics metricId, int distId, double value = 1)
        {
            lock (this.lockObj)
            {
                Dictionary<int, double> values = this.keyedAveragedMetrics[metricId];
                if (values.ContainsKey(distId))
                    values[distId] += value;
                else
                    values[distId] = value;
            }
        }

        public void IncrementDistributionValue(Metrics metricId, int distId, double value = 1)
        {
            lock (this.lockObj)
            {
                Dictionary<int, double> values = this.distributionMetrics[metricId];
                if (values.ContainsKey(distId))
                    values[distId] += value;
                else
                    values[distId] = value;
            }            
        }

        public void IncrementAvgDistributionValue(Metrics metricId, int distId, double value = 1)
        {
            lock (this.lockObj)
            {
                Dictionary<int, List<double>> values = this.avgDistributionMetrics[metricId];
                if (values.ContainsKey(distId))
                    values[distId].Add(value);
                else
                {
                    values.Add(distId, new List<double>() { value });
                }
            }                             
        }   

        public void AddIP(string ip)
        {
            this.ips.Add(ip);
        }

        private double GetMean(List<double> values)
        {
            double _avg = 0;
            foreach (var v in values)
                _avg += v;

            if (values.Count != 0)
                _avg /= values.Count;

            return _avg;
        }

        private double GetDispersion(List<double> values, double avg)
        {
            double _disp = 0;
            foreach (var v in values)
                _disp += Math.Pow(v - avg, 2);

            if (values.Count != 0)
                _disp = Math.Sqrt(_disp / values.Count);

            return _disp;
        }

        void CollectPerformanceMeasures()
        {
            CalculateProcessUsages();

            this.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.PerformanceTotalCpuUsagePercent, GetTotalCpuUsage());
            this.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.PerformanceProcessCpuUsagePercent, GetProcessCpuUsage());
            this.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.PerformanceTotalMemoryUsagePercent, GetTotalMemoryUsage());
            this.IncrementCumulativeValue(Analytics.MeasurementMgr.Metrics.PerformanceProcessMemoryUsagePercent, GetProcessMemoryUsage());           
        }

        private void ResetMetrics()
        {
            // Now reset all:
            List<Metrics> cumulativeKeys = new List<Metrics>(this.cumulativeMetrics.Keys);


            foreach (var k in cumulativeKeys)
            {
                this.cumulativeMetrics[k] = 0;
            }

            foreach (var k in this.averagedMetrics.Keys)
            {
                this.averagedMetrics[k].Clear();
            }

            foreach (var k in this.keyedAveragedMetrics.Keys)
            {
                this.keyedAveragedMetrics[k].Clear();
            }

            foreach (var k in this.distributionMetrics.Keys)
            {
                this.distributionMetrics[k].Clear();
            }

            foreach (var k in this.avgDistributionMetrics.Keys)
            {
                this.avgDistributionMetrics[k].Clear();
            }

            this.ips = new List<string>();
        }

        public Contracts.MesureSample Collect()
        {
            this.CollectPerformanceMeasures();

            lock (this.lockObj)
            {
                Contracts.MesureSample sample = new Contracts.MesureSample();
                sample.Timestamp = DateTime.Now;

                sample.CumulativeMeasures = new List<Tuple<int, double>>();
                foreach (var cPair in this.cumulativeMetrics)
                {
                    sample.CumulativeMeasures.Add(new Tuple<int, double>((int)cPair.Key, cPair.Value));
                }

                sample.AveragedMeasures = new List<Tuple<int, double, double>>();
                foreach (var cPair in this.averagedMetrics)
                {
                    double avg = this.GetMean(cPair.Value);
                    double disp = this.GetDispersion(cPair.Value, avg);

                    sample.AveragedMeasures.Add(new Tuple<int, double, double>((int)cPair.Key, avg, disp));
                }

                foreach (var cPair in this.keyedAveragedMetrics)
                {
                    List<double> values = cPair.Value.Values.ToList<double>();

                    double avg = this.GetMean(values);
                    double disp = this.GetDispersion(values, avg);

                    sample.AveragedMeasures.Add(new Tuple<int, double, double>((int)cPair.Key, avg, disp));
                }


                sample.DistributionMeasures = new List<Tuple<int, int, double>>();
                foreach (var cPair in this.distributionMetrics)
                {
                    foreach (var dPair in cPair.Value)
                    {
                        sample.DistributionMeasures.Add(new Tuple<int, int, double>((int)cPair.Key, dPair.Key, dPair.Value));
                    }
                }


                sample.AveragedDistributionMeasures = new List<Tuple<int, int, double, double>>();
                foreach (var cPair in this.avgDistributionMetrics)
                {
                    foreach (var dPair in cPair.Value)
                    {
                        double avg = this.GetMean(dPair.Value);
                        double disp = this.GetDispersion(dPair.Value, avg);

                        sample.AveragedDistributionMeasures.Add(new Tuple<int, int, double, double>((int)cPair.Key, dPair.Key, avg, disp));
                    }
                }


                sample.Visitors = this.ips;


                ResetMetrics();


                return sample;
            }
        }
    }
}
