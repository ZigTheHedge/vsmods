using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace fancydoors.src.MultiDoor
{
    class DoorFrameBE : BlockEntityContainer
    {
        public enum EnumState
        {
            IDLE,
            OPENING,
            CLOSING
        }

        internal ChiseledInventory inventory;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "chiseledinventory"; }
        }

        public DoorFrameBE()
        {
            inventory = new ChiseledInventory(null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            inventory.LateInitialize("chiseledinventory-" + Pos.ToString(), Api);
        }
    }
}
