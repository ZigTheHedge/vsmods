using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace tradeomat.src.Coins
{
    class BECoinsPile : BlockEntityItemPile, IBlockEntityItemPile
    {
        internal AssetLocation soundLocation = new AssetLocation("tradeomat:sounds/coin");

        public override AssetLocation SoundLocation { get { return soundLocation; } }

        public override string BlockCode
        {
            get { return "coinspile"; }
        }

        public override int DefaultTakeQuantity
        {
            get { return 4; }
        }

        public override int BulkTakeQuantity
        {
            get { return 16; }
        }

        public override int MaxStackSize { get { return 256; } }


        MeshData[] meshes
        {
            get
            {
                return ObjectCacheUtil.GetOrCreate(Api, "coinspile-meshes-" + Block.Variant["metal"], () =>
                {
                    MeshData[] meshes = new MeshData[65];

                    Block block = Api.World.BlockAccessor.GetBlock(Pos);

                    Shape shape = Api.Assets.TryGet("tradeomat:shapes/block/coinspile.json").ToObject<Shape>();

                    ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;

                    for (int j = 0; j <= 64; j++)
                    {
                        mesher.TesselateShape(block, shape, out meshes[j], null, j * 30);
                    }

                    return meshes;
                });
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RandomizeSoundPitch = true;
        }

        public override bool OnTesselation(ITerrainMeshPool meshdata, ITesselatorAPI tesselator)
        {
            lock (inventoryLock)
            {
                int index = Math.Min(64, (int)Math.Ceiling(inventory[0].StackSize / 4.0));
                meshdata.AddMeshData(meshes[index]);
            }

            return true;
        }

        public override bool OnPlayerInteract(IPlayer byPlayer)
        {
            BlockPos abovePos = Pos.UpCopy();

            BlockEntity be = Api.World.BlockAccessor.GetBlockEntity(abovePos);
            if (be is BlockEntityItemPile)
            {
                return ((BlockEntityItemPile)be).OnPlayerInteract(byPlayer);
            }

            bool sneaking = byPlayer.Entity.Controls.Sneak;


            ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            bool equalStack = hotbarSlot.Itemstack != null && hotbarSlot.Itemstack.Equals(Api.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes);

            if (sneaking && !equalStack)
            {
                return false;
            }

            if (sneaking && equalStack && OwnStackSize >= MaxStackSize)
            {
                return false;
            }

            lock (inventoryLock)
            {
                if (sneaking)
                {
                    return TryPutItem(byPlayer);
                }
                else
                {
                    return TryTakeItem(byPlayer);
                }
            }
        }
    }
}
