using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace fancydoors.src.Workbench
{
    class DoorWorkbench : Block
    {
        public bool buildWorkbench(IWorldAccessor world, BlockPos pos, bool build)
        {
            //Find lower-right corner
            bool north = false;
            bool east = false;
            bool south = false;
            bool west = false;

            string nCode = world.BlockAccessor.GetBlock(pos.NorthCopy()).Code.FirstPathPart();
            string eCode = world.BlockAccessor.GetBlock(pos.EastCopy()).Code.FirstPathPart();
            string sCode = world.BlockAccessor.GetBlock(pos.SouthCopy()).Code.FirstPathPart();
            string wCode = world.BlockAccessor.GetBlock(pos.WestCopy()).Code.FirstPathPart();

            string matchCode = "doorworkbench-pile";
            if (!build) matchCode = "doorworkbench";

            if (nCode.StartsWith(matchCode)) north = true;
            if (eCode.StartsWith(matchCode)) east = true;
            if (sCode.StartsWith(matchCode)) south = true;
            if (wCode.StartsWith(matchCode)) west = true;

            Block empty = world.GetBlock(CodeWithVariant("state", "empty"));
            Block pile = world.GetBlock(CodeWithVariant("state", "pile"));
            Block complete;

            if (north)
            {
                if (east)
                {
                    if (world.BlockAccessor.GetBlock(pos.NorthCopy().EastCopy()).Code.FirstPathPart().StartsWith(matchCode))
                    {
                        if (build)
                        {
                            complete = world.GetBlock(CodeWithVariants(new string[] { "state", "horizontalorientation" }, new string[] { "full", "east" }));
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.NorthCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.EastCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.NorthCopy().EastCopy());
                            world.BlockAccessor.ExchangeBlock(complete.Id, pos);
                        }
                        else
                        {
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.NorthCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.EastCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.NorthCopy().EastCopy());
                        }
                        return true;
                    }
                }
                if (west)
                {
                    if (world.BlockAccessor.GetBlock(pos.NorthCopy().WestCopy()).Code.FirstPathPart().StartsWith(matchCode))
                    {
                        if (build)
                        {
                            complete = world.GetBlock(CodeWithVariants(new string[] { "state", "horizontalorientation" }, new string[] { "full", "north" }));
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.NorthCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.WestCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.NorthCopy().WestCopy());
                            world.BlockAccessor.ExchangeBlock(complete.Id, pos);
                        } else
                        {
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.NorthCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.WestCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.NorthCopy().WestCopy());
                        }
                        return true;
                    }
                }
            }
            if(south)
            {
                if (east)
                {
                    if (world.BlockAccessor.GetBlock(pos.SouthCopy().EastCopy()).Code.FirstPathPart().StartsWith(matchCode))
                    {
                        if (build)
                        {
                            complete = world.GetBlock(CodeWithVariants(new string[] { "state", "horizontalorientation" }, new string[] { "full", "south" }));
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.SouthCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.EastCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.SouthCopy().EastCopy());
                            world.BlockAccessor.ExchangeBlock(complete.Id, pos);
                        }
                        else
                        {
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.SouthCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.EastCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.SouthCopy().EastCopy());
                        }
                        return true;
                    }
                }
                if (west)
                {
                    if (world.BlockAccessor.GetBlock(pos.SouthCopy().WestCopy()).Code.FirstPathPart().StartsWith(matchCode))
                    {
                        if (build)
                        {
                            complete = world.GetBlock(CodeWithVariants(new string[] { "state", "horizontalorientation" }, new string[] { "full", "west" }));
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.SouthCopy().WestCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.WestCopy());
                            world.BlockAccessor.ExchangeBlock(empty.Id, pos.SouthCopy());
                            world.BlockAccessor.ExchangeBlock(complete.Id, pos);
                        }
                        else
                        {
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.SouthCopy().WestCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.WestCopy());
                            world.BlockAccessor.ExchangeBlock(pile.Id, pos.SouthCopy());
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if(slot.Itemstack.Collectible.Code.FirstPathPart().StartsWith("linen"))
            {
                if (buildWorkbench(world, blockSel.Position, true))
                {
                    slot.TakeOut(1);
                }
                return true;
            } else
                return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            if(Variant["state"] != "pile") buildWorkbench(world, pos, false);
            base.OnBlockRemoved(world, pos);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "state", "pile" },
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            if (Variant["state"] != "pile") return Lang.Get("fancydoors:block-doorworkbench-complete-north");
            return base.GetPlacedBlockName(world, pos);
        }

    }
}
