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
using zeekea.src.vanillapatches;

namespace zeekea.src.tall_locker
{
    class BETallLocker : BEContainerDisplayPatch
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
            meshes = new MeshData[24];
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("zeetwentyfour-1", api);
            inventory.SlotModified += OnSlotModified;

            if (api.World.Side == EnumAppSide.Client)
            {
                animUtil.InitializeAnimator("zeekea:tall_locker", new Vec3f(0, Block.Shape.rotateY, 0));
            }

        }

        private void OnSlotModified(int slotid)
        {
            if (Api.World.Side == EnumAppSide.Client)
                if (!ModConfigFile.Current.hideContents) UpdateShape(slotid);
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

                ZEEkea.serverChannel.BroadcastPacket<AnimatedContainerUpdate>(new AnimatedContainerUpdate(Pos.X, Pos.Y, Pos.Z, data, ZEEContainerEnum.LOCKER, true), null);

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
            if (mesh == null) return;
            float x = (index % 4) * 4f/16f + 2f/16f;
            float y = 2f/16f + 25f/16f - (index / 4) * 5f/16f;
            float z = 8f / 16f;


            if (!Inventory[index].Empty)
            {
                if (Inventory[index].Itemstack.Class == EnumItemClass.Block)
                {
                    mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.15f, 0.15f, 0.15f);
                    mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 8 * GameMath.DEG2RAD, 0);
                }
                else
                {
                    mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.7f, 0.7f, 0.7f);
                    mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 15 * GameMath.DEG2RAD, 0);

                }
            }
            
            mesh.Translate(x - 0.5f, y, z - 0.5f);

            int orientationRotate = 0;
            if (Block.Variant["horizontalorientation"] == "east") orientationRotate = 270;
            if (Block.Variant["horizontalorientation"] == "south") orientationRotate = 180;
            if (Block.Variant["horizontalorientation"] == "west") orientationRotate = 90;
            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, orientationRotate * GameMath.DEG2RAD, 0);

        }
        public void UpdateShape(int slotId = -1)
        {
            if (slotId == -1)
            {
                for (int i = 0; i < 24; i++)
                {
                    updateMesh(i);
                }
            }
            else
            {
                updateMesh(slotId);
            }
            MarkDirty(Api.Side != EnumAppSide.Server);
        }

        public void Animate(bool start)
        {
            if (start)
            {
                animUtil.StartAnimation(new AnimationMetaData() { Animation = "open", Code = "open", AnimationSpeed = 1F, EaseInSpeed = 3F, EaseOutSpeed = 10F });
            }
            else
            {
                animUtil.StopAnimation("open");
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
                            Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/locker_close.ogg"), Pos.X, Pos.Y, Pos.Z);
                            ZEEkea.clientChannel.SendPacket<AnimatedContainerUpdate>(new AnimatedContainerUpdate(Pos.X, Pos.Y, Pos.Z, new byte[] { 0 }, ZEEContainerEnum.LOCKER, false));
                            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                            lockerDialog = null;
                        };
                        lockerDialog.TryOpen();
                    }
                    else
                    {
                        (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                        lockerDialog?.TryClose();
                        lockerDialog?.Dispose();
                        lockerDialog = null;
                    }

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
            bool parentSkip = base.OnTesselation(mesher, tessThreadTesselator);
            if (animUtil.activeAnimationsByAnimCode.Count > 0 || parentSkip || (animUtil.animator != null && animUtil.animator.ActiveAnimationCount > 0))
            {
                return true;
            } 

            return false;
        }



    }
}
