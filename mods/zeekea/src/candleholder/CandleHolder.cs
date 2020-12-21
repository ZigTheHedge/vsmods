using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace zeekea.src.candleholder
{
    class CandleHolder : Block
    {
        public void SetBlockState(string state, BlockPos pos)
        {
            AssetLocation loc = CodeWithVariant("candle", state);
            Block block = api.World.GetBlock(loc);
            if (block == null) return;

            api.World.BlockAccessor.ExchangeBlock(block.Id, pos);
        }

        public override void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            if(Variant["candle"] == "unlit")
            {
                SetBlockState("lit", pos);
            } else
                base.OnTryIgniteBlockOver(byEntity, pos, secondsIgniting, ref handling);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.Side == EnumAppSide.Server)
            {
                ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if (slot.Empty)
                {
                    if (Variant["candle"] != "no")
                    {
                        ItemStack candle = new ItemStack(api.World.GetItem(new AssetLocation("game", "candle")));
                        byPlayer.InventoryManager.TryGiveItemstack(candle);
                        slot.MarkDirty();
                        SetBlockState("no", blockSel.Position);
                    }
                    else
                        return false;
                }
                else
                {
                    if (slot.Itemstack.Collectible.Code.FirstPathPart().StartsWith("candle"))
                    {
                        if (Variant["candle"] == "no")
                        {
                            SetBlockState("unlit", blockSel.Position);
                            slot.TakeOut(1);
                            slot.MarkDirty();
                        }
                        else
                        {
                            ItemStack candle = new ItemStack(api.World.GetItem(new AssetLocation("game", "candle")));
                            if (!byPlayer.InventoryManager.TryGiveItemstack(candle))
                            {
                                api.World.SpawnItemEntity(candle, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                            }
                            SetBlockState("no", blockSel.Position);
                            slot.MarkDirty();
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool ShouldReceiveClientParticleTicks(IWorldAccessor world, IPlayer player, BlockPos pos, out bool isWindAffected)
        {
            base.ShouldReceiveClientParticleTicks(world, player, pos, out _);
            isWindAffected = true;

            return true;
        }

        public override void OnAsyncClientParticleTick(IAsyncParticleManager manager, BlockPos pos, float windAffectednessAtPos, float secondsTicking)
        {
            if (Variant["candle"] == "lit")
            {
                if (ParticleProperties != null && ParticleProperties.Length > 0)
                {
                    for (int i = 0; i < ParticleProperties.Length; i++)
                    {
                        AdvancedParticleProperties bps = ParticleProperties[i];
                        bps.WindAffectednesAtPos = windAffectednessAtPos;

                        if (Variant["horizontalorientation"] == "north")
                        {
                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 0.95F;
                            bps.basePos.Z = pos.Z + 0.21F;
                        } else if(Variant["horizontalorientation"] == "south")
                        {
                            bps.basePos.X = pos.X + 0.5F;
                            bps.basePos.Y = pos.Y + 0.95F;
                            bps.basePos.Z = pos.Z + 0.79F;
                        }
                        else if (Variant["horizontalorientation"] == "east")
                        {
                            bps.basePos.X = pos.X + 0.79F;
                            bps.basePos.Y = pos.Y + 0.95F;
                            bps.basePos.Z = pos.Z + 0.5F;
                        }
                        else
                        {
                            bps.basePos.X = pos.X + 0.21F;
                            bps.basePos.Y = pos.Y + 0.95F;
                            bps.basePos.Z = pos.Z + 0.5F;
                        }
                        manager.Spawn(bps);
                    }
                }
            }
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "candle", "no" },
                    { "metal", Variant["metal"] },
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if(world.Side == EnumAppSide.Server)
            {
                if(Variant["candle"] == "no")
                {
                    return new ItemStack[] { OnPickBlock(world, pos) };
                }
                else
                    return new ItemStack[] { OnPickBlock(world, pos), new ItemStack(world.GetItem(new AssetLocation("game", "candle"))) };
            }
            return new ItemStack[] { OnPickBlock(world, pos) };
        }
    }
}
