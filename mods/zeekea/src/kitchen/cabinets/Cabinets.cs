using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace zeekea.src.kitchen.cabinets
{
    class Cabinets : Block
    {

        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BECabinets becabinet = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BECabinets;
            if (becabinet != null) return becabinet.OnInteract(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            ItemStack[] drops;

            if (this.FirstCodePart().Equals("cabinetbottom"))
            {
                drops = new ItemStack[3];
                drops[0] = new ItemStack(world.Api.World.GetBlock(new AssetLocation("zeekea:cabinetbottomcase-" + Variant["internals"] + "-none-north")));
                drops[1] = new ItemStack(world.Api.World.GetBlock(new AssetLocation("game:polishedrockslab-" + Variant["tabletop"] + "-down-free")));
                drops[2] = new ItemStack(world.Api.World.GetItem(new AssetLocation("zeekea:cabinet" + Variant["type"] + "-" + Variant["doors"])));
            } else
            {
                drops = new ItemStack[2];
                drops[0] = new ItemStack(world.Api.World.GetBlock(new AssetLocation("zeekea:cabinettopcase-" + Variant["internals"] + "-" + Variant["type"] + "-north")));
                drops[1] = new ItemStack(world.Api.World.GetItem(new AssetLocation("zeekea:cabinet" + Variant["type"] + "-" + Variant["internals"])));
            }

            return drops;
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            string[] help = new string[1];

            help[0] = "zeekea:interactionhelp-cabinet-openclose";
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = help[0],
                    HotKeyCode = "sneak",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
