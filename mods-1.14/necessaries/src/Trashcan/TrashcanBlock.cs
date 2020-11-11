using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace necessaries.src.Trashcan
{
    class TrashcanBlock : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BETrashcan be = null;
            if (blockSel.Position != null)
            {
                be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETrashcan;
            }

            bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);

            if (!handled && !byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer);
                }

                return true;
            }

            return handled;
        }
    }
}
