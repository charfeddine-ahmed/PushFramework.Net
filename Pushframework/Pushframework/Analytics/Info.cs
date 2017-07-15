using System;
using System.Collections.Generic;

namespace PushFramework.Analytics.Contracts
{
    public class Info
    {
        public String Name
	    {
		    get;
		    set;
	    }

        public String Description
	    {
		    get;
		    set;
	    }

        public Int32 SamplingRate
	    {
		    get;
		    set;
	    }

        public List<Service> Services
	    {
		    get;
		    set;
	    }

        public List<Method> Methods
	    {
		    get;
		    set;
	    }

        public List<Queue> Queues
	    {
		    get;
		    set;
	    }

        public List<Listener> Listeners
	    {
		    get;
		    set;
	    }

        public List<ScheduledTask> ScheduledTasks
	    {
		    get;
		    set;
	    }

    }
}