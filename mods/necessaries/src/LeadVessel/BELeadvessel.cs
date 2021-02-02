using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace necessaries.src.LeadVessel
{
    public enum LeadVesselContents
    {
        EMPTY, WATER, SULFUR, WATERSULFUR, FULL, ACID, ACID1, ACID2, ACID3
    }

    class BELeadvessel : BlockEntity
    {
        public LeadVesselContents locals;
        public double burnTime = 0;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(Tick, 1000);
        }

        public void Tick(float dt)
        {
            if(burnTime > 0)
            {
                if(Api.World.Calendar.TotalHours >= burnTime)
                {
                    if (Block.Variant["contents"] == "full")
                    {
                        burnTime = 0;
                        locals = LeadVesselContents.ACID;
                        MarkDirty();
                    } else
                    {
                        burnTime = 0;
                        locals = LeadVesselContents.WATER;                        
                        MarkDirty();
                        SetBlockState("water");
                    }
                }
            }
        }

        public void SetBlockState(string state)
        {
            AssetLocation loc = Block.CodeWithVariant("contents", state);
            Block block = Api.World.GetBlock(loc);
            if (block == null) return;

            Api.World.BlockAccessor.ExchangeBlock(block.Id, Pos);
            this.Block = block;
        }

        public bool OnInteract(ItemStack stack, IPlayer byPlayer)
        {
            string contents = Block.Variant["contents"];
            if(stack == null)
            {
                if(contents == "full")
                {
                    if (locals == LeadVesselContents.WATER) SetBlockState("water");
                    else if (locals == LeadVesselContents.WATERSULFUR)
                    {
                        if (burnTime > 0)
                        {
                            burnTime = 0;
                            SetBlockState("water");
                            locals = LeadVesselContents.WATER;
                            MarkDirty();
                        }
                        else
                            SetBlockState("watersulfur");
                    }
                    else if (locals == LeadVesselContents.SULFUR) SetBlockState("sulfur");
                    else if (locals == LeadVesselContents.ACID) SetBlockState("acid");
                    else if (locals == LeadVesselContents.ACID1) SetBlockState("acid1");
                    else if (locals == LeadVesselContents.ACID2) SetBlockState("acid2");
                    else if (locals == LeadVesselContents.ACID3) SetBlockState("acid3");
                    else SetBlockState("empty");
                    ItemStack cover = new ItemStack(Api.World.GetBlock(new AssetLocation("necessaries:leadvessel_cover")));
                    Api.World.SpawnItemEntity(cover, Pos.ToVec3d().Add(0.5, 0.9, 0.5));
                } else if(contents == "acid3")
                {
                    SetBlockState("empty");
                    locals = LeadVesselContents.EMPTY;
                    ItemStack gluten = new ItemStack(Api.World.GetItem(new AssetLocation("necessaries:gluten")));
                    Api.World.SpawnItemEntity(gluten, Pos.ToVec3d().Add(0.5, 0.9, 0.5));
                }
                return true;
            }
            if (stack.Class == EnumItemClass.Item)
            {
                if (stack.Item.FirstCodePart().StartsWith("waterportion"))
                {
                    if (contents == "empty")
                    {
                        SetBlockState("water");
                        locals = LeadVesselContents.WATER;
                        MarkDirty();
                        return true;
                    }
                    else if (contents == "sulfur")
                    {
                        SetBlockState("watersulfur");
                        locals = LeadVesselContents.WATERSULFUR;
                        MarkDirty();
                        return true;
                    }
                    else
                        return false;
                }
                else if (stack.Item.FirstCodePart().StartsWith("sulfurpeter"))
                {
                    if (contents == "empty")
                    {
                        SetBlockState("sulfur");
                        locals = LeadVesselContents.SULFUR;
                        MarkDirty();
                        return true;
                    }
                    else if (contents == "water")
                    {
                        SetBlockState("watersulfur");
                        locals = LeadVesselContents.WATERSULFUR;
                        MarkDirty();
                        return true;
                    }
                    else
                        return false;
                }
                else if (stack.Item.FirstCodePart().StartsWith("bonemeal"))
                {
                    if (contents == "acid")
                    {
                        SetBlockState("acid1");
                        locals = LeadVesselContents.ACID1;
                        MarkDirty();
                        return true;
                    }
                    else if (contents == "acid1")
                    {
                        SetBlockState("acid2");
                        locals = LeadVesselContents.ACID2;
                        MarkDirty();
                        return true;
                    }
                    else if (contents == "acid2")
                    {
                        SetBlockState("acid3");
                        locals = LeadVesselContents.ACID3;
                        MarkDirty();
                        return true;
                    }
                    else
                        return false;
                }
                return false;
            }
            else
            {
                if (stack.Block.FirstCodePart().StartsWith("leadvessel_cover"))
                {
                    if (contents != "full")
                    {
                        SetBlockState("full");
                        if(locals == LeadVesselContents.WATERSULFUR && burnTime > 0)
                        {
                            burnTime = Api.World.Calendar.TotalHours + 12D;
                            MarkDirty();
                        }
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            locals = (LeadVesselContents)Enum.Parse(typeof(LeadVesselContents), tree.GetString("locals"));
            burnTime = tree.GetDouble("burnTime");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("locals", locals.ToString());
            tree.SetDouble("burnTime", burnTime);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
                return;
            }

            if (locals == LeadVesselContents.EMPTY)
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-addwaterandsulfurpeter"));
            }
            else if (locals == LeadVesselContents.WATER)
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-addsulfurpeter"));
            }
            else if (locals == LeadVesselContents.SULFUR)
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-addwater"));
            }
            else if (locals == LeadVesselContents.WATERSULFUR && burnTime == 0)
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-setonfire"));
            }
            else if (locals == LeadVesselContents.WATERSULFUR && burnTime > 0 && Block.Variant["contents"] != "full")
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-close", (int)(60D * (burnTime - Api.World.Calendar.TotalHours))));
            }
            else if (locals == LeadVesselContents.WATERSULFUR && burnTime > 0 && Block.Variant["contents"] == "full")
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-wait", (int)(burnTime - Api.World.Calendar.TotalHours)));
            }
            else if (locals == LeadVesselContents.ACID && Block.Variant["contents"] == "full")
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-acid"));
            }
            else if (locals == LeadVesselContents.ACID && Block.Variant["contents"] == "acid")
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-acid0"));
            }
            else if (locals == LeadVesselContents.ACID1 && Block.Variant["contents"] == "acid1")
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-acid1"));
            }
            else if (locals == LeadVesselContents.ACID2 && Block.Variant["contents"] == "acid2")
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-acid2"));
            }
            else if (locals == LeadVesselContents.ACID3 && Block.Variant["contents"] == "acid3")
            {
                sb.AppendLine(Lang.Get("necessaries:leadvessel-acid3"));
            }
        }
    }
}
