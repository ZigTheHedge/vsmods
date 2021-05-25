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
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace necessaries.src.Mailbox
{
    class BEMailBox : BlockEntityContainer
    {
        string address = "";

        internal MailBoxInventory inventory;
        GuiDialogMailbox clientDialog;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }
        public virtual string DialogTitle
        {
            get { return Lang.Get("necessaries:mailbox-title"); }
        }
        public override string InventoryClassName
        {
            get { return "mailbox"; }
        }

        public BEMailBox()
        {
            inventory = new MailBoxInventory(null, null);
            inventory.SlotModified += OnSlotModified;
        }

        private void OnSlotModified(int slotid)
        {
            if (slotid == 4) return;
            bool newMail = false;
            for (int i = 0; i < 4; i++)
                if (inventory[i].Itemstack != null) { newMail = true; break; }
            if (newMail) SetBlockState("newmail");
            else SetBlockState("nonewmail");
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("mailbox-1", api);
        }

        public void OnBlockInteract(IPlayer byPlayer)
        {
            if (Api.Side == EnumAppSide.Client)
            {
            }
            else
            {
                byte[] data;

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write(byPlayer.PlayerName);
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
            address = tree.GetString("address");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("address", address);
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
                        string newaddress = reader.ReadString();
                        if (newaddress == null) newaddress = "";
                        if (!Necessaries.EditMailbox(newaddress, Pos.X, Pos.Y, Pos.Z))
                        {
                            ((ICoreServerAPI)Api).SendMessage(fromPlayer, 0, Lang.Get("necessaries:mailbox-addressexists"), EnumChatType.CommandError);
                        }
                        else
                            address = newaddress;
                    }
                    MarkDirty();
                }
            }
            if (packetid == 1002)
            {
                if (data != null)
                {
                    string rcptAddress = "";
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        BinaryReader reader = new BinaryReader(ms);
                        rcptAddress = reader.ReadString();
                        if (rcptAddress == null) rcptAddress = "";
                    }
                    if (inventory[4].Itemstack?.Collectible.FirstCodePart().Equals("parcel") == true)
                    {

                        ItemStack stack = inventory[4].Itemstack.Clone();
                        stack.Attributes.SetString("rcpt", rcptAddress);

                        string rcptDestIndex = inventory[4].Itemstack.Attributes.GetString("destAddressIdx");
                        int rcptDestIdx = rcptDestIndex.ToInt(-1);
                        if (rcptDestIdx != -1 && Necessaries.postServicesServer.Count > rcptDestIdx)
                        {
                            PostService destPS = Necessaries.postServicesServer[rcptDestIdx];

                            ICoreServerAPI sapi = this.Api as ICoreServerAPI;
                            if (sapi != null)
                            {
                                sapi.WorldManager.LoadChunkColumnPriority(destPS.X / Api.World.BlockAccessor.ChunkSize, destPS.Z / Api.World.BlockAccessor.ChunkSize, new ChunkLoadOptions()
                                {
                                    OnLoaded = () =>
                                    {
                                        BEMailBox destMailbox = (BEMailBox)Api.World.BlockAccessor.GetBlockEntity(new BlockPos(destPS.X, destPS.Y, destPS.Z));
                                        if (destMailbox != null)
                                        {
                                            int freeSlot = -1;
                                            for (int i = 0; i < 4; i++)
                                            {
                                                if (destMailbox.Inventory[i].Itemstack == null) { freeSlot = i; break; }
                                            }
                                            if (freeSlot != -1)
                                            {
                                                destMailbox.Inventory[freeSlot].Itemstack = stack.Clone();
                                                destMailbox.Inventory[freeSlot].MarkDirty();
                                                inventory[4].Itemstack = null; inventory[4].MarkDirty();

                                                if (fromPlayer is IServerPlayer)
                                                {
                                                    IServerPlayer serverPlayer = (IServerPlayer)fromPlayer;
                                                    serverPlayer.SendMessage(0, Lang.Get("necessaries:mailbox-sent"), EnumChatType.CommandError);
                                                }
                                            }
                                            else
                                            {
                                                if (fromPlayer is IServerPlayer)
                                                {
                                                    IServerPlayer serverPlayer = (IServerPlayer)fromPlayer;
                                                    serverPlayer.SendMessage(0, Lang.Get("necessaries:mailbox-noplace"), EnumChatType.CommandError);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (fromPlayer is IServerPlayer)
                                            {
                                                IServerPlayer serverPlayer = (IServerPlayer)fromPlayer;
                                                serverPlayer.SendMessage(0, Lang.Get("necessaries:mailbox-nomailbox"), EnumChatType.CommandError);
                                            }
                                        }
                                    }
                                });

                            }

                            
                        } else
                        {
                            if (fromPlayer is IServerPlayer)
                            {
                                IServerPlayer serverPlayer = (IServerPlayer)fromPlayer;
                                serverPlayer.SendMessage(0, Lang.Get("necessaries:mailbox-nodest"), EnumChatType.CommandError);
                            }
                        }

                    } else
                    {
                        if(fromPlayer is IServerPlayer)
                        {
                            IServerPlayer serverPlayer = (IServerPlayer)fromPlayer;
                            serverPlayer.SendMessage(0, Lang.Get("necessaries:mailbox-noparcel"), EnumChatType.CommandError);
                        }
                    }
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
                    string playerName = reader.ReadString();
                    TreeAttribute tree = new TreeAttribute();
                    tree.FromBytes(reader);
                    Inventory.FromTreeAttributes(tree);
                    Inventory.ResolveBlocksOrItems();

                    IClientWorldAccessor clientWorld = (IClientWorldAccessor)Api.World;

                    if (clientDialog == null)
                    {
                        clientDialog = new GuiDialogMailbox(DialogTitle, Inventory, Pos, Api as ICoreClientAPI);
                        clientDialog.OnClosed += () =>
                        {
                            ((ICoreClientAPI)Api).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                            clientDialog = null;
                        };
                    }

                    clientDialog.TryOpen();

                    int maxAddress = 0;
                    for (int i = 0; i < Necessaries.postServicesClient.Count; i++)
                    {
                        if (Necessaries.postServicesClient[i].title.StartsWith(playerName))
                        {
                            int numPad = -1;
                            for(int j = Necessaries.postServicesClient[i].title.Length - 1; j >= 0; j--)
                            {
                                char isnum = Necessaries.postServicesClient[i].title[j];
                                if (isnum >= '0' && isnum <= '9') numPad = j;
                                else break;
                            }
                            if (numPad != -1)
                            {
                                int curAddress = int.Parse(Necessaries.postServicesClient[i].title.Substring(numPad));
                                if (curAddress > maxAddress) maxAddress = curAddress;
                            }
                        }
                    }

                    if (address == "") address = playerName + ", " + (maxAddress + 1);
                    clientDialog.DefAddress = address;
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
