using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace fancydoors.src
{
    abstract class DoorRenderer : IRenderer
    {
        public double RenderOrder => 0.5d;
        public int RenderRange => 24;
        public Matrixf ModelMat = new Matrixf();
        protected MeshRef doorMesh = null;
        protected Block doorBlock = null;
        protected MeshRef staticMesh = null;
        protected Block staticBlock = null;
        protected BEDoorPart BE = null;

        protected BlockPos pos;
        protected ICoreClientAPI cApi;

        public DoorRenderer(BlockPos pos, ICoreClientAPI api, BEDoorPart BE)
        {
            cApi = api;
            this.pos = pos;
            this.BE = BE;
            cApi.Event.RegisterRenderer(this, EnumRenderStage.Opaque);
        }

        public void UpdateDynamic(ItemStack block, MeshData meshData)
        {
            if (meshData == null)
            {
                if(doorMesh != null)
                    cApi.Render.DeleteMesh(doorMesh);
                doorMesh = null;
                this.doorBlock = null;
            } else
                cApi.Event.EnqueueMainThreadTask(() =>
                {
                    if (doorMesh != null)
                        cApi.Render.UpdateMesh(doorMesh, meshData);
                    else
                        doorMesh = cApi.Render.UploadMesh(meshData);
                    if (block == null)
                        this.doorBlock = null;
                    else
                        this.doorBlock = block.Block;
                }, "uploadmesh");
        }

        public void UpdateStatic(ItemStack block, MeshData meshData)
        {
            if (meshData == null)
            {
                if (staticMesh != null)
                    cApi.Render.DeleteMesh(staticMesh);
                staticMesh = null;
                this.staticBlock = null;
            }
            else
                cApi.Event.EnqueueMainThreadTask(() =>
                {
                    if (staticMesh != null)
                        cApi.Render.UpdateMesh(staticMesh, meshData);
                    else
                        staticMesh = cApi.Render.UploadMesh(meshData);
                    if (block == null)
                        this.staticBlock = null;
                    else
                        this.staticBlock = block.Block;
                }, "uploadstaticmesh");
        }

        public virtual void Dispose()
        {
            cApi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
        }

        public abstract void OnRenderFrame(float deltaTime, EnumRenderStage stage);

    }
}
