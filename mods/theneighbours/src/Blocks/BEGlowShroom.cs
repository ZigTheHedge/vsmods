using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace theneighbours.src.Blocks
{
    class BEGlowShroom : BlockEntity
    {
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(Ambiance, 1000);
        }

        public void Ambiance(float dt)
        {
            if(ModConfigFile.Current.glowShroomPlopping)
                if (Api.World.Rand.Next(50) == 0) 
                    Api.World.PlaySoundAt(new AssetLocation("theneighbours:sounds/block/glowshroom.ogg"), Pos.X, Pos.Y, Pos.Z, null, true, 16);
        }
    }
}
