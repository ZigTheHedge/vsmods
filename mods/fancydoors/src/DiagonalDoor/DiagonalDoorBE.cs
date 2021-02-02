using fancydoors.src.RegularDoor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace fancydoors.src.DiagonalDoor
{
    class DiagonalDoorBE : BEDoorPart
    {

        public override void LateInitialize()
        {

            inventory.LateInitialize("chiseledinventory-1", Api);
            if (Api is ICoreClientAPI cApi)
            {
                doorRenderer = new DiagonalDoorRenderer(Pos, cApi, this);
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
            if (world.BlockAccessor.GetBlock(Pos.UpCopy().NorthCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "upnorth") TouchMyBEE(world, Pos.UpCopy().NorthCopy(), "downsouth");
            if (world.BlockAccessor.GetBlock(Pos.DownCopy().SouthCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "downsouth") TouchMyBEE(world, Pos.DownCopy().SouthCopy(), "upnorth");
            if (world.BlockAccessor.GetBlock(Pos.UpCopy().EastCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "upeast") TouchMyBEE(world, Pos.UpCopy().EastCopy(), "downwest");
            if (world.BlockAccessor.GetBlock(Pos.DownCopy().WestCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "downwest") TouchMyBEE(world, Pos.DownCopy().WestCopy(), "upeast");
            if (world.BlockAccessor.GetBlock(Pos.UpCopy().SouthCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "upsouth") TouchMyBEE(world, Pos.UpCopy().SouthCopy(), "downnorth");
            if (world.BlockAccessor.GetBlock(Pos.DownCopy().NorthCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "downnorth") TouchMyBEE(world, Pos.DownCopy().NorthCopy(), "upsouth");
            if (world.BlockAccessor.GetBlock(Pos.UpCopy().WestCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "upwest") TouchMyBEE(world, Pos.UpCopy().WestCopy(), "downeast");
            if (world.BlockAccessor.GetBlock(Pos.DownCopy().EastCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "downeast") TouchMyBEE(world, Pos.DownCopy().EastCopy(), "upwest");
            else
            {
                if (!ignoreSide.StartsWith("down"))
                {
                    if (world.BlockAccessor.GetBlock(Pos.NorthCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "north") TouchMyBEE(world, Pos.NorthCopy(), "south");
                    if (world.BlockAccessor.GetBlock(Pos.SouthCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "south") TouchMyBEE(world, Pos.SouthCopy(), "north");
                    if (world.BlockAccessor.GetBlock(Pos.EastCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "east") TouchMyBEE(world, Pos.EastCopy(), "west");
                    if (world.BlockAccessor.GetBlock(Pos.WestCopy()).FirstCodePart() == "diagonaldoor" && ignoreSide != "west") TouchMyBEE(world, Pos.WestCopy(), "east");
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

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {

            if (!isOpened && enumState == EnumState.IDLE)
            {
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

        public Cuboidf[] GetCollisions()
        {
            float width = 0;
            if (Block.Variant["size"] == "2wide") width = 2f / 16f;
            if (Block.Variant["size"] == "3wide") width = 3f / 16f;
            if (Block.Variant["size"] == "4wide") width = 4f / 16f;
            if (Block.Variant["size"] == "5wide") width = 5f / 16f;

            float oneSixteen = 1f / 16f;

            if (Block.Variant["horizontalorientation"] == "north")
            {
                if (!isOpened)
                {
                    Cuboidf[] toRet = new Cuboidf[16];
                    for(int i = 0; i < 16; i++)
                    {
                        toRet[i] = new Cuboidf(0, oneSixteen * i, oneSixteen * (16 - i - 1), 1f, oneSixteen * (i + 1), oneSixteen * (16 - i));
                    }
                    return toRet;
                }
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf[] { new Cuboidf(1f - width, 0, 0, 1f, 1f, 1f) };
                    else return new Cuboidf[] { new Cuboidf(0, 0, 0, width, 1f, 1f) };
                }
            }
            if (Block.Variant["horizontalorientation"] == "east")
            {
                if (!isOpened)
                {
                    Cuboidf[] toRet = new Cuboidf[16];
                    for (int i = 0; i < 16; i++)
                    {
                        toRet[i] = new Cuboidf(oneSixteen * i, oneSixteen * i, 0, oneSixteen * (i + 1), oneSixteen * i, 1f);
                    }
                    return toRet;
                }
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf[] { new Cuboidf(0, 0, 1f - width, 1f, 1f, 1f) };
                    else return new Cuboidf[] { new Cuboidf(0, 0, 0, 1f, 1f, width) };
                }
            }
            if (Block.Variant["horizontalorientation"] == "south")
            {
                if (!isOpened)
                {
                    Cuboidf[] toRet = new Cuboidf[16];
                    for (int i = 0; i < 16; i++)
                    {
                        toRet[i] = new Cuboidf(0, oneSixteen * i, oneSixteen * i, 1f, oneSixteen * (i + 1), oneSixteen * (i + 1));
                    }
                    return toRet;
                }
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf[] { new Cuboidf(0, 0, 0, width, 1f, 1f) };
                    else return new Cuboidf[] { new Cuboidf(1f - width, 0, 0, 1f, 1f, 1f) };
                }
            }
            if (Block.Variant["horizontalorientation"] == "west")
            {
                if (!isOpened)
                {
                    Cuboidf[] toRet = new Cuboidf[16];
                    for (int i = 0; i < 16; i++)
                    {
                        toRet[i] = new Cuboidf(oneSixteen * (16 - i - 1), oneSixteen * i, 0, oneSixteen * (16 - i), oneSixteen * (i + 1), 1f);
                    }
                    return toRet;
                }
                else
                {
                    if (Block.Variant["orientation"] == "right") return new Cuboidf[] { new Cuboidf(0, 0, 0, 1f, 1f, width) };
                    else return new Cuboidf[] { new Cuboidf(0, 0, 1f - width, 1f, 1f, 1f) };
                }
            }
            return null;
        }
    }
}
