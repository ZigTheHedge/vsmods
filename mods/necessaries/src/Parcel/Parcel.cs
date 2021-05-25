using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace necessaries.src.Parcel
{
    class ParcelBlock : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEParcel be = null;
            if (blockSel.Position != null)
            {
                be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEParcel;
            }

            bool handled = false;

            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (activeSlot.Itemstack == null && !byPlayer.WorldData.EntityControls.Sneak)
            {
                ItemStack droppedItem = new ItemStack(this);

                if (be != null)
                {
                    ParcelInventory inv = (ParcelInventory)be.Inventory;
                    droppedItem.Attributes.SetInt("contentsCount", inv.Slots.Length);
                    for (int i = 0; i < inv.Slots.Length; i++)
                    {
                        if (inv.Slots[i].Itemstack == null) continue;
                        ItemStack stack = inv.Slots[i].Itemstack.Clone();

                        droppedItem.Attributes.SetItemstack("slot_" + i, stack);
                    }

                    droppedItem.Attributes.SetString("rcpt", be.rcptAddress);
                    droppedItem.Attributes.SetString("destAddressIdx", be.destAddress);
                    droppedItem.Attributes.SetString("message", be.message);

                    if (!byPlayer.InventoryManager.TryGiveItemstack(droppedItem))
                    {
                        world.SpawnItemEntity(droppedItem, be.Pos.ToVec3d().Add(0.5, 0.2, 0.5));
                    }

                    world.BlockAccessor.SetBlock(0, be.Pos);

                    if (this.Sounds?.Place != null)
                    {
                        world.PlaySoundAt(this.Sounds.Place, be.Pos.X, be.Pos.Y, be.Pos.Z, byPlayer, false);
                    }
                }

                handled = true;
            }

            if (!handled && byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer);
                }

                return true;
            }

            return handled;
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos blockPos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                ItemStack[] drops = GetDrops(world, blockPos, byPlayer, 1);
                ItemStack droppedItem = null;


                if (drops != null)
                {
                    if(drops.Length > 0)
                    {
                        droppedItem = drops[0].Clone();
                    }
                }

                if (EntityClass != null)
                {
                    BlockEntity entity = world.BlockAccessor.GetBlockEntity(blockPos);
                    if (entity != null)
                    {
                        if (entity is BEParcel)
                        {
                            ParcelInventory inv = (ParcelInventory)((BEParcel)entity).Inventory;
                            droppedItem.Attributes.SetInt("contentsCount", inv.Slots.Length);
                            for (int i = 0; i < inv.Slots.Length; i++)
                            {
                                if (inv.Slots[i].Itemstack == null) continue;
                                ItemStack stack = inv.Slots[i].Itemstack.Clone();

                                droppedItem.Attributes.SetItemstack("slot_" + i, stack);
                            }

                            droppedItem.Attributes.SetString("rcpt", ((BEParcel)entity).rcptAddress);
                            droppedItem.Attributes.SetString("destAddressIdx", ((BEParcel)entity).destAddress);
                            droppedItem.Attributes.SetString("message", ((BEParcel)entity).message);

                            world.SpawnItemEntity(droppedItem, new Vec3d(blockPos.X + 0.5, blockPos.Y + 0.5, blockPos.Z + 0.5), null);
                        }
                    }
                }

                world.PlaySoundAt(Sounds?.GetBreakSound(byPlayer), blockPos.X, blockPos.Y, blockPos.Z, byPlayer);
            }

            world.BlockAccessor.SetBlock(0, blockPos);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            if (byItemStack != null)
            {
                int contentsCount = byItemStack.Attributes.GetInt("contentsCount", -1);
                if (contentsCount != -1)
                {
                    if (EntityClass != null)
                    {
                        BlockEntity entity = world.BlockAccessor.GetBlockEntity(blockPos);
                        if (entity != null)
                        {
                            if (entity is BEParcel)
                            {
                                ParcelInventory inv = (ParcelInventory)((BEParcel)entity).Inventory;
                                for (int i = 0; i < contentsCount; i++)
                                {
                                    ItemStack stack = byItemStack.Attributes.GetItemstack("slot_" + i);
                                    if (stack == null) continue;
                                    stack.ResolveBlockOrItem(world);

                                    // stack is not a valid ItemStack! Block == null, Item == null ItemAttributes == null!
                                    inv.Slots[i].Itemstack = stack.Clone();
                                    inv.MarkSlotDirty(i);
                                }
                                ((BEParcel)entity).rcptAddress = byItemStack.Attributes.GetString("rcpt");
                                ((BEParcel)entity).message = byItemStack.Attributes.GetString("message");
                                ((BEParcel)entity).destAddress = byItemStack.Attributes.GetString("destAddressIdx");
                            }
                            entity.MarkDirty(true);
                            world.BlockAccessor.MarkBlockEntityDirty(blockPos);
                        }
                    }
                }
            }
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "necessaries:blockhelp-parcel-sneakopen",
                        HotKeyCode = "sneak",
                        RequireFreeHand = true,
                        MouseButton = EnumMouseButton.Right
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-behavior-rightclickpickup",
                        HotKeyCode = null,
                        RequireFreeHand = true,
                        MouseButton = EnumMouseButton.Right
                    }
                };

        }

    }
}
