using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace tastydays.src
{
    class MultiFood : Item
    {
        MultiNutritionProps nutritionProps = new MultiNutritionProps();


        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            nutritionProps.LoadFromAttributes(slot.Itemstack.Collectible.Attributes);
            if (!nutritionProps.isValid) return;

            byEntity.World.RegisterCallback((dt) =>
            {
                if (byEntity.Controls.HandUse == EnumHandInteract.HeldItemInteract)
                {
                    IPlayer player = null;
                    if (byEntity is EntityPlayer) player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                    byEntity.PlayEntitySound("eat", player);
                }
            }, 500);

            byEntity.AnimManager?.StartAnimation("eat");

            handling = EnumHandHandling.PreventDefault;

        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {

            Vec3d pos = byEntity.Pos.AheadCopy(0.4f).XYZ;
            pos.X += byEntity.LocalEyePos.X;
            pos.Y += byEntity.LocalEyePos.Y - 0.4f;
            pos.Z += byEntity.LocalEyePos.Z;

            if (secondsUsed > 0.5f && (int)(30 * secondsUsed) % 7 == 1)
            {
                byEntity.World.SpawnCubeParticles(pos, slot.Itemstack, 0.3f, 4, 0.5f, (byEntity as EntityPlayer)?.Player);
            }


            if (byEntity.World is IClientWorldAccessor)
            {
                ModelTransform tf = new ModelTransform();

                tf.EnsureDefaultValues();

                tf.Origin.Set(0f, 0, 0f);

                if (secondsUsed > 0.5f)
                {
                    tf.Translation.Y = Math.Min(0.02f, GameMath.Sin(20 * secondsUsed) / 10);
                }

                tf.Translation.X -= Math.Min(1f, secondsUsed * 4 * 1.57f);
                tf.Translation.Y -= Math.Min(0.05f, secondsUsed * 2);

                tf.Rotation.X += Math.Min(60f, secondsUsed * 350);
                tf.Rotation.Z += Math.Min(80f, secondsUsed * 350);

                byEntity.Controls.UsingHeldItemTransformAfter = tf;

                return secondsUsed <= 1f;
            }

            // Let the client decide when he is done eating
            return true;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            //FoodNutritionProperties nutriProps = GetNutritionProperties(byEntity.World, slot.Itemstack, byEntity as Entity);

            if (byEntity.World is IServerWorldAccessor && nutritionProps.isValid && secondsUsed >= 0.95f)
            {
                TransitionState state = UpdateAndGetTransitionState(api.World, slot, EnumTransitionType.Perish);
                float spoilState = state != null ? state.TransitionLevel : 0;

                float satLossMul = GlobalConstants.FoodSpoilageSatLossMul(spoilState, slot.Itemstack, byEntity);
                float healthLossMul = GlobalConstants.FoodSpoilageHealthLossMul(spoilState, slot.Itemstack, byEntity);

                foreach(FoodNutritionProperties nutrition in nutritionProps.foodNutritions)
                    byEntity.ReceiveSaturation(nutrition.Satiety * satLossMul, nutrition.FoodCategory);

                IPlayer player = null;
                if (byEntity is EntityPlayer) player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                if (nutritionProps.eatenStack != null)
                {
                    nutritionProps.eatenStack.Resolve(api.World, "MultiFood", true);
                    if (player == null)
                    {
                        byEntity.World.SpawnItemEntity(nutritionProps.eatenStack.ResolvedItemstack.Clone(), byEntity.SidedPos.XYZ);
                    } else
                    {
                        bool result = player.InventoryManager.TryGiveItemstack(nutritionProps.eatenStack.ResolvedItemstack.Clone(), true);
                        if(!result)
                            byEntity.World.SpawnItemEntity(nutritionProps.eatenStack.ResolvedItemstack.Clone(), byEntity.SidedPos.XYZ);
                    }
                }

                slot.Itemstack.StackSize--;

                float healthChange = nutritionProps.Health * healthLossMul;

                if (healthChange != 0)
                {
                    byEntity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Internal, Type = healthChange > 0 ? EnumDamageType.Heal : EnumDamageType.Poison }, Math.Abs(healthChange));
                }

                slot.MarkDirty();
                player.InventoryManager.BroadcastHotbarSlot();
            }
        }

    }
}
