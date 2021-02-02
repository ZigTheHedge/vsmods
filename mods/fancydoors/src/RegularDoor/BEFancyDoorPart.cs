using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace fancydoors.src.RegularDoor
{
    class BEFancyDoorPart : BEDoorPart
    {
        public override void LateInitialize()
        {
            inventory.LateInitialize("chiseledinventory-1", Api);
            if (Api is ICoreClientAPI cApi)
            {
                doorRenderer = new FancyDoorRenderer(Pos, cApi, this);
                meshes[0] = getMesh(0);
                meshes[1] = getMesh(1);
                doorRenderer.UpdateDynamic(inventory[0].Itemstack, getMesh(0));
                doorRenderer.UpdateStatic(inventory[1].Itemstack, getMesh(1));
                meshes[0] = TransformMesh(meshes[0]);
                meshes[1] = TransformMesh(meshes[1]);
            }
        }

        public override void OpenCloseDoor(IWorldAccessor world, string ignoreSide)
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

            if (world.Side == EnumAppSide.Server)
            {
                ICoreServerAPI sApi = (ICoreServerAPI)Api;
                sApi.Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1002, BitConverter.GetBytes(isOpened));
            }

            MarkDirty(true);
        }
    }
}
