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
using Vintagestory.GameContent;

namespace tradeomat.src.TradeomatBlock
{
    class BETradeBlock : BlockEntityContainer
    {
        public string ownerName;
        internal InventoryTradeBlock inventory;
        GuiOwnerTradeBlock ownerDialog;
        GuiCustomerTradeBlock customerDialog;

        DealRenderer dealRenderer;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "tomat"; }
        }

        public BETradeBlock()
        {
            inventory = new InventoryTradeBlock(null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("tomat-1", api);
            inventory.SlotModified += OnSlotModified;

            if(api is ICoreClientAPI cApi && Block.Variant["type"] == "crate")
            {
                dealRenderer = new DealRenderer(Pos, cApi);
                UpdateDeal();
            }
        }

        private void OnSlotModified(int slotid)
        {
            if (Block.Variant["type"] == "tall") return;
            dealRenderer?.UpdateDeal(Inventory[0], Inventory[1], GetFullness());
            if(Api.Side == EnumAppSide.Server)
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

                Tradeomat.serverChannel.BroadcastPacket<StallUpdate>(new StallUpdate(Pos.X, Pos.Y, Pos.Z, data), null);
            }
        }
        public void UpdateDeal()
        {
            dealRenderer?.UpdateDeal(Inventory[0], Inventory[1], GetFullness());
        }
        public void OnBlockInteract(IPlayer byPlayer, bool isOwner)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                //stub
            }
            else
            {
                if (isOwner)
                {
                    if (byPlayer.PlayerName.Equals(ownerName))
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

                        ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(
                            (IServerPlayer)byPlayer,
                            Pos.X, Pos.Y, Pos.Z,
                            (int)EnumBlockStovePacket.OpenGUI,
                            data
                        );

                        byPlayer.InventoryManager.OpenInventory(inventory);
                    } else
                        ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("tradeomat:notyours"), EnumChatType.CommandError);
                } else
                {
                    byte[] data;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryWriter writer = new BinaryWriter(ms);
                        writer.Write(false);
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
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            ownerName = tree.GetString("ownerName", "");
            //dealRenderer.UpdateDeal(Inventory[0], Inventory[1], GetFullness());
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("ownerName", ownerName);
        }

        private void Deal(int numberOfDeals)
        {
            int toPlace = inventory[0].Itemstack.StackSize * numberOfDeals;
            inventory[18].Itemstack.StackSize -= toPlace;
            if (inventory[18].Itemstack.StackSize == 0) inventory[18].Itemstack = null;
            inventory[18].MarkDirty();

            int curSlot = 2;
            //Passing the payment
            while(toPlace > 0)
            {
                int placeThisIteration = toPlace;
                if (placeThisIteration > inventory[0].Itemstack.Collectible.MaxStackSize) placeThisIteration = inventory[0].Itemstack.Collectible.MaxStackSize;
                if (inventory[curSlot].Itemstack == null || inventory[curSlot].Itemstack.Collectible.Equals(inventory[curSlot].Itemstack, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes))
                {
                    if (inventory[curSlot].Itemstack == null)
                    {
                        inventory[curSlot].Itemstack = inventory[0].Itemstack.GetEmptyClone();
                        inventory[curSlot].Itemstack.StackSize = placeThisIteration;
                        inventory[curSlot].MarkDirty();
                        toPlace -= placeThisIteration;
                    }
                    else
                    {
                        if (inventory[curSlot].Itemstack.StackSize < inventory[curSlot].Itemstack.Collectible.MaxStackSize)
                        {
                            if(inventory[curSlot].Itemstack.StackSize + placeThisIteration > inventory[curSlot].Itemstack.Collectible.MaxStackSize)
                            {
                                toPlace -= (inventory[curSlot].Itemstack.Collectible.MaxStackSize - inventory[curSlot].Itemstack.StackSize);
                                inventory[curSlot].Itemstack.StackSize = inventory[curSlot].Itemstack.Collectible.MaxStackSize;
                                inventory[curSlot].MarkDirty();
                            }
                            else
                            {
                                toPlace -= placeThisIteration;
                                inventory[curSlot].Itemstack.StackSize += placeThisIteration;
                                inventory[curSlot].MarkDirty();
                            }
                        }
                    }
                }
                curSlot++;
            }

            toPlace = inventory[1].Itemstack.StackSize * numberOfDeals;
            if(inventory[19].Itemstack == null)
                inventory[19].Itemstack = inventory[1].Itemstack.GetEmptyClone();
            inventory[19].Itemstack.StackSize += toPlace;
            inventory[19].MarkDirty();
            curSlot = 17;
            //Passing the goods
            while (toPlace > 0)
            {
                if (inventory[curSlot].Itemstack != null)
                {
                    if (inventory[curSlot].Itemstack.Collectible.Equals(inventory[curSlot].Itemstack, inventory[1].Itemstack, GlobalConstants.IgnoredStackAttributes))
                    {

                        if (inventory[curSlot].Itemstack.StackSize < toPlace)
                        {
                            toPlace -= inventory[curSlot].Itemstack.StackSize;
                            inventory[curSlot].Itemstack = null;
                            inventory[curSlot].MarkDirty();
                        }
                        else
                        {
                            inventory[curSlot].Itemstack.StackSize -= toPlace;
                            if (inventory[curSlot].Itemstack.StackSize == 0) inventory[curSlot].Itemstack = null;
                            inventory[curSlot].MarkDirty();
                            toPlace = 0;
                        }
                    }
                }
                curSlot--;
            }

            /*
            GuiElementDynamicText errorText = customerDialog.SingleComposer.GetDynamicText("errorText");
            if (inventory[0].Itemstack == null || inventory[1].Itemstack == null)
            {
                errorText.SetNewText(Lang.Get("tradeomat:isnotsetup"));
            }
            else
            {
                if (GetStorage(true) < inventory[1].Itemstack.StackSize)
                    errorText.SetNewText(Lang.Get("tradeomat:outofgoods"));
                if (GetStorage(false) < inventory[0].Itemstack.StackSize)
                    errorText.SetNewText(Lang.Get("tradeomat:outofspace"));
            }
            */
        }
        private int GetFullness()
        {
            if (inventory[1].Itemstack == null) return 0;
            int fullness = 0;
            for (int i = 10; i < 18; i++)
            {
                if (inventory[i].Itemstack != null && inventory[i].Itemstack.Collectible.Equals(inventory[i].Itemstack, inventory[1].Itemstack, GlobalConstants.IgnoredStackAttributes))
                {
                    fullness++;
                }
            }
            return fullness;
        }
        public int GetStorage(bool goodsStorage)
        {
            int freeSpace = 0;
            if(goodsStorage)
            {
                if (inventory[1].Itemstack == null) return 0;
                for (int i = 10; i < 18; i++)
                {
                    if (inventory[i].Itemstack != null && inventory[i].Itemstack.Collectible.Equals(inventory[i].Itemstack, inventory[1].Itemstack, GlobalConstants.IgnoredStackAttributes))
                    {
                        freeSpace += inventory[i].Itemstack.StackSize;
                    }
                }
            } else
            {
                if (inventory[0].Itemstack == null) return 0;
                for (int i = 2; i < 10; i++)
                {
                    if (inventory[i].Itemstack == null || inventory[i].Itemstack.Collectible.Equals(inventory[i].Itemstack, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes))
                    {
                        if (inventory[i].Itemstack == null)
                        {
                            freeSpace += inventory[0].Itemstack.Collectible.MaxStackSize;
                        }
                        else
                            freeSpace += inventory[0].Itemstack.Collectible.MaxStackSize - inventory[i].Itemstack.StackSize;
                    }
                }
            }
            return freeSpace;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if (packetid <= 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
            }

            if(packetid == 1002)
            {
                IServerPlayer serverPlayer = (IServerPlayer)fromPlayer;

                if (inventory[18].Itemstack != null && inventory[0].Itemstack != null && inventory[1].Itemstack != null)
                {
                    // Process Deal. Checking for payment match
                    int dealMaxPossible = 0;
                    
                    if (inventory[18].Itemstack.Equals(fromPlayer.Entity.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes))
                    {
                        //Passed. Calculating maximum deal
                        dealMaxPossible = (int)(inventory[18].Itemstack.StackSize / inventory[0].Itemstack.StackSize);

                        if (dealMaxPossible > 0)
                        {
                            //Calculating free space in payment storage
                            int freeSpace = GetStorage(false);
                            
                            if (freeSpace >= dealMaxPossible * inventory[0].Itemstack.StackSize)
                            {
                                //Passed. Checking for goods availability
                                int goodsAvailable = GetStorage(true);
                                
                                if (goodsAvailable >= dealMaxPossible * inventory[1].Itemstack.StackSize)
                                {
                                    //Passed. Checking for free space in output slot
                                    if (inventory[19].Itemstack == null || inventory[19].Itemstack.Collectible.Equals(inventory[19].Itemstack, inventory[1].Itemstack, GlobalConstants.IgnoredStackAttributes))
                                    {
                                        if (inventory[19].Itemstack == null)
                                        {
                                            if (dealMaxPossible * inventory[1].Itemstack.StackSize <= inventory[1].Itemstack.Collectible.MaxStackSize)
                                            {
                                                //Passed. Ready for exchange.
                                                Deal(dealMaxPossible);
                                            }
                                            else
                                            {
                                                //Not Passed. Decreasing deal amount.
                                                dealMaxPossible = (int)(inventory[1].Itemstack.Collectible.MaxStackSize / inventory[1].Itemstack.StackSize);
                                                Deal(dealMaxPossible);
                                            }
                                        }
                                        else
                                        {
                                            if (inventory[19].Itemstack.StackSize + dealMaxPossible * inventory[1].Itemstack.StackSize <= inventory[1].Itemstack.Collectible.MaxStackSize)
                                            {
                                                //Passed. Ready for exchange.
                                                Deal(dealMaxPossible);
                                            }
                                            else
                                            {
                                                //Not Passed. Decreasing deal amount.
                                                dealMaxPossible = (int)((inventory[1].Itemstack.Collectible.MaxStackSize - inventory[19].Itemstack.StackSize) / inventory[1].Itemstack.StackSize);
                                                Deal(dealMaxPossible);
                                            }
                                        }
                                    }
                                    else
                                        serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-slotnotempty"), EnumChatType.CommandError);
                                }
                                else
                                {
                                    dealMaxPossible = (int)(goodsAvailable / inventory[1].Itemstack.StackSize);
                                    if (dealMaxPossible > 0)
                                    {
                                        //Passed. Checking for free space in output slot
                                        if (inventory[19].Itemstack == null || inventory[19].Itemstack.Collectible.Equals(inventory[19].Itemstack, inventory[1].Itemstack, GlobalConstants.IgnoredStackAttributes))
                                        {
                                            if (inventory[19].Itemstack == null)
                                            {
                                                if (dealMaxPossible * inventory[1].Itemstack.StackSize <= inventory[1].Itemstack.Collectible.MaxStackSize)
                                                {
                                                    //Passed. Ready for exchange.
                                                    Deal(dealMaxPossible);
                                                }
                                                else
                                                {
                                                    //Not Passed. Decreasing deal amount.
                                                    dealMaxPossible = (int)(inventory[1].Itemstack.Collectible.MaxStackSize / inventory[1].Itemstack.StackSize);
                                                    Deal(dealMaxPossible);
                                                }
                                            }
                                            else
                                            {
                                                if (inventory[19].Itemstack.StackSize + dealMaxPossible * inventory[1].Itemstack.StackSize <= inventory[1].Itemstack.Collectible.MaxStackSize)
                                                {
                                                    //Passed. Ready for exchange.
                                                    Deal(dealMaxPossible);
                                                }
                                                else
                                                {
                                                    //Not Passed. Decreasing deal amount.
                                                    int newDealMaxPossible = (int)((inventory[1].Itemstack.Collectible.MaxStackSize - inventory[19].Itemstack.StackSize) / inventory[1].Itemstack.StackSize);
                                                    if (newDealMaxPossible > dealMaxPossible) newDealMaxPossible = dealMaxPossible;
                                                    Deal(newDealMaxPossible);
                                                }
                                            }
                                        }
                                        else
                                            serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-slotnotempty"), EnumChatType.CommandError);

                                    }
                                    else
                                        serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-notenoughgoods"), EnumChatType.CommandError);
                                }

                            }
                            else
                                serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-notenoughspace"), EnumChatType.CommandError);
                        }
                        else
                            serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-notenoughpayment"), EnumChatType.CommandError);
                    }
                    else
                        serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-payenterror"), EnumChatType.CommandError);
                } else
                    serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-payenterror"), EnumChatType.CommandError);
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
            if (packetid == (int)EnumBlockStovePacket.OpenGUI)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryReader reader = new BinaryReader(ms);
                    bool isOwner = reader.ReadBoolean();
                    TreeAttribute tree = new TreeAttribute();
                    tree.FromBytes(reader);
                    Inventory.FromTreeAttributes(tree);
                    Inventory.ResolveBlocksOrItems();

                    //IClientWorldAccessor clientWorld = (IClientWorldAccessor)Api.World;

                    if (isOwner)
                    {
                        if (ownerDialog == null || !ownerDialog.IsOpened())
                        {
                            ownerDialog = new GuiOwnerTradeBlock(Lang.Get("tradeomat:owner-title", ownerName), Inventory, Pos, Api as ICoreClientAPI);
                            ownerDialog.OnClosed += () =>
                            {
                                ownerDialog = null;
                            };

                            ownerDialog.TryOpen();
                        }
                    }
                    else
                    {
                        if (customerDialog == null || !customerDialog.IsOpened())
                        {
                            customerDialog = new GuiCustomerTradeBlock(Lang.Get("tradeomat:customer-title", ownerName), Inventory, Pos, Api as ICoreClientAPI);
                            customerDialog.OnClosed += () =>
                            {
                                customerDialog = null;
                            };

                            customerDialog.TryOpen();

                            GuiElementDynamicText errorText = customerDialog.SingleComposer.GetDynamicText("errorText");
                            if (inventory[0].Itemstack == null || inventory[1].Itemstack == null)
                            {
                                errorText.SetNewText(Lang.Get("tradeomat:isnotsetup"));
                            }
                            else
                            {
                                if (GetStorage(true) < inventory[1].Itemstack.StackSize)
                                    errorText.SetNewText(Lang.Get("tradeomat:outofgoods"));
                                if (GetStorage(false) < inventory[0].Itemstack.StackSize)
                                    errorText.SetNewText(Lang.Get("tradeomat:outofspace"));
                            }
                        }
                    }
                }
            }


            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                ownerDialog?.TryClose();
                ownerDialog?.Dispose();
                ownerDialog = null;
                customerDialog?.TryClose();
                customerDialog?.Dispose();
                customerDialog = null;
            }
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            dealRenderer?.Dispose();
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            dealRenderer?.Dispose();
            dealRenderer = null;
        }
    }
}
