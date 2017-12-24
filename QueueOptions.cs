using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework
{
    public class QueueOptions
    {
        public int MaxSize
        {
            get;
            set;
        }

        public bool ForgetHistory
        {
            get;
            set;
        }

        public int ExpireDuration
        {
            get;
            set;
        }

        public int Priority
        {
            get;
            set;
        }

        public int Quota
        {
            get;
            set;
        }

    }
}
