using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace omd.src.services
{
    public abstract class DonationService
    {
        public string serviceName = "";
        public bool wssEnabled = false;
        public int tickInterval = 5 * 20;
        public int ticks = 0;
        public bool isStarted = false;
        public bool isValid = false;

        virtual public void Start()
        {
            //TODO: get tickInterval from config
            ticks = tickInterval;
            /*
            OMD.clientApi.Event.EnqueueMainThreadTask(() => {
                OMD.clientApi.World.Player.ShowChatNotification(serviceName + " has started successfully (REST)");
            }, "servicestartmsg");
            */
            wssEnabled = false;
            isStarted = true;
        }

        virtual public void Tick(float timePassed)
        {
            if (!isStarted) return;
            if (ticks > 0) ticks--;
            if (ticks == 0)
            {
                ticks = tickInterval;
                Execute();
            }
        }

        public virtual void Execute()
        {
            if (!isValid)
            {
                isValid = true;
                OMD.clientApi.Event.EnqueueMainThreadTask(() => {
                    OMD.clientApi.World.Player.ShowChatNotification("OMD started for service \"" + serviceName + "\"");
                }, "rewardservicestart_" + serviceName);
            }
        }

        public string PerformSyncJSONRequest(string uri, string method, Dictionary<string, string> headers, string body)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = method;
                foreach (KeyValuePair<string, string> header in headers)
                {
                    if (header.Key.Equals("Content-Type")) request.ContentType = header.Value;
                    else if (header.Key.Equals("Accept")) request.Accept = header.Value;
                    else if (header.Key.Equals("User-Agent")) request.UserAgent = header.Value;
                    else request.Headers.Add(header.Key, header.Value);
                }
                if (body != "")
                {
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        using (StreamWriter os = new StreamWriter(reqStream))
                        {
                            os.Write(body);
                        }
                    }
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader os = new StreamReader(stream))
                    {
                        string retVal = "";
                        string line = "";
                        while ((line = os.ReadLine()) != null)
                        {
                            if (retVal == "") retVal = line;
                            else retVal += line;
                        }
                        return retVal;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return null;
        }
    }
}
