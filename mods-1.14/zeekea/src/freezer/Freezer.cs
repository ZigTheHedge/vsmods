using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace zeekea.src.freezer
{
    class Freezer : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            // Добавить проверку на лед, таяние льду


            BEFreezer be = null;
            if (blockSel.Position != null)
            {
                be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEFreezer;
            }

            bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);

            if (!handled && !byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    if (Variant["state"] == "open")
                        be.OnBlockInteract(byPlayer, false);
                    else
                        return false;
                }

                return true;
            }
            else
            {

                AssetLocation newCode;

                // -18C

                if (Variant["state"] == "closed")
                {
                    newCode = CodeWithVariant("state", "open");
                    world.PlaySoundAt(new AssetLocation("zeekea:sounds/freezer_open.ogg"), byPlayer, byPlayer, false);
                }
                else
                {
                    newCode = CodeWithVariant("state", "closed");
                    world.PlaySoundAt(new AssetLocation("zeekea:sounds/freezer_close.ogg"), byPlayer, byPlayer, false);
                }

                Block newBlock = world.BlockAccessor.GetBlock(newCode);

                world.BlockAccessor.ExchangeBlock(newBlock.BlockId, blockSel.Position);

                if (be != null)
                {
                    be.isOpened = (Variant["state"] == "closed") ? true : false;
                    if(be.fuelRemaining > 0 && !be.isOpened)
                    {
                        world.PlaySoundAt(new AssetLocation("zeekea:sounds/freezer_start.ogg"), byPlayer, byPlayer, false);
                    }
                }

                return true;
            }
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "state", "closed" },
                    { "status", "melted" },
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

            dsc.AppendLine("\n" + Lang.Get("zeekea:freezer-help"));
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "zeekea:freezer-over-sneak-help",
                    HotKeyCode = "sneak",
                    MouseButton = EnumMouseButton.Right,
                },
                new WorldInteraction()
                {
                    ActionLangCode = "zeekea:freezer-over-help",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
