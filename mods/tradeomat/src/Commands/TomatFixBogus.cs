using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace tradeomat.src.Commands
{
    public static class TomatFixBogus
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            List<string> playersAffected = new List<string>();
            if (player != null)
            {
                player.SendMessage(0, "Searching for bogus tradeomats (" + Tradeomat.tomatoesServer.Count + " in total)...", EnumChatType.CommandSuccess);
            }

            for (int i = Tradeomat.tomatoesServer.Count - 1; i >= 0 ; i--)
            {
                Block biq = Tradeomat.serverApi.World.BlockAccessor.GetBlock(new BlockPos(Tradeomat.tomatoesServer[i].X, Tradeomat.tomatoesServer[i].Y, Tradeomat.tomatoesServer[i].Z));
                if (player != null)
                {
                    player.SendMessage(0, "Checking tradeomat at: " + Tradeomat.tomatoesServer[i].X + ", " + Tradeomat.tomatoesServer[i].Y + ", " + Tradeomat.tomatoesServer[i].Z + "...", EnumChatType.CommandSuccess);
                }
                if (biq.BlockId != 0)
                {
                    if (biq.FirstCodePart().Equals("tomat") || biq.FirstCodePart().Equals("rug")) continue;
                }
                if (!playersAffected.Contains(Tradeomat.tomatoesServer[i].owner))
                    playersAffected.Add(Tradeomat.tomatoesServer[i].owner);
                Tradeomat.RemoveTomat(Tradeomat.tomatoesServer[i].X, Tradeomat.tomatoesServer[i].Y, Tradeomat.tomatoesServer[i].Z);
            }
            if(playersAffected.Count > 0)
            {
                if(player != null)
                {
                    player.SendMessage(0, "Bogus tradeomats was found for the following players: " + String.Join(", ", playersAffected.ToArray()), EnumChatType.CommandSuccess);
                }
            } else
            {
                if (player != null)
                {
                    player.SendMessage(0, "Bogus tradeomats hasn't been found.", EnumChatType.CommandSuccess);
                }
            }
        }
    }
}
