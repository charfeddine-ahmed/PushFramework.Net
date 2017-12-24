using System;
using System.Collections.Generic;

namespace PushFramework.Analytics.Contracts
{
    public class Service
    {
        public String Name
	    {
		    get;
		    set;
	    }

        public Int32 RoutingId
	    {
		    get;
		    set;
	    }

        public String Description
	    {
		    get;
		    set;
	    }

        public Boolean IsShared
	    {
		    get;
		    set;
	    }

    }
}