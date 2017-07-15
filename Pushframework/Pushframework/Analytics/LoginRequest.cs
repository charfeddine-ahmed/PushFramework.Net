using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework.Analytics
{
    public class LoginRequest
    {
        public int AnalyticsVersion
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }
    }
}
