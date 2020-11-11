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
        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel)
        {
            if (world.Side == EnumAppSide.Server)
            {
                Block block = world.BlockAccessor.GetBlock(blockSel.Position);
                if (block.Code.BeginsWith("game", "leaves-"))
                {
                    world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                    DamageItem(world, byEntity, itemslot);
                    return true;
                } else
                    return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel);
            } else
                return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel);
        }
    }
}
