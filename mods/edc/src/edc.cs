using edc.src.CommandHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace edc.src
{
    class Edc : ModSystem
    {
        public static ICoreServerAPI sApi;


        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            sApi = api;
            api.RegisterCommand("tpx", "Teleports player to random coords (use max-values as arguments)", "", Tpx.Execute, Privilege.tp);
            api.RegisterCommand("spawnrandom", "Spawns a random entity from the provided list", "", SpawnRandom.Execute, Privilege.tp);
            api.RegisterCommand("temporalset", "Sets current Temporal Stability value to sender", "", TemporalSet.Execute, Privilege.tp);
            api.RegisterCommand("hungerset", "Sets current Hunger Level value to sender", "", HungerSet.Execute, Privilege.tp);
            api.RegisterCommand("tpbottom", "Teleports player to most lower block for current X and Z", "", TpBottom.Execute, Privilege.tp);
            api.RegisterCommand("setmyspawn", "Sets sender Spawn point to current position", "", SetSpawn.Execute, Privilege.tp);
            api.RegisterCommand("disarm", "Drops player armor and both hands items in world", "", Disarm.Execute, Privilege.tp);
            api.RegisterCommand("clearall", "Clear all player inventory including armor and clothing", "", ClearAll.Execute, Privilege.tp);
        }

    }
}
