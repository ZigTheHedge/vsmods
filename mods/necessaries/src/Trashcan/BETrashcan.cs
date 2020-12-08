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

namespace necessaries.src.Trashcan
{
    class BETrashcan : BlockEntityContainer
    {
        private bool trashMoving = false;

        internal TrashcanInventory inventory;
        GuiTrashcan clientDialog;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }
        public virtual string DialogTitle
        {
            get { return Lang.Get("necessaries:trashcan-title"); }
        }
        public override string InventoryClassName
        {
            get { return "trashcan"; }
        }

        public BETrashcan()
        {
            inventory = new TrashcanInventory(null, null);
            inventory.SlotModified += OnSlotModified;
        }

        private void MoveTrashDown(bool putIn, int startFrom = 3)
        {
            trashMoving = true;
            if (putIn)
            {
                if (inventory[1].Itemstack == null) {
                    inventory[1].Itemstack = inventory[0].Itemstack;
                    inventory[1].MarkDirty();
                    inventory[0].Itemstack = null;
                    inventory[0].MarkDirty();
                    trashMoving = false;
                    return; 
                }
            }
            int curSlot = startFrom;
            while(curSlot > 0)
            {
                if(inventory[curSlot].Itemstack != null)
                {
                    bool freeSlotFound = false;
                    for(int i = curSlot - 1; i > 0; i--)
                        if(inventory[i].Itemstack == null) { freeSlotFound = true; break; }
                    if(!freeSlotFound)
                    {
                        for(int i = curSlot; i > 1; i--)
                        {
                            inventory[i].Itemstack = inventory[i - 1].Itemstack;
                            inventory[i].MarkDirty();
                            inventory[i - 1].Itemstack = null;
                            inventory[i - 1].MarkDirty();
                        }
                    }
                } else
                {
                    if (curSlot == 1)
                    {
                        inventory[1].Itemstack = inventory[0].Itemstack;
                        inventory[1].MarkDirty();
                        inventory[0].Itemstack = null;
                        inventory[0].MarkDirty();
                    }
                    else
                    {
                        for (int i = curSlot; i > 1; i--)
                        {
                            inventory[i].Itemstack = inventory[i - 1].Itemstack;
                            inventory[i].MarkDirty();
                            inventory[i - 1].Itemstack = null;
                            inventory[i - 1].MarkDirty();
                        }
                    }
                }
                curSlot--;
            }
            trashMoving = false;
        }

        private void OnSlotModified(int slotid)
        {
            if (Api.World.Side == EnumAppSide.Client) return;
            if (trashMoving) return;
            if (slotid == 0)
            {
                if(inventory[0].Itemstack != null)
                {
                    MoveTrashDown(true);
                }
            } else
            {
                MoveTrashDown(false, slotid);
            }
            
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("trashcan-1", api);
        }

        public void OnBlockInteract(IPlayer byPlayer)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                /*
                if (clientDialog == null)
                {
                    clientDialog = new GuiTrashcan(DialogTitle, Inventory, Pos, Api as ICoreClientAPI);
                    clientDialog.OnClosed += () =>
                    {
                        clientDialog = null;
                        (Api as ICoreClientAPI).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                        byPlayer.InventoryManager.CloseInventory(inventory);
                    };
                }

                clientDialog.TryOpen();

                (Api as ICoreClientAPI).Network.SendPacketClient(inventory.Open(byPlayer));
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
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if (packetid <= 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
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
                        clientDialog = new GuiTrashcan(DialogTitle, Inventory, Pos, Api as ICoreClientAPI);
                        clientDialog.OnClosed += () =>
                        {
                            clientDialog = null;
                        };
                    }

                    clientDialog.TryOpen();
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
