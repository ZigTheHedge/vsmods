using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace cavepaintings.src
{
    class CavePaintings : ModSystem
    {
        ICoreServerAPI serverApi;
        ICoreClientAPI clientApi;
        public static IServerNetworkChannel serverChannel;
        public static IClientNetworkChannel clientChannel;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterItemClass("pigmentitem", typeof(PigmentItem));
            api.RegisterBlockClass("wallcanvas", typeof(WallCanvas));
            api.RegisterBlockEntityClass("bewallcanvas", typeof(BEWallCanvas));

            api.Network.RegisterChannel("cavepaintings");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            clientApi = api;

            clientChannel = api.Network.GetChannel("cavepaintings");

        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;

            serverChannel = api.Network.GetChannel("cavepaintings");

        }
    }
}
