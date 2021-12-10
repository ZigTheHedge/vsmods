using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common.Entities;

namespace edc.src.Behaviors
{
    class ControlsMessup : EntityBehavior
    {
        public ControlsMessup(Entity entity) : base(entity)
        {
        }

        public override string PropertyName()
        {
            return "controlsmessup";
        }
    }
}
