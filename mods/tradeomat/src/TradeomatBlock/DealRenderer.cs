using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace tradeomat.src.TradeomatBlock
{
    enum EnumGoodsRenderType
    {
        Generic = 101,
        FlatItem = 102,
        Reverse = 103
    }

    class DealRenderer : IRenderer
    {
        private BlockPos pos;
        private ICoreClientAPI cApi;
        private int fullness;
        ItemRenderInfo info;
        //ItemRenderInfo infoPrice;
        public Matrixf ModelMat = new Matrixf();
        public int rotTest;
        EnumGoodsRenderType goodsRender;

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
            rotTest = 0;

            cApi.Event.RegisterRenderer(this, EnumRenderStage.Opaque);

        }

        public void UpdateDeal(ItemSlot price, ItemSlot goods, int fullness)
        {
            this.fullness = fullness;
            info = cApi.Render.GetItemStackRenderInfo(goods, EnumItemRenderTarget.Ground);
            if (goods.Itemstack != null && goods.Itemstack.Class == EnumItemClass.Item)
            {
                if (goods.Itemstack.Item.Shape == null) goodsRender = EnumGoodsRenderType.FlatItem;
                else
                {
                    string fp = goods.Itemstack.Item.Code.FirstPathPart();
                    if (fp.StartsWith("ingot") ||
                       fp.StartsWith("stone")
                        ) 
                        goodsRender = EnumGoodsRenderType.Reverse;
                }
            }
            else
                goodsRender = EnumGoodsRenderType.Generic;
            //infoPrice = cApi.Render.GetItemStackRenderInfo(price, EnumItemRenderTarget.Ground);
        }

        public void Dispose()
        {
            cApi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (fullness == 0) return;

            rotTest++;
            if (rotTest > 360) rotTest = 0;

            int faceRotation = 0;
            if (cApi.World.BlockAccessor.GetBlock(pos).Variant["horizontalorientation"] == "north") faceRotation = 270;
            if (cApi.World.BlockAccessor.GetBlock(pos).Variant["horizontalorientation"] == "east") faceRotation = 180;
            if (cApi.World.BlockAccessor.GetBlock(pos).Variant["horizontalorientation"] == "south") faceRotation = 90;

            IRenderAPI rpi = cApi.Render;
            Vec3d camPos = cApi.World.Player.Entity.CameraPos;

            if (camPos.SquareDistanceTo(pos.X, pos.Y, pos.Z) > 20 * 20) return;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;

            if (info.ModelRef != null)
            {
                prog.Tex2D = info.TextureId;

                if (goodsRender == EnumGoodsRenderType.Generic)
                {
                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.5f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -1.5f)
                        .Values
                    ;

                    rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.5f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-0.5f, 0, -1.5f)
                        .Values
                    ;

                    if (fullness > 1)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.5f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(0.75f, 0, -1.5f)
                        .Values
                    ;

                    if (fullness > 2)
                        rpi.RenderMesh(info.ModelRef);

                    //Second row

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, -1f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, 1f, 1f)
                        .Translate(-1.75f, -1.5f, -1.5f)
                        .Values
                    ;

                    if (fullness > 3)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, -1f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, 1f, 1f)
                        .Translate(-0.5f, -1.5f, -1.5f)
                        .Values
                    ;

                    if (fullness > 4)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, -1f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, 1f, 1f)
                        .Translate(0.75f, -1.5f, -1.5f)
                        .Values
                    ;

                    if (fullness > 5)
                        rpi.RenderMesh(info.ModelRef);

                    // Second Level

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.25f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.25f, 1f)
                        .Translate(-1.25f, -0.75f, -0.5f)
                        .Values
                    ;

                    if (fullness > 6)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.25f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.25f, 1f)
                        .Translate(0f, -0.75f, -0.5f)
                        .Values
                    ;

                    if (fullness > 7)
                        rpi.RenderMesh(info.ModelRef);

                } else if (goodsRender == EnumGoodsRenderType.FlatItem)
                {
                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.5f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -1.5f)
                        .Values
                    ;

                    rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.5f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-0.5f, 0, -1.5f)
                        .Values
                    ;

                    if (fullness > 1)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.5f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(0.75f, 0, -1.5f)
                        .Values
                    ;

                    if (fullness > 2)
                        rpi.RenderMesh(info.ModelRef);

                    //Second row

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, -1f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, 1f, 1f)
                        .Translate(-1.75f, -1.5f, -1.5f)
                        .Values
                    ;

                    if (fullness > 3)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, -1f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, 1f, 1f)
                        .Translate(-0.5f, -1.5f, -1.5f)
                        .Values
                    ;

                    if (fullness > 4)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, -1f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, 1f, 1f)
                        .Translate(0.75f, -1.5f, -1.5f)
                        .Values
                    ;

                    if (fullness > 5)
                        rpi.RenderMesh(info.ModelRef);

                    // Second Level

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.25f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.25f, 1f)
                        .Translate(-1.25f, -0.75f, -1.2f)
                        .Values
                    ;

                    if (fullness > 6)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(.25f, .25f, .25f)
                        .RotateZ(-10 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0, 0.25f, -1f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.25f, 1f)
                        .Translate(0f, -0.75f, -1.2f)
                        .Values
                    ;

                    if (fullness > 7)
                        rpi.RenderMesh(info.ModelRef);
                } else if (goodsRender == EnumGoodsRenderType.Reverse)
                {
                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -0.3f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -1.5f)
                        .Values
                    ;

                    rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -0.75f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -1.5f)
                        .Values
                    ;

                    if (fullness > 1)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -1.2f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -1.5f)
                        .Values
                    ;

                    if (fullness > 2)
                        rpi.RenderMesh(info.ModelRef);

                    //Second row

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -0.3f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -2.0f)
                        .Values
                    ;

                    if (fullness > 3)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -0.75f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -2.0f)
                        .Values
                    ;

                    if (fullness > 4)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -1.2f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.75f, 0, -2.0f)
                        .Values
                    ;

                    if (fullness > 5)
                        rpi.RenderMesh(info.ModelRef);

                    // Second Level

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -0.75f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.98f, 0, -1.75f)
                        .Values
                    ;

                    if (fullness > 6)
                        rpi.RenderMesh(info.ModelRef);

                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                        .Translate(0.5f, 0.5f, 0.5f)
                        .RotateY(faceRotation * GameMath.PI / 180)
                        .Scale(0.7f, 0.7f, 0.7f)
                        .Translate(0f, -0.4f, -1.2f)
                        .RotateZ(90 * GameMath.PI / 180)
                        .RotateX(-90 * GameMath.PI / 180)
                        .RotateZ(90 * GameMath.PI / 180)
                        .Translate(0.5f, 0.45f, 0.2f)
                        .RotateX(15 * GameMath.PI / 180)
                        .Translate(0, -0.5f, 1f)
                        .Translate(-1.98f, 0, -1.75f)
                        .Values
                    ;

                    if (fullness > 7)
                        rpi.RenderMesh(info.ModelRef);
                }
            }

            /*
            if(infoPrice.ModelRef != null)
            {
                prog.Tex2D = infoPrice.TextureId;

                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                    .Translate(0.5f, 0.5f, 0.5f)
                    .RotateY(faceRotation * GameMath.PI / 180)
                    .Scale(.25f, .25f, .25f)
                    .RotateZ(-22.6f * GameMath.PI / 180)
                    .RotateY(45 * GameMath.PI / 180)
                    .Translate(-0.5f, -0.5f, -0.5f)
                    .Values
                ;

                rpi.RenderMesh(infoPrice.ModelRef);

            }
            */

            prog.Stop();
        }
    }
}
