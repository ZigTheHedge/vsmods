using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace zeekea.src.standardInventories
{
    class GuiFreezer : GuiDialogBlockEntity
    {
        public GuiFreezer(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
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

            ElementBounds mainBounds = ElementBounds.Fixed(0, 0, 200, 100);

            ElementBounds iceBoundsTop = ElementStdBounds.SlotGrid(EnumDialogArea.None, 168, 20, 1, 1);
            ElementBounds iceBoundsLeft = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 102, 1, 1);
            ElementBounds iceBoundsRight = ElementStdBounds.SlotGrid(EnumDialogArea.None, 336, 102, 1, 1);
            ElementBounds iceBoundsBottom = ElementStdBounds.SlotGrid(EnumDialogArea.None, 168, 184, 1, 1);
            ElementBounds slotsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 92, 102, 4, 1);

            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mainBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            ClearComposers();
            SingleComposer = capi.Gui
                .CreateCompo("beeightslots" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, iceBoundsTop)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, iceBoundsLeft)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, iceBoundsRight)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 3 }, iceBoundsBottom)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 4, new int[] { 4, 5, 6, 7 }, slotsBounds)
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
        }

        public override bool OnEscapePressed()
        {
            base.OnEscapePressed();
            OnTitleBarClose();
            return TryClose();
        }
    }
}
