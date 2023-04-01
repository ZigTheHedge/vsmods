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

        public override string AttributeTransformCode => "onGrindstoneTransform";

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "grindstone"; }
        }

        private BlockEntityAnimationUtil animUtil => (GetBehavior<BEBehaviorAnimatable>())?.animUtil;

        public BEGrindstone()
        {
            inventory = new GrindstoneInventory(null, null);

        }

        public void InitAnimator()
        {
            if (Api.World.Side == EnumAppSide.Client)
            {
                if (ModConfigFile.Current.grindstoneEnabled)
                    animUtil.InitializeAnimator("necessaries:grindstone", null, null, new Vec3f(0, Block.Shape.rotateY, 0));
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            // Hack to stop infinitely rotating grindstone after reload.
            // If IAmRotating is loaded from FromTreeAttributes and is true, it won't be changed until the next interaction.
            // Initialize is called aftger FromTreeAttributes, so it can set IAmRotating to false.
            this.IAmRotating = false;
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

                    int leftDurability = itemstack.Attributes.GetInt("durability", itemstack.Item.GetMaxDurability(itemstack));
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

                    int maxRepair = toolstack.Attributes.GetInt("maxRepair", toolstack.Item.GetMaxDurability(toolstack));

                    if (toolstack.Attributes.GetInt("durability", toolstack.Item.GetMaxDurability(toolstack)) < maxRepair)
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

            // Player hotbar slot is empty
            if (slot.Empty)
            {
                // Try taking off the tool.
                if (slotSel == 1)
                {
                    if (inventory[1].Empty) return false;
                    byPlayer.InventoryManager.TryGiveItemstack(inventory[1].TakeOutWhole());
                    updateMesh(1);
                    MarkDirty(true);
                    return true;
                }
                // Try taking off the grindstone.
                if (slotSel == 2)
                {
                    if (inventory[0].Empty) return false;
                    byPlayer.InventoryManager.TryGiveItemstack(inventory[0].TakeOutWhole());
                    SetBlockState("none");
                    MarkDirty(true);
                    return true;
                }
            }
            // Player is holding something in their hand.
            else
            {
                // Player is trying to put a tool into an empty tool inventory slot.
                if (slotSel == 1 && ToolSlot.IsTool(slot) && inventory[1].Empty)
                {
                    int res = slot.TryPutInto(Api.World, inventory[1], 1);
                    updateMesh(1);
                    MarkDirty(true);
                    return true;
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
                var doAnimation = BitConverter.ToBoolean(data, 0);
                if (IAmRotating == doAnimation) return;
                IAmRotating = doAnimation;
                ((ICoreServerAPI)Api).Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1101, data);
                MarkDirty();
            } else
                base.OnReceivedClientPacket(fromPlayer, packetid, data);
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if(packetid == 1101)
            {
                var doAnimation = BitConverter.ToBoolean(data, 0);
                Animate(doAnimation);
                if (doAnimation)
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

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {

            bool parentSkip = base.OnTesselation(mesher, tessThreadTesselator);
            if (animUtil.activeAnimationsByAnimCode.Count > 0 || parentSkip || (animUtil.animator != null && animUtil.animator.ActiveAnimationCount > 0))
            {
                return true;
            }

            return base.OnTesselation(mesher, tessThreadTesselator);
        }
        protected override void updateMesh(int index)
        {
            if (index == 0) return;
            base.updateMesh(index);
        }
        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[2][];
            tfMatrices[0] = new Matrixf()
                            .Identity()
                            .Values;
            tfMatrices[1] = new Matrixf()
                            .Identity()
                            .Values;
            return tfMatrices;
        }

        protected override MeshData getOrCreateMesh(ItemStack stack, int index)
        {
            MeshData mesh = getMesh(stack);
            if (mesh != null) return mesh;

            var meshSource = stack.Collectible as IContainedMeshSource;

            if (meshSource != null)
            {
                mesh = meshSource.GenMesh(stack, capi.BlockTextureAtlas, Pos);
            }
            else
            {
                ICoreClientAPI capi = Api as ICoreClientAPI;
                if (stack.Class == EnumItemClass.Block)
                {
                    mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
                }
                else
                {
                    nowTesselatingObj = stack.Collectible;
                    nowTesselatingShape = null;
                    if (stack.Item.Shape?.Base != null)
                    {
                        nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
                    }
                    capi.Tesselator.TesselateItem(stack.Item, out mesh, this);

                    mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);
                }
            }

            if (stack.Collectible.Attributes?[AttributeTransformCode].Exists == true)
            {

                ModelTransform transform = stack.Collectible.Attributes?[AttributeTransformCode].AsObject<ModelTransform>();
                transform.EnsureDefaultValues();
                mesh.ModelTransform(transform);

                // The Grindstone is facing in a certain direction, but the tool is always placed facing
                // in a compass direction. So we need to rotate the tool in the plane of the ground
                // (xz-plane in the game) by 90 degrees around the y-axis, or the any point at the center of
                // of the xz-plane (e.g. (0.5f, 0, 0.5f); others might work too).
                // The tool faces east, so we need to subtract 1 from HorizontalAngleIndex.
                Block block = Api.World.BlockAccessor.GetBlock(Pos);
                BlockFacing facing = BlockFacing.FromCode(block.LastCodePart());
                mesh.Rotate(new Vec3f(0.5f, 0f, 0.5f), 0, (facing.HorizontalAngleIndex - 1) * 90 * GameMath.DEG2RAD, 0);
            }

            if (stack.Class == EnumItemClass.Item && (stack.Item.Shape == null || stack.Item.Shape.VoxelizeTexture))
            {
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), GameMath.PIHALF, 0, 0);
                mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.33f, 0.33f, 0.33f);
                mesh.Translate(0, -7.5f / 16f, 0f);
            }

            string key = getMeshCacheKey(stack);
            MeshCache[key] = mesh;

            return mesh;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
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
                sb.AppendLine(Lang.Get("necessaries:grindstone-sharpenerdurability") + " " + grindstone.Itemstack.Attributes.GetInt("durability", grindstone.Itemstack.Item.GetMaxDurability(grindstone.Itemstack)) + "/" + grindstone.Itemstack.Item.GetMaxDurability(grindstone.Itemstack));
            }
            else if (tool.Itemstack != null && grindstone.Itemstack == null)
            {
                sb.AppendLine(Lang.Get("necessaries:grindstone-tooldurability") + " " + tool.Itemstack.Attributes.GetInt("durability", tool.Itemstack.Item.GetMaxDurability(tool.Itemstack)) + "/" + tool.Itemstack.Attributes.GetInt("maxRepair", tool.Itemstack.Item.GetMaxDurability(tool.Itemstack)));
                sb.AppendLine(Lang.Get("necessaries:grindstone-place-grindstone"));
            }
            else if (tool.Itemstack != null && grindstone.Itemstack != null)
            {
                sb.AppendLine(Lang.Get("necessaries:grindstone-tooldurability") + " " + tool.Itemstack.Attributes.GetInt("durability", tool.Itemstack.Item.GetMaxDurability(tool.Itemstack)) + "/" + tool.Itemstack.Attributes.GetInt("maxRepair", tool.Itemstack.Item.GetMaxDurability(tool.Itemstack)));
                sb.AppendLine(Lang.Get("necessaries:grindstone-sharpenerdurability") + " " + grindstone.Itemstack.Attributes.GetInt("durability", grindstone.Itemstack.Item.GetMaxDurability(grindstone.Itemstack)) + "/" + grindstone.Itemstack.Item.GetMaxDurability(grindstone.Itemstack));
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
