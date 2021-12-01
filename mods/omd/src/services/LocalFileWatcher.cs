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
        public new string serviceName = "Local File Watcher";

        public override void Start()
        {
            base.Start();
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
                    string msg = threshold.GetMessage(reward);
                    OMD.clientApi.Event.EnqueueMainThreadTask(() => {
                        OMD.clientApi.World.Player.ShowChatNotification(msg);
                    }, "rewardmsg");
                    threshold.RunCommands();
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
            }
        }

        public override void Execute()
        {
            Task.Run(() => Executor());
        }

    }
}
