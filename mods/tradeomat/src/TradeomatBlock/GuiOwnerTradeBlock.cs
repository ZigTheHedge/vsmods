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
        public GuiOwnerTradeBlock(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;
            Inventory.SlotModified += OnInventorySlotModified;
            capi.World.Player.InventoryManager.OpenInventory(Inventory);

            SetupDialog();
        }
        public void OnInventorySlotModified(int slotid)
        {
            SetupDialog();
        }
        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory?.InventoryID != Inventory?.InventoryID)
            {
                hoveredSlot = null;
            }

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 200, 150);

            ElementBounds priceTitleBounds = ElementBounds.Fixed(30, 12, 100, 35);
            ElementBounds goodsTitleBounds = ElementBounds.Fixed(140, 12, 100, 35);
            ElementBounds transferTitleBounds = ElementBounds.Fixed(95, 55, 60, 35);

            ElementBounds priceSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24, 40, 1, 1);
            ElementBounds goodsSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 144, 40, 1, 1);

            ElementBounds priceStorageSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 91, 2, 4);
            ElementBounds goodsStorageSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 120, 91, 2, 4);

            ElementBounds paymentInSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24, 295, 1, 1);
            ElementBounds goodsOutSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 144, 295, 1, 1);
            
            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            //ClearComposers();
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
                .EndChildElements()
                .Compose()
            ;

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
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
            base.OnEscapePressed();
            OnTitleBarClose();
            return TryClose();
        }
    }
}
