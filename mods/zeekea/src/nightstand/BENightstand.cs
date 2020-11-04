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

namespace zeekea.src.nightstand
{
    class BENightstand : BlockEntityContainer
    {
        internal InventoryEight inventory;
        GuiEightSlots nightstandDialog;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return "zeeeight"; }
        }

        public BENightstand()
        {
            inventory = new InventoryEight(null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("zeeeight-1", api);
        }

        public void OnBlockInteract(IPlayer byPlayer, bool isOwner)
        {
            if (Api.Side == EnumAppSide.Client)
            {                
                /*
                if (nightstandDialog == null)
                {
                    nightstandDialog = new GuiEightSlots(Lang.Get("zeekea:nightstand-title"), Inventory, Pos, Api as ICoreClientAPI);
                    nightstandDialog.OnClosed += () =>
                    {
                        nightstandDialog = null;
                        (Api as ICoreClientAPI).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                        byPlayer.InventoryManager.CloseInventory(inventory);
                    };
                }

                nightstandDialog.TryOpen();

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

        public override void FromTreeAtributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAtributes(tree, worldForResolving);
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

                    if (nightstandDialog == null)
                    {
                        nightstandDialog = new GuiEightSlots(Lang.Get("zeekea:nightstand-title"), Inventory, Pos, Api as ICoreClientAPI);
                        nightstandDialog.OnClosed += () =>
                        {
                            nightstandDialog = null;
                        };
                    }

                    nightstandDialog.TryOpen();
                }
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                nightstandDialog?.TryClose();
                nightstandDialog?.Dispose();
                nightstandDialog = null;
            }
        }
    }
}
