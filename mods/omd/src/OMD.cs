using omd.src.data;
using omd.src.services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace omd.src
{
    class OMD : ModSystem
    {
        public static ICoreClientAPI clientApi;
        public static LocalFileWatcher LOCAL = new LocalFileWatcher();
        public static DonationAlerts DA = new DonationAlerts();
        public static Thresholds thresholds = new Thresholds();

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            //Load Thresholds
            var path = Path.Combine(GamePaths.DataPath, "ModConfig", "omd_rewards.json");
            try
            {
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    thresholds.list = JsonUtil.FromString<List<ThresholdItem>>(content).ToList();
                }
            }
            catch (Exception e)
            {
                api.World.Logger.Log(EnumLogType.Error, $"Failed loading OMD rewards file ({path}), error {e}.");
            }
            thresholds.SortByAmounts();
            clientApi = api;
            //Start services
            LOCAL.Start();

            DA.Start();


        }

        public override void Dispose()
        {
            base.Dispose();
            clientApi = null;
        }
    }
}
