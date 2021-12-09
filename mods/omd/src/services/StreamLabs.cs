using omd.src.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace omd.src.services
{
    class StreamLabs : DonationService
    {
        public string wssToken = "";
        public string serviceUserID = "";
        public string wssClientId = "";
        public string wssChannelToken = "";
        public WSSSTATE wssState;
        string OAUTH = "";
        public string lastDonationId = "";

        public override void Start()
        {
            serviceName = "Stream Labs";
            OAUTH = OMD.ModConfigFile.Current.slOATH;
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
            headers.Add("Content-Type", "application/json; utf-8");
            headers.Add("Accept", "application/json");
            string response = PerformSyncJSONRequest("https://streamlabs.com/api/v1.0/donations?access_token=" + OAUTH + "&limit=30", "GET", headers, "");
            if (response == null)
            {
            }
            else
            {
                JsonObject obj = JsonObject.FromJson(response);
                //JsonObject obj = new JsonParser().parse(response).getAsJsonObject();
                if (obj["error"].Exists)
                {
                }
                else
                {
                    JsonObject[] dataElement = obj["data"].AsArray();
                    if (dataElement.Length > 0)
                        lastDonationId = dataElement[0]["donation_id"].AsInt().ToString();
                    isStarted = true;
                    tickInterval = 100;
                    OMD.clientApi.Event.RegisterGameTickListener(OMD.SL.Tick, 50);
                }
            }
        }


        public void Executor()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
            headers.Add("Content-Type", "application/json; utf-8");
            headers.Add("Accept", "application/json");
            string response = PerformSyncJSONRequest("https://streamlabs.com/api/v1.0/donations?access_token=" + OAUTH + "&limit=30", "GET", headers, "");

            JsonObject obj = JsonObject.FromJson(response);
            if (obj["error"].Exists) return;
            JsonObject[] dataElement = obj["data"].AsArray();
            if (dataElement.Length == 0) return;
            string lastId = dataElement[0]["donation_id"].AsInt().ToString();
            if (!lastId.Equals(lastDonationId))
            {
                int idx = 0;
                string last = lastId;
                while (!last.Equals(lastDonationId))
                {
                    try
                    {
                        last = dataElement[++idx]["donation_id"].AsInt().ToString();
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        idx = 0;
                        break;
                    }
                }
                if (lastDonationId == null) idx = 1;
                while (idx > 0)
                {
                    idx--;
                    JsonObject data = dataElement[idx];
                    int amount = (int)data["amount"].AsFloat();
                    string nickname = data["name"].AsString();
                    string msg = data["message"].AsString();
                    ThresholdItem match = OMD.thresholds.GetSuitableThreshold(amount);
                    if (match != null)
                    {
                        OMD.clientApi.Event.EnqueueMainThreadTask(() => {
                            OMD.clientApi.World.Player.ShowChatNotification(match.GetMessage(amount, nickname, msg));
                        }, "rewardmsg");
                        match.RunCommands();
                    } else
                    {
                        OMD.clientApi.Event.EnqueueMainThreadTask(() => {
                            OMD.clientApi.World.Player.ShowChatNotification(nickname + " has donated " + amount.ToString() + ", but no match was found.");
                        }, "rewardmsg");
                    }
                    lastDonationId = data["donation_id"].AsInt().ToString();
                }
            }
        }

        public override void Execute()
        {
            base.Execute();
            Task.Run(() => Executor());
        }
    }
}
