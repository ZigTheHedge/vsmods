using Foundation.Extensions;
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
using zeekea.src.candleholder;
using zeekea.src.curtains;
using zeekea.src.doorbell;
using zeekea.src.freezer;
using zeekea.src.multirotatable;
using zeekea.src.nightlamp;
using zeekea.src.nightstand;
using zeekea.src.orebox;
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

    public class ModConfigFile
    {
        public static ModConfigFile Current { get; set; }

        public bool disableArmchair { get; set; } = false;
        public bool disableArtlamp { get; set; } = false;
        public bool disableBarchair { get; set; } = false;
        public bool disableBell { get; set; } = false;
        public bool disableBench { get; set; } = false;
        public bool disableCandleholder { get; set; } = false;
        public bool disableCurtains { get; set; } = false;
        public bool disableDoorbell { get; set; } = false;
        public bool disableDrifterlamp { get; set; } = false;
        public bool disableSoftFancychair { get; set; } = false;
        public bool disableWoodenFancychair { get; set; } = false;
        public bool disableFreezer { get; set; } = false;
        public bool disableNightlamp { get; set; } = false;
        public bool disableNightstand { get; set; } = false;
        public bool disableOrebox { get; set; } = false;
        public bool disableTalllocker { get; set; } = false;
        public bool disableLinen { get; set; } = false;
        public bool disableIcepick { get; set; } = false;
    }

    class ZEEkea : ModSystem
    {
        ICoreServerAPI serverApi;
        ICoreClientAPI clientApi;
        public static IServerNetworkChannel serverChannel;
        public static IClientNetworkChannel clientChannel;

        public override void StartPre(ICoreAPI api)
        {
            ModConfigFile.Current = api.LoadOrCreateConfig<ModConfigFile>("ZeekeaConfig.json");

            api.World.Config.SetBool("disableArmchair", ModConfigFile.Current.disableArmchair);
            api.World.Config.SetBool("disableArtlamp", ModConfigFile.Current.disableArtlamp);
            api.World.Config.SetBool("disableBarchair", ModConfigFile.Current.disableBarchair);
            api.World.Config.SetBool("disableBell", ModConfigFile.Current.disableBell);
            api.World.Config.SetBool("disableBench", ModConfigFile.Current.disableBench);
            api.World.Config.SetBool("disableCandleholder", ModConfigFile.Current.disableCandleholder);
            api.World.Config.SetBool("disableCurtains", ModConfigFile.Current.disableCurtains);
            api.World.Config.SetBool("disableDoorbell", ModConfigFile.Current.disableDoorbell);
            api.World.Config.SetBool("disableDrifterlamp", ModConfigFile.Current.disableDrifterlamp);
            api.World.Config.SetBool("disableSoftFancychair", ModConfigFile.Current.disableSoftFancychair);
            api.World.Config.SetBool("disableWoodenFancychair", ModConfigFile.Current.disableWoodenFancychair);
            api.World.Config.SetBool("disableFreezer", ModConfigFile.Current.disableFreezer);
            api.World.Config.SetBool("disableNightlamp", ModConfigFile.Current.disableNightlamp);
            api.World.Config.SetBool("disableNightstand", ModConfigFile.Current.disableNightstand);
            api.World.Config.SetBool("disableOrebox", ModConfigFile.Current.disableOrebox);
            api.World.Config.SetBool("disableTalllocker", ModConfigFile.Current.disableTalllocker);
            api.World.Config.SetBool("disableLinen", ModConfigFile.Current.disableLinen);
            api.World.Config.SetBool("disableIcepick", ModConfigFile.Current.disableIcepick);

            base.StartPre(api);
        }


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
            api.RegisterBlockClass("candleholder", typeof(CandleHolder));
            api.RegisterBlockClass("bell", typeof(Bell));
            api.RegisterBlockEntityClass("bebell", typeof(BEBell));
            api.RegisterBlockClass("orebox", typeof(OreBox));
            api.RegisterBlockEntityClass("beorebox", typeof(BEOreBox));

            api.RegisterBlockClass("multirotatable", typeof(BlockMultiRotatable));
            api.RegisterBlockEntityClass("bemultirotatable", typeof(BEMultiRotatable));

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

        public override void Dispose()
        {
            base.Dispose();
            serverChannel = null;
            clientChannel = null;
        }
    }
}
