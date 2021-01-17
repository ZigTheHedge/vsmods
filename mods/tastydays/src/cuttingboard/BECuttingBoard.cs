using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace tastydays.src.cuttingboard
{
    class BECuttingBoard : BlockEntityDisplay
    {
        internal InventoryCuttingBoard inventory;
        ItemStack cutItem = null;
        int numHits = 0;

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
        }

        public bool SuccessfulHit()
        {
            if (inventory[0].Empty) return false;
            if (cutItem == null)
            {
                if (inventory[0].Itemstack.Collectible.Attributes != null)
                {
                    if (inventory[0].Itemstack.Collectible.Attributes["cutOutput"]["type"].Exists)
                    {
                        bool isItem = (inventory[0].Itemstack.Collectible.Attributes["cutOutput"]["type"].AsString() == "item") ? true : false;
                        int qty = inventory[0].Itemstack.Collectible.Attributes["cutOutput"]["quantity"].AsInt(1);
                        if (isItem)
                        {
                            cutItem = new ItemStack(Api.World.GetItem(new AssetLocation(inventory[0].Itemstack.Collectible.Attributes["cutOutput"]["code"].AsString())), qty);
                        }
                        else
                        {
                            cutItem = new ItemStack(Api.World.GetBlock(new AssetLocation(inventory[0].Itemstack.Collectible.Attributes["cutOutput"]["code"].AsString())), qty);
                        }
                        numHits = inventory[0].Itemstack.Collectible.Attributes["cutDurability"].AsInt(10);
                    } else
                    {
                        cutItem = null;
                        numHits = 0;
                        MarkDirty();
                        return false;
                    }
                }
            }
            if (numHits > 0)
            {
                numHits--;
                if(numHits == 0)
                {
                    inventory[0].Itemstack = cutItem.Clone();
                    cutItem = null;
                    MarkDirty(true);
                    return true;
                }
                MarkDirty();
                return true;
            }
            return false;
        }

        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            int selBS = blockSel.SelectionBoxIndex;

            if (slot.Empty)
            {
                if (inventory[0].Empty) return false;
                byPlayer.InventoryManager.TryGiveItemstack(inventory[0].TakeOutWhole());
                cutItem = null;
                numHits = 0;
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

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            cutItem = tree.GetItemstack("cutItem");
            cutItem.ResolveBlockOrItem(worldForResolving);
            numHits = tree.GetInt("numHits");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetItemstack("cutItem", cutItem);
            tree.SetInt("numHits", numHits);
        }
    }
}
