using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace theneighbours.src.Blocks
{
    class GlowShroom : Block
    {
        public virtual bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block block = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
            return block.Fertility > 0;
        }

        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            if (!CanPlantStay(blockAccessor, pos)) return false;
            return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldGenRand);
        }

        public override bool ShouldReceiveClientParticleTicks(IWorldAccessor world, IPlayer player, BlockPos pos, out bool isWindAffected)
        {
            isWindAffected = false;
            return true;
        }

        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
        {
            if (isImpact && facing.Axis == EnumAxis.Y)
            {
                if (Sounds?.Break != null && System.Math.Abs(collideSpeed.Y) > 0.2)
                {
                    world.PlaySoundAt(Sounds.Break, entity.Pos.X, entity.Pos.Y, entity.Pos.Z);
                }
                entity.Pos.Motion.Y = GameMath.Clamp(-entity.Pos.Motion.Y * 0.8, -0.5, 0.5);
            }
        }
    }
}
