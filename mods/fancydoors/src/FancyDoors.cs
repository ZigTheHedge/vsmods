using fancydoors.src.DiagonalDoor;
using fancydoors.src.MultiDoor;
using fancydoors.src.RegularDoor;
using fancydoors.src.Workbench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace fancydoors.src
{
    class FancyDoors : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("fancydoor", typeof(FancyDoorPart));
            api.RegisterBlockEntityClass("befancydoor", typeof(BEFancyDoorPart));

            api.RegisterBlockClass("diagonaldoor", typeof(DiagonalDoorPart));
            api.RegisterBlockEntityClass("bediagonaldoor", typeof(DiagonalDoorBE));

            api.RegisterBlockClass("doorframe", typeof(DoorFrame));
            api.RegisterBlockEntityClass("doorframebe", typeof(DoorFrameBE));

            api.RegisterBlockClass("doorworkbench", typeof(DoorWorkbench));
            api.RegisterBlockEntityClass("doorworkbenchbe", typeof(DoorWorkbenchBE));
        }
    }
}
