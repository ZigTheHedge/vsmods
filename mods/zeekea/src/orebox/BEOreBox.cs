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
using zeekea.src.nightstand;
using zeekea.src.vanillapatches;

namespace zeekea.src.orebox
{
    class BEOreBox : BEContainerDisplayPatch
    {
        internal InventoryEight inventory;
        GuiEightSlots oreboxDlg;

        private float meshAngle = 999;
        public bool isOpened = false;
        MeshData currentMesh;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "zeeeight"; }
        }

        public BEOreBox()
        {
            inventory = new InventoryEight(null, null);
            meshes = new MeshData[8];
        }

        private BlockEntityAnimationUtil animUtil => ((BEBehaviorAnimatable)GetBehavior<BEBehaviorAnimatable>())?.animUtil;

        public void SetMeshAngle(float meshAngle)
        {
            this.meshAngle = meshAngle;
            if (Api.World.Side == EnumAppSide.Client)
            {
                animUtil.InitializeAnimator("zeekea:orebox", new Vec3f(0, meshAngle * GameMath.RAD2DEG, 0));
            } else
            {
                ((ICoreServerAPI)Api).Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1102, BitConverter.GetBytes(meshAngle));
            }
            MarkDirty();
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            
            inventory.LateInitialize(InventoryClassName + "-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);
            inventory.SlotModified += OnSlotModified;

            if (api.World.Side == EnumAppSide.Client)
            {
                ICoreClientAPI capi = (ICoreClientAPI)api;
                if (meshAngle != 999) SetMeshAngle(meshAngle);
                capi.Tesselator.TesselateBlock(Block, out currentMesh);
                Animate();
            }

        }

        public bool OnBlockInteract(IPlayer byPlayer)
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
            
            return true;
        }

        public void Animate()
        {
            if (animUtil == null) return;
            if (isOpened)
                animUtil.StartAnimation(new AnimationMetaData() { Animation = "open", Code = "open", AnimationSpeed = 1F, EaseInSpeed = 3F, EaseOutSpeed = 10F });
            else
                animUtil.StopAnimation("open");
        }


        public bool Open()
        {
            if (Api.World.Side == EnumAppSide.Client)
            {
                ((ICoreClientAPI)Api).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1101);
            }
            return true;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if (packetid <= 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
            }

            if (packetid == 1101)
            {
                ICoreServerAPI sApi = (ICoreServerAPI)Api;
                if(isOpened) Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/orebox_close.ogg"), Pos.X, Pos.Y, Pos.Z);
                else Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/orebox_open.ogg"), Pos.X, Pos.Y, Pos.Z); 
                isOpened = !isOpened;
                sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1101, BitConverter.GetBytes(isOpened));
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                if (fromPlayer.InventoryManager != null)
                {
                    fromPlayer.InventoryManager.CloseInventory(Inventory);
                }
            }

        }

        private void OnSlotModified(int slotid)
        {
            //Api.World.BlockAccessor.GetChunkAtBlockPos(Pos)?.MarkModified();
            if (Api.World.Side == EnumAppSide.Server)
            {
                if (slotid > 3)
                {
                    if(inventory[slotid - 4].Empty && !inventory[slotid].Empty)
                    {
                        inventory[slotid - 4].Itemstack = inventory[slotid].Itemstack;
                        inventory[slotid - 4].MarkDirty();
                        inventory[slotid].Itemstack = null;
                        inventory[slotid].MarkDirty();
                    }
                } else
                {
                    if (inventory[slotid].Empty && !inventory[slotid + 4].Empty)
                    {
                        inventory[slotid].Itemstack = inventory[slotid + 4].Itemstack;
                        inventory[slotid].MarkDirty();
                        inventory[slotid + 4].Itemstack = null;
                        inventory[slotid + 4].MarkDirty();
                    }
                }
            }
            if(Api.World.Side == EnumAppSide.Client)
                if (!ModConfigFile.Current.hideContents ) UpdateShape();

        }

        public void UpdateShape()
        {
            for (int i = 0; i < 8; i++)
            {
                updateMesh(i);
            }
            MarkDirty(Api.Side != EnumAppSide.Server);
        }

        protected override void translateMesh(MeshData mesh, int index)
        {
            if (mesh == null) return;
            float x = 5f / 16f + (index % 2) * 6f / 16f;
            float y = 1f / 16f + (index / 4) * 7f / 16f;
            float z = 5f / 16f + ((index % 4) / 2) * 6f / 16f;

            if (!Inventory[index].Empty)
            {
                if (index > 3)
                {
                    if (!Inventory[index - 4].Empty)
                    {
                        if (Inventory[index - 4].Itemstack.Class == EnumItemClass.Item) y = 5f / 16f;
                    }
                    else 
                        y = 1f / 16f;
                }

                if (Inventory[index].Itemstack.Class == EnumItemClass.Block)
                    mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.25f, 0.25f, 0.25f);
                else
                {
                    string itmCode = Inventory[index].Itemstack.Item.Code.FirstPathPart();
                    if (itmCode.StartsWith("nugget"))
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 1.2f, 1.2f, 1.2f);
                    else
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);
                }
            }

            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 45 * GameMath.DEG2RAD, 0);
            mesh.Translate(x - 0.5f, y, z - 0.5f);

            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, meshAngle, 0);
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if (packetid == 1101)
            {
                isOpened = BitConverter.ToBoolean(data, 0);
                Animate();
            }

            if (packetid == 1102)
            {
                SetMeshAngle(BitConverter.ToSingle(data, 0));
            }

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

                    if (oreboxDlg == null)
                    {
                        Open();
                        oreboxDlg = new GuiEightSlots(Lang.Get("zeekea:orebox-title"), Inventory, Pos, Api as ICoreClientAPI);
                        oreboxDlg.OnClosed += () =>
                        {                            
                            Open();
                            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                            oreboxDlg = null;
                        };
                        oreboxDlg.TryOpen();
                    }
                    else
                    {
                        (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                        oreboxDlg?.TryClose();
                        oreboxDlg?.Dispose();
                        oreboxDlg = null;
                    }

                }
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                oreboxDlg?.TryClose();
                oreboxDlg?.Dispose();
                oreboxDlg = null;
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            meshAngle = tree.GetFloat("meshAngle");
            isOpened = tree.GetBool("isOpened");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("meshAngle", meshAngle);
            tree.SetBool("isOpened", isOpened);
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            bool parentSkip = base.OnTesselation(mesher, tessThreadTesselator);
            if (animUtil.activeAnimationsByAnimCode.Count > 0 || parentSkip || (animUtil.animator != null && animUtil.animator.ActiveAnimationCount > 0))
            {
                return true;
            } else
            {
                if(currentMesh != null)
                mesher.AddMeshData(currentMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, meshAngle, 0));
                return true;
            }
        }
    }
}
