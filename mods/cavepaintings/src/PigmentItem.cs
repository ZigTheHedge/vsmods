using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cavepaintings.src
{
    class PigmentItem : ItemOre
    {
        /*
            z = 1 - looking North
            z = 0 - looking South
            x = 1 - looking West
            x = 0 - looking East
        */
        BlockPos lastTriggered;
        BlockFacing lastFacing;
        public static BlockFacing GetFacing(Vec3d hitPosition)
        {
            BlockFacing ret;
            if (hitPosition.Z == 1) { ret = BlockFacing.NORTH; }
            else if (hitPosition.Z == 0) { ret = BlockFacing.SOUTH; }
            else if (hitPosition.X == 1) { ret = BlockFacing.WEST; }
            else if (hitPosition.X == 0) { ret = BlockFacing.EAST; }
            else if (hitPosition.Y == 1) { ret = BlockFacing.DOWN; }
            else { ret = BlockFacing.UP; }

            return ret;
        }
        public static BEWallCanvas AccessSurface(IWorldAccessor world, BlockPos pos, BlockFacing facing)
        {
            BlockPos blockPos = pos;
            if (facing == BlockFacing.NORTH) { blockPos.South(); }
            else if (facing == BlockFacing.SOUTH) { blockPos.North(); }
            else if (facing == BlockFacing.WEST) { blockPos.East(); }
            else if (facing == BlockFacing.EAST) { blockPos.West(); }
            else if (facing == BlockFacing.UP) { blockPos.Down(); }
            else { blockPos.Up(); }
            Block block = world.GetBlock(new AssetLocation("cavepaintings:wallcanvas"));
            if (!world.BlockAccessor.GetBlock(blockPos).Code.FirstPathPart().StartsWith("wallcanvas"))
            {
                if (world.BlockAccessor.GetBlock(blockPos).Code.FirstPathPart().StartsWith("air"))
                    world.BlockAccessor.SetBlock(block.BlockId, blockPos);
                else
                    return null;
            }
            BEWallCanvas be = world.BlockAccessor.GetBlockEntity(blockPos) as BEWallCanvas;
            if (be == null) return be;
            be.CreateSurface(facing.Opposite);
            return be;
        }

        public int GetColIndex()
        {
            if (Variant.ContainsKey("col"))
            {
                if (Variant["col"] == "red") return 1;
                else if (Variant["col"] == "blue") return 2;
                else if (Variant["col"] == "green") return 3;
                else if (Variant["col"] == "white") return 4;
                return 0;
            } else
            {
                return -1;
            }
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            if (blockSel == null) return;
            if (((EntityPlayer)byEntity).Player.WorldData.EntityControls.Sneak) return;
            if (!byEntity.World.BlockAccessor.GetBlock(blockSel.Position).SideSolid[GetFacing(blockSel.HitPosition).Index]) return;
            if (api.Side == EnumAppSide.Server)
            {
                if(GetColIndex() != -1)
                    AccessSurface(byEntity.World, blockSel.Position.Copy(), GetFacing(blockSel.HitPosition));
            } else
            {
                lastTriggered = blockSel.Position.Copy();
                lastFacing = GetFacing(blockSel.HitPosition);
            }
            handling = EnumHandHandling.Handled;
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            bool updateLast = false;
            if (blockSel == null || blockSel.Position != lastTriggered)
            {
                if (api.Side == EnumAppSide.Client)
                {
                    updateLast = true;
                    BEWallCanvas lastBE = AccessSurface(byEntity.World, lastTriggered, lastFacing);
                    if (lastBE != null) lastBE.SendToServer(lastFacing.Opposite);
                }
            }
            if (blockSel == null) return false;
            if (!byEntity.World.BlockAccessor.GetBlock(blockSel.Position).SideSolid[GetFacing(blockSel.HitPosition).Index]) return false;

            int colIndex = GetColIndex();
            if (colIndex == -1) return false;

            BlockFacing facing = GetFacing(blockSel.HitPosition);

            BEWallCanvas be = AccessSurface(byEntity.World, blockSel.Position.Copy(), facing);
            facing = facing.Opposite;
            if (be != null)
            {
                if (facing == BlockFacing.NORTH)
                    be.DrawPoint(facing, 1d - blockSel.HitPosition.X, blockSel.HitPosition.Y, (byte)colIndex);
                else if (facing == BlockFacing.SOUTH)
                    be.DrawPoint(facing, blockSel.HitPosition.X, blockSel.HitPosition.Y, (byte)colIndex);
                else if (facing == BlockFacing.EAST)
                    be.DrawPoint(facing, 1d - blockSel.HitPosition.Z, blockSel.HitPosition.Y, (byte)colIndex);
                else if (facing == BlockFacing.WEST)
                    be.DrawPoint(facing, blockSel.HitPosition.Z, blockSel.HitPosition.Y, (byte)colIndex);
                else if (facing == BlockFacing.UP)
                    be.DrawPoint(facing, 1d - blockSel.HitPosition.X, blockSel.HitPosition.Z, (byte)colIndex);
                else
                    be.DrawPoint(facing, 1d - blockSel.HitPosition.X, 1d - blockSel.HitPosition.Z, (byte)colIndex);
            }
            if(updateLast)
            {
                if(api.Side == EnumAppSide.Client)
                {
                    lastTriggered = blockSel.Position.Copy();
                    lastFacing = GetFacing(blockSel.HitPosition);
                }
            }

            return true;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            if (blockSel == null) return;
            if (!byEntity.World.BlockAccessor.GetBlock(blockSel.Position).SideSolid[GetFacing(blockSel.HitPosition).Index]) return;
            if (GetColIndex() == -1) return;
            BlockFacing facing = GetFacing(blockSel.HitPosition);

            BEWallCanvas be = AccessSurface(byEntity.World, blockSel.Position.Copy(), facing);

            if (api.Side == EnumAppSide.Server)
            {
            }
            else
            {
                facing = facing.Opposite;
                if (be != null) be.SendToServer(facing);
            }
        }
    }
}
