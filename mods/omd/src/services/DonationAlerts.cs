using omd.src.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace omd.src.services
{
    enum WSSSTATE
    {
        START, WAITFORCLIENTID, WAITFORCHANNELID, READY
    }

    class DonationAlerts : DonationService
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
            serviceName = "Donation Alerts";
            OAUTH = OMD.ModConfigFile.Current.daOATH;
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json; utf-8");
            headers.Add("Accept", "application/json");
            headers.Add("Authorization", "Bearer " + OAUTH);
            string response = PerformSyncJSONRequest("https://www.donationalerts.com/api/v1/alerts/donations", "GET", headers, "");
            if (response == null)
            {
            }
            else
            {
                JsonObject obj = JsonObject.FromJson(response);
                JsonObject[] dataElement = obj["data"].AsArray();
                if (dataElement.Length > 0)
                    lastDonationId = dataElement[0]["id"].AsInt().ToString();
                tickInterval = 100;
                OMD.clientApi.Event.RegisterGameTickListener(OMD.DA.Tick, 50);
                isStarted = true;
            }
            /*

                        wssState = WSSSTATE.START;
                        //TODO: Error handling
                        if (response == null)
                        {

                        }
                        else
                        {
                            JsonObject mainJson = JsonObject.FromJson(response);
                            wssToken = mainJson["data"]["socket_connection_token"].AsString();
                            serviceUserID = mainJson["data"]["id"].AsInt().ToString();
                            isStarted = true;
                            OMD.clientApi.Event.RegisterGameTickListener(OMD.DA.Tick, 50);
                        }
                    }
                    /*
                                    executorThread.submit(()-> {
                                        while (true)
                                        {
                                            switch (wssState)
                                            {
                                                case START:
                                                    {
                                                        // System.out.println("Sending handshake. ");
                                                        websocket.sendMessage("{\"params\": {\"token\": \"" + wssToken + "\"}, \"id\": 1}");
                                                        break;
                                                    }
                                                case WAITFORCHANNELID:
                                                    {
                                                        // System.out.println("Sending request for channel_id. ");
                                                        websocket.sendMessage("{ \"params\": { \"channel\": \"$alerts:donation_" + serviceUserID + "\", \"token\": \"" + wssChannelToken + "\" }, \"method\": 1, \"id\": 2 }");
                                                        break;
                                                    }
                                            }
                                            try
                                            {
                                                Thread.sleep(500);
                                            }
                                            catch (InterruptedException e)
                                            {
                                                e.printStackTrace();
                                            }
                                            if (!started)
                                            {
                                                websocket.disconnect();
                                                break;
                                            }
                                        }
                                    });

                                }
                            } catch (URISyntaxException e)
                        {
                            e.printStackTrace();
                        }
                    }).start();
                } 

            }
            */
        }

        public void Executor()
        {
            /*    
            Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json; utf-8");
                headers.Add("Accept", "application/json");
                headers.Add("Authorization", "Bearer " + OAUTH);

                var exitEvent = new ManualResetEvent(false);
                var url = new Uri("wss://centrifugo.donationalerts.com/connection/websocket");

                using (ClientWebSocket client = new ClientWebSocket())
                {
                    client.
                    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ReconnectionHappened.Subscribe(info =>
                    {
                        wssState = WSSSTATE.START;
                    });

                    client.MessageReceived.Subscribe(msg =>
                    {
                        JsonObject msgJson = JsonObject.FromJson(msg.Text);
                        if (msgJson["id"].Exists)
                        {
                            if (msgJson["id"].AsInt() == 1)
                            {
                                wssClientId = msgJson["result"]["client"].AsString();
                                wssState = WSSSTATE.WAITFORCLIENTID;
                                string response = PerformSyncJSONRequest("https://www.donationalerts.com/api/v1/centrifuge/subscribe", "POST", headers, "{\"channels\":[\"$alerts:donation_" + serviceUserID + "\"], \"client\":\"" + wssClientId + "\"}");
                                JsonObject mainJson = JsonObject.FromJson(response);
                                wssChannelToken = mainJson["channels"]["token"].AsString();
                                wssState = WSSSTATE.WAITFORCHANNELID;
                            }
                            else if (msgJson["id"].AsInt() == 2)
                            {
                                wssState = WSSSTATE.READY;
                            }
                        }
                        else if (msgJson["result"].Exists)
                        {
                            if (!msgJson["result"]["type"].Exists)
                            {
                                JsonObject reward = JsonObject.FromJson(msgJson["result"]["data"]["data"].AsString());
                                int rAmount = reward["amount_in_user_currency"].AsInt();
                                string rNickname = reward["username"].AsString();
                                string rMsg = reward["message"].AsString();
                                ThresholdItem match = OMD.thresholds.GetSuitableThreshold(rAmount);
                                if (match != null)
                                {
                                    OMD.clientApi.Event.EnqueueMainThreadTask(() =>
                                    {
                                        OMD.clientApi.World.Player.ShowChatNotification(rMsg);
                                    }, "rewardmsg");
                                    match.RunCommands();
                                }
                            }

                        }

                    });
                    client.Start();

                    //Task.Run(() => client.Send("{ message }"));

                    exitEvent.WaitOne();
                }
                OMD.clientApi.World.Player.ShowChatNotification("!!!");
            */

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json; utf-8");
            headers.Add("Accept", "application/json");
            headers.Add("Authorization", "Bearer " + OAUTH);
            string response = PerformSyncJSONRequest("https://www.donationalerts.com/api/v1/alerts/donations", "GET", headers, "");

            JsonObject obj = JsonObject.FromJson(response);
            JsonObject[] dataElement = obj["data"].AsArray();

            if (dataElement.Length == 0) return;
            string lastId = dataElement[0]["id"].AsInt().ToString();
            if (!lastId.Equals(lastDonationId))
            {
                int idx = 0;
                string last = lastId;
                while (!last.Equals(lastDonationId))
                {
                    try
                    {
                        last = dataElement[++idx]["id"].AsInt().ToString();
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
                    int amount = data["amount_in_user_currency"].AsInt();
                    string nickname = data["username"].AsString();
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
                    lastDonationId = data["id"].AsInt().ToString();
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
