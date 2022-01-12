using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace tradeomat.src.TradeomatBlock.Rug
{
    public class GuiElementItemSlotButton : GuiElement
    {
        public Action<string> OnClick;
        public string tag;
        public string bottomText;
        public static double unscaledItemSize = 32 * 0.8f;
        public static double unscaledSlotSize = 48;

        protected ItemSlot slot;
        protected IInventory inventory;

        bool drawBackground;
        bool isChecked = false;

        public CairoFont Font;
        GuiElementStaticText textComposer;
        protected LoadedTexture highlightSlotTexture;
        protected LoadedTexture selectedSlotTexture;

        bool isOver;

        bool active = true;
        bool currentlyMouseDownOnElement = false;

        public bool PlaySound = true;

        public override bool Focusable { get { return true; } }

        public bool IsChecked { get => isChecked; 
            set {
                isChecked = value; 
            }
        }

        public GuiElementItemSlotButton(ICoreClientAPI capi, ElementBounds bounds, Action<string> onClick, IInventory inventory, ItemSlot slot, bool drawBackground = true, string bottomText = null, string tag = null) : base(capi, bounds)
        {
            this.slot = slot;
            this.inventory = inventory;
            this.drawBackground = drawBackground;

            bounds.fixedWidth = unscaledSlotSize;
            bounds.fixedHeight = unscaledSlotSize;

            OnClick = onClick;
            this.tag = tag;
            this.bottomText = bottomText;
            Font = CairoFont.WhiteSmallText();

            highlightSlotTexture = new LoadedTexture(capi);
            selectedSlotTexture = new LoadedTexture(capi);
        }

        public override bool OnMouseLeaveSlot(ICoreClientAPI api, ItemSlot slot)
        {
            if (isOver)
            {
                isOver = false;
                api.Input.TriggerOnMouseLeaveSlot(slot);
            }
            return false;
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            if ((active && Bounds.PointInside(api.Input.MouseX, api.Input.MouseY)))
            {
                if (!isOver && PlaySound) api.Gui.PlaySound("menubutton");

                if (!isOver && slot != null)
                {
                    api.Input.TriggerOnMouseEnterSlot(slot);
                    isOver = true;
                }

            }
            else
            {
                if (isOver)
                {
                    api.Input.TriggerOnMouseLeaveSlot(slot);
                }

                isOver = false;
            }
        }

        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            if (!active) return;

            base.OnMouseDownOnElement(api, args);

            currentlyMouseDownOnElement = true;
        }

        public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseUp(api, args);

            currentlyMouseDownOnElement = false;
        }


        public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args)
        {
            if (active && currentlyMouseDownOnElement && Bounds.PointInside(args.X, args.Y) && args.Button == EnumMouseButton.Left)
            {
                if (PlaySound)
                {
                    api.Gui.PlaySound("menubutton_press");
                }
                OnClick(tag);
            }

            currentlyMouseDownOnElement = false;
        }

        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            if (isOver) { api.Input.TriggerOnMouseLeaveSlot(slot); isOver = false; }
            //base.OnKeyDown(api, args);
        }

        public override void OnKeyPress(ICoreClientAPI api, KeyEvent args)
        {
            //base.OnKeyPress(api, args);
        }

        /// <summary>
        /// Sets the button as active or inactive.
        /// </summary>
        /// <param name="active">Active == clickable</param>
        public void SetActive(bool active)
        {
            this.active = active;
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (slot.Itemstack != null)
            {
                double absSlotWidth = scaled(unscaledSlotSize);
                double absItemstackSize = scaled(unscaledItemSize);
                double offset = absSlotWidth / 2;
                api.Render.RenderItemstackToGui(slot, Bounds.renderX + offset, Bounds.renderY + offset, 450, (float)scaled(unscaledItemSize), ColorUtil.WhiteArgb);
            }
            if (isOver)
            {
                api.Render.Render2DTexturePremultipliedAlpha(
                    highlightSlotTexture.TextureId, (int)(Bounds.renderX - 2), (int)(Bounds.renderY - 2), Bounds.OuterWidthInt + 4, Bounds.OuterHeightInt + 4
                );
            }
            if (IsChecked)
            {
                api.Render.Render2DTexturePremultipliedAlpha(
                    selectedSlotTexture.TextureId, (int)(Bounds.renderX - 2), (int)(Bounds.renderY - 2), Bounds.OuterWidthInt + 4, Bounds.OuterHeightInt + 4
                );
            }
        }

        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();

            if (drawBackground)
            {
                ctx.SetSourceRGBA(69 / 255.0, 52 / 255.0, 36 / 255.0, 0.8);
                ElementRoundRectangle(ctx, Bounds);
                ctx.Fill();
                EmbossRoundRectangleElement(ctx, Bounds, true);
            }

            double absSlotWidth = scaled(unscaledSlotSize);
            double offset = (absSlotWidth - unscaledSlotSize) / 2;
            double embossHeight = scaled(2.5);

            // ----------- Hover Slot

            ImageSurface slotSurface = new ImageSurface(Format.Argb32, (int)absSlotWidth + 4, (int)absSlotWidth + 4);
            Context slotCtx = genContext(slotSurface);

            slotCtx.SetSourceRGBA(GuiStyle.ActiveSlotColor);
            RoundRectangle(slotCtx, 0, 0, absSlotWidth + 4, absSlotWidth + 4, GuiStyle.ElementBGRadius);
            slotCtx.LineWidth = scaled(9);
            slotCtx.StrokePreserve();
            slotSurface.Blur(scaled(6), true);
            slotCtx.StrokePreserve();
            slotSurface.Blur(scaled(6), true);

            slotCtx.LineWidth = scaled(3);
            slotCtx.Stroke();

            slotCtx.LineWidth = scaled(1);
            slotCtx.SetSourceRGBA(GuiStyle.ActiveSlotColor);
            slotCtx.Stroke();

            generateTexture(slotSurface, ref highlightSlotTexture);

            slotCtx.Dispose();
            slotSurface.Dispose();

            //---------- Selected Slot

            slotSurface = new ImageSurface(Format.Argb32, (int)absSlotWidth + 4, (int)absSlotWidth + 4);
            slotCtx = genContext(slotSurface);

            slotCtx.SetSourceRGBA(217 / 255.0, 229 / 255.0, 29 / 255.0, 1);
            RoundRectangle(slotCtx, 0, 0, absSlotWidth + 4, absSlotWidth + 4, GuiStyle.ElementBGRadius);
            slotCtx.LineWidth = scaled(9);
            slotCtx.StrokePreserve();
            slotSurface.Blur(scaled(6), true);
            slotCtx.StrokePreserve();
            slotSurface.Blur(scaled(6), true);

            slotCtx.LineWidth = scaled(3);
            slotCtx.Stroke();

            slotCtx.LineWidth = scaled(1);
            slotCtx.SetSourceRGBA(217 / 255.0, 229 / 255.0, 29 / 255.0, 1);
            slotCtx.Stroke();

            generateTexture(slotSurface, ref selectedSlotTexture);

            slotCtx.Dispose();
            slotSurface.Dispose();

            // ---------------------------- 

            if (bottomText == null) return;
            
            ElementBounds textBounds = ElementBounds
                .Fixed(Bounds.fixedX + unscaledSlotSize / 2 - 4, Bounds.fixedY + GuiStyle.SmallFontSize + unscaledSlotSize, unscaledSlotSize, unscaledSlotSize)
                .WithEmptyParent();

            textBounds.CalcWorldBounds();

            ElementBounds bgBounds = ElementBounds
                .Fixed(scaled(Bounds.fixedX + 2) + absSlotWidth / 2, scaled(Bounds.fixedY + unscaledSlotSize - 8), absSlotWidth - scaled(12), scaled(GuiStyle.SmallFontSize + 6))
                .WithEmptyParent();

            Rectangle(ctx, bgBounds.fixedX, bgBounds.fixedY + bgBounds.fixedHeight, bgBounds.fixedWidth, bgBounds.fixedHeight);
            ctx.SetSourceRGBA(69 / 255.0, 52 / 255.0, 36 / 255.0, 1);
            ctx.Fill();

            Rectangle(ctx, bgBounds.fixedX + embossHeight, bgBounds.fixedY + bgBounds.fixedHeight + embossHeight, bgBounds.fixedWidth - embossHeight * 2, bgBounds.fixedHeight - embossHeight * 2);
            ctx.SetSourceRGBA(244 / 255.0, 216 / 255.0, 125 / 255.0, 1);
            ctx.Fill();

            Font.FontWeight = FontWeight.Bold;
            Font.Color[0] = 69 / 255.0;
            Font.Color[1] = 52 / 255.0;
            Font.Color[2] = 36 / 255.0;
            textComposer = new GuiElementStaticText(api, bottomText, EnumTextOrientation.Center, textBounds, Font);

            textComposer.ComposeElements(ctx, surface);

            
        }

        public override void Dispose()
        {
            base.Dispose();

            highlightSlotTexture?.Dispose();
            selectedSlotTexture?.Dispose();
            textComposer?.Dispose();
        }
    }

    public static partial class GuiComposerHelpers
    {

        /// <summary>
        /// Adds a passive item slot to the GUI.
        /// </summary>
        /// <param name="bounds">The bounds of the Slot</param>
        /// <param name="onClick">The event fired when the slot is clicked.</param>
        /// <param name="inventory">The inventory attached to the slot.</param>
        /// <param name="slot">The internal slot of the slot.</param>
        /// <param name="drawBackground">Do we draw the background for this slot? (Default: true)</param>
        public static GuiComposer AddItemSlotButton(this GuiComposer composer, ElementBounds bounds, Action<string> onClick, IInventory inventory, ItemSlot slot, bool drawBackground = true, string bottomText = null, string tag = null)
        {
            composer.AddInteractiveElement(new GuiElementItemSlotButton(composer.Api, bounds, onClick, inventory, slot, drawBackground, bottomText, tag), tag);

            return composer;
        }

        /// <summary>
        /// Gets the button by name.
        /// </summary>
        /// <param name="key">The name of the button.</param>
        public static GuiElementItemSlotButton GetItemSlotButton(this GuiComposer composer, string key)
        {
            return (GuiElementItemSlotButton)composer.GetElement(key);
        }
    }
}
