using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework
{
    internal class QueueContext
    {
        public QueueContext(Queue queue)
        {
            this.Queue = queue;
        }

        public Queue Queue
        {
            get;
            set;
        }

        public QueueItem LastSentItem
        {
            get;
            set;
        }

        public QueueContext Next
        {
            get;
            set;
        }

        public int SentCount
        {
            get;
            internal set;
        }


        public object GetNextMessage()
        {
            object result = this.Queue.GetNextMessage(this);

            if (result == null)
            {
                return null;
            }

            this.SentCount++;
            if (this.SentCount == this.Queue.QueueOptions.Quota)
            {
                this.SentCount = 0;
            }

            return result;
        }
    }
}
