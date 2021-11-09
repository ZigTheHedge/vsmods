using omd.src.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace omd.src
{
    class OMD : ModSystem
    {
        public static ICoreClientAPI clientApi;
        public static LocalFileWatcher LOCAL = new LocalFileWatcher();

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            clientApi = api;
            LOCAL.start();
            //clientApi.Event.RegisterGameTickListener

        }

        public override void Dispose()
        {
            base.Dispose();
            clientApi = null;
        }
    }
}
