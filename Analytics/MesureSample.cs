using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework.Analytics.Contracts
{
    public class MesureSample
    {
        public DateTime Timestamp
        {
            get;
            set;
        }

        public List<Tuple<int, double>> CumulativeMeasures
        {
            get;
            set;
        }

        public List<Tuple<int, double, double>> AveragedMeasures
        {
            get;
            set;
        }

        public List<Tuple<int, int, double>> DistributionMeasures
        {
            get;
            set;
        }

        public List<Tuple<int, int, double, double>> AveragedDistributionMeasures
        {
            get;
            set;
        }

        public List<string> Visitors
        {
            get;
            set;
        }
    }
}
