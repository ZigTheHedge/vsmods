using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using zeekea.src.standardInventories;

namespace zeekea.src.tall_locker
{
    class BETallLocker : BlockEntityContainer
    {
        internal InventoryTwentyFour inventory;
        GuiTwentyFourSlots lockerDialog;

        private BlockEntityAnimationUtil animUtil => ((BEBehaviorAnimatable)GetBehavior<BEBehaviorAnimatable>())?.animUtil;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "zeetwentyfour"; }
        }

        public BETallLocker()
        {
            inventory = new InventoryTwentyFour(null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("zeetwentyfour-1", api);

            if (api.World.Side == EnumAppSide.Client)
            {
                animUtil.InitializeAnimator("zeekea:tall_locker", new Vec3f(0, Block.Shape.rotateY, 0));
            }

        }


        public void OnBlockInteract(IPlayer byPlayer, bool isOwner)
        {
            if (Api.Side == EnumAppSide.Client)
            {
            }
            else
            {
                byte[] data;

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(ms);
                    TreeAttribute tree = new TreeAttribute();
                    inventory.ToTreeAttributes(tree);
                    tree.ToBytes(writer);
                    data = ms.ToArray();
                }

                ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(
                    (IServerPlayer)byPlayer,
                    Pos.X, Pos.Y, Pos.Z,
                    (int)EnumBlockStovePacket.OpenGUI,
                    data
                );

                byPlayer.InventoryManager.OpenInventory(inventory);
            }
        }

        public override void FromTreeAtributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAtributes(tree, worldForResolving);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }


        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if (packetid <= 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                if (fromPlayer.InventoryManager != null)
                {
                    fromPlayer.InventoryManager.CloseInventory(Inventory);
                }
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            base.OnReceivedServerPacket(packetid, data);

            if (packetid == (int)EnumBlockStovePacket.OpenGUI)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryReader reader = new BinaryReader(ms);
                    TreeAttribute tree = new TreeAttribute();
                    tree.FromBytes(reader);
                    Inventory.FromTreeAttributes(tree);
                    Inventory.ResolveBlocksOrItems();

                    IClientWorldAccessor clientWorld = (IClientWorldAccessor)Api.World;

                    if (lockerDialog == null)
                    {
                        Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/locker_open.ogg"), Pos.X, Pos.Y, Pos.Z);

                        lockerDialog = new GuiTwentyFourSlots(Lang.Get("zeekea:tall_locker-title"), Inventory, Pos, Api as ICoreClientAPI);
                        lockerDialog.OnClosed += () =>
                        {
                            lockerDialog = null;
                            Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/locker_close.ogg"), Pos.X, Pos.Y, Pos.Z);
                            animUtil.StopAnimation("open");
                        };
                    }

                    lockerDialog.TryOpen();
                    animUtil.StartAnimation(new AnimationMetaData() { Animation = "open", Code = "open", AnimationSpeed = 1F, EaseInSpeed = 3F, EaseOutSpeed = 0.5F });
                }
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                lockerDialog?.TryClose();
                lockerDialog?.Dispose();
                lockerDialog = null;
            }
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (animUtil.activeAnimationsByAnimCode.Count > 0)
            {
                return true;
            } 

            return false;
        }



    }
}
