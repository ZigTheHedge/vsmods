using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theneighbours.src.Weapons;
using Vintagestory.API.Common;

namespace theneighbours.src
{
    class TheNeighbours : ModSystem
    {
        public override void Start(ICoreAPI api)
        {

            api.RegisterItemClass("scissor", typeof(Scissor));

        }
    }
}
