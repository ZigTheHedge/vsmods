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

namespace tradeomat.src.TradeomatBlock
{
    class GuiCustomerTradeBlock : GuiDialogBlockEntity
    {

        public GuiCustomerTradeBlock(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;

            capi.World.Player.InventoryManager.OpenInventory(Inventory);

            SetupDialog();
        }

        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory == Inventory)
            {
                capi.Input.TriggerOnMouseLeaveSlot(hoveredSlot);
            }
            else
            {
                hoveredSlot = null;
            }

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 220, 150);

            ElementBounds priceTitleBounds = ElementBounds.Fixed(30, 12, 100, 35);
            ElementBounds goodsTitleBounds = ElementBounds.Fixed(140, 12, 100, 35);
            ElementBounds transferTitleBounds = ElementBounds.Fixed(95, 55, 60, 35);
            ElementBounds errorTitleBounds = ElementBounds.Fixed(0, 60, 220, 100);

            ElementBounds priceSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24, 40, 1, 1);
            ElementBounds goodsSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 144, 40, 1, 1);

            ElementBounds paymentInSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 24, 91, 1, 1);
            ElementBounds goodsOutSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 144, 91, 1, 1);

            ElementBounds dealButtonBounds = ElementBounds.FixedSize(0, 0).FixedUnder(goodsOutSlotsBounds).WithAlignment(EnumDialogArea.CenterFixed).WithFixedPadding(10, 2);


            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            ClearComposers();
            SingleComposer = capi.Gui
                .CreateCompo("blockentitytradeomat" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText(Lang.Get("tradeomat:tomat-goods"), CairoFont.WhiteSmallText(), goodsTitleBounds)
                    .AddStaticText(Lang.Get("tradeomat:tomat-price"), CairoFont.WhiteSmallText(), priceTitleBounds)
                    .AddPassiveItemSlot(goodsSlotsBounds, Inventory, Inventory[1], false)
                    .AddPassiveItemSlot(priceSlotsBounds, Inventory, Inventory[0], false)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 18 }, paymentInSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 19 }, goodsOutSlotsBounds)
                    .AddSmallButton(Lang.Get("tradeomat:agree-button"), OnButtonSend, dealButtonBounds, EnumButtonStyle.Normal, EnumTextOrientation.Center, "sendBtn")
                    .AddDynamicText("", CairoFont.WhiteSmallText(), EnumTextOrientation.Center, errorTitleBounds, "errorText")
                .EndChildElements()
                .Compose()
            ;

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
        }

        private bool OnButtonSend()
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1002);
            return true;
        }

        private void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        public override bool OnEscapePressed()
        {
            base.OnEscapePressed();
            OnTitleBarClose();
            return TryClose();
        }
    }
}
