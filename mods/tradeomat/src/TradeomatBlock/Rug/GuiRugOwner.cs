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

namespace tradeomat.src.TradeomatBlock.Rug
{
    class GuiRugOwner : GuiDialogBlockEntity
    {
        int[] prices;

        public GuiRugOwner(int[] prices, string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
            if (IsDuplicate) return;
            this.prices = prices;
            Inventory.SlotModified += OnInventorySlotModified;
            capi.World.Player.InventoryManager.OpenInventory(Inventory);

            SetupDialog();
        }
        public void OnInventorySlotModified(int slotid)
        {
            capi.Event.EnqueueMainThreadTask(SetupDialog, "setuprugdlg");
        }
        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory?.InventoryID != Inventory?.InventoryID)
            {
                hoveredSlot = null;
            }

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 200, 350);

            ElementBounds priceTitleBounds = ElementBounds.Fixed(5, 35, 105, 35);
            ElementBounds priceSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 116, 20, 1, 1);
            ElementBounds paymentTitleBounds = ElementBounds.Fixed(15, 80, 250, 35);
            ElementBounds paymentSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 15, 100, 5, 1);

            ElementBounds goodsTitleBounds = ElementBounds.Fixed(15, 160, 250, 35);
            ElementBounds goodsSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 15, 180, 5, 1);
            ElementBounds goodsPricesTitleBounds = ElementBounds.Fixed(15, 230, 200, 35);
            ElementBounds[] goodPrice = { ElementBounds.Fixed(15, 250, 48, 35), ElementBounds.Fixed(66, 250, 48, 35), ElementBounds.Fixed(117, 250, 48, 35), ElementBounds.Fixed(168, 250, 48, 35), ElementBounds.Fixed(219, 250, 48, 35) };

            ElementBounds exchangeTitleBounds = ElementBounds.Fixed(0, 290, 280, 35);
            ElementBounds paymentInSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 66, 310, 1, 1);
            ElementBounds goodsOutSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 168, 310, 1, 1);

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
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, priceSlotsBounds)
                    .AddStaticText(Lang.Get("tradeomat:rug-payment"), CairoFont.WhiteSmallText(), paymentTitleBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 5, new int[] { 1, 2, 3, 4, 5 }, paymentSlotsBounds)
                    .AddStaticText(Lang.Get("tradeomat:rug-goods"), CairoFont.WhiteSmallText(), goodsTitleBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 5, new int[] { 6, 7, 8, 9, 10 }, goodsSlotsBounds)
                    .AddStaticText(Lang.Get("tradeomat:rug-goodsprices"), CairoFont.WhiteSmallText(), goodsPricesTitleBounds)
                    .AddTextInput(goodPrice[0], OnTextChanged, null, "price0")
                    .AddTextInput(goodPrice[1], OnTextChanged, null, "price1")
                    .AddTextInput(goodPrice[2], OnTextChanged, null, "price2")
                    .AddTextInput(goodPrice[3], OnTextChanged, null, "price3")
                    .AddTextInput(goodPrice[4], OnTextChanged, null, "price4")
                    .AddStaticText(Lang.Get("tradeomat:rug-exchange"), CairoFont.WhiteSmallText(), EnumTextOrientation.Center, exchangeTitleBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 11 }, paymentInSlotsBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 12 }, goodsOutSlotsBounds)
                .EndChildElements()
                .Compose()
            ;

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }

            for(int i = 0; i < 5; i++)
                SingleComposer.GetTextInput("price"+i).SetValue(prices[i]);

        }

        private void OnTextChanged(string str)
        {

        }

        private void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

        private void OnTitleBarClose()
        {
            TryClose();
            Inventory.SlotModified -= OnInventorySlotModified;
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                for (int i = 0; i < 5; i++)
                {
                    int price = 0;
                    try
                    {
                        price = int.Parse(SingleComposer.GetTextInput("price" + i).GetText());
                    } catch {}
                    writer.Write(price);
                }
                data = ms.ToArray();
            }
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1101, data);
            //Send to server!!!
        }

        public override bool OnEscapePressed()
        {
            Inventory.SlotModified -= OnInventorySlotModified;
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                for (int i = 0; i < 5; i++)
                {
                    int price = 0;
                    try
                    {
                        price = int.Parse(SingleComposer.GetTextInput("price" + i).GetText());
                    }
                    catch { }
                    writer.Write(price);
                }
                data = ms.ToArray();
            }
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1101, data);
            return base.OnEscapePressed();
        }
    }
}
