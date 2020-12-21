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
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.Start(api);

            var harmony = new Harmony("com.cwelth.survivalcats");
            harmony.PatchAll();
        }
    }
}
