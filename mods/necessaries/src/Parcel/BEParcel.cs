using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace necessaries.src.Parcel
{
    class BEParcel : BlockEntityContainer
    {
        public string destAddress = "";
        public string rcptAddress = "";
        public string message = "";


        internal ParcelInventory inventory;
        GuiParcel clientDialog;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }
        public virtual string DialogTitle
        {
            get { return Lang.Get("necessaries:parcel-title"); }
        }
        public override string InventoryClassName
        {
            get { return "parcel"; }
        }

        public BEParcel()
        {
            inventory = new ParcelInventory(null, null);
            inventory.SlotModified += OnSlotModified;
        }

        private void OnSlotModified(int slotid)
        {
            int countAfter = 0;
            if (!inventory.Slots[0].Empty) countAfter++;
            if (!inventory.Slots[1].Empty) countAfter++;
            if (countAfter == 0) SetBlockState("empty");
            if (countAfter == 1) SetBlockState("1slot");
            if (countAfter == 2) SetBlockState("2slot");
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("parcel-1", api);
        }

        public void OnBlockInteract(IPlayer byPlayer)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                /*
                if (clientDialog == null)
                {
                    clientDialog = new GuiParcel(DialogTitle, Inventory, Pos, Api as ICoreClientAPI);
                    clientDialog.OnClosed += () =>
                    {
                        clientDialog = null;
                        (Api as ICoreClientAPI).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                        byPlayer.InventoryManager.CloseInventory(inventory);
                    };
                }

                clientDialog.TryOpen();

                (Api as ICoreClientAPI).Network.SendPacketClient(inventory.Open(byPlayer));

                clientDialog.DefMessage = message;
                clientDialog.DefRcpt = rcptAddress;
                clientDialog.DefDest = destAddress;
                */
            }
            else
            {

                byte[] data;

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(ms);
                    TreeAttribute tree = new TreeAttribute();
                    inventory.ToTreeAttributes(tree);
                    tree.ToBytes(writer);
                    data = ms.ToArray();
                }

                ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(
                    (IServerPlayer)byPlayer,
                    Pos.X, Pos.Y, Pos.Z,
                    (int)EnumBlockStovePacket.OpenGUI,
                    data
                );

                byPlayer.InventoryManager.OpenInventory(inventory);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            rcptAddress = tree.GetString("rcptAddress");
            message = tree.GetString("message");
            destAddress = tree.GetString("destAddress");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("rcptAddress", rcptAddress);
            tree.SetString("message", message);
            tree.SetString("destAddress", destAddress);
        }

        public void SetBlockState(string state)
        {
            AssetLocation loc = Block.CodeWithVariant("state", state);
            Block block = Api.World.GetBlock(loc);
            if (block == null) return;

            Api.World.BlockAccessor.ExchangeBlock(block.Id, Pos);
            this.Block = block;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if (packetid <= 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
            }

            if (packetid == 1001)
            {
                if (data != null)
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        BinaryReader reader = new BinaryReader(ms);
                        message = reader.ReadString();
                        if (message == null) message = "";
                        destAddress = reader.ReadString();
                        if (destAddress == null) destAddress = "";
                    }
                    MarkDirty();
                }
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                if (fromPlayer.InventoryManager != null)
                {
                    fromPlayer.InventoryManager.CloseInventory(Inventory);
                }
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            base.OnReceivedServerPacket(packetid, data);

            if (packetid == (int)EnumBlockStovePacket.OpenGUI)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryReader reader = new BinaryReader(ms);
                    TreeAttribute tree = new TreeAttribute();
                    tree.FromBytes(reader);
                    Inventory.FromTreeAttributes(tree);
                    Inventory.ResolveBlocksOrItems();

                    IClientWorldAccessor clientWorld = (IClientWorldAccessor)Api.World;

                    if (clientDialog == null)
                    {
                        clientDialog = new GuiParcel(DialogTitle, Inventory, Pos, Api as ICoreClientAPI);
                        clientDialog.OnClosed += () =>
                        {
                            clientDialog = null;
                        };
                    }

                    clientDialog.TryOpen();

                    clientDialog.DefMessage = message;
                    clientDialog.DefRcpt = rcptAddress;
                    clientDialog.DefDest = destAddress;
                }
            }


            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                clientDialog?.TryClose();
                clientDialog?.Dispose();
                clientDialog = null;
            }
        }
    }
}
