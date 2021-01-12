using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using zeekea.src.multirotatable;

namespace zeekea.src.orebox
{
    class OreBox : Block
    {
        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool ok = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (!ok) return false;

            BEOreBox be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEOreBox;
            if (be != null)
            {
                BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                double dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                float angleHor = (float)Math.Atan2(dx, dz);

                float deg22dot5rad = GameMath.PIHALF / 4;
                float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
                be.SetMeshAngle(roundRad);
            }

            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEOreBox be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEOreBox;
            if (be != null)
            {
                return be.OnBlockInteract(byPlayer);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
