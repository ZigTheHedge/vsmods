using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace fancydoors.src.RegularDoor
{
    class FancyDoorRenderer : DoorRenderer
    {
        public FancyDoorRenderer(BlockPos pos, ICoreClientAPI api, BEDoorPart BE) : base(pos, api, BE)
        {

        }

        public override void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (doorMesh == null) return;

            if (BE.enumState == BEFancyDoorPart.EnumState.OPENING)
            {
                BE.delta += 3;
                if (BE.delta > 90)
                {
                    BE.delta = 90;
                    BE.enumState = BEFancyDoorPart.EnumState.IDLE;
                    cApi.Network.SendBlockEntityPacket(pos.X, pos.Y, pos.Z, 1000, BitConverter.GetBytes(BE.delta));
                }
            }
            if (BE.enumState == BEFancyDoorPart.EnumState.CLOSING)
            {
                BE.delta -= 3;
                if (BE.delta < 0)
                {
                    BE.delta = 0;
                    BE.enumState = BEFancyDoorPart.EnumState.IDLE;
                    cApi.Network.SendBlockEntityPacket(pos.X, pos.Y, pos.Z, 1000, BitConverter.GetBytes(BE.delta));
                }
            }

            if (BE.isOpened == false && BE.enumState == BEFancyDoorPart.EnumState.IDLE && BE.meshState > 1) return;

            int faceRotation = 0;
            if (BE.Block.Variant["horizontalorientation"] == "north") faceRotation = 0;
            if (BE.Block.Variant["horizontalorientation"] == "east") faceRotation = 270;
            if (BE.Block.Variant["horizontalorientation"] == "south") faceRotation = 180;
            if (BE.Block.Variant["horizontalorientation"] == "west") faceRotation = 90;

            float width = 0;
            if (BE.Block.Variant["size"] == "2wide") width = 1f / 16f;
            if (BE.Block.Variant["size"] == "3wide") width = 1.5f / 16f;
            if (BE.Block.Variant["size"] == "4wide") width = 2f / 16f;
            if (BE.Block.Variant["size"] == "5wide") width = 2.5f / 16f;

            IRenderAPI rpi = cApi.Render;
            Vec3d camPos = cApi.World.Player.Entity.CameraPos;

            if (camPos.SquareDistanceTo(pos.X, pos.Y, pos.Z) > 20 * 20) return;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;

            prog.Tex2D = cApi.BlockTextureAtlas.Positions[doorBlock.FirstTextureInventory.Baked.TextureSubId].atlasTextureId;

            if (BE.Block.Variant["orientation"] == "right")
            {
                prog.ModelMatrix = ModelMat
                            .Identity()
                            .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                            .Translate(0.5f, 0.5f, 0.5f)
                            .RotateY(faceRotation * GameMath.PI / 180)
                            .Translate(-0.5f, -0.5f, -0.5f)
                            .Translate(1f - width, 0, 1f - width)
                            .RotateY(-BE.delta * GameMath.PI / 180)
                            .Translate(-(1f - width), 0, -(1f - width))
                            .Values
                        ;
                rpi.RenderMesh(doorMesh);

                if (staticMesh != null)
                {
                    //prog.Tex2D = cApi.BlockTextureAtlas.Positions[staticBlock.FirstTextureInventory.Baked.TextureSubId].atlasTextureId;
                    prog.ModelMatrix = ModelMat
                            .Identity()
                            .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                            .Translate(0.5f, 0.5f, 0.5f)
                            .RotateY(faceRotation * GameMath.PI / 180)
                            .Translate(-0.5f, -0.5f, -0.5f)
                            .Values
                        ;
                    rpi.RenderMesh(staticMesh);
                }
            }
            else
            {
                prog.ModelMatrix = ModelMat
                            .Identity()
                            .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                            .Translate(0.5f, 0.5f, 0.5f)
                            .RotateY(faceRotation * GameMath.PI / 180)
                            .Translate(-0.5f, -0.5f, -0.5f)
                            .Translate(width, 0, 1f - width)
                            .RotateY(BE.delta * GameMath.PI / 180)
                            .Translate(-width, 0, -(1f - width))
                            .Values
                        ;
                rpi.RenderMesh(doorMesh);

                if (staticMesh != null)
                {
                    //prog.Tex2D = cApi.BlockTextureAtlas.Positions[staticBlock.FirstTextureInventory.Baked.TextureSubId].atlasTextureId;
                    prog.ModelMatrix = ModelMat
                            .Identity()
                            .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                            .Translate(0.5f, 0.5f, 0.5f)
                            .RotateY(faceRotation * GameMath.PI / 180)
                            .Translate(-0.5f, -0.5f, -0.5f)
                            .Values
                        ;
                    rpi.RenderMesh(staticMesh);
                }
            }

            prog.Stop();
            if (BE.meshState == 1) BE.meshState++;
        }
    }
}
