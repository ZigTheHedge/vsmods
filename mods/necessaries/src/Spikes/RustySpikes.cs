using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace necessaries.src.Spikes
{
    class RustySpikes : Block
    {
        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
        {
            if (world.Side == EnumAppSide.Server)
            {
                if (isImpact && facing.Axis == EnumAxis.Y)
                {
                    float dmg = (float)Math.Abs(collideSpeed.Y) * 30;
                    if(dmg > 0.4F)
                        entity.ReceiveDamage(new DamageSource() { Type = EnumDamageType.PiercingAttack, Source = EnumDamageSource.Block, SourceBlock = this, SourcePos = pos.ToVec3d() }, dmg);
                }
            }
        }
    }
}
