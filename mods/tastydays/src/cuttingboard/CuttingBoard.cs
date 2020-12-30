using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace tastydays.src.cuttingboard
{
    class CuttingBoard : Block
    {
        public override float OnGettingBroken(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
        {
            if(itemslot != null)
            {
                if(itemslot.Itemstack != null)
                {
                    if(itemslot.Itemstack.Collectible.FirstCodePart().StartsWith("cleaver"))
                    {
                        if(counter % 10 == 0)
                            TastyDays.clientChannel.SendPacket(new CuttingBoardHit(blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z));
                    }
                }
            }
            return remainingResistance;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BECuttingBoard be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BECuttingBoard;
            if (be != null)
            {
                if (!be.OnInteract(byPlayer, blockSel))
                    return base.OnBlockInteractStart(world, byPlayer, blockSel);
                else
                    return true;
            }

            return false;
        }
    }
}
