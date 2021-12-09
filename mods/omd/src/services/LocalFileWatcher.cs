using omd.src.data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace omd.src.services
{
    class LocalFileWatcher : DonationService
    {

        public override void Start()
        {
            base.Start();
            serviceName = "Local File Watcher";
            OMD.clientApi.Event.RegisterGameTickListener(OMD.LOCAL.Tick, 50);

        }

        public void Executor()
        {
            var path = Path.Combine(GamePaths.Binaries, "omd_local.json");
            try
            {
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    Reward reward = JsonUtil.FromString<Reward>(content);
                    ThresholdItem threshold = OMD.thresholds.GetSuitableThreshold(reward.amount);
                    File.Delete(path);
                    if (threshold == null)
                    {
                        OMD.clientApi.Event.EnqueueMainThreadTask(() => {
                            OMD.clientApi.World.Player.ShowChatNotification(reward.name + " has donated " + reward.amount.ToString() + ", but no action is defined for this amount.");
                        }, "rewardmsg");
                        return;
                    }
                    string msg = threshold.GetMessage(reward);
                    OMD.clientApi.Event.EnqueueMainThreadTask(() => {
                        OMD.clientApi.World.Player.ShowChatNotification(msg);
                    }, "rewardmsg");
                    threshold.RunCommands();
                }
            }
            catch (Exception e)
            {
            }
        }

        public override void Execute()
        {
            base.Execute();
            Task.Run(() => Executor());
        }

    }
}
