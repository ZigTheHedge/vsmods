using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace necessaries.src.SharpenerStuff
{
    class Grindstone : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            int slot = blockSel.SelectionBoxIndex;
            if (slot == 0)
            {
                if(world.Side == EnumAppSide.Client)
                {
                    ((ICoreClientAPI)api).Network.SendBlockEntityPacket(blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, 1101, BitConverter.GetBytes(true));
                }
                return true;
            }
            else
            {
                BEGrindstone be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEGrindstone;
                if (be != null)
                {
                    if (!be.OnInteract(byPlayer, blockSel))
                        return base.OnBlockInteractStart(world, byPlayer, blockSel);
                    else
                        return true;
                }
            }
            return false;
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            return true;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStop(secondsUsed, world, byPlayer, blockSel);
            if (world.Side == EnumAppSide.Client)
            {
                ((ICoreClientAPI)api).Network.SendBlockEntityPacket(blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, 1101, BitConverter.GetBytes(false));
            }
        }

        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason)
        {
            if (world.Side == EnumAppSide.Client)
            {
                ((ICoreClientAPI)api).Network.SendBlockEntityPacket(blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, 1101, BitConverter.GetBytes(false));
            }
            return base.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel, cancelReason);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "rock", "none" },
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

    }
}
