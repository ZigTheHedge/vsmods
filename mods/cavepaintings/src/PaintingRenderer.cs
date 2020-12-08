using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace cavepaintings.src
{
    class PaintingRenderer : IRenderer
    {
        private BlockPos pos;
        private ICoreClientAPI cApi;

        protected Dictionary<BlockFacing, LoadedTexture> loadedTexture;
        protected Dictionary<BlockFacing, MeshRef> quadModelRef;
        
        private Dictionary<BlockFacing, ImageSurface> surface;
        private Dictionary<BlockFacing, Context> ctx;


        public double RenderOrder
        {
            get { return 0.5; }
        }

        public int RenderRange
        {
            get { return 24; }
        }


        public PaintingRenderer(BlockPos pos, ICoreClientAPI api)
        {
            this.cApi = api;
            this.pos = pos;

            cApi.Event.RegisterRenderer(this, EnumRenderStage.Opaque);

            MeshData modeldata = QuadMeshUtil.GetQuad();
            modeldata.Uv = new float[]
            {
                1, 1,
                0, 1,
                0, 0,
                1, 0
            };
            modeldata.Rgba = new byte[4 * 4];
            modeldata.Rgba.Fill((byte)255);

            MeshData modeldata_south = modeldata.Clone();

            quadModelRef = new Dictionary<BlockFacing, MeshRef>();
            quadModelRef[BlockFacing.NORTH] = api.Render.UploadMesh(modeldata);
            quadModelRef[BlockFacing.SOUTH] = api.Render.UploadMesh(modeldata_south);
            quadModelRef[BlockFacing.EAST] = api.Render.UploadMesh(modeldata);
            quadModelRef[BlockFacing.WEST] = api.Render.UploadMesh(modeldata);
            quadModelRef[BlockFacing.UP] = api.Render.UploadMesh(modeldata);
            quadModelRef[BlockFacing.DOWN] = api.Render.UploadMesh(modeldata);

            surface = new Dictionary<BlockFacing, ImageSurface>();
            surface[BlockFacing.NORTH] = new ImageSurface(Format.Argb32, 32, 32);
            surface[BlockFacing.SOUTH] = new ImageSurface(Format.Argb32, 32, 32);
            surface[BlockFacing.EAST] = new ImageSurface(Format.Argb32, 32, 32);
            surface[BlockFacing.WEST] = new ImageSurface(Format.Argb32, 32, 32);
            surface[BlockFacing.UP] = new ImageSurface(Format.Argb32, 32, 32);
            surface[BlockFacing.DOWN] = new ImageSurface(Format.Argb32, 32, 32);

            ctx = new Dictionary<BlockFacing, Context>();
            ctx[BlockFacing.NORTH] = new Context(surface[BlockFacing.NORTH]);
            ctx[BlockFacing.SOUTH] = new Context(surface[BlockFacing.SOUTH]);
            ctx[BlockFacing.EAST] = new Context(surface[BlockFacing.EAST]);
            ctx[BlockFacing.WEST] = new Context(surface[BlockFacing.WEST]);
            ctx[BlockFacing.UP] = new Context(surface[BlockFacing.UP]);
            ctx[BlockFacing.DOWN] = new Context(surface[BlockFacing.DOWN]);

            loadedTexture = new Dictionary<BlockFacing, LoadedTexture>();

        }

        public void Dispose()
        {
            cApi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);

            ctx[BlockFacing.NORTH]?.Dispose();
            ctx[BlockFacing.SOUTH]?.Dispose();
            ctx[BlockFacing.EAST]?.Dispose();
            ctx[BlockFacing.WEST]?.Dispose();
            ctx[BlockFacing.UP]?.Dispose();
            ctx[BlockFacing.DOWN]?.Dispose();

            surface[BlockFacing.NORTH]?.Dispose();
            surface[BlockFacing.SOUTH]?.Dispose();
            surface[BlockFacing.EAST]?.Dispose();
            surface[BlockFacing.WEST]?.Dispose();
            surface[BlockFacing.UP]?.Dispose();
            surface[BlockFacing.DOWN]?.Dispose();

            if (loadedTexture.ContainsKey(BlockFacing.NORTH)) loadedTexture[BlockFacing.NORTH]?.Dispose();
            if (loadedTexture.ContainsKey(BlockFacing.SOUTH)) loadedTexture[BlockFacing.SOUTH]?.Dispose();
            if (loadedTexture.ContainsKey(BlockFacing.EAST)) loadedTexture[BlockFacing.EAST]?.Dispose();
            if (loadedTexture.ContainsKey(BlockFacing.WEST)) loadedTexture[BlockFacing.WEST]?.Dispose();
            if (loadedTexture.ContainsKey(BlockFacing.UP)) loadedTexture[BlockFacing.UP]?.Dispose();
            if (loadedTexture.ContainsKey(BlockFacing.DOWN)) loadedTexture[BlockFacing.DOWN]?.Dispose();

            quadModelRef[BlockFacing.NORTH]?.Dispose();
            quadModelRef[BlockFacing.SOUTH]?.Dispose();
            quadModelRef[BlockFacing.EAST]?.Dispose();
            quadModelRef[BlockFacing.WEST]?.Dispose();
            quadModelRef[BlockFacing.UP]?.Dispose();
            quadModelRef[BlockFacing.DOWN]?.Dispose();
        }

        public void Clear()
        {
            /*
            ctx.Save();
            ctx.LineWidth = 0;
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
            ctx.Operator = Operator.Clear;
            ctx.Rectangle(0, 0, 32d, 32d);
            ctx.PaintWithAlpha(1.0d);
            ctx.Restore();
            */
        }
        public void CreateSurface(BlockFacing facing)
        {
            if(!loadedTexture.ContainsKey(facing))
                loadedTexture[facing] = new LoadedTexture(cApi);
        }

        public void RemoveSurface(BlockFacing facing)
        {
            if (loadedTexture.ContainsKey(facing))
            {
                ctx[facing]?.Dispose();
                surface[facing]?.Dispose();
                surface[facing] = new ImageSurface(Format.Argb32, 32, 32);
                ctx[facing] = new Context(surface[facing]);

                loadedTexture.Remove(facing);
            }
        }

        public void Paint(BlockFacing facing, double X, double Y, byte colIndex)
        {
            ctx[facing].Save();
            if (colIndex == 1) ctx[facing].SetSourceRGBA(0.7d, 0, 0, 1.0d);
            if (colIndex == 2) ctx[facing].SetSourceRGBA(0, 0, 0.7d, 1.0d);
            if (colIndex == 3) ctx[facing].SetSourceRGBA(0, 0.7d, 0, 1.0d);
            if (colIndex == 4) ctx[facing].SetSourceRGBA(0.8d, 0.8d, 0.8d, 1.0d);
            ctx[facing].Arc(X * 32, (32 - Y * 32), 1, 0, 2.0f * 3.1415f);
            ctx[facing].Stroke();
            ctx[facing].Restore();
            LoadedTexture texture;
            if (loadedTexture[facing] == null)
                texture = new LoadedTexture(cApi);
            else
                texture = loadedTexture[facing];
            cApi.Gui.LoadOrUpdateCairoTexture(surface[facing], true, ref texture);
            loadedTexture[facing] = texture;
            
            //texture.Dispose();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (loadedTexture == null) return;

            IRenderAPI rpi = cApi.Render;
            Vec3d camPos = cApi.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            prog.NormalShaded = 0;
            
            Matrixf ModelMat = new Matrixf();

            if (loadedTexture.ContainsKey(BlockFacing.NORTH))
            {
                prog.Tex2D = loadedTexture[BlockFacing.NORTH].TextureId;
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                    .Translate(0.5f, 0.5f, 0.5f)
                    .RotateYDeg(0)
                    .Translate(0f, 0f, 0.4999f)
                    .Scale(0.5f, 0.5f, 0.5f)
                    .Values
                ;
                if (loadedTexture[BlockFacing.NORTH] != null) rpi.RenderMesh(quadModelRef[BlockFacing.NORTH]);
            }

            if (loadedTexture.ContainsKey(BlockFacing.SOUTH))
            {
                prog.Tex2D = loadedTexture[BlockFacing.SOUTH].TextureId;
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                    .Translate(0.5f, 0.5f, 0.5f)
                    .RotateYDeg(180)
                    .Translate(0f, 0f, 0.4999f)
                    .Scale(0.5f, 0.5f, 0.5f)
                    .Values
                ;
                if (loadedTexture[BlockFacing.SOUTH] != null) rpi.RenderMesh(quadModelRef[BlockFacing.SOUTH]);
            }

            if (loadedTexture.ContainsKey(BlockFacing.EAST))
            {
                prog.Tex2D = loadedTexture[BlockFacing.EAST].TextureId;
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                    .Translate(0.5f, 0.5f, 0.5f)
                    .RotateYDeg(270)
                    .Translate(0f, 0f, 0.4999f)
                    .Scale(0.5f, 0.5f, 0.5f)
                    .Values
                ;
                if (loadedTexture[BlockFacing.EAST] != null) rpi.RenderMesh(quadModelRef[BlockFacing.EAST]);
            }

            if (loadedTexture.ContainsKey(BlockFacing.WEST))
            {

                prog.Tex2D = loadedTexture[BlockFacing.WEST].TextureId;
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                    .Translate(0.5f, 0.5f, 0.5f)
                    .RotateYDeg(90)
                    .Translate(0f, 0f, 0.4999f)
                    .Scale(0.5f, 0.5f, 0.5f)
                    .Values
                ;
                if (loadedTexture[BlockFacing.WEST] != null) rpi.RenderMesh(quadModelRef[BlockFacing.WEST]);
            }

            if (loadedTexture.ContainsKey(BlockFacing.UP))
            {
                prog.Tex2D = loadedTexture[BlockFacing.UP].TextureId;
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                    .Translate(0.5f, 0.5f, 0.5f)
                    .RotateXDeg(90)
                    .Translate(0f, 0f, 0.4999f)
                    .Scale(0.5f, 0.5f, 0.5f)
                    .Values
                ;
                if (loadedTexture[BlockFacing.UP] != null) rpi.RenderMesh(quadModelRef[BlockFacing.UP]);
            }

            if (loadedTexture.ContainsKey(BlockFacing.DOWN))
            {
                prog.Tex2D = loadedTexture[BlockFacing.DOWN].TextureId;
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                    .Translate(0.5f, 0.5f, 0.5f)
                    .RotateXDeg(270)
                    .Translate(0f, 0f, 0.4999f)
                    .Scale(0.5f, 0.5f, 0.5f)
                    .Values
                ;
                if (loadedTexture[BlockFacing.DOWN] != null) rpi.RenderMesh(quadModelRef[BlockFacing.DOWN]);
            }

            prog.Stop();
        }
    }
}
