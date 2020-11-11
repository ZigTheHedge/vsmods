using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace tradeomat.src.TradeomatBlock
{
    class DealRenderer : IRenderer
    {
        private BlockPos pos;
        private ICoreClientAPI cApi;
        ItemRenderInfo info;
        public Matrixf ModelMat = new Matrixf();

        public double RenderOrder
        {
            get { return 0.5; }
        }

        public int RenderRange
        {
            get { return 24; }
        }

        public DealRenderer(BlockPos pos, ICoreClientAPI api)
        {
            this.cApi = api;
            this.pos = pos;

            cApi.Event.RegisterRenderer(this, EnumRenderStage.Opaque);
        }

        public void UpdateDeal(ItemSlot slot)
        {

            info = cApi.Render.GetItemStackRenderInfo(slot, EnumItemRenderTarget.Gui);
        }

        public void Dispose()
        {
            cApi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            /*
            IRenderAPI rpi = cApi.Render;
            Vec3d camPos = cApi.World.Player.Entity.CameraPos;

            if (camPos.SquareDistanceTo(pos.X, pos.Y, pos.Z) > 20 * 20) return;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);

            prog.Tex2D = info.TextureId;
            
            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f, 0.5f)
                .RotateY(90 + GameMath.PI)
                .Values
            ;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;


            rpi.RenderMesh(info.ModelRef);
            prog.Stop();
            */
        }
    }
}
