using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace xmastime.src
{
    class StaticUtils
    {
        public static void SetBlockState(IWorldAccessor world, BlockPos pos, string newState, string newValue)
        {
            Block curBlock = world.BlockAccessor.GetBlock(pos);
            Block newBlock = world.GetBlock(curBlock.CodeWithVariant(newState, newValue));
            world.BlockAccessor.ExchangeBlock(newBlock.BlockId, pos);
        }
    }
}
