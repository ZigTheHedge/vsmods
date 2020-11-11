using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using zeekea.src.armchair;
using zeekea.src.doorbell;
using zeekea.src.freezer;
using zeekea.src.nightlamp;
using zeekea.src.nightstand;
using zeekea.src.tall_locker;

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
            api.RegisterBlockClass("tall_locker", typeof(TallLocker));
            api.RegisterBlockEntityClass("betall_locker", typeof(BETallLocker));
            api.RegisterBlockClass("dummy-up", typeof(DummyUP));
            api.RegisterBlockClass("freezer", typeof(Freezer));
            api.RegisterBlockEntityClass("befreezer", typeof(BEFreezer));
            api.RegisterBlockClass("doorbell", typeof(DoorBell));

            api.RegisterItemClass("icepick", typeof(IcepickItem));
        }
    }
}
