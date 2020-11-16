using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace zeekea.src.curtains
{
    class Curtains : Block
    {
        public Block GetBlockVariant(IWorldAccessor world, BlockPos pos, string orientation)
        {
            AssetLocation blockCode;

            string topside = "";
            string side = "";
            string state = "";

            string upBlock = world.BlockAccessor.GetBlock(pos.UpCopy()).Code.FirstPathPart();
            if (upBlock.StartsWith("rod")) topside = "yes"; else topside = "no";

            string rightBlock;
            if (orientation == "north") rightBlock = world.BlockAccessor.GetBlock(pos.EastCopy()).Code.FirstPathPart();
            else if (orientation == "east") rightBlock = world.BlockAccessor.GetBlock(pos.SouthCopy()).Code.FirstPathPart();
            else if (orientation == "south") rightBlock = world.BlockAccessor.GetBlock(pos.WestCopy()).Code.FirstPathPart();
            else rightBlock = world.BlockAccessor.GetBlock(pos.NorthCopy()).Code.FirstPathPart();

            if (rightBlock.StartsWith("curtain"))
            {
                side = "left";
                if (orientation == "north") state = world.BlockAccessor.GetBlock(pos.EastCopy()).Variant["state"];
                else if(orientation == "east") state = world.BlockAccessor.GetBlock(pos.SouthCopy()).Variant["state"];
                else if(orientation == "south") state = world.BlockAccessor.GetBlock(pos.WestCopy()).Variant["state"];
                else state = world.BlockAccessor.GetBlock(pos.NorthCopy()).Variant["state"];
            }
            else
            {
                if (topside == "yes")
                {
                    string leftBlock;
                    if (orientation == "north") leftBlock = world.BlockAccessor.GetBlock(pos.UpCopy().WestCopy()).Code.FirstPathPart();
                    else if (orientation == "east") leftBlock = world.BlockAccessor.GetBlock(pos.UpCopy().NorthCopy()).Code.FirstPathPart();
                    else if (orientation == "south") leftBlock = world.BlockAccessor.GetBlock(pos.UpCopy().EastCopy()).Code.FirstPathPart();
                    else leftBlock = world.BlockAccessor.GetBlock(pos.UpCopy().SouthCopy()).Code.FirstPathPart();
                    if (!leftBlock.StartsWith("rod")) side = "left"; else side = "right";
                    state = "shut";
                }
                else
                {
                    state = world.BlockAccessor.GetBlock(pos.UpCopy()).Variant["state"];
                    side = world.BlockAccessor.GetBlock(pos.UpCopy()).Variant["side"];
                }
            }


            blockCode = CodeWithVariants(new Dictionary<string, string>() {
                                { "color", Variant["color"] },
                                { "topside", topside },
                                { "side", side },
                                { "state", state },
                                { "horizontalorientation", orientation }
                            });

            return world.GetBlock(blockCode);
        }

        public int CountCurtainWidth(IWorldAccessor world, BlockPos leftmostFount, BlockPos rightmostFound)
        {
            string orientation = Variant["horizontalorientation"];

            int CloseCount = 0;
            if(orientation == "north" || orientation == "south")
            {
                CloseCount = Math.Abs(rightmostFound.X - leftmostFount.X);
            } else
            {
                CloseCount = Math.Abs(rightmostFound.Z - leftmostFount.Z);
            }

            return CloseCount;
        }

        public BlockPos GetLeftmostBlock(IWorldAccessor world, BlockPos pos)
        {
            string orientation = Variant["horizontalorientation"];
            string originalSide = world.BlockAccessor.GetBlock(pos).Variant["side"];

            BlockPos leftmostBlock = pos.Copy();
            while (true)
            {
                if (orientation == "north")
                {
                    if (!world.BlockAccessor.GetBlock(leftmostBlock.WestCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(leftmostBlock.WestCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            leftmostBlock.West();
                        else
                            break;
                    }
                }
                else if (orientation == "east")
                {
                    if (!world.BlockAccessor.GetBlock(leftmostBlock.NorthCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(leftmostBlock.NorthCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            leftmostBlock.North();
                        else
                            break;
                    }
                }
                else if (orientation == "south")
                {
                    if (!world.BlockAccessor.GetBlock(leftmostBlock.EastCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(leftmostBlock.EastCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            leftmostBlock.East();
                        else
                            break;
                    }
                }
                else
                {
                    if (!world.BlockAccessor.GetBlock(leftmostBlock.SouthCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(leftmostBlock.SouthCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            leftmostBlock.South();
                        else
                            break;
                    }
                }
            }
            return leftmostBlock;
        }

        public BlockPos GetRightmostBlock(IWorldAccessor world, BlockPos pos)
        {
            string orientation = Variant["horizontalorientation"];
            string originalSide = world.BlockAccessor.GetBlock(pos).Variant["side"];

            BlockPos rightmostBlock = pos.Copy();
            while (true)
            {
                if (orientation == "north")
                {
                    if (!world.BlockAccessor.GetBlock(rightmostBlock.EastCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(rightmostBlock.EastCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            rightmostBlock.East();
                        else
                            break;
                    }
                }
                else if (orientation == "east")
                {
                    if (!world.BlockAccessor.GetBlock(rightmostBlock.SouthCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(rightmostBlock.SouthCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            rightmostBlock.South();
                        else
                            break;
                    }
                }
                else if (orientation == "south")
                {
                    if (!world.BlockAccessor.GetBlock(rightmostBlock.WestCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(rightmostBlock.WestCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            rightmostBlock.West();
                        else
                            break;
                    }
                }
                else
                {
                    if (!world.BlockAccessor.GetBlock(rightmostBlock.NorthCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                    else
                    {
                        string varSide = world.BlockAccessor.GetBlock(rightmostBlock.NorthCopy()).Variant["side"];
                        if (varSide == originalSide || (originalSide == "bothleft" && varSide == "left") || (originalSide == "left" && varSide == "bothleft") || (originalSide == "bothright" && varSide == "right") || (originalSide == "right" && varSide == "bothright") || originalSide == "both" || varSide == "both")
                            rightmostBlock.North();
                        else
                            break;
                    }
                }
            }
            return rightmostBlock;
        }

        public void OpenCurtain(IWorldAccessor world, BlockPos pos)
        {
            string orientation = Variant["horizontalorientation"];
            string originalSide = world.BlockAccessor.GetBlock(pos).Variant["side"];

            //Find vertical bounds of a curtain
            BlockPos topmostBlock = pos.Copy();
            while (topmostBlock.Y <= world.BlockAccessor.MapSizeY)
            {
                if (!world.BlockAccessor.GetBlock(topmostBlock.UpCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                topmostBlock.Up();
            }
            BlockPos bottommostBlock = pos.Copy();
            while (bottommostBlock.Y > 0)
            {
                if (!world.BlockAccessor.GetBlock(bottommostBlock.DownCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                bottommostBlock.Down();
            }

            //Find horizontal bounds of a curtain
            BlockPos leftmostBlock = GetLeftmostBlock(world, pos);
            BlockPos rightmostBlock = GetRightmostBlock(world, pos);


            BlockPos blockPos = topmostBlock.Copy();
            BlockPos fillBlockPos = blockPos.Copy();

            if (originalSide == "left" || originalSide == "bothleft")
            {
                if (orientation == "north") { fillBlockPos.X = rightmostBlock.X; blockPos.X = leftmostBlock.X; }
                else if (orientation == "east") { fillBlockPos.Z = rightmostBlock.Z; blockPos.Z = leftmostBlock.Z; }
                else if (orientation == "south") { fillBlockPos.X = rightmostBlock.X; blockPos.X = leftmostBlock.X; }
                else { fillBlockPos.Z = rightmostBlock.Z; blockPos.Z = leftmostBlock.Z; }
            }
            if (originalSide == "right" || originalSide == "bothright")
            {
                if (orientation == "north") { fillBlockPos.X = leftmostBlock.X; blockPos.X = rightmostBlock.X; }
                else if (orientation == "east") { fillBlockPos.Z = leftmostBlock.Z; blockPos.Z = rightmostBlock.Z; }
                else if (orientation == "south") { fillBlockPos.X = leftmostBlock.X; blockPos.X = rightmostBlock.X; }
                else { fillBlockPos.Z = leftmostBlock.Z; blockPos.Z = rightmostBlock.Z; }
            }

            int cWidth = CountCurtainWidth(world, leftmostBlock, rightmostBlock);

            for (int i = 0; i <= topmostBlock.Y - bottommostBlock.Y; i++)
            {
                if (cWidth > 0)
                {
                    BlockPos localBlockPos = fillBlockPos.Copy();
                    if (originalSide == "left" || originalSide == "bothleft")
                    {
                        for (int j = 0; j < cWidth; j++)
                        {
                            if(world.BlockAccessor.GetBlock(localBlockPos).Variant["side"] == "both")
                            {
                                AssetLocation middleBlock;

                                if (originalSide == "right")
                                    middleBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants( new string[] { "side", "topside" }, new string[] { "bothleft", "no" });
                                else
                                    middleBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants( new string[] { "side", "topside" }, new string[] { "bothright", "no" });

                                world.BlockAccessor.ExchangeBlock(world.GetBlock(middleBlock).Id, localBlockPos);
                            } else
                                world.BlockAccessor.SetBlock(0, localBlockPos);

                            if (orientation == "north") localBlockPos.West();
                            else if (orientation == "east") localBlockPos.North();
                            else if (orientation == "south") localBlockPos.East();
                            else localBlockPos.South();

                        }
                    }
                    else
                    {
                        for (int j = 0; j < cWidth; j++)
                        {
                            if (world.BlockAccessor.GetBlock(localBlockPos).Variant["side"] == "both")
                            {
                                AssetLocation middleBlock;

                                if (originalSide == "right")
                                    middleBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants(new string[] { "side", "topside" }, new string[] { "bothleft", "no" });
                                else
                                    middleBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants(new string[] { "side", "topside" }, new string[] { "bothright", "no" });

                                world.BlockAccessor.ExchangeBlock(world.GetBlock(middleBlock).Id, localBlockPos);
                            }
                            else
                                world.BlockAccessor.SetBlock(0, localBlockPos);

                            if (orientation == "north") localBlockPos.East();
                            else if (orientation == "east") localBlockPos.South();
                            else if (orientation == "south") localBlockPos.West();
                            else localBlockPos.North();
                        }
                    }
                }

                AssetLocation newBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariant("state", "open");
                world.BlockAccessor.ExchangeBlock(world.GetBlock(newBlock).Id, blockPos);
                blockPos.Down();
                fillBlockPos.Down();
            }
        }

        public int CountCurtainSpace(IWorldAccessor world, BlockPos pos)
        {
            string orientation = Variant["horizontalorientation"];

            BlockPos rightPos = pos.Copy();
            int CloseCount = 0;
            
            if (Variant["side"] == "left")
            {
                while (true)
                {
                    if (orientation == "north")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().EastCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.East();
                        }
                        else
                            break;
                    }
                    if (orientation == "east")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().SouthCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.South();
                        }
                        else
                            break;
                    }
                    if (orientation == "south")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().WestCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.West();
                        }
                        else
                            break;
                    }
                    if (orientation == "west")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().NorthCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.North();
                        }
                        else
                            break;
                    }
                }
            } else
            {
                while (true)
                {
                    if (orientation == "north")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().WestCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.West();
                        }
                        else
                            break;
                    }
                    if (orientation == "east")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().NorthCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.North();
                        }
                        else
                            break;
                    }
                    if (orientation == "south")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().EastCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.East();
                        }
                        else
                            break;
                    }
                    if (orientation == "west")
                    {
                        string upSupport = world.BlockAccessor.GetBlock(rightPos.UpCopy().SouthCopy()).Code.FirstPathPart();
                        if (upSupport.StartsWith("rod") || upSupport.StartsWith("curtain"))
                        {
                            CloseCount++;
                            rightPos.South();
                        }
                        else
                            break;
                    }
                }
            }

            return CloseCount - 1;
        }

        public void ShutCurtain(IWorldAccessor world, BlockPos pos)
        {
            //Find vertical bounds of a curtain
            BlockPos topmostBlock = pos.Copy();
            while (topmostBlock.Y <= world.BlockAccessor.MapSizeY)
            {
                if (!world.BlockAccessor.GetBlock(topmostBlock.UpCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                topmostBlock.Up();
            }
            BlockPos bottommostBlock = pos.Copy();
            while (bottommostBlock.Y > 0)
            {
                if (!world.BlockAccessor.GetBlock(bottommostBlock.DownCopy()).Code.FirstPathPart().StartsWith("curtain")) break;
                bottommostBlock.Down();
            }

            BlockPos blockPos = topmostBlock.Copy();
            string orientation = Variant["horizontalorientation"];

            int cWidth = CountCurtainSpace(world, blockPos);
            bool isOdd = (cWidth % 2 == 0) ? false : true;
            int hcWidth = (int)(cWidth / 2);

            for (int i = 0; i <= topmostBlock.Y - bottommostBlock.Y; i++)
            {
                if(cWidth > 0)
                {
                    BlockPos fillBlockPos = blockPos.Copy();
                    if(Variant["side"] == "left")
                    {
                        for(int j = 0; j < hcWidth; j++)
                        {
                            if (orientation == "north") fillBlockPos.East();
                            else if (orientation == "east") fillBlockPos.South();
                            else if (orientation == "south") fillBlockPos.West();
                            else fillBlockPos.North();

                            AssetLocation fillBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants(new string[] { "state", "topside" }, new string[] { "shut", "no" });
                            world.BlockAccessor.SetBlock(world.GetBlock(fillBlock).Id, fillBlockPos);
                        }
                    } else
                    {
                        for (int j = 0; j < hcWidth; j++)
                        {
                            if (orientation == "north") fillBlockPos.West();
                            else if (orientation == "east") fillBlockPos.North();
                            else if (orientation == "south") fillBlockPos.East();
                            else fillBlockPos.South();

                            AssetLocation fillBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants(new string[] { "state", "topside" }, new string[] { "shut", "no" });
                            world.BlockAccessor.SetBlock(world.GetBlock(fillBlock).Id, fillBlockPos);
                        }
                    }
                    if(isOdd)
                    {
                        if (Variant["side"] == "left")
                        {
                            if (orientation == "north") fillBlockPos.East();
                            else if (orientation == "east") fillBlockPos.South();
                            else if (orientation == "south") fillBlockPos.West();
                            else fillBlockPos.North();
                        }
                        else
                        {
                            if (orientation == "north") fillBlockPos.West();
                            else if (orientation == "east") fillBlockPos.North();
                            else if (orientation == "south") fillBlockPos.East();
                            else fillBlockPos.South();
                        }
                        string biqCenter = world.BlockAccessor.GetBlock(fillBlockPos).Code.FirstPathPart();
                        if (biqCenter.StartsWith("air"))
                        {
                            AssetLocation fillBlock;
                            if (Variant["side"] == "left")
                            {
                                fillBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants(new string[] { "state", "side", "topside" }, new string[] { "shut", "bothleft", "no" });
                            } else
                            {
                                fillBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants(new string[] { "state", "side", "topside" }, new string[] { "shut", "bothright", "no" });
                            }
                            world.BlockAccessor.SetBlock(world.GetBlock(fillBlock).Id, fillBlockPos);
                        } else if(biqCenter.StartsWith("curtain"))
                        {
                            AssetLocation fillBlock;
                            fillBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariants(new string[] { "state", "side", "topside" }, new string[] { "shut", "both", "no" });
                            world.BlockAccessor.ExchangeBlock(world.GetBlock(fillBlock).Id, fillBlockPos);
                        }
                    }
                }

                AssetLocation newBlock = world.BlockAccessor.GetBlock(blockPos).CodeWithVariant("state", "shut");
                world.BlockAccessor.ExchangeBlock(world.GetBlock(newBlock).Id, blockPos);
                blockPos.Down();
            }
        }


        public void InteractWithCurtains(IWorldAccessor world, BlockPos pos)
        {
            string state = (Variant["state"] == "open") ? "shut" : "open";
            if (state == "shut") ShutCurtain(world, pos);
            else
            {
                if(Variant["side"] == "both")
                {
                    string orientation = Variant["horizontalorientation"];

                    if (orientation == "north" || orientation == "south")
                    {
                        OpenCurtain(world, pos.WestCopy());
                        OpenCurtain(world, pos.EastCopy());
                    } else
                    {
                        OpenCurtain(world, pos.NorthCopy());
                        OpenCurtain(world, pos.SouthCopy());
                    }

                } else
                    OpenCurtain(world, pos);
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            //return base.OnBlockInteractStart(world, byPlayer, blockSel);
            InteractWithCurtains(world, blockSel.Position);
            return true;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);

            string topBlock = world.BlockAccessor.GetBlock(blockSel.Position.UpCopy()).Code.FirstPathPart();
            if (!topBlock.StartsWith("rod") && !topBlock.StartsWith("curtain")) return false;

            Block newBlock = GetBlockVariant(world, blockSel.Position, horVer[0].Code);

            newBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);

            return true;
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (Variant["state"] == "shut") 
            {
                if (Variant["side"] == "both")
                {
                    string orientation = Variant["horizontalorientation"];

                    if (orientation == "north" || orientation == "south")
                    {
                        OpenCurtain(world, pos.WestCopy());
                        OpenCurtain(world, pos.EastCopy());
                    }
                    else
                    {
                        OpenCurtain(world, pos.NorthCopy());
                        OpenCurtain(world, pos.SouthCopy());
                    }

                }
                else
                    OpenCurtain(world, pos);
            } else
                base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if (world.Side == EnumAppSide.Server)
            {
                string topBlock = world.BlockAccessor.GetBlock(pos.UpCopy()).Code.FirstPathPart();
                if (!topBlock.StartsWith("rod") && !topBlock.StartsWith("curtain"))
                {
                    world.BlockAccessor.BreakBlock(pos, null);
                }
            }
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "topside", "no" },
                    { "color", Variant["color"] },
                    { "side", "left" },
                    { "state", "shut" },
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            dsc.AppendLine("\n" + Lang.Get("zeekea:curtain-help"));
        }
    }
}
