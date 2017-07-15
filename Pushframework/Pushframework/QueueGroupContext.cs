using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework
{
    internal class QueueGroupContext
    {
        public QueueGroupContext(Queue queue)
        {
            this.Head = new QueueContext(queue);
            this.Current = this.Head;
        }
        public QueueGroupContext Next
        {
            get;
            set;
        }

        public QueueContext Head
        {
            get;
            internal set;
        }

        public QueueContext Current
        {
            get;
            internal set;
        }

        public void MoveCurrentToNext()
        {
            this.Current = this.Current.Next;

            if (this.Current == null)
            {
                this.Current = this.Head;
            }
        }

        public object GetNextMessage(out Queue originQueue)
        {
            QueueContext searchStart = this.Current;

            while (true)
            {
                object result = this.Current.GetNextMessage();

                if (result != null)
                {
                    originQueue = this.Current.Queue;

                    if (this.Current.SentCount == 0)
                    {
                        this.MoveCurrentToNext();
                    }

                    return result;
                }

                this.MoveCurrentToNext();

                if (searchStart == this.Current)
                {
                    originQueue = null;
                    return null;
                }
            }
        }

        public void RemoveQueue(string queueName)
        {
            QueueContext context = this.Head;
            QueueContext previous = null;

            while (context != null)
            {
                if (context.Queue.Name == queueName)
                {
                    break;
                }

                previous = context;
                context = context.Next;
            }

            if (context != null)
            {
                if (previous != null)
                {
                    previous.Next = context.Next;
                }
                else
                {
                    this.Head = context.Next;
                }

                //
                context.Next = null;
                if (this.Current == context)
                {
                    this.Current = this.Head;
                }
            }
            else
            {
                if (this.Next != null)
                {
                    this.Next.RemoveQueue(queueName);

                    QueueGroupContext nextGroup = this.Next;

                    if (nextGroup.Head == null)
                    {
                        this.Next = nextGroup.Next;
                        nextGroup.Next = null;
                    }
                }
            }

            //

        }

        public bool AddQueue(Queue queue)
        {
            if (this.Head.Queue.QueueOptions.Priority < queue.QueueOptions.Priority)
            {
                return false;
            }
            
            if (this.Head.Queue.QueueOptions.Priority == queue.QueueOptions.Priority)
            {
                this.AddQueueIntern(queue);
                return true;
            }

            // queue has a lower priority:
            if (this.Next == null || !this.Next.AddQueue(queue))
            {
                QueueGroupContext group = new QueueGroupContext(queue);
                group.Next = this.Next;
                this.Next = group;
            }

            return true;
        }

        private void AddQueueIntern(Queue queue)
        {
            QueueContext context = new QueueContext(queue);

            if (this.Head.Queue.QueueOptions.Quota <= queue.QueueOptions.Quota )
            {
                context.Next = this.Head;
                this.Head = context;
                return;
            }

            QueueContext parent = this.Head;
            while (parent.Next != null)
            {
                if (queue.QueueOptions.Quota >= parent.Next.Queue.QueueOptions.Quota)
                {
                    break;
                }
            }

            context.Next = parent.Next;
            parent.Next = context;
        }
    }
}
