using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using zeekea.src.armchair;
using zeekea.src.curtains;
using zeekea.src.doorbell;
using zeekea.src.freezer;
using zeekea.src.nightlamp;
using zeekea.src.nightstand;
using zeekea.src.tall_locker;

namespace zeekea.src
{
    enum ZEEContainerEnum
    {
        LOCKER = 0,
        NIGHTSTAND = 1
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    class AnimatedContainerUpdate
    {
        public int X;
        public int Y;
        public int Z;
        public byte[] invSync;
        public ZEEContainerEnum containerType;
        public bool open;

        public AnimatedContainerUpdate()
        {

        }

        public AnimatedContainerUpdate(int X, int Y, int Z, byte[] invSync, ZEEContainerEnum containerType, bool open)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.invSync = invSync;
            this.containerType = containerType;
            this.open = open;
        }
    }

    class ZEEkea : ModSystem
    {
        ICoreServerAPI serverApi;
        ICoreClientAPI clientApi;
        public static IServerNetworkChannel serverChannel;
        public static IClientNetworkChannel clientChannel;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("nightstand", typeof(Nightstand));
            api.RegisterBlockEntityClass("benightstand", typeof(BENightstand));
            api.RegisterBlockClass("nightlamp", typeof(Nightlamp));
            api.RegisterBlockClass("armchair", typeof(Armchair));
            api.RegisterBlockClass("tall_locker", typeof(TallLocker));
            api.RegisterBlockEntityClass("betall_locker", typeof(BETallLocker));
            api.RegisterBlockClass("dummy-up", typeof(DummyUP));
            api.RegisterBlockClass("freezer", typeof(Freezer));
            api.RegisterBlockEntityClass("befreezer", typeof(BEFreezer));
            api.RegisterBlockClass("doorbell", typeof(DoorBell));
            api.RegisterBlockClass("curtain", typeof(Curtains));

            api.RegisterItemClass("icepick", typeof(IcepickItem));

            api.Network.RegisterChannel("zeekea")
                .RegisterMessageType(typeof(AnimatedContainerUpdate));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            clientChannel = api.Network.GetChannel("zeekea");

            clientChannel
                .SetMessageHandler<AnimatedContainerUpdate>(OnContainerUpdate)
            ;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;

            serverChannel = serverApi.Network.GetChannel("zeekea");

            serverChannel
                .SetMessageHandler<AnimatedContainerUpdate>(OnServerContainerUpdate)
            ;
        }

        private void OnServerContainerUpdate(IPlayer sender, AnimatedContainerUpdate msg)
        {
            serverChannel.BroadcastPacket<AnimatedContainerUpdate>(new AnimatedContainerUpdate(msg.X, msg.Y, msg.Z, msg.invSync, msg.containerType, msg.open), null);

        }

        private void OnContainerUpdate(AnimatedContainerUpdate msg)
        {
            BlockPos bePos = new BlockPos(msg.X, msg.Y, msg.Z);
            BlockEntityContainer be;
            if(msg.containerType == ZEEContainerEnum.LOCKER)
                be = clientApi.World.BlockAccessor.GetBlockEntity(bePos) as BETallLocker;
            else
                be = clientApi.World.BlockAccessor.GetBlockEntity(bePos) as BENightstand;
            if (be != null)
            {
                if (msg.open)
                { 
                    using (MemoryStream ms = new MemoryStream(msg.invSync))
                    {
                        BinaryReader reader = new BinaryReader(ms);
                        TreeAttribute tree = new TreeAttribute();
                        tree.FromBytes(reader);
                        be.Inventory.FromTreeAttributes(tree);
                        be.Inventory.ResolveBlocksOrItems();
                    }
                }
                if (msg.containerType == ZEEContainerEnum.LOCKER)
                    ((BETallLocker)be).Animate(msg.open);
                if (msg.containerType == ZEEContainerEnum.NIGHTSTAND)
                    ((BENightstand)be).Animate(msg.open);
            }
        }
    }
}
