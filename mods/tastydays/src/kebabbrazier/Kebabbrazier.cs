using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace tastydays.src.kebabbrazier
{
    class Kebabbrazier : Block
    {
        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEKebabbrazier BE = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEKebabbrazier;
            if (BE != null) return BE.OnInteract(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            BEKebabbrazier BE = byEntity.World.BlockAccessor.GetBlockEntity(pos) as BEKebabbrazier;
            if (BE != null)
            {
                if (Variant["coalstate"] == "coal")
                {
                    BE.SetCoalState("burningcoal");
                } else
                {
                    base.OnTryIgniteBlockOver(byEntity, pos, secondsIgniting, ref handling);
                }
            } else
                base.OnTryIgniteBlockOver(byEntity, pos, secondsIgniting, ref handling);
        }

        public override bool ShouldReceiveClientParticleTicks(IWorldAccessor world, IPlayer player, BlockPos pos, out bool isWindAffected)
        {
            base.ShouldReceiveClientParticleTicks(world, player, pos, out _);
            isWindAffected = true;

            return true;
        }

        public override void OnAsyncClientParticleTick(IAsyncParticleManager manager, BlockPos pos, float windAffectednessAtPos, float secondsTicking)
        {
            if (Variant["coalstate"] == "burningcoal")
            {
                if (ParticleProperties != null && ParticleProperties.Length > 0)
                {
                    for (int i = 0; i < ParticleProperties.Length; i++)
                    {
                        AdvancedParticleProperties bps = ParticleProperties[i];
                        bps.WindAffectednesAtPos = windAffectednessAtPos;

                        if (Variant["horizontalorientation"] == "north")
                        {
                            
                            bps.basePos.X = pos.X + 0.2F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.8F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);

                        }
                        else if (Variant["horizontalorientation"] == "south")
                        {
                            bps.basePos.X = pos.X + 0.2F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.8F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);
                        }
                        else if (Variant["horizontalorientation"] == "east")
                        {
                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.2F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.8F;
                            manager.Spawn(bps);

                        }
                        else
                        {
                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.2F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.5F;
                            manager.Spawn(bps);

                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 1.2F;
                            bps.basePos.Z = pos.Z + 0.8F;
                            manager.Spawn(bps);
                        }
                    }
                }
            }
            base.OnAsyncClientParticleTick(manager, pos, windAffectednessAtPos, secondsTicking);
        }


    }
}
