using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace zeekea.src.multirotatable
{
    class BlockMultiRotatable : Block
    {
        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool ok = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (!ok) return false;

            BEMultiRotatable be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMultiRotatable;
            if (be != null)
            {
                BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                double dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                float angleHor = (float)Math.Atan2(dx, dz);

                float deg22dot5rad = GameMath.PIHALF / 4;
                float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
                be.meshAngle = roundRad;
            }

            return true;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            bool ret = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            if (Attributes != null && Attributes["tall"].AsBool(false))
            {
                BlockPos upperPart = blockSel.Position.UpCopy();
                if (world.BlockAccessor.GetBlockId(upperPart) != 0) return false;
                Block upperBlock = world.GetBlock(new AssetLocation("zeekea:dummy-up"));
                world.BlockAccessor.SetBlock(upperBlock.BlockId, upperPart);
            }
            return ret;
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            if (Attributes != null && Attributes["tall"].AsBool(false))
            {
                world.BlockAccessor.SetBlock(0, pos.UpCopy());
            }
            base.OnBlockRemoved(world, pos);
        }
    }
}
