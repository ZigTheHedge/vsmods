using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
using zeekea.src.nightstand;
using zeekea.src.standardInventories;

namespace zeekea.src.freezer
{
    class BEFreezer : BlockEntityContainer
    {
        internal FreezerInventory inventory;
        GuiFreezer freezerDialog;
        public bool isOpened;
        public int fuelRemaining;
        public int closedDelay;
        private bool refueling;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "zeefreezer"; }
        }

        public BEFreezer()
        {
            inventory = new FreezerInventory(null, null);
            isOpened = false;
            fuelRemaining = 0;
            closedDelay = 0;
            refueling = false;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            inventory.LateInitialize("zeefreezer-1", api);
            inventory.SlotModified += OnSlotModified;

            RegisterGameTickListener(IceTick, 5000);
        }

        private void IceTick(float dt)
        {
            if (Api.Side == EnumAppSide.Server)
            {
                if (fuelRemaining > 0)
                {
                    if (isOpened)
                        fuelRemaining--;
                    else
                    {
                        if (closedDelay > 0)
                        {
                            closedDelay--;
                            if (closedDelay <= 0)
                            {
                                closedDelay = 100;
                                fuelRemaining--;
                            }
                        }
                    }
                    MarkDirty();
                    if (fuelRemaining == 0)
                    {
                        TryRefuel();
                        if (fuelRemaining == 0)
                        {
                            Block originalBlock = Api.World.BlockAccessor.GetBlock(Pos);
                            AssetLocation newBlockAL = originalBlock.CodeWithVariant("status", "melted");
                            Block newBlock = Api.World.GetBlock(newBlockAL);
                            Api.World.BlockAccessor.ExchangeBlock(newBlock.Id, Pos);
                            Api.World.PlaySoundAt(new AssetLocation("zeekea:sounds/freezer_stop.ogg"), Pos.X, Pos.Y, Pos.Z, null, false);
                        }
                    }
                }
            }
        }

        private void TryRefuel()
        {
            int iceCount = 0;
            string matchingItem = "ice";
            for (int i = 0; i < 4; i++)
            {
                if (inventory[i].Itemstack != null)
                    if (inventory[i].Itemstack.Collectible.FirstCodePart().EndsWith(matchingItem)) iceCount++;
            }
            if (iceCount == 4)
            {
                refueling = true;
                for (int i = 0; i < 4; i++)
                {
                    inventory[i].Itemstack.StackSize--;
                    if (inventory[i].Itemstack.StackSize == 0) inventory[i].Itemstack = null;
                    inventory[i].MarkDirty();
                }
                fuelRemaining = 12;
                refueling = false;
                Block originalBlock = Api.World.BlockAccessor.GetBlock(Pos);
                AssetLocation newBlockAL = originalBlock.CodeWithVariant("status", "frozen");
                Block newBlock = Api.World.GetBlock(newBlockAL);
                Api.World.BlockAccessor.ExchangeBlock(newBlock.Id, Pos);
                MarkDirty();
            }

        }

        private void OnSlotModified(int slotid)
        {
            if (slotid > 3 || refueling) return;
            if(Api.Side == EnumAppSide.Server)
                if (fuelRemaining == 0) TryRefuel();
        }


        public void OnBlockInteract(IPlayer byPlayer, bool isOwner)
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

        public override void FromTreeAtributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAtributes(tree, worldForResolving);
            fuelRemaining = tree.GetInt("fuelRemaining");
            closedDelay = tree.GetInt("closedDelay");
            isOpened = tree.GetBool("isOpened");
            refueling = tree.GetBool("refueling");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("fuelRemaining", fuelRemaining);
            tree.SetInt("closedDelay", closedDelay);
            tree.SetBool("isOpened", isOpened);
            tree.SetBool("refueling", refueling);
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

                    if (freezerDialog == null)
                    {
                        freezerDialog = new GuiFreezer(Lang.Get("zeekea:freezer-title"), Inventory, Pos, Api as ICoreClientAPI);
                        freezerDialog.OnClosed += () =>
                        {
                            freezerDialog = null;
                        };
                    }

                    freezerDialog.TryOpen();
                }
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                freezerDialog?.TryClose();
                freezerDialog?.Dispose();
                freezerDialog = null;
            }
        }

        public override float GetPerishRate()
        {
            float initial = base.GetPerishRate();
            if (fuelRemaining == 0) return initial;
            return initial / 4;
        }
    }
}
