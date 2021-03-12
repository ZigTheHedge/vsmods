using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theneighbours.src.Blocks;
using theneighbours.src.Weapons;
using Vintagestory.API.Common;

namespace theneighbours.src
{
    public class ModConfigFile
    {
        public static ModConfigFile Current { get; set; }

        public bool disableTickling { get; set; } = false;
        public bool disableShintorikae { get; set; } = false;
        public bool disableBrayer { get; set; } = false;
        public bool disableTurtor { get; set; } = false;
        public bool disableGlowshroom { get; set; } = false;
        public bool glowShroomPlopping { get; set; } = true;
        public bool disableAmpel { get; set; } = false;
        public bool disablePedestals { get; set; } = false;
    }

    class TheNeighbours : ModSystem
    {
        public override void StartPre(ICoreAPI api)
        {
            ModConfigFile.Current = api.LoadOrCreateConfig<ModConfigFile>("TheNeighboursConfig.json");

            api.World.Config.SetBool("disableTickling", ModConfigFile.Current.disableTickling);
            api.World.Config.SetBool("disableShintorikae", ModConfigFile.Current.disableShintorikae);
            api.World.Config.SetBool("disableBrayer", ModConfigFile.Current.disableBrayer);
            api.World.Config.SetBool("disableTurtor", ModConfigFile.Current.disableTurtor);
            api.World.Config.SetBool("disableGlowshroom", ModConfigFile.Current.disableGlowshroom);
            api.World.Config.SetBool("disableAmpel", ModConfigFile.Current.disableAmpel);
            api.World.Config.SetBool("disablePedestals", ModConfigFile.Current.disablePedestals);

            base.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("glowshroom", typeof(GlowShroom));
            api.RegisterBlockEntityClass("beglowshroom", typeof(BEGlowShroom));

            api.RegisterItemClass("scissor", typeof(Scissor));

        }
    }
}
