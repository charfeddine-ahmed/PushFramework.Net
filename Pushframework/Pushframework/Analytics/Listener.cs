using System;
using System.Collections.Generic;

namespace PushFramework.Analytics.Contracts
{
    public class Listener
    {
        public Int32 Port
	    {
		    get;
		    set;
	    }

        public List<Protocol> Protocols
	    {
		    get;
		    set;
	    }

    }
}