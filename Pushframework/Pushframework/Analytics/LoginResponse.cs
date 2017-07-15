using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework.Analytics
{
    public class LoginResponse
    {
        public bool IsSynchronous
        {
            get;
            set;
        }

        public int OriginalRequestId
        {
            get;
            set;
        }

        public bool IsSucceeded
        {
            get;
            set;
        }

        public string FailReason
        {
            get;
            set;
        }

    }
}
