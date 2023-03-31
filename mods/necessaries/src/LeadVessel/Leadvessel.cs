using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace necessaries.src.LeadVessel
{
    class Leadvessel : Block, IIgnitable
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BELeadvessel be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELeadvessel;
            if (be != null)
            {
                ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
                if (!activeSlot.Empty)
                {
                    if (activeSlot.Itemstack.Class == EnumItemClass.Block)
                    {
                        if (activeSlot.Itemstack.Block is BlockLiquidContainerBase)
                        {
                            BlockLiquidContainerBase container = activeSlot.Itemstack.Block as BlockLiquidContainerBase;
                            ItemStack contents = container.GetContent(activeSlot.Itemstack);
                            if (contents != null)
                            {
                                if (contents.Class == EnumItemClass.Item)
                                {
                                    if (contents.Item.FirstCodePart().StartsWith("waterportion"))
                                    {
                                        if (contents.StackSize >= 5)
                                        {

                                            if (be.OnInteract(contents, byPlayer))
                                            {

                                                contents.StackSize -= 5;
                                                if (contents.StackSize == 0)
                                                    contents = null;

                                            }
                                            container.SetContent(activeSlot.Itemstack, contents);
                                            activeSlot.MarkDirty();
                                        }
                                        else
                                            return false;
                                    }
                                    else
                                        return false;
                                }
                            }
                            else
                                return false;
                        }
                        else if (activeSlot.Itemstack.Block.Code.FirstPathPart().StartsWith("leadvessel_cover"))
                        {
                            if (be.OnInteract(activeSlot.Itemstack, byPlayer))
                                activeSlot.Itemstack = null;
                            activeSlot.MarkDirty();
                        }
                        else
                            return false;
                    } else
                    {
                        if (activeSlot.Itemstack.Item.Code.FirstPathPart().StartsWith("sulfurpeter"))
                        {
                            if (be.OnInteract(activeSlot.Itemstack, byPlayer))
                            {
                                activeSlot.Itemstack.StackSize--;
                                if(activeSlot.Itemstack.StackSize == 0)
                                    activeSlot.Itemstack = null;
                            }
                            activeSlot.MarkDirty();
                        }
                        else if (activeSlot.Itemstack.Item.Code.FirstPathPart().StartsWith("bonemeal"))
                        {
                            if (be.OnInteract(activeSlot.Itemstack, byPlayer))
                            {
                                activeSlot.Itemstack.StackSize--;
                                if (activeSlot.Itemstack.StackSize == 0)
                                    activeSlot.Itemstack = null;
                            }
                            activeSlot.MarkDirty();
                        }
                        else

                            return false;
                    }
                } else
                {
                    if (!be.OnInteract(activeSlot.Itemstack, byPlayer))
                        return false;
                }
            } else
                return false;
            return true;
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            Block block = world.GetBlock(new AssetLocation("necessaries:leadvessel-empty"));

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (Variant["contents"] == "sulfur" || Variant["contents"] == "watersulfur")
                return new ItemStack[] { OnPickBlock(world, pos), new ItemStack(world.GetItem(new AssetLocation("necessaries:sulfurpeter"))) };
            else if (Variant["contents"] == "full")
            {
                BELeadvessel be = world.BlockAccessor.GetBlockEntity(pos) as BELeadvessel;
                if (be != null)
                {
                    if((be.locals == LeadVesselContents.SULFUR || be.locals == LeadVesselContents.WATERSULFUR) && be.burnTime == 0)
                        return new ItemStack[] { OnPickBlock(world, pos), new ItemStack(world.GetItem(new AssetLocation("necessaries:sulfurpeter"))) };
                    else
                        return new ItemStack[] { OnPickBlock(world, pos), new ItemStack(world.GetBlock(new AssetLocation("necessaries:leadvessel_cover"))) };
                }
            }
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

        public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            BELeadvessel be = byEntity.World.BlockAccessor.GetBlockEntity(pos) as BELeadvessel;
            if (be != null)
            {
                if (be.burnTime == 0 && be.locals == LeadVesselContents.WATERSULFUR)
                {
                    be.burnTime = byEntity.World.Calendar.TotalHours + 0.5f;
                    be.MarkDirty(true);
                }
            }
        }

        public EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
        {
            BELeadvessel be = api.World.BlockAccessor.GetBlockEntity(pos) as BELeadvessel;
            if (be == null || be.locals != LeadVesselContents.WATERSULFUR) return EnumIgniteState.NotIgnitable;
            return EnumIgniteState.Ignitable;
        }

        public override bool ShouldReceiveClientParticleTicks(IWorldAccessor world, IPlayer player, BlockPos pos, out bool isWindAffected)
        {
            base.ShouldReceiveClientParticleTicks(world, player, pos, out _);
            isWindAffected = true;

            BELeadvessel be = world.BlockAccessor.GetBlockEntity(pos) as BELeadvessel;
            if (be != null)
            {
                if (be.burnTime > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public override void OnAsyncClientParticleTick(IAsyncParticleManager manager, BlockPos pos, float windAffectednessAtPos, float secondsTicking)
        {
            if (Variant["contents"] == "watersulfur")
            {
                if (ParticleProperties != null && ParticleProperties.Length > 0)
                {
                    for (int i = 0; i < ParticleProperties.Length; i++)
                    {
                        AdvancedParticleProperties bps = ParticleProperties[i];
                        bps.WindAffectednesAtPos = windAffectednessAtPos;

                        bps.basePos.X = pos.X + 0.5F;
                        bps.basePos.Y = pos.Y + 1F;
                        bps.basePos.Z = pos.Z + 0.5F;

                        manager.Spawn(bps);
                    }
                }
            }
        }
    }
}
