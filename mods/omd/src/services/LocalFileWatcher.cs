using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omd.src.services
{
    class LocalFileWatcher : DonationService
    {
        public override void execute()
        {
            OMD.clientApi.World.Player.ShowChatNotification("Test Message is here!");
        }
    }
}
