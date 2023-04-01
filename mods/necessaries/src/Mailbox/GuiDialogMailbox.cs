using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace necessaries.src.Mailbox
{
    class GuiDialogMailbox : GuiDialogBlockEntity
    {
        private string defAddress = "";

        public string DefAddress
        {
            get => defAddress;
            set
            {
                defAddress = value;
                SingleComposer.GetTextInput("address").SetValue(defAddress);
            }
        }

        public GuiDialogMailbox(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;

            capi.World.Player.InventoryManager.OpenInventory(Inventory);
            Inventory.SlotModified += OnInventorySlotModified;
            SetupDialog();
        }

        private void OnInventorySlotModified(int slotid)
        {
            if(slotid == 4)
            {
                if(Inventory[4].Itemstack?.Collectible.FirstCodePart().Equals("parcel") == true)
                {
                    SingleComposer.GetButton("sendBtn").Enabled = true;
                } else
                    SingleComposer.GetButton("sendBtn").Enabled = false;
            }
        }

        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory?.InventoryID != Inventory?.InventoryID)
            {
                hoveredSlot = null;
            }

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 300, 150);

            ElementBounds addressTitleBounds = ElementBounds.Fixed(0, 30, 300, 30);

            ElementBounds addressBounds = ElementBounds.Fixed(0, 60, 300, 30);

            ElementBounds inboxSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 100, 4, 1);

            ElementBounds sendDetailsBounds = ElementBounds.Fixed(0, 160, 300, 30);

            ElementBounds mediaSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 190, 1, 1);

            ElementBounds sendButtonBounds = ElementBounds.FixedSize(0, 0).FixedUnder(mediaSlotBounds, 2 * 5).WithAlignment(EnumDialogArea.CenterFixed).WithFixedPadding(10, 2);

            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            ClearComposers();
            SingleComposer = capi.Gui
                .CreateCompo("blockentitymailbox" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText(Lang.Get("necessaries:mailbox-select-address"), CairoFont.WhiteSmallText(), addressTitleBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 4, new int[] { 0, 1, 2, 3 }, inboxSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 4 }, mediaSlotBounds, "mediumSlot")
                    .AddTextInput(addressBounds, OnAddressChanged, null, "address")
                    .AddStaticText(Lang.Get("necessaries:mailbox-send-to"), CairoFont.WhiteSmallText(), sendDetailsBounds)
                    .AddSmallButton(Lang.Get("necessaries:mailbox-send"), OnButtonSend, sendButtonBounds, EnumButtonStyle.Normal, "sendBtn")
                .EndChildElements()
                .Compose()
            ;

            SingleComposer.GetButton("sendBtn").Enabled = false;

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
        }

        private bool OnButtonSend()
        {
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(SingleComposer.GetTextInput("address").GetText());
                data = ms.ToArray();
            }

            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1002, data);
            TryClose();
            return true;
        }

        private void OnAddressChanged(string newText)
        {

        }

        private void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

        private void OnTitleBarClose()
        {
            byte[] data;
            Inventory.SlotModified -= OnInventorySlotModified;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(SingleComposer.GetTextInput("address").GetText());
                data = ms.ToArray();
            }

            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1001, data);
            TryClose();
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            //Inventory.SlotModified += OnInventorySlotModified;
        }

        public override bool OnEscapePressed()
        {
            OnTitleBarClose();
            return base.OnEscapePressed();
        }
    }
}
