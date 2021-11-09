using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace tradeomat.src.TradeomatBlock.Rug
{
    class GuiRugCustomer : GuiDialogBlockEntity
    {
        int[] prices;
        string selectedTag = "-1";
        public GuiRugCustomer(int[] prices, string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;
            this.prices = prices;
            Inventory.SlotModified += OnInventorySlotModified;
            capi.World.Player.InventoryManager.OpenInventory(Inventory);

            SetupDialog();
        }
        public void OnInventorySlotModified(int slotid)
        {
            //SetupDialog();
            capi.Event.EnqueueMainThreadTask(SetupDialog, "setuprugdlg");

        }
        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory?.InventoryID != Inventory?.InventoryID)
            {
                hoveredSlot = null;
            }

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 200, 260);

            ElementBounds priceTitleBounds = ElementBounds.Fixed(5, 35, 105, 35);
            ElementBounds priceSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 116, 20, 1, 1);

            ElementBounds goodsTitleBounds = ElementBounds.Fixed(15, 80, 120, 35);
            ElementBounds[] goodPrice = { ElementBounds.Fixed(15, 100, 48, 35), ElementBounds.Fixed(66, 100, 48, 35), ElementBounds.Fixed(117, 100, 48, 35), ElementBounds.Fixed(168, 100, 48, 35), ElementBounds.Fixed(219, 100, 48, 35) };
            ElementBounds goodsSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 15, 120, 5, 1);

            ElementBounds exchangeTitleBounds = ElementBounds.Fixed(0, 180, 280, 35);
            ElementBounds paymentInSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 66, 200, 1, 1);
            ElementBounds goodsOutSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 168, 200, 1, 1);
            ElementBounds dealButtonBounds = ElementBounds.Fixed(80, 260, 122, 35);

            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            ClearComposers();
            SingleComposer = capi.Gui
                .CreateCompo("berug" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText(Lang.Get("tradeomat:rug-price"), CairoFont.WhiteSmallText(), EnumTextOrientation.Right, priceTitleBounds)
                    .AddItemSlotButton(priceSlotsBounds, OnSlotClick, Inventory, Inventory[0], true, null, "passive")
                    .AddStaticText(Lang.Get("tradeomat:rug-goods"), CairoFont.WhiteSmallText(), goodsTitleBounds)
                    .AddItemSlotButton(goodPrice[0], OnSlotClick, Inventory, Inventory[6], true, prices[0].ToString(), "0")
                    .AddItemSlotButton(goodPrice[1], OnSlotClick, Inventory, Inventory[7], true, prices[1].ToString(), "1")
                    .AddItemSlotButton(goodPrice[2], OnSlotClick, Inventory, Inventory[8], true, prices[2].ToString(), "2")
                    .AddItemSlotButton(goodPrice[3], OnSlotClick, Inventory, Inventory[9], true, prices[3].ToString(), "3")
                    .AddItemSlotButton(goodPrice[4], OnSlotClick, Inventory, Inventory[10], true, prices[4].ToString(), "4")
                    .AddStaticText(Lang.Get("tradeomat:rug-exchange"), CairoFont.WhiteSmallText(), EnumTextOrientation.Center, exchangeTitleBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 11 }, paymentInSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 12 }, goodsOutSlotsBounds)
                    .AddButton(Lang.Get("tradeomat:agree-button"), DealClick, dealButtonBounds)
                .EndChildElements()
                .Compose()
            ;

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }

            if(selectedTag != "-1")
                SingleComposer.GetItemSlotButton(selectedTag).IsChecked = true;
        }

        private bool DealClick()
        {            
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1102, BitConverter.GetBytes(int.Parse(selectedTag)));
            if (selectedTag != "-1")
            {
                SingleComposer.GetItemSlotButton(selectedTag).IsChecked = false;
                selectedTag = "-1";
            }
            return true;
        }

        private void OnSlotClick(string key)
        {
            if(key != "passive")
            {
                if (selectedTag != "-1")
                    SingleComposer.GetItemSlotButton(selectedTag).IsChecked = false;
                if (!Inventory[6 + int.Parse(key)].Empty)
                    SingleComposer.GetItemSlotButton(selectedTag = key).IsChecked = true;
                else
                    selectedTag = "-1";
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
            Inventory.SlotModified -= OnInventorySlotModified;
            return base.OnEscapePressed();
        }
    }
}
