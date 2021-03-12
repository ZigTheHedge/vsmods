using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace zeekea.src
{
    class IcepickItem : Item
    {
        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQtyMultiplier = 1)
        {
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            if (block.Code.FirstPathPart().EndsWith("ice"))
            {
                /*
                if (world.Side == EnumAppSide.Server)
                {
                    world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                    world.BlockAccessor.SetBlock(0, blockSel.Position);
                    DamageItem(world, byEntity, itemslot);
                }
                return true;
                */
                world.SpawnItemEntity(new ItemStack(block, 1), new Vec3d(blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5));
                DamageItem(world, byEntity, itemslot);
                if(!ModConfigFile.Current.icepickMode)
                {
                    world.BlockAccessor.SetBlock(0, blockSel.Position);
                    return true;
                }
                else
                    return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel);
            }
            else
                return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel);
        }
    }
}
