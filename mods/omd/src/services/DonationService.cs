using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omd.src.services
{
    public abstract class DonationService
    {
        public int tickInterval = 5 * 20;
        public int ticks = 0;
        public bool isStarted = false;

        public void start()
        {
            //TODO: get tickInterval from config
            ticks = tickInterval;
            isStarted = true;
        }

        public void tick()
        {
            if (!isStarted) return;
            if(ticks > 0) ticks--;
            if(ticks == 0)
            {
                ticks = tickInterval;
                execute();
            }
        }

        public abstract void execute();
    }
}
