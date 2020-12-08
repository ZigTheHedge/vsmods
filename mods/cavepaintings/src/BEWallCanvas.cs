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

namespace cavepaintings.src
{
    class DrawingPoint
    {
        public double X;
        public double Y;
        public byte colIndex;

        public DrawingPoint(double X, double Y, byte colIndex)
        {
            this.X = X;
            this.Y = Y;
            this.colIndex = colIndex;
        }
    }
    class BEWallCanvas : BlockEntity
    {
        PaintingRenderer renderer;
        public Dictionary<BlockFacing, List<DrawingPoint>> drawing;

        public BEWallCanvas()
        {
            drawing = new Dictionary<BlockFacing, List<DrawingPoint>>();
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if(api.Side == EnumAppSide.Client)
            {
                renderer = new PaintingRenderer(Pos, (ICoreClientAPI)api);
                DrawFromTree();
            }
        }
        public void CreateSurface(BlockFacing facing)
        {
            if (!drawing.ContainsKey(facing))
                drawing[facing] = new List<DrawingPoint>();
            if (Api != null && Api.Side == EnumAppSide.Client)
                renderer.CreateSurface(facing);
        }

        public void RemoveSurface(BlockFacing facing)
        {
            if (drawing.ContainsKey(facing))
                drawing.Remove(facing);
            if (Api != null && Api.Side == EnumAppSide.Client)
                renderer.RemoveSurface(facing);
        }
        private void DrawFromTree()
        {
            if (Api == null) return;
            if (Api.Side == EnumAppSide.Client)
            {
                renderer.Clear();
                int i;
                if (drawing.ContainsKey(BlockFacing.NORTH))
                {
                    CreateSurface(BlockFacing.NORTH);
                    for (i = 0; i < drawing[BlockFacing.NORTH].Count; i++)
                        renderer.Paint(BlockFacing.NORTH, drawing[BlockFacing.NORTH][i].X, drawing[BlockFacing.NORTH][i].Y, drawing[BlockFacing.NORTH][i].colIndex);
                }
                if (drawing.ContainsKey(BlockFacing.SOUTH))
                {
                    CreateSurface(BlockFacing.SOUTH);
                    for (i = 0; i < drawing[BlockFacing.SOUTH].Count; i++)
                        renderer.Paint(BlockFacing.SOUTH, drawing[BlockFacing.SOUTH][i].X, drawing[BlockFacing.SOUTH][i].Y, drawing[BlockFacing.SOUTH][i].colIndex);
                }
                if (drawing.ContainsKey(BlockFacing.EAST))
                {
                    CreateSurface(BlockFacing.EAST);
                    for (i = 0; i < drawing[BlockFacing.EAST].Count; i++)
                        renderer.Paint(BlockFacing.EAST, drawing[BlockFacing.EAST][i].X, drawing[BlockFacing.EAST][i].Y, drawing[BlockFacing.EAST][i].colIndex);
                }
                if (drawing.ContainsKey(BlockFacing.WEST))
                {
                    CreateSurface(BlockFacing.WEST);
                    for (i = 0; i < drawing[BlockFacing.WEST].Count; i++)
                        renderer.Paint(BlockFacing.WEST, drawing[BlockFacing.WEST][i].X, drawing[BlockFacing.WEST][i].Y, drawing[BlockFacing.WEST][i].colIndex);
                }
                if (drawing.ContainsKey(BlockFacing.UP))
                {
                    CreateSurface(BlockFacing.UP);
                    for (i = 0; i < drawing[BlockFacing.UP].Count; i++)
                        renderer.Paint(BlockFacing.UP, drawing[BlockFacing.UP][i].X, drawing[BlockFacing.UP][i].Y, drawing[BlockFacing.UP][i].colIndex);
                }
                if (drawing.ContainsKey(BlockFacing.DOWN))
                {
                    CreateSurface(BlockFacing.DOWN);
                    for (i = 0; i < drawing[BlockFacing.DOWN].Count; i++)
                        renderer.Paint(BlockFacing.DOWN, drawing[BlockFacing.DOWN][i].X, drawing[BlockFacing.DOWN][i].Y, drawing[BlockFacing.DOWN][i].colIndex);
                }
            }
        }
        public void DrawPoint(BlockFacing facing, double x, double y, byte colIndex)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                renderer.Paint(facing, x, y, colIndex);
                DrawingPoint point = new DrawingPoint(x, y, colIndex);
                if (!drawing[facing].Contains(point))
                    drawing[facing].Add(point);
            }
        }
        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            renderer?.Dispose();
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            renderer?.Dispose();
            renderer = null;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            drawing.Clear();
            int i, Size;
            
            Size = tree.GetInt("drawSize_NORTH");
            if (Size > 0) CreateSurface(BlockFacing.NORTH);
            for(i = 0; i < Size; i++)
                drawing[BlockFacing.NORTH].Add(new DrawingPoint(tree.GetDouble("N_X_" + i), tree.GetDouble("N_Y_" + i), (byte)tree.GetInt("N_color_" + i)));
            Size = tree.GetInt("drawSize_SOUTH");
            if (Size > 0) CreateSurface(BlockFacing.SOUTH);
            for (i = 0; i < Size; i++)
                drawing[BlockFacing.SOUTH].Add(new DrawingPoint(tree.GetDouble("S_X_" + i), tree.GetDouble("S_Y_" + i), (byte)tree.GetInt("S_color_" + i)));
            Size = tree.GetInt("drawSize_EAST");
            if (Size > 0) CreateSurface(BlockFacing.EAST);
            for (i = 0; i < Size; i++)
                drawing[BlockFacing.EAST].Add(new DrawingPoint(tree.GetDouble("E_X_" + i), tree.GetDouble("E_Y_" + i), (byte)tree.GetInt("E_color_" + i)));
            Size = tree.GetInt("drawSize_WEST");
            if (Size > 0) CreateSurface(BlockFacing.WEST);
            for (i = 0; i < Size; i++)
                drawing[BlockFacing.WEST].Add(new DrawingPoint(tree.GetDouble("W_X_" + i), tree.GetDouble("W_Y_" + i), (byte)tree.GetInt("W_color_" + i)));
            Size = tree.GetInt("drawSize_UP");
            if (Size > 0) CreateSurface(BlockFacing.UP);
            for (i = 0; i < Size; i++)
                drawing[BlockFacing.UP].Add(new DrawingPoint(tree.GetDouble("U_X_" + i), tree.GetDouble("U_Y_" + i), (byte)tree.GetInt("U_color_" + i)));
            Size = tree.GetInt("drawSize_DOWN");
            if (Size > 0) CreateSurface(BlockFacing.DOWN);
            for (i = 0; i < Size; i++)
                drawing[BlockFacing.DOWN].Add(new DrawingPoint(tree.GetDouble("D_X_" + i), tree.GetDouble("D_Y_" + i), (byte)tree.GetInt("D_color_" + i)));

            DrawFromTree();
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            int i;

            if (drawing.ContainsKey(BlockFacing.NORTH))
            {
                tree.SetInt("drawSize_NORTH", drawing[BlockFacing.NORTH].Count);
                for (i = 0; i < drawing[BlockFacing.NORTH].Count; i++)
                {
                    tree.SetDouble("N_X_" + i, drawing[BlockFacing.NORTH][i].X);
                    tree.SetDouble("N_Y_" + i, drawing[BlockFacing.NORTH][i].Y);
                    tree.SetInt("N_color_" + i, drawing[BlockFacing.NORTH][i].colIndex);
                }
            }
            if (drawing.ContainsKey(BlockFacing.SOUTH))
            {
                tree.SetInt("drawSize_SOUTH", drawing[BlockFacing.SOUTH].Count);
                for (i = 0; i < drawing[BlockFacing.SOUTH].Count; i++)
                {
                    tree.SetDouble("S_X_" + i, drawing[BlockFacing.SOUTH][i].X);
                    tree.SetDouble("S_Y_" + i, drawing[BlockFacing.SOUTH][i].Y);
                    tree.SetInt("S_color_" + i, drawing[BlockFacing.SOUTH][i].colIndex);
                }
            }
            if (drawing.ContainsKey(BlockFacing.EAST))
            {
                tree.SetInt("drawSize_EAST", drawing[BlockFacing.EAST].Count);
                for (i = 0; i < drawing[BlockFacing.EAST].Count; i++)
                {
                    tree.SetDouble("E_X_" + i, drawing[BlockFacing.EAST][i].X);
                    tree.SetDouble("E_Y_" + i, drawing[BlockFacing.EAST][i].Y);
                    tree.SetInt("E_color_" + i, drawing[BlockFacing.EAST][i].colIndex);
                }
            }
            if (drawing.ContainsKey(BlockFacing.WEST))
            {
                tree.SetInt("drawSize_WEST", drawing[BlockFacing.WEST].Count);
                for (i = 0; i < drawing[BlockFacing.WEST].Count; i++)
                {
                    tree.SetDouble("W_X_" + i, drawing[BlockFacing.WEST][i].X);
                    tree.SetDouble("W_Y_" + i, drawing[BlockFacing.WEST][i].Y);
                    tree.SetInt("W_color_" + i, drawing[BlockFacing.WEST][i].colIndex);
                }
            }
            if (drawing.ContainsKey(BlockFacing.UP))
            {
                tree.SetInt("drawSize_UP", drawing[BlockFacing.UP].Count);
                for (i = 0; i < drawing[BlockFacing.UP].Count; i++)
                {
                    tree.SetDouble("U_X_" + i, drawing[BlockFacing.UP][i].X);
                    tree.SetDouble("U_Y_" + i, drawing[BlockFacing.UP][i].Y);
                    tree.SetInt("U_color_" + i, drawing[BlockFacing.UP][i].colIndex);
                }
            }
            if (drawing.ContainsKey(BlockFacing.DOWN))
            {
                tree.SetInt("drawSize_DOWN", drawing[BlockFacing.DOWN].Count);
                for (i = 0; i < drawing[BlockFacing.DOWN].Count; i++)
                {
                    tree.SetDouble("D_X_" + i, drawing[BlockFacing.DOWN][i].X);
                    tree.SetDouble("D_Y_" + i, drawing[BlockFacing.DOWN][i].Y);
                    tree.SetInt("D_color_" + i, drawing[BlockFacing.DOWN][i].colIndex);
                }
            }
        }
        public void SendToServer(BlockFacing facing)
        {
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                TreeAttribute tree = new TreeAttribute();
                tree.SetString("facing", facing.Code);
                tree.SetInt("drawSize", drawing[facing].Count);
                for (int i = 0; i < drawing[facing].Count; i++)
                {
                    tree.SetDouble("X_" + i, drawing[facing][i].X);
                    tree.SetDouble("Y_" + i, drawing[facing][i].Y);
                    tree.SetInt("color_" + i, drawing[facing][i].colIndex);
                }
                tree.ToBytes(writer);
                data = ms.ToArray();
            }
            ((ICoreClientAPI)Api).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 101, data);
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            base.OnReceivedClientPacket(fromPlayer, packetid, data);
            if(packetid == 101)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryReader reader = new BinaryReader(ms);
                    TreeAttribute tree = new TreeAttribute();
                    tree.FromBytes(reader);
                    BlockFacing facing = BlockFacing.FromCode(tree.GetString("facing"));
                    CreateSurface(facing);
                    drawing[facing].Clear();
                    int Size = tree.GetInt("drawSize");
                    for (int i = 0; i < Size; i++)
                    {
                        drawing[facing].Add(new DrawingPoint(tree.GetDouble("X_" + i), tree.GetDouble("Y_" + i), (byte)tree.GetInt("color_" + i)));
                    }
                }
                MarkDirty();
            }
        }
    }
}
