using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace xmastime.src
{
    class XMasTime : ModSystem
    {
        ICoreServerAPI serverApi;
        ICoreClientAPI clientApi;
        public static IServerNetworkChannel serverChannel;
        public static IClientNetworkChannel clientChannel;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("lights", typeof(LightsOne));
            api.RegisterBlockClass("snowflake", typeof(LightsTwo));

            api.Network.RegisterChannel("xmastime");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            clientApi = api;

            clientChannel = api.Network.GetChannel("xmastime");

        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;

            serverChannel = api.Network.GetChannel("xmastime");

        }

        public override void Dispose()
        {
            base.Dispose();
            serverChannel = null;
            clientChannel = null;
        }

    }
}
