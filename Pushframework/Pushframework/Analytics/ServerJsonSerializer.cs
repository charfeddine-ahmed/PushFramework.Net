using Newtonsoft.Json;
using PushFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtocolFramework;

namespace PushFramework.Analytics.Contracts
{

    public class JsonResponse
    {
        public bool IsSynchronous
        {
            get;
            set;
        }

		public bool IsCallSucceeded
        {
            get;
            set;
        }

		public string Exception
        {
            get;
            set;
        }

        public int OriginatingRequestId
        {
            get;
            set;
        }

        public string AsynchronousSource
        {
            get;
            set;
        }

        public object Data
        {
            get;
            set;
        }
    }

    public class JsonSerializer : Serializer
    {   
        public JsonSerializer()
        {
        }

        public override bool Deserialize(ProtocolFramework.Buffer bytes, out int serviceId, out int methodId, out object message)
        {
            string str = System.Text.Encoding.UTF8.GetString(bytes.Data, 0, bytes.Size);

            int indexFirstToken = str.IndexOf(' ', 0);
            serviceId = int.Parse(str.Substring(0, indexFirstToken));

            int indexSecondToken = str.IndexOf('|', indexFirstToken);
            methodId = int.Parse(str.Substring(indexFirstToken + 1, indexSecondToken - indexFirstToken - 1));

            return this.DeserializeMessage(str.Substring(indexSecondToken + 1), serviceId, methodId, out message);
        }

        public override bool Serialize(object message, out ProtocolFramework.Buffer bytes)
        {
            JsonResponse response = (JsonResponse) message;
 
            string json = JsonConvert.SerializeObject(response, Formatting.Indented);

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

            bytes = new ProtocolFramework.Buffer(buffer, buffer.Length);

            return true;
        }

        public bool DeserializeMessage(string jsonText, int serviceId, int methodId, out object message)
        {
            if (serviceId == 1)
            {
                if (methodId == 1)
                {
                    MonitorServiceAbs.GetServerInfoRequest request = JsonConvert.DeserializeObject<MonitorServiceAbs.GetServerInfoRequest>(jsonText);
                    message = request;
                    return true;
                }
                if (methodId == 2)
                {
                    MonitorServiceAbs.StartProfilingRequest request = JsonConvert.DeserializeObject<MonitorServiceAbs.StartProfilingRequest>(jsonText);
                    message = request;
                    return true;
                }
                if (methodId == 3)
                {
                    MonitorServiceAbs.StopProfilingRequest request = JsonConvert.DeserializeObject<MonitorServiceAbs.StopProfilingRequest>(jsonText);
                    message = request;
                    return true;
                }
                if (methodId == 4)
                {
                    MonitorServiceAbs.SubscribeToFeedsRequest request = JsonConvert.DeserializeObject<MonitorServiceAbs.SubscribeToFeedsRequest>(jsonText);
                    message = request;
                    return true;
                }
                if (methodId == 5)
                {
                    MonitorServiceAbs.UnsubscribeFromFeedsRequest request = JsonConvert.DeserializeObject<MonitorServiceAbs.UnsubscribeFromFeedsRequest>(jsonText);
                    message = request;
                    return true;
                }
            }

            message = null;
            return false;
        }
    }
}
