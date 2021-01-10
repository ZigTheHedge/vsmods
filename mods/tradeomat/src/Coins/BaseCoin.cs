using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace tradeomat.src.Coins
{
    class BaseCoin : ItemPileable
    {
        protected override AssetLocation PileBlockCode => new AssetLocation("tradeomat", "coinspile-" + Variant["metal"]);
        
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {

            if (blockSel == null || byEntity?.World == null || !byEntity.Controls.Sneak) return;

            /*
            api.World.Config

            BlockPos onBlockPos = blockSel.Position;
            Block block = byEntity.World.BlockAccessor.GetBlock(onBlockPos);

            if (block is BlockFirepit)
            {
                // Prevent placing firewoodpiles when trying to construct firepits
                return;
            }
            */
            
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {

            if (CombustibleProps?.SmeltedStack == null)
            {
                base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
                return;
            }

            CombustibleProperties props = CombustibleProps;

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            string smelttype = CombustibleProps.SmeltingType.ToString().ToLowerInvariant();
            int instacksize = CombustibleProps.SmeltedRatio;
            int outstacksize = CombustibleProps.SmeltedStack.ResolvedItemstack.StackSize;
            float units = outstacksize * 100f / instacksize;

            string metalname = CombustibleProps.SmeltedStack.ResolvedItemstack.GetName().Replace(" ingot", "");

            string str = Lang.Get("game:smeltdesc-" + smelttype + "ore-plural", units.ToString("0.#"), metalname);
            dsc.AppendLine(str);
        }

    }
}
