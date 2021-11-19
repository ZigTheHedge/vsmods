using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace zeekea.src.tall_locker
{
    class TallLocker : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BETallLocker be = null;
            if (blockSel.Position != null)
            {
                be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETallLocker;
            }

            bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);

            if (!handled && !byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer, false);
                }

                return true;
            }

            return handled;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockPos upperPart = blockSel.Position.UpCopy();
            if (world.BlockAccessor.GetBlockId(upperPart) != 0) return false;
            
            bool ret = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            Block upperBlock = world.GetBlock(new AssetLocation("zeekea:dummy-up"));
            world.BlockAccessor.SetBlock(upperBlock.BlockId, upperPart);

            return ret;
        }
        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            world.BlockAccessor.SetBlock(0, pos.UpCopy());
            base.OnBlockRemoved(world, pos);
        }

    }
}
