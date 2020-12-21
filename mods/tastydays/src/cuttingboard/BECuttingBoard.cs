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

namespace tastydays.src.cuttingboard
{
    class BECuttingBoard : BlockEntityDisplay
    {
        int rotTest;
        internal InventoryCuttingBoard inventory;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "cuttingboard"; }
        }

        public BECuttingBoard()
        {
            inventory = new InventoryCuttingBoard(null, null);
            meshes = new MeshData[1];

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("cuttingboard-1", api);
            RegisterGameTickListener(Tick, 100);
        }

        public void Tick(float dt)
        {
            rotTest += 2;
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
                if (inventory[0].Empty) return false;
                byPlayer.InventoryManager.TryGiveItemstack(inventory[0].TakeOutWhole());
                MarkDirty(true);
                return true;
            }
            else
            {
                if (FoodSlot.IsCuttable(slot))
                {
                    if (inventory[selBS].Empty)
                    {
                        int res = slot.TryPutInto(Api.World, inventory[selBS], 1);
                        MarkDirty(true);
                        return true;
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
            //if (index == 0) return null;

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

                mesh.RenderPasses.Fill((short)EnumChunkRenderPass.BlendNoCull);
            }


            ModelTransform transform;

            if (stack.Collectible.Attributes?["onCuttingBoardTransform"].Exists == true)
            {
                transform = stack.Collectible.Attributes?["onCuttingBoardTransform"].AsObject<ModelTransform>();
                transform.EnsureDefaultValues();
                
                transform.Rotation.Y += Block.Shape.rotateY;
            }
            else
            {

                transform = ModelTransform.NoTransform;
                transform.EnsureDefaultValues();

                transform.Origin.X = 0.5f;
                transform.Origin.Y = 0;
                transform.Origin.Z = 0.5f;

                transform.Rotation.X = 0;
                transform.Rotation.Y = Block.Shape.rotateY;
                transform.Rotation.Z = 90;

                transform.Scale = 1f;
            }
            mesh.ModelTransform(transform);
            

            float x = 0f;
            float y = 0.07f;
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
