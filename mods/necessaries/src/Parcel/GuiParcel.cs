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

namespace necessaries.src.Parcel
{
    class GuiParcel : GuiDialogBlockEntity
    {
        private string defRcpt = "";
        private string defMessage = "";
        private string defDest = "";

        public string DefRcpt {
            get => defRcpt;
            set
            {
                defRcpt = value;
                SingleComposer.GetDynamicText("rcpt").SetNewText(defRcpt);
            }
        }

        public string DefMessage {
            get => defMessage;
            set
            {
                defMessage = value;
                GuiElementTextArea msgArea = SingleComposer.GetTextArea("message");
                msgArea.SetValue(defMessage);
                SingleComposer.GetScrollbar("scrollbar").SetHeights(
                    (float)msgArea.Bounds.fixedHeight, (float)msgArea.Bounds.fixedHeight
                );
            }
        }

        public string DefDest
        {
            get => defDest;
            set
            {
                defDest = value;
                if(value.ToInt(-1) != -1)
                {
                    SingleComposer.GetDropDown("destination").SetSelectedValue(value);

                }
            }
        }

        public GuiParcel(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
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

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 300, 150);

            ElementBounds rcptTitleBounds = ElementBounds.Fixed(0, 30, 300, 20);
            ElementBounds rcptBounds = ElementBounds.Fixed(0, 60, 300, 20);
            
            ElementBounds destTitleBounds = ElementBounds.Fixed(0, 90, 300, 20);
            ElementBounds destBounds = ElementBounds.Fixed(0, 120, 300, 30);

            ElementBounds messageTitleBounds = ElementBounds.Fixed(0, 160, 300, 20);
            ElementBounds messageBounds = ElementBounds.Fixed(0, 190, 277, 100);

            ElementBounds messageClippingBounds = messageBounds.ForkBoundingParent().WithFixedPosition(0, 190);

            ElementBounds messageScrollbarBounds = messageClippingBounds.CopyOffsetedSibling(messageBounds.fixedWidth + 3).WithFixedWidth(20);

            ElementBounds containsSlotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 300, 2, 1);

            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            int listLen = 0;

            for (int i = 0; i < Necessaries.postServicesClient.Count; i++)
            {
                PostService postService = Necessaries.postServicesClient[i];
                if (postService.isValid && postService.title != "") listLen++;
            }

            if(listLen == 0)
            {
                listLen = 1;
            }

            string[] rcptIdx = new string[listLen];
            string[] rcptAddr = new string[listLen];
            int rcptCount = 0;

            for (int i = 0; i < Necessaries.postServicesClient.Count; i++)
            {
                PostService postService = Necessaries.postServicesClient[i];
                if (!postService.isValid || postService.title == "") continue;
                rcptIdx[rcptCount] = i.ToString();
                rcptAddr[rcptCount++] = postService.title;
            }

            if(rcptCount == 0)
            {
                rcptIdx[0] = "-1";
                rcptAddr[0] = Lang.Get("mailbox-no-destinations");
            }


            ClearComposers();
            SingleComposer = capi.Gui
                .CreateCompo("blockentitymailbox" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticText(Lang.Get("necessaries:parcel-rcpt-title"), CairoFont.WhiteSmallText(), rcptTitleBounds)
                    .AddDynamicText("", CairoFont.WhiteSmallText(), rcptBounds, "rcpt")
                    .AddStaticText(Lang.Get("necessaries:mailbox-recipient"), CairoFont.WhiteSmallText(), destTitleBounds)
                    .AddDropDown(rcptIdx, rcptAddr, 0, null, destBounds, "destination")
                    .AddStaticText(Lang.Get("necessaries:parcel-message-title"), CairoFont.WhiteSmallText(), messageTitleBounds)
                    .AddIf(ModConfigFile.Current.reducedParcelVolume)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, containsSlotsBounds)
                    .EndIf()
                    .AddIf(!ModConfigFile.Current.reducedParcelVolume)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 2, new int[] { 0, 1 }, containsSlotsBounds)
                    .EndIf()
                    .BeginClip(messageClippingBounds)
                    .AddTextArea(messageBounds, OnMessageChanged, CairoFont.WhiteDetailText(), "message")
                    .EndClip()
                    .AddVerticalScrollbar(OnMessageScrollbar, messageScrollbarBounds, "scrollbar")
                .EndChildElements()
                .Compose()
            ;

            SingleComposer.GetScrollbar("scrollbar").SetHeights(
                (float)messageBounds.fixedHeight, (float)messageBounds.fixedHeight
            );

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
        }

        private void OnMessageScrollbar(float value)
        {
            GuiElementTextArea textArea = SingleComposer.GetTextArea("message");

            textArea.Bounds.fixedY = 3 - value;
            textArea.Bounds.CalcWorldBounds();
        }

        private void OnMessageChanged(string newText)
        {
            GuiElementTextArea textArea = SingleComposer.GetTextArea("message");
            SingleComposer.GetScrollbar("scrollbar").SetNewTotalHeight((float)textArea.Bounds.fixedHeight);

        }

        private void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

        private void OnTitleBarClose()
        {
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(SingleComposer.GetTextArea("message").GetText());
                writer.Write(SingleComposer.GetDropDown("destination").SelectedValue);
                data = ms.ToArray();
            }

            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, 1001, data);
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
