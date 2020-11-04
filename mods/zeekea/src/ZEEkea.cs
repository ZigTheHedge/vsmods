using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using zeekea.src.armchair;
using zeekea.src.nightlamp;
using zeekea.src.nightstand;

namespace zeekea.src
{
    class ZEEkea : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("nightstand", typeof(Nightstand));
            api.RegisterBlockEntityClass("benightstand", typeof(BENightstand));
            api.RegisterBlockClass("nightlamp", typeof(Nightlamp));
            api.RegisterBlockClass("armchair", typeof(Armchair));
        }
    }
}
