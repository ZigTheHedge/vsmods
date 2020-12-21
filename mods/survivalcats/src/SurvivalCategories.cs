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
    class SurvivalCategories : ModSystem
    {
        Harmony harmony = new Harmony("com.cwelth.survivalcats");

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.Start(api);

            harmony.PatchAll();
        }

        public override void Dispose()
        {
            base.Dispose();
            harmony.UnpatchAll("com.cwelth.survivalcats");
        }
    }
}
