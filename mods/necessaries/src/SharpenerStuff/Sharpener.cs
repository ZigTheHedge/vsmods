using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace necessaries.src.SharpenerStuff
{
    class Sharpener : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            /*
            byEntity.Attributes.SetBool("isInsertGear", false);

            if (ToolSlot.IsTool(byEntity.LeftHandItemSlot))
            {
                byEntity.Attributes.SetBool("isInsertGear", true);
                byEntity.Attributes.SetBool("sharpened", false);

                handling = EnumHandHandling.PreventDefault;
                return;
            }
            */
        }
    }
}
