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
        }
    }
}
