﻿using System;
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
using zeekea.src.vanillapatches;

namespace zeekea.src.kitchen.winerack
{
    class BEWineRack : BlockEntityDisplay
    {
        InventoryGeneric inv;
        public override InventoryBase Inventory => inv;
        public override string InventoryClassName => "winerack";
        Block block;

        public BEWineRack()
        {
            inv = new InventoryGeneric(6, "winerack-0", null, null);
            meshes = new MeshData[6];
        }

        public override void Initialize(ICoreAPI api)
        {
            block = api.World.BlockAccessor.GetBlock(Pos);
            base.Initialize(api);
        }

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool rackable = colObj?.Attributes != null && colObj.Attributes["winerackable"].AsBool(false) == true;

            if (slot.Empty || !rackable)
            {
                if (TryTake(byPlayer, blockSel))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (rackable)
                {
                    AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                    if (TryPut(slot, blockSel))
                    {
                        Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                        return true;
                    }

                    return false;
                }
            }


            return false;
        }

        private bool TryPut(ItemSlot slot, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;

            for (int i = 0; i < inv.Count; i++)
            {
                int slotnum = (index + i) % inv.Count;
                if (inv[slotnum].Empty)
                {
                    int moved = slot.TryPutInto(Api.World, inv[slotnum]);
                    updateMeshes();
                    MarkDirty(true);
                    return moved > 0;
                }
            }

            return false;
        }

        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;

            if (!inv[index].Empty)
            {
                ItemStack stack = inv[index].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                updateMeshes();
                MarkDirty(true);
                return true;
            }

            return false;
        }

        Matrixf mat = new Matrixf();

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mat.Identity();
            mat.RotateYDeg(block.Shape.rotateY);

            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        protected override void updateMeshes()
        {
            mat.Identity();
            mat.RotateYDeg(block.Shape.rotateY);

            base.updateMeshes();
        }

        protected override MeshData genMesh(ItemStack stack, int index)
        {
            MeshData mesh = null;

            ICoreClientAPI capi = Api as ICoreClientAPI;
            if (stack.Class == EnumItemClass.Block)
            {
                if (capi.ModLoader.IsModEnabled("expandedfoods"))
                {
                    if (stack.Collectible.FirstCodePart().Equals("bottle") && !stack.Collectible.Code.Path.Contains("clay"))
                    {
                        dynamic blockBottleType = stack.Block;
                        if (blockBottleType != null)
                        {
                            ItemStack content = blockBottleType.GetContent(capi.World, stack);
                            if (content == null) return null;
                            mesh = blockBottleType.GenMeshSideways(capi, content, Pos);
                        }
                    }
                }
                if(mesh == null)
                    mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
            }
            else
            {
                nowTesselatingItem = stack.Item;
                nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
                capi.Tesselator.TesselateItem(stack.Item, out mesh, this);

                mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);
            }

            if(block.Shape.rotateY == 0)
                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 1.57f, block.Shape.rotateY * 0.017453f, 0);
            if (block.Shape.rotateY == 90)
                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), -1.57f, (block.Shape.rotateY + 90) * 0.017453f, 1.57f);
            if (block.Shape.rotateY == 180)
                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), -1.57f, block.Shape.rotateY * 0.017453f, 0);
            if (block.Shape.rotateY == 270)
                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 1.57f, (block.Shape.rotateY + 90) * 0.017453f, 1.57f);


            Vec2f[] positions = new Vec2f[]
            {
                new Vec2f(0f, 0.27f),
                new Vec2f(-0.21875f, 0.48f),
                new Vec2f(0.21875f, 0.48f),
                new Vec2f(0f, 0.7f),
                new Vec2f(-0.21875f, 0.95f),
                new Vec2f(0.21875f, 0.95f),
            };

            float x = positions[index].X;
            float y = positions[index].Y;
            float z = -0.25f;

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
            ItemSlot slot = inv[index];
            if (slot.Empty)
            {
                sb.AppendLine(Lang.Get("Empty"));
            }
            else
            {
                bool expFoods = Api.ModLoader.IsModEnabled("expandedfoods");
                if (expFoods && slot.Itemstack.Collectible.FirstCodePart().Equals("bottle"))
                {
                    dynamic blockBottleType = slot.Itemstack.Block;
                    if (blockBottleType != null)
                    {
                        blockBottleType.GetShelfInfo(slot, sb, Api.World);
                    }
                } else
                    sb.AppendLine(slot.Itemstack.GetName());
            }
        }
    }
}
