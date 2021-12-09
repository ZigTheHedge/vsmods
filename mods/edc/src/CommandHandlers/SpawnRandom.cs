using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace edc.src.CommandHandlers
{
    public static class SpawnRandom
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            //entity1 entity2 entity3 ... entityN
            if (args.Length == 0) return;
            string chosenEntity = args[Edc.sApi.World.Rand.Next(0, args.Length)];

            try
            {
                EntityProperties entityProperties = Edc.sApi.World.GetEntityType(new AssetLocation(chosenEntity));
                if (entityProperties == null)
                {
                    player.SendMessage(0, "Cannot spawn: " + chosenEntity, EnumChatType.CommandError);
                    return;
                }
                Entity entity = Edc.sApi.World.ClassRegistry.CreateEntity(entityProperties);
                entity.ServerPos.SetPos(player.Entity.ServerPos);
                Edc.sApi.World.SpawnEntity(entity);
            }
            catch (Exception e)
            {

            }
        }
    }
}
