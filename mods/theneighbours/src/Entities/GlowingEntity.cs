using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace theneighbours.src.Entities
{
    class GlowingEntity : EntityHumanoid
    {
        byte[] lightHsv = new byte[] { 6, 6, 10 };

        public override byte[] LightHsv
        {
            get
            {
                return lightHsv;
            }
        }

    }
}
