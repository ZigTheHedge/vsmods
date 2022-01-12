using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace necessaries.src.SharpenerStuff
{
    class BEGrindstone : BlockEntityDisplay
    {
        public static SimpleParticleProperties particlesStab;
        static BEGrindstone()
        {
            particlesStab = new SimpleParticleProperties(
                1, 1,
                ColorUtil.ToRgba(50, 220, 220, 220),
                new Vec3d(),
                new Vec3d(),
                new Vec3f(0.2f, -0.2f, 0f),
                new Vec3f(),
                0.6f,
                0.3f,
                0.01f,
                0.05f,
                EnumParticleModel.Cube
            );

            particlesStab.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -0.6f);
            particlesStab.addLifeLength = 0.5f;
            particlesStab.RandomVelocityChange = true;
            particlesStab.MinQuantity = 50;
            particlesStab.AddQuantity = 30;
            particlesStab.ParticleModel = EnumParticleModel.Quad;
            particlesStab.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -150);
        }

        ILoadedSound emptySound;
        ILoadedSound sharpenSound;

        internal GrindstoneInventory inventory;
        public int phase = 0;
        bool IAmRotating = false;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "grindstone"; }
        }

        private BlockEntityAnimationUtil animUtil => ((BEBehaviorAnimatable)GetBehavior<BEBehaviorAnimatable>())?.animUtil;

        public BEGrindstone()
        {
            inventory = new GrindstoneInventory(null, null);
            meshes = new MeshData[2];

        }

        public void InitAnimator()
        {
            if (Api.World.Side == EnumAppSide.Client)
            {
                if (animUtil != null)
                {
                    animUtil.renderer?.Dispose();
                    (Api as ICoreClientAPI)?.Event.UnregisterRenderer(animUtil, EnumRenderStage.Opaque);
                }
                if(ModConfigFile.Current.grindstoneEnabled)
                    animUtil.InitializeAnimator("necessaries:grindstone", new Vec3f(0, Block.Shape.rotateY, 0));
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("grindstone-1", api);
            InitAnimator();
            RegisterGameTickListener(Tick, 500);
            RegisterGameTickListener(ParticleTick, 50);

            if(api.Side == EnumAppSide.Client)
            {
                if(emptySound == null)
                {
                    emptySound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams()
                    {
                        Location = new AssetLocation("necessaries:sounds/empty.ogg"),
                        ShouldLoop = true,
                        Position = Pos.ToVec3f().Add(0.5f, 0.5f, 0.5f),
                        DisposeOnFinish = false,
                        Volume = 1f
                    });
                }
                if (sharpenSound == null)
                {
                    sharpenSound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams()
                    {
                        Location = new AssetLocation("necessaries:sounds/sharpen.ogg"),
                        ShouldLoop = true,
                        Position = Pos.ToVec3f().Add(0.5f, 0.5f, 0.5f),
                        DisposeOnFinish = false,
                        Volume = 1f
                    });
                }
            }
        }

        void ParticleTick(float dt)
        {
            if (Api.Side != EnumAppSide.Client) return;
            if (IAmRotating)
            {
                if (Inventory[0].Empty || Inventory[1].Empty) return;
                particlesStab.Color = ColorUtil.ColorFromRgba(143, 208, 240, 255);
                /*
                particlesStab.BlueEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -10f);
                particlesStab.GreenEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, 10f);
                */

                if (Block.Variant["horizontalorientation"] == "north")
                {
                    particlesStab.MinPos = new Vec3d(Pos.X + 0.7f, Pos.Y + 0.6f, Pos.Z + 0.7f);
                    particlesStab.AddPos = new Vec3d(0.1f, 0.1f, 0.1f);
                    particlesStab.MinVelocity = new Vec3f(1.3f, 1.3f, -1f);
                    particlesStab.AddVelocity = new Vec3f(0.6f, 0.5f, 0.6f);
                }
                if (Block.Variant["horizontalorientation"] == "west")
                {
                    particlesStab.MinPos = new Vec3d(Pos.X + 0.7f, Pos.Y + 0.6f, Pos.Z + 0.2f);
                    particlesStab.AddPos = new Vec3d(0.1f, 0.1f, 0.1f);
                    particlesStab.MinVelocity = new Vec3f(-1f, 1.3f, -1.3f);
                    particlesStab.AddVelocity = new Vec3f(0.6f, 0.5f, 0.6f);
                }
                if (Block.Variant["horizontalorientation"] == "south")
                {
                    particlesStab.MinPos = new Vec3d(Pos.X + 0.2f, Pos.Y + 0.6f, Pos.Z + 0.2f);
                    particlesStab.AddPos = new Vec3d(0.1f, 0.1f, 0.1f);
                    particlesStab.MinVelocity = new Vec3f(-1.3f, 1.3f, 1f);
                    particlesStab.AddVelocity = new Vec3f(0.6f, 0.5f, 0.6f);
                }
                if (Block.Variant["horizontalorientation"] == "east")
                {
                    particlesStab.MinPos = new Vec3d(Pos.X + 0.3f, Pos.Y + 0.6f, Pos.Z + 0.7f);
                    particlesStab.AddPos = new Vec3d(0.1f, 0.1f, 0.1f);
                    particlesStab.MinVelocity = new Vec3f(1f, 1.3f, 1.3f);
                    particlesStab.AddVelocity = new Vec3f(0.6f, 0.5f, 0.6f);
                }
                Api.World.SpawnParticles(particlesStab);
            }
        }

        void Tick(float dt)
        {
            if(IAmRotating)
            {
                if (!Inventory[0].Empty && !Inventory[1].Empty)
                {
                    ItemStack itemstack = Inventory[0].Itemstack;

                    int leftDurability = itemstack.Attributes.GetInt("durability", itemstack.Item.GetDurability(itemstack));
                    leftDurability -= 1;
                    itemstack.Attributes.SetInt("durability", leftDurability);

                    if (leftDurability <= 0)
                    {
                        Inventory[0].Itemstack = null;
                        Api.World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), Pos.X, Pos.Y, Pos.Z);
                        SetBlockState("none");
                        if (Api.Side == EnumAppSide.Server)
                        {
                            ((ICoreServerAPI)Api).Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1102);
                        }
                    }

                    Inventory[0].MarkDirty();

                    ItemStack toolstack = Inventory[1].Itemstack;

                    int totalDurabilityLoss = 1;
                    int totalDurabilityRestored = 2;

                    if(itemstack.Item.Variant["rock"] == "granite")
                    {
                        totalDurabilityLoss = ModConfigFile.Current.graniteDiskDamagePerCycle;
                        totalDurabilityRestored = ModConfigFile.Current.graniteDiskRepairPerCycle;
                    }
                    if (itemstack.Item.Variant["rock"] == "basalt")
                    {
                        totalDurabilityLoss = ModConfigFile.Current.basaltDiskDamagePerCycle;
                        totalDurabilityRestored = ModConfigFile.Current.basaltDiskRepairPerCycle;
                    }
                    if (itemstack.Item.Variant["rock"] == "sandstone")
                    {
                        totalDurabilityLoss = ModConfigFile.Current.sandstoneDiskDamagePerCycle;
                        totalDurabilityRestored = ModConfigFile.Current.sandstoneDiskRepairPerCycle;
                    }
                    if (itemstack.Item.Variant["rock"] == "obsidian")
                    {
                        totalDurabilityLoss = ModConfigFile.Current.obsidianDiskDamagePerCycle;
                        totalDurabilityRestored = ModConfigFile.Current.obsidianDiskRepairPerCycle;
                    }
                    if (itemstack.Item.Variant["rock"] == "diamond")
                    {
                        totalDurabilityLoss = ModConfigFile.Current.diamondDiskDamagePerCycle;
                        totalDurabilityRestored = ModConfigFile.Current.diamondDiskRepairPerCycle;
                    }

                    int maxRepair = toolstack.Attributes.GetInt("maxRepair", toolstack.Item.GetDurability(toolstack));

                    if (toolstack.Attributes.GetInt("durability", toolstack.Item.GetDurability(toolstack)) < maxRepair)
                    {
                        toolstack.Attributes.SetInt("maxRepair", maxRepair - totalDurabilityLoss);
                        int newDurability = toolstack.Attributes.GetInt("durability") + totalDurabilityRestored;
                        if (newDurability > toolstack.Attributes.GetInt("maxRepair")) newDurability = toolstack.Attributes.GetInt("maxRepair");
                        toolstack.Attributes.SetInt("durability", newDurability);
                        if (newDurability < 1)
                        {
                            Inventory[1].Itemstack = null;
                            Api.World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), Pos.X, Pos.Y, Pos.Z);
                            updateMesh(1);
                            MarkDirty(true);
                        }
                    }
                    else if (toolstack.Attributes.GetInt("durability") > maxRepair)
                    {
                        toolstack.Attributes.SetInt("durability", maxRepair);
                    }

                    Inventory[1].MarkDirty();
                } 
            }
        }

        public void SetBlockState(string state)
        {
            AssetLocation loc = Block.CodeWithVariant("rock", state);
            Block block = Api.World.GetBlock(loc);
            if (block == null) return;

            Api.World.BlockAccessor.ExchangeBlock(block.Id, Pos);
            this.Block = block;
            InitAnimator();
            MarkDirty(true);
        }

        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            int slotSel = blockSel.SelectionBoxIndex;

            if (slot.Empty)
            {
                if (slotSel == 1)
                {
                    if (inventory[1].Empty) return false;
                    byPlayer.InventoryManager.TryGiveItemstack(inventory[1].TakeOutWhole());
                    updateMesh(1);
                    MarkDirty(true);
                    return true;
                }
                if (slotSel == 2)
                {
                    if (inventory[0].Empty) return false;
                    byPlayer.InventoryManager.TryGiveItemstack(inventory[0].TakeOutWhole());
                    SetBlockState("none");
                    MarkDirty(true);
                    return true;
                }
            }
            else
            {
                if (slotSel == 1)
                {
                    if (ToolSlot.IsTool(slot))
                    {
                        if (inventory[1].Empty)
                        {
                            int res = slot.TryPutInto(Api.World, inventory[1], 1);
                            updateMesh(1);
                            MarkDirty(true);
                            return true;
                        }
                    }
                }
                if (slotSel == 2)
                {
                    if (ToolSlot.IsSharpener(slot))
                    {
                        if (inventory[0].Empty)
                        {
                            SetBlockState(slot.Itemstack.Item.Variant["rock"]);
                            int res = slot.TryPutInto(Api.World, inventory[0], 1);
                            MarkDirty(true);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Animate(bool Start)
        {
            if (Start)
                animUtil.StartAnimation(new AnimationMetaData() { Animation = "rotate", Code = "rotate", AnimationSpeed = 2F, EaseInSpeed = 3F, EaseOutSpeed = 10F });
            else
                animUtil.StopAnimation("rotate");
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if(packetid == 1101)
            {
                if (IAmRotating == BitConverter.ToBoolean(data, 0)) return;
                IAmRotating = BitConverter.ToBoolean(data, 0);
                ((ICoreServerAPI)Api).Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1101, data);
                MarkDirty();
            } else
                base.OnReceivedClientPacket(fromPlayer, packetid, data);
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if(packetid == 1101)
            {
                Animate(BitConverter.ToBoolean(data, 0));
                if (BitConverter.ToBoolean(data, 0))
                {
                    if (!Inventory[0].Empty)
                    {
                        if (!Inventory[1].Empty)
                        {
                            sharpenSound?.Start();
                        } else
                        {
                            emptySound?.Start();
                        }
                    }
                } else
                {
                    sharpenSound?.Stop();
                    emptySound?.Stop();
                }
                MarkDirty(true);
            } else
            if (packetid == 1102)
            {
                Animate(false);
                sharpenSound?.Stop();
                SetBlockState("none");
                MarkDirty(true);
                Animate(true);
                MarkDirty(true);
            }
            else
                base.OnReceivedServerPacket(packetid, data);
        }

        Matrixf mat = new Matrixf();

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {

            bool parentSkip = base.OnTesselation(mesher, tessThreadTesselator);
            if (animUtil.activeAnimationsByAnimCode.Count > 0 || parentSkip || (animUtil.animator != null && animUtil.animator.ActiveAnimationCount > 0))
            {
                return true;
            }

            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        public override void updateMeshes()
        {
            mat.Identity();
            mat.RotateYDeg(Block.Shape.rotateY);

            base.updateMeshes();
        }

        protected override MeshData genMesh(ItemStack stack)
        {
            
            MeshData mesh;

            ICoreClientAPI capi = Api as ICoreClientAPI;
            nowTesselatingObj = stack.Item;
            nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
            capi.Tesselator.TesselateItem(stack.Item, out mesh, this);

            mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);


            ModelTransform transform;

            if (stack.Collectible.Attributes?["onGrindstoneTransform"].Exists == true)
            {
                transform = stack.Collectible.Attributes?["onGrindstoneTransform"].AsObject<ModelTransform>();
                transform.EnsureDefaultValues();

                //transform.Rotation.Y += Block.Shape.rotateY;
                mesh.ModelTransform(transform);
            }

            /*
            // ------ Debug
            transform = ModelTransform.NoTransform;
            transform.EnsureDefaultValues();

            transform.Origin.Set(0.5f, 0.1f, 0.5f);
            transform.Rotation.Set(230f, 0f, 0f);
            transform.Translation.Set(-0.6f, 0.35f, 0.25f);
            transform.Scale = 1f;

            mesh.ModelTransform(transform);
            // ------ End Debug
            */

            transform = ModelTransform.NoTransform;
            transform.EnsureDefaultValues();

            transform.Origin.X = 0.5f;
            transform.Origin.Y = 0;
            transform.Origin.Z = 0.5f;

            transform.Rotation.X = 0;
            transform.Rotation.Y = Block.Shape.rotateY;
            transform.Rotation.Z = 0;

            transform.Scale = 1f;
            
            mesh.ModelTransform(transform);

            float x = 0f;
            float y = 0.07f;
            float z = 0;

            Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
            mesh.Translate(offset.XYZ);

            return mesh;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            IAmRotating = tree.GetBool("IAmRotating");

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBool("IAmRotating", IAmRotating);

        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
                return;
            }

            ItemSlot tool = inventory[1];
            ItemSlot grindstone = inventory[0];

            if (tool.Itemstack == null && grindstone.Itemstack == null)
            {
                sb.AppendLine(Lang.Get("necessaries:grindstone-place-both"));
            }
            else if(tool.Itemstack == null && grindstone.Itemstack != null)
            {
                sb.AppendLine(Lang.Get("necessaries:grindstone-place-tool"));
                sb.AppendLine(Lang.Get("necessaries:grindstone-sharpenerdurability") + " " + grindstone.Itemstack.Attributes.GetInt("durability", grindstone.Itemstack.Item.GetDurability(grindstone.Itemstack)) + "/" + grindstone.Itemstack.Item.GetDurability(grindstone.Itemstack));
            }
            else if (tool.Itemstack != null && grindstone.Itemstack == null)
            {
                sb.AppendLine(Lang.Get("necessaries:grindstone-tooldurability") + " " + tool.Itemstack.Attributes.GetInt("durability", tool.Itemstack.Item.GetDurability(tool.Itemstack)) + "/" + tool.Itemstack.Attributes.GetInt("maxRepair", tool.Itemstack.Item.GetDurability(tool.Itemstack)));
                sb.AppendLine(Lang.Get("necessaries:grindstone-place-grindstone"));
            }
            else if (tool.Itemstack != null && grindstone.Itemstack != null)
            {
                sb.AppendLine(Lang.Get("necessaries:grindstone-tooldurability") + " " + tool.Itemstack.Attributes.GetInt("durability", tool.Itemstack.Item.GetDurability(tool.Itemstack)) + "/" + tool.Itemstack.Attributes.GetInt("maxRepair", tool.Itemstack.Item.GetDurability(tool.Itemstack)));
                sb.AppendLine(Lang.Get("necessaries:grindstone-sharpenerdurability") + " " + grindstone.Itemstack.Attributes.GetInt("durability", grindstone.Itemstack.Item.GetDurability(grindstone.Itemstack)) + "/" + grindstone.Itemstack.Item.GetDurability(grindstone.Itemstack));
            }
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            
            if (sharpenSound != null)
            {
                sharpenSound.Stop();
                sharpenSound.Dispose();
            }

            if (emptySound != null)
            {
                emptySound.Stop();
                emptySound.Dispose();
            }
        }

        ~BEGrindstone()
        {
            if (sharpenSound != null) sharpenSound.Dispose();
            if (emptySound != null) emptySound.Dispose();
        }
    }
}
