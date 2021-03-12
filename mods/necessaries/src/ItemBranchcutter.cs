using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace necessaries.src
{
    class ItemBranchcutter : Item
    {

        

        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
        {
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            if (block.Code.BeginsWith("game", "leaves-") || block.Code.BeginsWith("game", "bambooleaves-"))
            {
                world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                DamageItem(world, byEntity, itemslot);
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                return true;
            } else if (block.Code.BeginsWith("game", "glowworms-"))
            {
                world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                DamageItem(world, byEntity, itemslot);
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                return true;
            }
            else if (block.Code.BeginsWith("game", "saguarocactus-"))
            {
                world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                DamageItem(world, byEntity, itemslot);
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                return true;
            }
            else if (block.Code.BeginsWith("game", "ferntree-"))
            {
                world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                DamageItem(world, byEntity, itemslot);
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                if(world.BlockAccessor.GetBlock(blockSel.Position.UpCopy()).Code.BeginsWith("game", "ferntree-"))
                {
                    BlockSelection blockSelection = blockSel.Clone();
                    blockSelection.Position.Up();
                    OnBlockBrokenWith(world, byEntity, itemslot, blockSelection, dropQuantityMultiplier);
                }
                return true;
            }
            else if (block.Code.BeginsWith("game", "bamboo-"))
            {
                world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                DamageItem(world, byEntity, itemslot);
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                if (world.BlockAccessor.GetBlock(blockSel.Position.UpCopy()).Code.BeginsWith("game", "bamboo-"))
                {
                    BlockSelection blockSelection = blockSel.Clone();
                    blockSelection.Position.Up();
                    OnBlockBrokenWith(world, byEntity, itemslot, blockSelection, dropQuantityMultiplier);
                }
                return true;
            }
            else
                return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel, dropQuantityMultiplier);
        }
    }
}
