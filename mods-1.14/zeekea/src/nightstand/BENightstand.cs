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

namespace zeekea.src.nightstand
{
    class BENightstand : BlockEntityContainer
    {
        internal InventoryEight inventory;
        GuiEightSlots nightstandDialog;

        private BlockEntityAnimationUtil animUtil => ((BEBehaviorAnimatable)GetBehavior<BEBehaviorAnimatable>())?.animUtil;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "zeeeight"; }
        }

        public BENightstand()
        {
            inventory = new InventoryEight(null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("zeeeight-1", api);
            if (api.World.Side == EnumAppSide.Client)
            {
                animUtil.InitializeAnimator("zeekea:nightstand", new Vec3f(0, Block.Shape.rotateY, 0));
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

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
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

                    if (nightstandDialog == null)
                    {
                        Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/shelf_open.ogg"), Pos.X, Pos.Y, Pos.Z);

                        nightstandDialog = new GuiEightSlots(Lang.Get("zeekea:nightstand-title"), Inventory, Pos, Api as ICoreClientAPI);
                        nightstandDialog.OnClosed += () =>
                        {
                            nightstandDialog = null;
                            Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/shelf_close.ogg"), Pos.X, Pos.Y, Pos.Z);
                            animUtil.StartAnimation(new AnimationMetaData() { Animation = "close", Code = "close", AnimationSpeed = 0.2F });
                            animUtil.StopAnimation("open");

                        };
                    }

                    nightstandDialog.TryOpen();
                    animUtil.StartAnimation(new AnimationMetaData() { Animation = "open", Code = "open", AnimationSpeed = 0.2F });

                }
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                nightstandDialog?.TryClose();
                nightstandDialog?.Dispose();
                nightstandDialog = null;
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
