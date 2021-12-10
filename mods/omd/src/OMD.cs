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
        public class ModConfigFile
        {
            public static ModConfigFile Current { get; set; }

            public string localEnabledDesc { get; } = "Enable Local Watcher module?";
            public bool localEnabled { get; set; } = true;
            public string localOATHDesc { get; } = "Not used for Local Watcher. Should be empty.";
            public string localOATH { get; set; } = "";
            public string daEnabledDesc { get; } = "Enable Donation Alerts watcher module?";
            public bool daEnabled { get; set; } = false;
            public string daOATHDesc { get; } = "OAUTH key for Donation Alerts.";
            public string daOATH { get; set; } = "";
            public string slEnabledDesc { get; } = "Enable StreamLabs watcher module?";
            public bool slEnabled { get; set; } = false;
            public string slOATHDesc { get; } = "OAUTH key for StreamLabs";
            public string slOATH { get; set; } = "";
        }

        public static ICoreClientAPI clientApi;
        public static LocalFileWatcher LOCAL = new LocalFileWatcher();
        public static DonationAlerts DA = new DonationAlerts();
        public static StreamLabs SL = new StreamLabs();
        public static Thresholds thresholds = new Thresholds();

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            ModConfigFile.Current = api.LoadOrCreateConfig<ModConfigFile>("omd_config.json");

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
            /*
            clientApi.Input.RegisterHotKey("walkleft", "walkleft", GlKeys.D);
            clientApi.Input.RegisterHotKey("walkright", "walkright", GlKeys.A);
            clientApi.Input.RegisterHotKey("walkforward", "walkforward", GlKeys.S);
            clientApi.Input.RegisterHotKey("walkbackward", "walkbackward", GlKeys.W);
            */

            if (ModConfigFile.Current.localEnabled) LOCAL.Start();
            if (ModConfigFile.Current.daEnabled && ModConfigFile.Current.daOATH != "") DA.Start();
            if (ModConfigFile.Current.slEnabled && ModConfigFile.Current.slOATH != "") SL.Start();
        }

        public override void Dispose()
        {
            base.Dispose();
            clientApi = null;
        }
    }
}
