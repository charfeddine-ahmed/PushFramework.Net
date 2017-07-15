using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework
{
    internal class QueueItem
    {
        public QueueItem(object message, int id)
        {
            this.Message = message;
            this.CreationTime = DateTime.Now;
            this.InternalId = id;
        }

        public object Message
        {
            get;
            internal set;
        }

        public DateTime CreationTime
        {
            get;
            internal set;
        }

        public int InternalId
        {
            get;
            internal set;
        }

        public QueueItem Next
        {
            get;
            set;
        }

    }
}
