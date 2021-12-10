using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace omd.src.notifications
{
    class EventNotificationGUI : HudElement
    {
        public EventNotificationGUI(ICoreClientAPI capi) : base(capi)
        {
        }


        public override void OnRenderGUI(float deltaTime)
        {
            base.OnRenderGUI(deltaTime);
        }

    }
}
