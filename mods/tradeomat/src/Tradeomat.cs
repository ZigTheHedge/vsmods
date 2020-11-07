using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tradeomat.src.TradeomatBlock;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace tradeomat.src
{
    class Tradeomat : ModSystem
    {

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("tomat", typeof(TradeBlock));
            api.RegisterBlockEntityClass("betomat", typeof(BETradeBlock));
            api.RegisterBlockClass("tomat-up", typeof(TradeBlockUp));

        }

    }
}
