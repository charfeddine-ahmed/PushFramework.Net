using System;
using System.Collections.Generic;

namespace PushFramework.Analytics.Contracts
{
    public class Method
    {
        public String Name
	    {
		    get;
		    set;
	    }

        public Int32 Guid
	    {
		    get;
		    set;
	    }

        public Int32 RoutingId
	    {
		    get;
		    set;
	    }

        public Int32 ServiceRoutingId
	    {
		    get;
		    set;
	    }

    }
}