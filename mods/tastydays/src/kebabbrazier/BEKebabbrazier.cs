using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace tastydays.src.kebabbrazier
{
    class BEKebabbrazier : BlockEntityDisplay
    {
        int rotTest;
        Random random = new Random();
        internal InventoryKebabbrazier inventory;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "brazier"; }
        }

        public BEKebabbrazier()
        {
            inventory = new InventoryKebabbrazier(null, null);
            meshes = new MeshData[5];

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("brazier-1", api);
            RegisterGameTickListener(Tick, 100);
        }

        public void Tick(float dt)
        {
            rotTest+=2;
            if (rotTest > 359) rotTest = 0;
            //MarkDirty(true);
        }

        public void SetCoalState(string state)
        {
            AssetLocation loc = Block.CodeWithVariant("coalstate", state);
            Block block = Api.World.GetBlock(loc);
            if (block == null) return;

            Api.World.BlockAccessor.ExchangeBlock(block.Id, Pos);
            this.Block = block;
        }

        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            int selBS = blockSel.SelectionBoxIndex;

            if (slot.Empty)
            {
                if (selBS == 0)
                {
                    if (byPlayer.WorldData.EntityControls.Sneak)
                        if (!inventory[selBS].Empty)
                        {
                            byPlayer.InventoryManager.TryGiveItemstack(inventory[selBS].TakeOutWhole());
                            //Disable burning!
                            SetCoalState("nocoal");
                            MarkDirty(true);
                            return true;
                        }
                } else
                {
                    if (!inventory[selBS].Empty)
                    {
                        byPlayer.InventoryManager.TryGiveItemstack(inventory[selBS].TakeOutWhole());
                        MarkDirty(true);
                        return true;
                    }
                }
            } else {
                if (selBS == 0)
                {
                    if (CoalSlot.IsCoal(slot))
                    {
                        if(inventory[selBS].Empty || inventory[selBS].Itemstack.Equals(Api.World, slot.Itemstack))
                        {
                            int res = slot.TryPutInto(Api.World, inventory[selBS], slot.StackSize);
                            SetCoalState("coal");
                            MarkDirty(true);
                            return true;
                        }
                    }
                } else
                {
                    if (BrazierSlot.IsSkewer(slot))
                    {
                        if (inventory[selBS].Empty)
                        {
                            int res = slot.TryPutInto(Api.World, inventory[selBS], slot.StackSize);
                            MarkDirty(true);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        Matrixf mat = new Matrixf();

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mat.Identity();
            mat.RotateYDeg(Block.Shape.rotateY);

            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        protected override void updateMeshes()
        {
            mat.Identity();
            mat.RotateYDeg(Block.Shape.rotateY);

            base.updateMeshes();
        }

        protected override MeshData genMesh(ItemStack stack, int index)
        {
            if (index == 0) return null;

            MeshData mesh;

            ICoreClientAPI capi = Api as ICoreClientAPI;
            if (stack.Class == EnumItemClass.Block)
            {
                mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
            }
            else
            {
                nowTesselatingItem = stack.Item;
                nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
                capi.Tesselator.TesselateItem(stack.Item, out mesh, this);

                mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);
            }

            ModelTransform transform = ModelTransform.NoTransform;


            transform.Rotation.X = 0;
            transform.Rotation.Y = 0;
            transform.Rotation.Z = random.Next(395);
            
            mesh.ModelTransform(transform);

            transform.Rotation.X = 0;
            transform.Rotation.Y = Block.Shape.rotateY;
            transform.Rotation.Z = 0;

            mesh.ModelTransform(transform);


            float x = 3 / 16f + 3 / 16f * (index) - 0.655f;
            float y = 0.47f;
            float z = 0;

            Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
            mesh.Translate(offset.XYZ);

            return mesh;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
                return;
            }

            int index = forPlayer.CurrentBlockSelection.SelectionBoxIndex;
            ItemSlot slot = inventory[index];
            if (slot == null || slot != null && slot.Empty)
            {
                sb.AppendLine(Lang.Get("Empty"));
            }
            else
            {
                sb.AppendLine(slot.Itemstack.GetName() + " x" + slot.Itemstack.StackSize);
            }
        }

    }
}
