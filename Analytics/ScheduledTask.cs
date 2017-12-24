using System;
using System.Collections.Generic;

namespace PushFramework.Analytics.Contracts
{
    public class ScheduledTask
    {
        public Int32 InternalId
	    {
		    get;
		    set;
	    }

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

        public Int32 Periodicity
	    {
		    get;
		    set;
	    }

    }
}