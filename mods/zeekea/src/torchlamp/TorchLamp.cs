using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using zeekea.src.multirotatable;

namespace zeekea.src.torchlamp
{
    class TorchLamp : BlockMultiRotatable
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            int count = int.Parse(Variant["count"]);
            if (slot.Empty)
            {
                if (count == 0) return true;
                count--;
                world.SpawnItemEntity(new ItemStack(world.GetBlock(new AssetLocation("game", "torch-up"))), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                Block newBlock = world.BlockAccessor.GetBlock(new AssetLocation("zeekea", "torchlamp-" + count.ToString()));
                world.BlockAccessor.ExchangeBlock(newBlock.BlockId, blockSel.Position);
                return true;
            }
            else if(slot.Itemstack.Collectible.Code.FirstPathPart().StartsWith("torch"))
            {
                if (count < 4)
                {
                    count++;
                    slot.TakeOut(1);
                    Block newBlock = world.BlockAccessor.GetBlock(new AssetLocation("zeekea", "torchlamp-" + count.ToString()));
                    world.BlockAccessor.ExchangeBlock(newBlock.BlockId, blockSel.Position);
                    return true;
                }
                else
                    return true;
            } else
                return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            int count = int.Parse(Variant["count"]);
            if(count > 0)
            {
                for(int i = 0; i < count; i++)
                    world.SpawnItemEntity(new ItemStack(world.GetBlock(new AssetLocation("game", "torch-up"))), pos.ToVec3d().Add(0.5, 0.5, 0.5));

            }
            base.OnBlockRemoved(world, pos);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "count", "0" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }
    }
}
