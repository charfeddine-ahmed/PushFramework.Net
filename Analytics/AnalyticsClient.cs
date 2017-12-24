using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushFramework.Analytics
{
    public class AnalyticsClient : PushFramework.Connection
    {
        public AnalyticsClient(Server monitoringServer)
        {
            this.MonitoringServer = monitoringServer;
        }

        public Server MonitoringServer
        {
            get;
            set;
        }

        public override bool HandleMessage(ProtocolFramework.Buffer bytes)
        {
            if (this.IsAuthenticated)
                return false;

            string jsonText = System.Text.Encoding.UTF8.GetString(bytes.Data, 0, bytes.Size);
            LoginResponse response = new LoginResponse();

            try
            {
                LoginRequest request = JsonConvert.DeserializeObject<LoginRequest>(jsonText);
                response.IsSucceeded = (request.Password == this.MonitoringServer.MonitoringPassword);
                if (!response.IsSucceeded)
                {
                    response.FailReason = "Wrong Password";
                }
                else
                {
                    this.MarkAsAuthenticated();
                }
            }
            catch(System.Exception ex)
            {
                response.IsSucceeded = false;
                if (!response.IsSucceeded)
                {
                    response.FailReason = ex.Message;
                }
            }

            string json = JsonConvert.SerializeObject(response);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

            this.SendSerializedMessage(new ProtocolFramework.Buffer(buffer, buffer.Length));
            return true;
        }
    }
}
