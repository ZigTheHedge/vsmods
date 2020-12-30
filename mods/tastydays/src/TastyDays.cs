using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tastydays.src.cuttingboard;
using tastydays.src.kebabbrazier;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace tastydays.src
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    class CuttingBoardHit
    {
        public int X;
        public int Y;
        public int Z;

        public CuttingBoardHit()
        {
        }

        public CuttingBoardHit(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public BlockPos GetBlockpos()
        {
            return new BlockPos(X, Y, Z);
        }
    }

    class TastyDays : ModSystem
    {
        ICoreServerAPI serverApi;
        ICoreClientAPI clientApi;
        public static IServerNetworkChannel serverChannel;
        public static IClientNetworkChannel clientChannel;
        public static string CHANNELNAME = "tastydays";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("kebabbrazier", typeof(Kebabbrazier));
            api.RegisterBlockEntityClass("kebabbrazierbe", typeof(BEKebabbrazier));
            api.RegisterItemClass("multifood", typeof(MultiFood));
            api.RegisterBlockClass("cuttingboard", typeof(CuttingBoard));
            api.RegisterBlockEntityClass("cuttingboardbe", typeof(BECuttingBoard));

            api.Network.RegisterChannel(CHANNELNAME).RegisterMessageType(typeof(CuttingBoardHit));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            clientApi = api;
            clientChannel = api.Network.GetChannel(CHANNELNAME);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;
            serverChannel = api.Network.GetChannel(CHANNELNAME);
            serverChannel.SetMessageHandler<CuttingBoardHit>(OnCuttingBoardHit);
        }

        public void OnCuttingBoardHit(IPlayer sender, CuttingBoardHit msg)
        {
            BECuttingBoard be = serverApi.World.BlockAccessor.GetBlockEntity(msg.GetBlockpos()) as BECuttingBoard;
            if(be != null)
            {
                if(be.SuccessfulHit())
                {
                    ItemSlot cleaver = ((IServerPlayer)sender).InventoryManager.ActiveHotbarSlot;
                    cleaver.Itemstack.Collectible.DamageItem(serverApi.World, sender.Entity, cleaver);
                }
            }
        }
    }
}
