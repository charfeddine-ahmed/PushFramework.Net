using System;
using System.Collections.Generic;

namespace PushFramework.Analytics.Contracts
{
    public class Queue
    {
        public Int32 Guid
	    {
		    get;
		    set;
	    }

        public String Name
	    {
		    get;
		    set;
	    }

        public Int32 MaxSize
	    {
		    get;
		    set;
	    }

        public Boolean ForgetHistory
	    {
		    get;
		    set;
	    }

        public Int32 ExpireDuration
	    {
		    get;
		    set;
	    }

        public Int32 Priority
	    {
		    get;
		    set;
	    }

        public Int32 Quota
	    {
		    get;
		    set;
	    }

    }
}