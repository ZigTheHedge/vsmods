using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace zeekea.src.doorbell
{
    class BEBell : BlockEntity
    {
        private BlockEntityAnimationUtil animUtil => ((BEBehaviorAnimatable)GetBehavior<BEBehaviorAnimatable>())?.animUtil;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api.World.Side == EnumAppSide.Client)
            {
                animUtil.InitializeAnimator("zeekea:bell", new Vec3f(0, Block.Shape.rotateY, 0));
            }

        }

        public bool Ring()
        {
            if (Api.World.Side == EnumAppSide.Client)
            {
                ((ICoreClientAPI)Api).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001);
            }
            return true;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if(packetid == 1001)
            {
                ICoreServerAPI sApi = (ICoreServerAPI)Api;
                sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001);
            } else
                base.OnReceivedClientPacket(fromPlayer, packetid, data);

        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if(packetid == 1001)
            {
                Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/bell.ogg"), Pos.X, Pos.Y, Pos.Z);
                animUtil.StartAnimation(new AnimationMetaData() { Animation = "ring", Code = "ring", AnimationSpeed = 1F, EaseInSpeed = 3F, EaseOutSpeed = 10F });
            } else
                base.OnReceivedServerPacket(packetid, data);
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            bool parentSkip = base.OnTesselation(mesher, tessThreadTesselator);
            if (animUtil.activeAnimationsByAnimCode.Count > 0 || parentSkip || (animUtil.animator != null && animUtil.animator.ActiveAnimationCount > 0))
            {
                return true;
            }

            return false;
        }
    }
}
