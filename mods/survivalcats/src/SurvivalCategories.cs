using Foundation.Extensions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace survivalcats.src
{
    public class ModConfigFile
    {
        public static ModConfigFile Current { get; set; }

        public bool addModNameToInfo { get; set; } = true;
        public bool displayVanillaTabsAsCategories { get; set; } = true;
        public bool displayModsAsCategories { get; set; } = true;
    }

    class SurvivalCategories : ModSystem
    {
        Harmony harmony = new Harmony("com.cwelth.survivalcats");

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.Start(api);

            ModConfigFile.Current = api.LoadOrCreateConfig<ModConfigFile>("SurvivalCatsConfig.json");

            //Harmony.DEBUG = true;
            
            harmony.PatchAll();
        }

        public override void Dispose()
        {
            base.Dispose();
            harmony.UnpatchAll("com.cwelth.survivalcats");
        }
    }
}
