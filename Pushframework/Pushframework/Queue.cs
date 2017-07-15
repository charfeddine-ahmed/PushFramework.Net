using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushFramework
{
    internal class Queue
    {

        public Queue(int id, string name, QueueOptions options)
        {
            this.Id = id;
            this.Name = name;
            this.QueueOptions = options;
            this.subscribers = new HashSet<Connection>();
        }

        public QueueOptions QueueOptions
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
            internal set;
        }

        public int Id
        {
            get;
            internal set;
        }

        private HashSet<Connection> subscribers;

        private int _lastGeneratedId = 0;
        protected int LastGeneratedId
        {
            get
            {
                _lastGeneratedId++;
                return _lastGeneratedId;
            }
        }


        QueueItem OldestItem;
        QueueItem LastInsertedItem;
        private int _itemCount;


        public void PushMessage(object message)
        {
            lock (this)
            {
                QueueItem item = new QueueItem(message, this.LastGeneratedId);
                if (this.LastInsertedItem == null)
                {
                    this.LastInsertedItem = item;
                    this.OldestItem = item;
                    _itemCount++;
                }
                else
                {
                    this.LastInsertedItem.Next = item;
                    this.LastInsertedItem = item;

                    if (_itemCount == this.QueueOptions.MaxSize)
                    {
                        this.RemoveOldest();
                    }
                }
            }

            this.ActivateSubscribers();
        }

        void RemoveOldest()
        {
            QueueItem oldestItem = this.OldestItem;

            if (this.QueueOptions.MaxSize == 1)
            {
                this.OldestItem = this.LastInsertedItem;
            }
            else
            {
                this.OldestItem = this.OldestItem.Next;
            }

            oldestItem.Next = null; // element is unlinked from other queue items.
        }

        public object GetNextMessage(QueueContext context)
        {
            lock (this)
            {
                QueueItem result = null;

                if (context.LastSentItem == null)
                {
                    // Send oldest message to this new client.
                    result = this.OldestItem;
                }
                else if (context.LastSentItem.Next != null)
                {
                    // This means there is a new message in the queue that arrived 
                    // after what we previously sent.
                    // return that message:
                    result = context.LastSentItem.Next;
                }
                else
                {
                    if (this.LastInsertedItem == context.LastSentItem)
                    {
                        // Client is then up-to-date.
                        result = null;
                    }
                    else
                    {
                        // What we previously sent is now outdated.
                        // context.LastSentItem.Next is null when the message leaves the queue.
                        // In that case, just return the oldest element.
                        result = this.OldestItem;
                    }                    
                }

                if (result == null)
                {
                    return null;
                }

                context.LastSentItem = result;
                return result.Message;                
            }
        }

        private void ActivateSubscribers()
        {
            foreach (Connection connection in this.subscribers)
            {
                connection.PhysicalConnection.CheckAndProcessPendingBroadcast(true);
            }
        }

        public void AddSubscriber(Connection connection)
        {
            this.subscribers.Add(connection);
        }

        public void RemoveSubscriber(Connection connection)
        {
            this.subscribers.Remove(connection);
        }
    }
}
