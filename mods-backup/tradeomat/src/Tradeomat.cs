using Foundation.Extensions;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using tradeomat.src.TradeomatBlock;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace tradeomat.src
{
    [ProtoContract]
    class Tomatoes
    {
        [ProtoMember(1)]
        public string owner;
        [ProtoMember(2)]
        public int X;
        [ProtoMember(3)]
        public int Y;
        [ProtoMember(4)]
        public int Z;
        [ProtoMember(5)]
        public bool doRemove = false;

        public Tomatoes()
        {

        }
        public Tomatoes(string owner, int X, int Y, int Z)
        {
            this.owner = owner;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    class StallUpdate
    {
        public int X;
        public int Y;
        public int Z;

        public StallUpdate()
        {

        }

        public StallUpdate(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }
    public class SDCFileConfig
    {
        public static SDCFileConfig Current { get; set; }

        public string NumberOfTomatsAllowedVariants { get; set; } = "Number of Trade'o'mats which every player allowed to place. Set to 0 to disable the limit.";
        public int NumberOfTomatsAllowed { get; set; } = 0;
    }
    class Tradeomat : ModSystem
    {
        ICoreServerAPI serverApi;
        ICoreClientAPI clientApi;
        public static List<Tomatoes> tomatoesServer;
        public static List<Tomatoes> tomatoesClient = new List<Tomatoes>();
        public static IServerNetworkChannel serverChannel;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("tomat", typeof(TradeBlock));
            api.RegisterBlockEntityClass("betomat", typeof(BETradeBlock));
            api.RegisterBlockClass("tomat-up", typeof(TradeBlockUp));

            api.Network.RegisterChannel("tradeomat")
                .RegisterMessageType(typeof(Tomatoes))
                .RegisterMessageType(typeof(StallUpdate));

        }
        public static int CountTomatoes(IPlayer player, ICoreAPI api)
        {
            int count = 0;
            if (api.Side == EnumAppSide.Client)
            {
                for (int i = 0; i < tomatoesClient.Count; i++)
                {
                    if (tomatoesClient[i].owner == player.PlayerName) count++;
                }
            }
            else
            {
                for (int i = 0; i < tomatoesServer.Count; i++)
                {
                    if (tomatoesServer[i].owner == player.PlayerName) count++;
                }
            }
            return count;
        }
        public static bool AbleToPlaceTomat(IPlayer player, ICoreAPI api)
        {
            int count = CountTomatoes(player, api);
            if (count >= SDCFileConfig.Current.NumberOfTomatsAllowed && SDCFileConfig.Current.NumberOfTomatsAllowed != 0) return false;
            return true;
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            api.Network.GetChannel("tradeomat")
                .SetMessageHandler<Tomatoes>(OnServerMessage)
                .SetMessageHandler<StallUpdate>(OnStallUpdate)
            ;
        }
        private void OnStallUpdate(StallUpdate msg)
        {
            BlockPos bePos = new BlockPos(msg.X, msg.Y, msg.Z);
            BETradeBlock be = clientApi.World.BlockAccessor.GetBlockEntity(bePos) as BETradeBlock;
            if(be != null)
            {
                be.UpdateDeal();
            }
        }
        private void OnServerMessage(Tomatoes msg)
        {
            if (!msg.doRemove)
            {
                // Edit?
                bool found = false;
                for (int i = 0; i < tomatoesClient.Count; i++)
                {
                    if (tomatoesClient[i].X == msg.X &&
                        tomatoesClient[i].Y == msg.Y &&
                        tomatoesClient[i].Z == msg.Z)
                    {
                        tomatoesClient[i].owner = msg.owner;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    tomatoesClient.Add(msg);
            }
            else
            {
                for (int i = 0; i < tomatoesClient.Count; i++)
                {
                    if (tomatoesClient[i].X == msg.X &&
                        tomatoesClient[i].Y == msg.Y &&
                        tomatoesClient[i].Z == msg.Z)
                    {
                        tomatoesClient.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;

            serverChannel = serverApi.Network.GetChannel("tradeomat");

            api.Event.SaveGameLoaded += LoadPlayerTomatoes;
            api.Event.GameWorldSave += SavePlayerTomatoes;
            api.Event.PlayerJoin += PushTomatoes;
        }
        public void PushTomatoes(IServerPlayer serverPlayer)
        {
            for (int i = 0; i < tomatoesServer.Count; i++)
            {
                if(tomatoesServer[i].owner == serverPlayer.PlayerName)
                    serverChannel.SendPacket(new Tomatoes()
                    {
                        owner = tomatoesServer[i].owner,
                        X = tomatoesServer[i].X,
                        Y = tomatoesServer[i].Y,
                        Z = tomatoesServer[i].Z,
                        doRemove = false
                    }, serverPlayer);
            }
        }
        public static void AddTomat(IServerPlayer player, int X, int Y, int Z)
        {
            tomatoesServer.Add(new Tomatoes(player.PlayerName, X, Y, Z));
            serverChannel.SendPacket(new Tomatoes()
            {
                owner = player.PlayerName,
                X = X,
                Y = Y,
                Z = Z,
                doRemove = false
            }, player);
        }
        public static void RemoveTomat(int X, int Y, int Z)
        {
            for (int i = 0; i < tomatoesServer.Count; i++)
            {
                if (tomatoesServer[i].X == X &&
                    tomatoesServer[i].Y == Y &&
                    tomatoesServer[i].Z == Z)
                {
                    tomatoesServer.RemoveAt(i);
                    break;
                }
            }
            serverChannel.BroadcastPacket(new Tomatoes()
            {
                owner = "",
                X = X,
                Y = Y,
                Z = Z,
                doRemove = true
            });
        }
        public override void StartPre(ICoreAPI api)
        {
            SDCFileConfig.Current = api.LoadOrCreateConfig<SDCFileConfig>("TradeomatConfig.json");
        }
        public void SavePlayerTomatoes()
        {
            serverApi.WorldManager.SaveGame.StoreData("tomatoes", SerializerUtil.Serialize(tomatoesServer));
        }

        public void LoadPlayerTomatoes()
        {
            byte[] data = serverApi.WorldManager.SaveGame.GetData("tomatoes");

            tomatoesServer = data == null ? new List<Tomatoes>() : SerializerUtil.Deserialize<List<Tomatoes>>(data);
        }

    }
}
