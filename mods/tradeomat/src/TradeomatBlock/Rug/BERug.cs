using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tradeomat.src.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace tradeomat.src.TradeomatBlock.Rug
{
    class BERug : BEContainerDisplayPatch
    {
        public string ownerName;
        internal RugInventory inventory;
        int[] prices = new int[5];
        GuiRugOwner ownerDialog;
        GuiRugCustomer customerDialog;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "rug"; }
        }

        public BERug()
        {
            inventory = new RugInventory(null, null);
            meshes = new MeshData[13];
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("rug-1", api);
            inventory.SlotModified += OnSlotModified;

            if (api is ICoreClientAPI cApi)
            {

            }
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
                    }
                    else
                        ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("tradeomat:notyours"), EnumChatType.CommandError);
                }
                else
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
            for (int i = 0; i < 5; i++)
                prices[i] = tree.GetInt("prices" + i);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("ownerName", ownerName);
            for (int i = 0; i < 5; i++)
                tree.SetInt("prices" + i, prices[i]);
        }

        private void Deal(int selectedSlot)
        {
            int toPlace = prices[selectedSlot];
            inventory[11].Itemstack.StackSize -= toPlace;
            if (inventory[11].Itemstack.StackSize == 0) inventory[11].Itemstack = null;
            inventory[11].MarkDirty();

            int curSlot = 1;
            //Passing the payment
            while (toPlace > 0)
            {
                int placeThisIteration = toPlace;
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
                            if (inventory[curSlot].Itemstack.StackSize + placeThisIteration > inventory[curSlot].Itemstack.Collectible.MaxStackSize)
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

            inventory[12].Itemstack = inventory[selectedSlot + 6].Itemstack.Clone();
            inventory[selectedSlot + 6].Itemstack = null;
            inventory[12].MarkDirty();
            inventory[selectedSlot + 6].MarkDirty();

            Api.World.PlaySoundAt(new AssetLocation("tradeomat:sounds/deal.ogg"), Pos.X, Pos.Y, Pos.Z);

        }

        public int GetStorage()
        {
            int freeSpace = 0;
            
            if (inventory[0].Itemstack == null) return 0;
            for (int i = 1; i <= 5; i++)
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
            return freeSpace;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if (packetid <= 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
            }

            if (packetid == 1101)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryReader reader = new BinaryReader(ms);
                    for (int i = 0; i < 5; i++)
                        prices[i] = reader.ReadInt32();
                }
                MarkDirty();
            }

            if(packetid == 1102)
            {
                IServerPlayer serverPlayer = (IServerPlayer)fromPlayer;
                int selectedSlot = BitConverter.ToInt32(data, 0);
                // Checking if good has been chosen
                if (selectedSlot != -1)
                {
                    // Checking if something is put in payment slot
                    if (inventory[11].Itemstack != null && inventory[0].Itemstack != null)
                    {
                        // Process Deal. Checking for payment match

                        if (inventory[11].Itemstack.Equals(fromPlayer.Entity.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes))
                        {
                            //Passed. Checking enough payment

                            if (inventory[11].Itemstack.StackSize >= prices[selectedSlot])
                            {
                                //Calculating free space in payment storage
                                int freeSpace = GetStorage();

                                if (freeSpace >= prices[selectedSlot])
                                {
                                    //Passed. Checking for free space in output slot
                                    if (inventory[12].Empty)
                                    {
                                        //Passed. Ready for exchange.
                                        Deal(selectedSlot);
                                    }
                                    else
                                        serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-slotnotempty"), EnumChatType.CommandError);
                                }
                                else
                                    serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-notenoughspace"), EnumChatType.CommandError);
                            }
                            else
                                serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-notenoughpayment"), EnumChatType.CommandError);
                        }
                        else
                            serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-payenterror"), EnumChatType.CommandError);
                    }
                    else
                        serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-payenterror"), EnumChatType.CommandError);
                } else
                    serverPlayer.SendMessage(0, Lang.Get("tradeomat:deal-notselected"), EnumChatType.CommandError);
            }

            if (packetid == 1103)
            {
                ((ICoreServerAPI)Api).Network.BroadcastBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1103);
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

                    if (isOwner)
                    {
                        if (ownerDialog == null || !ownerDialog.IsOpened())
                        {
                            ownerDialog = new GuiRugOwner(prices, Lang.Get("tradeomat:owner-title", ownerName), Inventory, Pos, Api as ICoreClientAPI);
                            ownerDialog.OnClosed += () =>
                            {
                                ownerDialog = null;
                                ((ICoreClientAPI)Api).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1103);
                            };

                            ownerDialog.TryOpen();
                        }
                    }
                    else
                    {
                        if (customerDialog == null || !customerDialog.IsOpened())
                        {
                            customerDialog = new GuiRugCustomer(prices, Lang.Get("tradeomat:customer-title", ownerName), Inventory, Pos, Api as ICoreClientAPI);
                            customerDialog.OnClosed += () =>
                            {
                                customerDialog = null;
                                ((ICoreClientAPI)Api).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1103);
                            };

                            customerDialog.TryOpen();

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
                    }
                }
            }

            if (packetid == 1103)
            {
                UpdateShape();
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

        private void OnSlotModified(int slotid)
        {
            UpdateShape();
        }

        public void UpdateShape()
        {
            for (int i = 6; i <= 10; i++)
            {
                updateMesh(i);
            }
            MarkDirty(Api.Side != EnumAppSide.Server);
        }

        protected override void updateMesh(int index)
        {
            if (index < 6 || index > 10)
            {
                meshes[index] = null;
                return;
            }
            base.updateMesh(index);
        }

        public override void TranslateMesh(MeshData mesh, int index)
        {
            if (index < 6 || index > 10) return;
            float x = 2f / 16f + (index - 6) * 3f / 16f;
            float y = 1f / 16f;
            float z = 5f / 16f + ((index - 6) % 2) * 6f / 16f;
            bool isWearable = false;

            
            if (!Inventory[index].Empty)
            {
                if (Inventory[index].Itemstack.Class == EnumItemClass.Block)
                {
                    if(Inventory[index].Itemstack.Collectible is BlockPie)
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.15f, 0.15f, 0.15f);
                    else
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.25f, 0.25f, 0.25f);
                }
                else
                {
                    string itmCode = Inventory[index].Itemstack.Item.Code.FirstPathPart();
                    if (itmCode.StartsWith("nugget"))
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 1.2f, 1.2f, 1.2f);
                    else
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);
                    if (Inventory[index].Itemstack.Item is ItemWearable) isWearable = true;
                }
            }
            
            if (isWearable)
            {
                mesh.Rotate(new Vec3f(3f / 16f, 0, 4f / 16f), 0, 90 * GameMath.DEG2RAD, 0);
                mesh.Translate(x - 3f / 16f, y, z - 4f / 16f);
            }
            else
            {
                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 90 * GameMath.DEG2RAD, 0);
                mesh.Translate(x - 0.5f, y, z - 0.5f);
            }

            int orientationRotate = 0;
            if (Block.Variant["horizontalorientation"] == "east") orientationRotate = 270;
            if (Block.Variant["horizontalorientation"] == "south") orientationRotate = 180;
            if (Block.Variant["horizontalorientation"] == "west") orientationRotate = 90;

                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, orientationRotate * GameMath.DEG2RAD, 0);
        }
    }
}
