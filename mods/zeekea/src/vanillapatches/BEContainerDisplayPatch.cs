using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace zeekea.src.vanillapatches
{
    public abstract class BEContainerDisplayPatch : BlockEntityDisplay, ITexPositionSource
    {
        new public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                AssetLocation texturePath = null;
                CompositeTexture tex;
                if (nowTesselatingItem.Textures.TryGetValue(textureCode, out tex))
                {
                    texturePath = tex.Baked.BakedName;
                }
                else
                {
                    nowTesselatingShape?.Textures.TryGetValue(textureCode, out texturePath);
                }

                if (texturePath == null)
                {
                    texturePath = new AssetLocation(textureCode);
                }

                TextureAtlasPosition texpos = capi.BlockTextureAtlas[texturePath];



                if (texpos == null)
                {
                    IAsset texAsset = capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                    if (texAsset != null)
                    {
                        BitmapRef bmp = texAsset.ToBitmap(capi);
                        capi.BlockTextureAtlas.InsertTextureCached(texturePath, bmp, out _, out texpos);
                    }
                    else
                    {
                        textureCode = "all";
                        if (nowTesselatingItem.Textures.TryGetValue(textureCode, out tex))
                        {
                            texturePath = tex.Baked.BakedName;
                        }
                        else
                        {
                            nowTesselatingShape?.Textures.TryGetValue(textureCode, out texturePath);
                        }

                        if (texturePath == null)
                        {
                            texturePath = new AssetLocation(textureCode);
                        }

                        texpos = capi.BlockTextureAtlas[texturePath];

                        if (texpos == null)
                        {
                            texAsset = capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                            if (texAsset != null)
                            {
                                BitmapRef bmp = texAsset.ToBitmap(capi);
                                capi.BlockTextureAtlas.InsertTextureCached(texturePath, bmp, out _, out texpos);
                            }
                            else
                            {
                                texAsset = capi.Assets.TryGet(new AssetLocation("game", "textures/unknown.png"));
                                if (texAsset != null)
                                {
                                    BitmapRef bmp = texAsset.ToBitmap(capi);
                                    capi.BlockTextureAtlas.InsertTextureCached(texturePath, bmp, out _, out texpos);
                                }
                                capi.World.Logger.Warning("Display cased item {0} defined texture {1}, not no such texture found. 'All' is also absent.", nowTesselatingItem.Code, texturePath);
                            }
                        }
                    }
                }

                return texpos;
            }
        }

        public BEContainerDisplayPatch(): base()
        {

        }
        public bool GenMeshOfChiseledBlock(ItemStack itemStack, out MeshData mesh)
        {
            ICoreClientAPI capi = Api as ICoreClientAPI;
            mesh = null;

            if (itemStack.Class != EnumItemClass.Block || !(itemStack.Block is BlockChisel))
                return false;

            ITreeAttribute tree = itemStack.Attributes ?? new TreeAttribute();
            int[] materials = BlockEntityChisel.MaterialIdsFromAttributes(tree, capi.World);
            uint[] numArray = null;
            if (tree["cuboids"] is IntArrayAttribute intArrayAttribute)
                numArray = intArrayAttribute.AsUint;
            if (numArray == null && tree["cuboids"] is LongArrayAttribute longArrayAttribute)
                numArray = longArrayAttribute.AsUint;

            List<uint> voxelCuboids = numArray != null ? new List<uint>(numArray) : new List<uint>();
            mesh = BlockEntityChisel.CreateMesh(capi, voxelCuboids, materials);
            if (mesh == null)
                return false;

            for (int index = 0; index < mesh.GetVerticesCount(); ++index)
                mesh.Flags[index] &= -256;

            for (int index = 0; index < mesh.RenderPassCount; ++index)
            {
                if (mesh.RenderPasses[index] != 3)
                    mesh.RenderPasses[index] = 1;
            }
            return true;
        }

        protected override MeshData genMesh(ItemStack stack, int index)
        {

            MeshData mesh;
            ICoreClientAPI capi = Api as ICoreClientAPI;

            if (stack.Class == EnumItemClass.Block && stack.Block is BlockChisel)
            {
                GenMeshOfChiseledBlock(stack, out mesh);
                return mesh;
            }

            if (stack.Class == EnumItemClass.Block)
            {
                try
                {
                    //capi.Tesselator.TesselateBlock(stack.Block, out mesh);
                    mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
                } catch(System.Collections.Generic.KeyNotFoundException e)
                {
                    mesh = CubeMeshUtil.GetCubeOnlyScaleXyz(0.5f, 0.5f, new Vec3f(0.5f, 0.5f, 0.5f));
                    Api.Logger.Error("[ZEEKEA]: Cannot render model of: " + stack.Collectible.Code.ToString() + "! Skipping.");
                }
                if (stack.Collectible.Attributes?["onDisplayTransform"].Exists == true)
                {
                    ModelTransform transform = stack.Collectible.Attributes?["onDisplayTransform"].AsObject<ModelTransform>();
                    transform.EnsureDefaultValues();
                    mesh.ModelTransform(transform);
                } else if (stack.Collectible.GroundTransform != null) {
                    ModelTransform transform = stack.Collectible.GroundTransform;
                    transform.EnsureDefaultValues();
                    mesh.ModelTransform(transform);
                }
                else
                    mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
            }
            else
            {
                nowTesselatingItem = stack.Item;
                if (stack.Item.Shape != null)
                {
                    nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
                }

                capi.Tesselator.TesselateItem(stack.Item, out mesh, this);

                if (stack.Collectible.Attributes?["onDisplayTransform"].Exists == true)
                {
                    ModelTransform transform = stack.Collectible.Attributes?["onDisplayTransform"].AsObject<ModelTransform>();
                    transform.EnsureDefaultValues();
                    mesh.ModelTransform(transform);
                }

                if (stack.Item.Shape == null || stack.Item.Shape.VoxelizeTexture)
                {
                    mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), GameMath.PIHALF, 0, 0);
                    mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.33f, 0.5f, 0.33f);
                    mesh.Translate(0, -7.5f / 16f, 0f);
                }

                mesh.RenderPasses.Fill((byte)EnumChunkRenderPass.BlendNoCull);


            }

            return mesh;
        }

        protected override void updateMeshes()
        {
            if (!ModConfigFile.Current.hideContents) base.updateMeshes();
        }

    }
}
