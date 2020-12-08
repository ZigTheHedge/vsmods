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
using zeekea.src.vanillapatches;

namespace zeekea.src.nightstand
{
    class BENightstand : BEContainerDisplayPatch
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
            inventory = new InventoryEight("zeeeight-1", null);
            meshes = new MeshData[8];
        }

        public override void Initialize(ICoreAPI api)
        {
            //inventory.LateInitialize();
            
            base.Initialize(api);
            inventory.SlotModified += OnSlotModified;

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

                ZEEkea.serverChannel.BroadcastPacket<AnimatedContainerUpdate>(new AnimatedContainerUpdate(Pos.X, Pos.Y, Pos.Z, data, ZEEContainerEnum.NIGHTSTAND, true), null);
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
        protected override void translateMesh(MeshData mesh, int index)
        {
            if(index > 3)
            {
                mesh.Clear();
                return;
            }
            float x = (index % 2 == 0) ? 5 / 16f : 11 / 16f;
            float y = 0.56f;
            float z = (index > 1) ? 11 / 16f : 5 / 16f;

            if(!Inventory[index].Empty)
            {
                if(Inventory[index].Itemstack.Class == EnumItemClass.Block)
                    mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.35f, 0.35f, 0.35f);
                else
                    mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);
            }

            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 45 * GameMath.DEG2RAD, 0);
            mesh.Translate(x - 0.5f, y, z - 0.5f);
            int orientationRotate = 0;
            if (Block.Variant["horizontalorientation"] == "east") orientationRotate = 270;
            if (Block.Variant["horizontalorientation"] == "south") orientationRotate = 180;
            if (Block.Variant["horizontalorientation"] == "west") orientationRotate = 90;
            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, orientationRotate * GameMath.DEG2RAD, 0);
        }
        private void OnSlotModified(int slotid)
        {
            if(slotid < 4)
                UpdateShape();
        }
        public void UpdateShape()
        {
            for(int i = 0; i < 8; i++)
            {
                    updateMesh(i);
            }
            MarkDirty(Api.Side != EnumAppSide.Server);
        }
        public void Animate(bool start)
        {
            if(start)
                animUtil.StartAnimation(new AnimationMetaData() { Animation = "open", Code = "open", AnimationSpeed = 1F, EaseInSpeed = 3F, EaseOutSpeed = 0.5F });
            else
                animUtil.StopAnimation("open");
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
                            Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/shelf_close.ogg"), Pos.X, Pos.Y, Pos.Z);
                            UpdateShape();
                            ZEEkea.clientChannel.SendPacket<AnimatedContainerUpdate>(new AnimatedContainerUpdate(Pos.X, Pos.Y, Pos.Z, new byte[] { 0 }, ZEEContainerEnum.NIGHTSTAND, false));
                            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                            nightstandDialog = null;
                        };
                    }

                    nightstandDialog.TryOpen();
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
            base.OnTesselation(mesher, tessThreadTesselator);
            if (animUtil.activeAnimationsByAnimCode.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
