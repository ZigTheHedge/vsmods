using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace fancydoors.src
{
    class BEFancyDoorPart : BlockEntityContainer
    {
        public enum EnumState
        {
            IDLE,
            OPENING,
            CLOSING
        }

        internal ChiseledInventory inventory;
        public bool isOpened = false;
        public byte meshState = 0;
        public EnumState enumState = EnumState.IDLE;
        public float delta = 0;
        DoorRenderer doorRenderer;
        protected MeshData[] meshes;

        public BEFancyDoorPart()
        {
            inventory = new ChiseledInventory(null, null);
            meshes = new MeshData[2];
        }

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "chiseledinventory"; }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("chiseledinventory-1", api);
            if (api is ICoreClientAPI cApi)
            {
                doorRenderer = new DoorRenderer(Pos, cApi, this);
                meshes[0] = getMesh(0);
                meshes[1] = getMesh(1);
                doorRenderer.UpdateDynamic(inventory[0].Itemstack, getMesh(0));
                doorRenderer.UpdateStatic(inventory[1].Itemstack, getMesh(1));
                meshes[0] = TransformMesh(meshes[0]);
                meshes[1] = TransformMesh(meshes[1]);
            }
        }

        public void TouchMyBEE(IWorldAccessor world, BlockPos pos, string ignoreSide)
        {
            BEFancyDoorPart be = world.BlockAccessor.GetBlockEntity(pos) as BEFancyDoorPart;
            if (be != null) be.OpenCloseDoor(world, ignoreSide);
        }

        public void OpenCloseDoor(IWorldAccessor world, string ignoreSide)
        {
            if (world.BlockAccessor.GetBlock(Pos.UpCopy()).FirstCodePart() == "fancydoor" && ignoreSide != "up") TouchMyBEE(world, Pos.UpCopy(), "down");
            if (world.BlockAccessor.GetBlock(Pos.DownCopy()).FirstCodePart() == "fancydoor" && ignoreSide != "down") TouchMyBEE(world, Pos.DownCopy(), "up");
            else
            {
                if (ignoreSide != "down")
                {
                    if (world.BlockAccessor.GetBlock(Pos.NorthCopy()).FirstCodePart() == "fancydoor" && ignoreSide != "north") TouchMyBEE(world, Pos.NorthCopy(), "south");
                    if (world.BlockAccessor.GetBlock(Pos.SouthCopy()).FirstCodePart() == "fancydoor" && ignoreSide != "south") TouchMyBEE(world, Pos.SouthCopy(), "north");
                    if (world.BlockAccessor.GetBlock(Pos.EastCopy()).FirstCodePart() == "fancydoor" && ignoreSide != "east") TouchMyBEE(world, Pos.EastCopy(), "west");
                    if (world.BlockAccessor.GetBlock(Pos.WestCopy()).FirstCodePart() == "fancydoor" && ignoreSide != "west") TouchMyBEE(world, Pos.WestCopy(), "east");
                }
            }

            isOpened = !isOpened;


            if (!isOpened) enumState = EnumState.CLOSING;
            else enumState = EnumState.OPENING;

            if(world.Side == EnumAppSide.Server)
            {
                ICoreServerAPI sApi = (ICoreServerAPI)Api;
                sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1002, BitConverter.GetBytes(isOpened));
            }    

            MarkDirty(true);
        }

        public bool GenMeshOfChiseledBlock(ItemStack itemStack, out MeshData mesh)
        {
            ICoreClientAPI capi = Api as ICoreClientAPI;
            mesh = null;
            if (capi == null) return false;

            if (itemStack.Class != EnumItemClass.Block || !(itemStack.Block is BlockChisel))
                return false;

            ITreeAttribute tree = itemStack.Attributes ?? new TreeAttribute();
            int[] materials = BlockEntityMicroBlock.MaterialIdsFromAttributes(tree, capi.World);
            uint[] numArray = null;
            if (tree["cuboids"] is IntArrayAttribute intArrayAttribute)
                numArray = intArrayAttribute.AsUint;
            if (numArray == null && tree["cuboids"] is LongArrayAttribute longArrayAttribute)
                numArray = longArrayAttribute.AsUint;

            List<uint> voxelCuboids = numArray != null ? new List<uint>(numArray) : new List<uint>();
            mesh = BlockEntityMicroBlock.CreateMesh(capi, voxelCuboids, materials);
            if (mesh == null)
                return false;

            for (int index = 0; index < mesh.GetVerticesCount(); ++index)
                mesh.Flags[index] &= -256;

            for (int index = 0; index < mesh.RenderPassCount; ++index)
            {
                if (mesh.RenderPasses[index] != 3)
                    mesh.RenderPasses[index] = 1;
            }
            //mesh.GetRgba

            return true;
        }

        private MeshData getMesh(int index)
        {
            MeshData mesh;
            ICoreClientAPI capi = Api as ICoreClientAPI;
            ItemStack stack = inventory[index].Itemstack;
            if (stack == null) return null;

            if (stack.Class == EnumItemClass.Block && stack.Block is BlockChisel)
            {
                GenMeshOfChiseledBlock(stack, out mesh);
                return mesh;
            }
            return null;
        }

        public MeshData TransformMesh(MeshData meshData)
        {
            if (meshData == null) return null;
            ModelTransform transform;
            transform = ModelTransform.NoTransform;
            transform.EnsureDefaultValues();

            transform.Origin.X = 0.5f;
            transform.Origin.Y = 0;
            transform.Origin.Z = 0.5f;

            transform.Rotation.X = 0;
            transform.Rotation.Y = Block.Shape.rotateY;
            transform.Rotation.Z = 0;

            transform.Scale = 1f;
            meshData.ModelTransform(transform);
            return meshData;
        }

        public byte[] SyncInventory()
        {
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(true);
                TreeAttribute tree = new TreeAttribute();
                inventory.ToTreeAttributes(tree);
                tree.ToBytes(writer);
                data = ms.ToArray();
            }
            
            return data;
        }

        public bool OnInteract(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

            EnumAppSide side = world.Side;

            if (byPlayer.WorldData.EntityControls.Sneak)
            {

                if (slot.Empty)
                {
                    if (!inventory[1].Empty)
                    {
                        ItemStack stack = inventory[1].TakeOut(1);
                        Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                        // Desync!!!!
                        //bool result = StaticUtils.TryGiveItemstackToPlayer(world, byPlayer, ref stack);
                        meshes[1] = getMesh(1);
                        MarkDirty(true);
                        doorRenderer?.UpdateStatic(inventory[1].Itemstack, getMesh(1));
                        meshes[1] = TransformMesh(meshes[1]);
                        if (side == EnumAppSide.Server)
                        {
                            ICoreServerAPI sApi = (ICoreServerAPI)Api;
                            sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001, SyncInventory());
                        }
                        return true;
                    }
                    if (!inventory[0].Empty)
                    {
                        ItemStack stack = inventory[0].TakeOut(1);
                        Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                        // Desync!!!!
                        //bool result = StaticUtils.TryGiveItemstackToPlayer(world, byPlayer, ref stack);
                        meshes[0] = getMesh(0);
                        MarkDirty(true);
                        doorRenderer?.UpdateDynamic(inventory[0].Itemstack, getMesh(0));
                        meshes[0] = TransformMesh(meshes[0]);
                        if (side == EnumAppSide.Server)
                        {
                            ICoreServerAPI sApi = (ICoreServerAPI)Api;
                            sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001, SyncInventory());
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (ChiseledSlot.IsChiseled(slot))
                {
                    if (inventory[0].Empty)
                    {
                        int res = slot.TryPutInto(Api.World, inventory[0], 1);
                        meshes[0] = getMesh(0);
                        MarkDirty(true);
                        doorRenderer?.UpdateDynamic(inventory[0].Itemstack, getMesh(0));
                        meshes[0] = TransformMesh(meshes[0]);
                        if (side == EnumAppSide.Server)
                        {
                            ICoreServerAPI sApi = (ICoreServerAPI)Api;
                            sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001, SyncInventory());
                        }
                        return true;
                    }
                    if (inventory[1].Empty)
                    {
                        int res = slot.TryPutInto(Api.World, inventory[1], 1);
                        meshes[1] = getMesh(1);
                        MarkDirty(true);
                        doorRenderer?.UpdateStatic(inventory[1].Itemstack, getMesh(1));
                        meshes[1] = TransformMesh(meshes[1]);
                        if (side == EnumAppSide.Server)
                        {
                            ICoreServerAPI sApi = (ICoreServerAPI)Api;
                            sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001, SyncInventory());
                        }
                        return true;
                    }
                }
            }
            

            OpenCloseDoor(world, "");

            if (world.Side == EnumAppSide.Server)
                Api.World.PlaySoundAt(new AssetLocation("game:sounds/block/door.ogg"), Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f);

            return true;
        }

        public Cuboidf GetCollision()
        {
            float width = 0;
            if (Block.Variant["size"] == "2wide") width = 2f / 16f;
            if (Block.Variant["size"] == "3wide") width = 3f / 16f;
            if (Block.Variant["size"] == "4wide") width = 4f / 16f;
            if (Block.Variant["size"] == "5wide") width = 5f / 16f;

            if (Block.Variant["horizontalorientation"] == "north")
            {
                if (!isOpened) return new Cuboidf(0, 0, 1f - width, 1f, 1f, 1f);
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf(1f - width, 0, 0, 1f, 1f, 1f);
                    else return new Cuboidf(0, 0, 0, width, 1f, 1f);
                }
            }
            if (Block.Variant["horizontalorientation"] == "east")
            {
                if (!isOpened) return new Cuboidf(0, 0, 0, width, 1f, 1f);
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf(0, 0, 1f - width, 1f, 1f, 1f);
                    else return new Cuboidf(0, 0, 0, 1f, 1f, width);
                }
            }
            if (Block.Variant["horizontalorientation"] == "south")
            {
                if (!isOpened) return new Cuboidf(0, 0, 0, 1f, 1f, width);
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf(0, 0, 0, width, 1f, 1f);
                    else return new Cuboidf(1f - width, 0, 0, 1f, 1f, 1f);
                }
            }
            if (Block.Variant["horizontalorientation"] == "west")
            {
                if (!isOpened) return new Cuboidf(1f - width, 0, 0, 1f, 1f, 1f);
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf(0, 0, 0, 1f, 1f, width);
                    else return new Cuboidf(0, 0, 1f - width, 1f, 1f, 1f);
                }
            }
            return null;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if(packetid == 1000)
            {
                delta = BitConverter.ToSingle(data, 0);
                enumState = EnumState.IDLE;
                if (fromPlayer is IServerPlayer)
                {
                    //((IServerPlayer)fromPlayer).SendMessage(0, "PacketReceived. delta = " + delta, EnumChatType.AllGroups);
                }
                MarkDirty(true);            
            } else
                base.OnReceivedClientPacket(fromPlayer, packetid, data);
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if (packetid == 1001)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryReader reader = new BinaryReader(ms);
                    bool isOwner = reader.ReadBoolean();
                    TreeAttribute tree = new TreeAttribute();
                    tree.FromBytes(reader);
                    Inventory.FromTreeAttributes(tree);
                    Inventory.ResolveBlocksOrItems();
                }

                meshes[0] = getMesh(0);
                meshes[1] = getMesh(1);
                doorRenderer?.UpdateDynamic(inventory[0].Itemstack, getMesh(0));
                doorRenderer?.UpdateStatic(inventory[1].Itemstack, getMesh(1));
                meshes[0] = TransformMesh(meshes[0]);
                meshes[1] = TransformMesh(meshes[1]);
                MarkDirty(true);
            } else if (packetid == 1002)
            {
                if (BitConverter.ToBoolean(data, 0))
                {
                    enumState = EnumState.OPENING;
                } else
                    enumState = EnumState.CLOSING;
                    //MarkDirty(true);
            } else
                base.OnReceivedServerPacket(packetid, data);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            isOpened = tree.GetBool("isOpened");
            if(delta == 0 || delta == 90) delta = tree.GetFloat("delta");
            /*
            int es = tree.GetInt("enumState");
            if (es == 0) enumState = EnumState.IDLE;
            if (es == 1) enumState = EnumState.CLOSING;
            if (es == 2) enumState = EnumState.OPENING;
            */
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBool("isOpened", isOpened);
            tree.SetFloat("delta", delta);
            /*
            if (enumState == EnumState.IDLE) tree.SetInt("enumState", 0);
            if (enumState == EnumState.CLOSING) tree.SetInt("enumState", 1);
            if (enumState == EnumState.OPENING) tree.SetInt("enumState", 2);
            */
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            doorRenderer?.Dispose();
            doorRenderer = null;
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            doorRenderer?.Dispose();
            doorRenderer = null;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {

        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {

            if (!isOpened && enumState == EnumState.IDLE)
            {
                if (meshes[0] != null) mesher.AddMeshData(meshes[0]);
                if (meshes[1] != null) mesher.AddMeshData(meshes[1]);
                if(meshState == 0) meshState = 1;
                if (!Inventory[0].Empty) return true;
                else return false;
            }
            else
            {
                meshState = 0;
                if (!Inventory[0].Empty) return true;
                else return false;
            }
        }
    }
}

