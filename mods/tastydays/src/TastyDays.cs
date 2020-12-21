using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tastydays.src.cuttingboard;
using tastydays.src.kebabbrazier;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace tastydays.src
{
    class TastyDays : ModSystem
    {
        ICoreServerAPI serverApi;
        ICoreClientAPI clientApi;
        public static IServerNetworkChannel serverChannel;
        public static IClientNetworkChannel clientChannel;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("kebabbrazier", typeof(Kebabbrazier));
            api.RegisterBlockEntityClass("kebabbrazierbe", typeof(BEKebabbrazier));
            api.RegisterBlockClass("skewer", typeof(Skewer));
            api.RegisterBlockClass("cuttingboard", typeof(CuttingBoard));
            api.RegisterBlockEntityClass("cuttingboardbe", typeof(BECuttingBoard));


        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;
        }
    }
}
