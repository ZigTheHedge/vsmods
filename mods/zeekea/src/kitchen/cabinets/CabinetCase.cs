using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace zeekea.src.kitchen.cabinets
{
    class CabinetCase : Block
    {
        public void SetBlockState(string state, BlockPos pos)
        {
            AssetLocation loc = new AssetLocation("zeekea:" + state);
            Block block = api.World.GetBlock(loc);
            if (block == null) return;

            api.World.BlockAccessor.ExchangeBlock(block.Id, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.Side == EnumAppSide.Server)
            {
                ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if (!slot.Empty)
                {
                    if (slot.Itemstack.Collectible.Code.FirstPathPart().StartsWith("cabinetd"))
                    {
                        Item part = slot.Itemstack.Collectible as Item;
                        string type = (part.Code.FirstPathPart().StartsWith("cabinetdoors")) ? "doors" : "drawers";
                        if (this.Code.FirstPathPart().StartsWith("cabinetbottomcase"))
                        {
                            //Bottom cabinet
                            if (Variant["tabletop"] == "none")
                            {
                                //Place tabletop first
                                (byPlayer as IServerPlayer).SendIngameError("tabletopfirst", "You need to place Tabletop first.");
                                return false;
                            }
                            else
                            {
                                SetBlockState("cabinetbottom-" + Variant["tabletop"] + "-" + part.Variant["wood"] + "-" + Variant["internals"] + "-" + type + "-" + Variant["horizontalorientation"], blockSel.Position);
                                world.BlockAccessor.SpawnBlockEntity("becabinets", blockSel.Position);
                                slot.TakeOut(1);
                                slot.MarkDirty();
                                return true;
                            }
                        }
                        if (this.Code.FirstPathPart().StartsWith("cabinettopcase"))
                        {
                            if (Variant["type"].Equals(type))
                            {
                                SetBlockState("cabinettop-" + part.Variant["wood"] + "-" + Variant["internals"] + "-" + type + "-" + Variant["horizontalorientation"], blockSel.Position);
                                world.BlockAccessor.SpawnBlockEntity("becabinets", blockSel.Position);
                                slot.TakeOut(1);
                                slot.MarkDirty();
                                return true;
                            }
                            else
                            {
                                //Drawers to drawers. Doors to doors. Dust to dust...
                                ICoreServerAPI cApi = world.Api as ICoreServerAPI;
                                if (type.Equals("doors")) (byPlayer as IServerPlayer).SendIngameError("drawerstodrawers", "You can only install Drawers on this cabinet");
                                if (type.Equals("drawers")) (byPlayer as IServerPlayer).SendIngameError("doorstodoors", "You can only install Doors on this cabinet");
                                return false;
                            }
                        }
                    }
                    else if (slot.Itemstack.Collectible.Code.FirstPathPart().StartsWith("polishedrockslab"))
                    {
                        if (this.Code.FirstPathPart().StartsWith("cabinetbottomcase"))
                        {
                            Block part = slot.Itemstack.Collectible as Block;
                            if (part.Variant["rock"].Equals("whitemarble") || part.Variant["rock"].Equals("greenmarble") || part.Variant["rock"].Equals("redmarble") || part.Variant["rock"].Equals("suevite") || part.Variant["rock"].Equals("kimberlite"))
                                return false;
                            if (Variant["tabletop"] == "none")
                            {
                                SetBlockState("cabinetbottomcase-" + Variant["internals"] + "-" + part.Variant["rock"] + "-" + Variant["horizontalorientation"], blockSel.Position);
                                slot.TakeOut(1);
                                slot.MarkDirty();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            ItemStack[] drops;
            if (this.FirstCodePart().Equals("cabinetbottomcase"))
            {
                if (Variant["tabletop"].Equals("none"))
                {
                    drops = new ItemStack[1];
                    drops[0] = new ItemStack(world.Api.World.GetBlock(new AssetLocation("zeekea:cabinetbottomcase-" + Variant["internals"] + "-none-north")));
                }
                else
                {
                    drops = new ItemStack[2];
                    drops[0] = new ItemStack(world.Api.World.GetBlock(new AssetLocation("zeekea:cabinetbottomcase-" + Variant["internals"] + "-none-north")));
                    drops[1] = new ItemStack(world.Api.World.GetBlock(new AssetLocation("game:polishedrockslab-" + Variant["tabletop"] + "-down-free")));
                }
            }
            else
            {
                drops = new ItemStack[1];
                drops[0] = new ItemStack(world.Api.World.GetBlock(new AssetLocation("zeekea:cabinettopcase-" + Variant["internals"] + "-" + Variant["type"] + "-north")));
            }
            return drops;
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            string[] help = new string[1];

            string[] tabletops = new string[] { "andesite", "chalk", "chert", "conglomerate", "limestone", "claystone", "granite", "sandstone", "shale", "basalt", "peridotite", "phyllite", "slate", "bauxite" };
            string[] doors = new string[] { "acacia", "birch", "kapok", "maple", "oak", "pine", "baldcypress", "larch", "redwood", "ebony", "walnut", "purpleheart" };
            string[] drawers = new string[] { "acacia", "birch", "kapok", "maple", "oak", "pine", "baldcypress", "larch", "redwood", "ebony", "walnut", "purpleheart" };

            List<ItemStack> allowedStacks = new List<ItemStack>();
            if (this.FirstCodePart().Equals("cabinetbottomcase"))
            {
                if (Variant["tabletop"].Equals("none"))
                {
                    help[0] = "zeekea:interactionhelp-cabinetcase-placetabletop";
                    foreach(string type in tabletops)
                    {
                        allowedStacks.Add(new ItemStack(world.GetBlock(new AssetLocation("game:polishedrockslab-" + type + "-down-free"))));
                    }
                }
                else
                {
                    help[0] = "zeekea:interactionhelp-cabinetcase-placedoorsordrawers";
                    foreach (string type in doors)
                    {
                        allowedStacks.Add(new ItemStack(world.GetItem(new AssetLocation("zeekea:cabinetdoors-" + type))));
                    }
                    foreach (string type in drawers)
                    {
                        allowedStacks.Add(new ItemStack(world.GetItem(new AssetLocation("zeekea:cabinetdrawers-" + type))));
                    }
                }
            }
            else
            {
                if(Variant["type"].Equals("doors"))
                {
                    help[0] = "zeekea:interactionhelp-cabinetcase-placedoors";
                    foreach (string type in doors)
                    {
                        allowedStacks.Add(new ItemStack(world.GetItem(new AssetLocation("zeekea:cabinetdoors-" + type))));
                    }
                } else
                {
                    help[0] = "zeekea:interactionhelp-cabinetcase-placedrawers";
                    foreach (string type in drawers)
                    {
                        allowedStacks.Add(new ItemStack(world.GetItem(new AssetLocation("zeekea:cabinetdrawers-" + type))));
                    }
                }
            }
            return new WorldInteraction[]
            {
                new WorldInteraction() 
                {
                    ActionLangCode = help[0],
                    Itemstacks = allowedStacks.ToArray(),
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
