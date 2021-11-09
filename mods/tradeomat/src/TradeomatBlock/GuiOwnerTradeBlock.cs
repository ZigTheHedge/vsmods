using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace tradeomat.src.TradeomatBlock
{
    class GuiOwnerTradeBlock : GuiDialogBlockEntity
    {
        bool isCreative;
        public GuiOwnerTradeBlock(bool isCreative, string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;
            this.isCreative = isCreative;
            Inventory.SlotModified += OnInventorySlotModified;
            capi.World.Player.InventoryManager.OpenInventory(Inventory);

            SetupDialog();
        }
        public void OnInventorySlotModified(int slotid)
        {
            //SetupDialog();
            capi.Event.EnqueueMainThreadTask(SetupDialog, "setuptradedlg");
        }
        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory?.InventoryID != Inventory?.InventoryID)
            {
                hoveredSlot = null;
            }

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 240, 390);

            ElementBounds priceTitleBounds = ElementBounds.Fixed(30, 12, 100, 35);
            ElementBounds goodsTitleBounds = ElementBounds.Fixed(140, 12, 100, 35);
            ElementBounds transferTitleBounds = ElementBounds.Fixed(95, 55, 60, 35);

            ElementBounds priceSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24, 40, 1, 1);
            ElementBounds goodsSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 144, 40, 1, 1);

            ElementBounds priceStorageSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 91, 2, 4);
            ElementBounds goodsStorageSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 120, 91, 2, 4);

            ElementBounds paymentInSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24, 295, 1, 1);
            ElementBounds goodsOutSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 144, 295, 1, 1);

            ElementBounds isCreativeSwitchBounds = ElementStdBounds.ToggleButton(24, 350, 30, 30);
            ElementBounds isCreativeTextBounds = ElementStdBounds.ToggleButton(60, 355, 170, 30);

            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            ClearComposers();
            bool displayAdminShop = false;
            if (capi.World.Player != null)
            {
                displayAdminShop = capi.World.Player.HasPrivilege("gamemode");
            }

            SingleComposer = capi.Gui
                .CreateCompo("blockentitytradeomat" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText(Lang.Get("tradeomat:tomat-goods"), CairoFont.WhiteSmallText(), goodsTitleBounds)
                    .AddStaticText(Lang.Get("tradeomat:tomat-price"), CairoFont.WhiteSmallText(), priceTitleBounds)
                    .AddStaticText("==>>", CairoFont.WhiteSmallText(), transferTitleBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, goodsSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, priceSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 2, new int[] { 2, 3, 4, 5, 6, 7, 8, 9 }, priceStorageSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 2, new int[] { 10, 11, 12, 13, 14, 15, 16, 17 }, goodsStorageSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 18 }, paymentInSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 19 }, goodsOutSlotsBounds)
                    .AddIf(displayAdminShop)
                    .AddSwitch(onCreativeToggle, isCreativeSwitchBounds, "isCreative")
                    .AddStaticText(Lang.Get("tradeomat:tomat-creative"), CairoFont.WhiteSmallText(), isCreativeTextBounds)
                    .EndIf()
                .EndChildElements()
                .Compose()
            ;

            if (displayAdminShop)
            {
                if (isCreative)
                    SingleComposer.GetSwitch("isCreative").SetValue(true);
                else
                    SingleComposer.GetSwitch("isCreative").SetValue(false);
            }

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
        }

        private void onCreativeToggle(bool onOff)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1102, BitConverter.GetBytes(onOff));
        }

        private void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

        private void OnTitleBarClose()
        {
            TryClose();
            Inventory.SlotModified -= OnInventorySlotModified;
        }

        public override bool OnEscapePressed()
        {
            Inventory.SlotModified -= OnInventorySlotModified;
            return base.OnEscapePressed();
        }
    }
}
