using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace tastydays.src
{
    class MultiNutritionProps
    {
        public List<FoodNutritionProperties> foodNutritions = new List<FoodNutritionProperties>();
        public JsonItemStack eatenStack = null;
        public float Health = 0;
        public bool isValid = false;

        public void LoadFromAttributes(JsonObject attributes)
        {
            if (attributes == null) return;

            if(attributes["multinutrition"].Exists)
            {
                if (attributes["multinutrition"]["nutritionProps"].Exists)
                {
                    foreach (JsonObject obj in attributes["multinutrition"]["nutritionProps"].AsArray())
                    {
                        FoodNutritionProperties props = new FoodNutritionProperties();
                        string category = obj["foodcategory"].AsString("unknown");
                        if (category.Equals("diary", StringComparison.InvariantCultureIgnoreCase)) props.FoodCategory = EnumFoodCategory.Dairy;
                        else if (category.Equals("fruit", StringComparison.InvariantCultureIgnoreCase)) props.FoodCategory = EnumFoodCategory.Fruit;
                        else if (category.Equals("grain", StringComparison.InvariantCultureIgnoreCase)) props.FoodCategory = EnumFoodCategory.Grain;
                        else if (category.Equals("protein", StringComparison.InvariantCultureIgnoreCase)) props.FoodCategory = EnumFoodCategory.Protein;
                        else if (category.Equals("vegetables", StringComparison.InvariantCultureIgnoreCase)) props.FoodCategory = EnumFoodCategory.Vegetable;
                        else props.FoodCategory = EnumFoodCategory.Unknown;
                        props.Satiety = obj["satiety"].AsFloat();

                        foodNutritions.Add(props);
                    }
                    isValid = true;
                }
                if (attributes["multinutrition"]["eatenstack"].Exists)
                {
                    /*
                    eatenStack = new JsonItemStack();
                    eatenStack.Type = (attributes["multinutrition"]["eatenstack"]["type"].AsString().Equals("item", StringComparison.InvariantCultureIgnoreCase)) ? EnumItemClass.Item : EnumItemClass.Block;
                    eatenStack.Code = new AssetLocation(attributes["multinutrition"]["eatenstack"]["code"].AsString());
                    */
                    eatenStack = attributes["multinutrition"]["eatenstack"].AsObject<JsonItemStack>();
                }
                Health = attributes["multinutrition"]["health"].AsFloat(0);
            }
        }
    }
}
