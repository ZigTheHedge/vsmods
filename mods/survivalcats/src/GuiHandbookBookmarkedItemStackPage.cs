using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace survivalcats.src
{
    class GuiHandbookBookmarkedItemStackPage : GuiHandbookItemStackPage
    {
        public override string CategoryCode => "bookmark";

        public GuiHandbookBookmarkedItemStackPage(ICoreClientAPI capi, ItemStack stack) : base(capi, stack)
        {
            Visible = true;
        }
    }
}
