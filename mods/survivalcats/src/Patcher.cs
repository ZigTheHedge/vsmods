using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace survivalcats.src
{
    [HarmonyPatch(typeof(GuiDialogHandbook), "loadEntries")]
    class PatcherLoadEntries
    {
        public static void Postfix(List<string> ___categoryCodes, List<GuiHandbookPage> ___allHandbookPages)
        {
            List<string> creativeTabs = new List<string>();

            foreach (GuiHandbookPage page in ___allHandbookPages)
            {
                if (!(page is GuiHandbookItemStackPage)) continue;
                GuiHandbookItemStackPage isp = (GuiHandbookItemStackPage)page;

                foreach (string category in isp.Stack.Collectible.CreativeInventoryTabs)
                {
                    if (!creativeTabs.Contains(category) && category != "general")
                    {
                        creativeTabs.Add(category);
                        ___categoryCodes.Add("#" + category);
                        string translateKey = "game:handbook-category-#" + category;
                        if (Lang.GetIfExists(translateKey) == null)
                            Lang.Inst.LangEntries.Add(translateKey, Lang.Get("game:tabname-"+category));
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GuiHandbookItemStackPage), "TextMatchWeight")]
    class PatcherTextMatchWeight
    {
        public static bool Prefix(ref float __result, ItemStack ___Stack, ref string searchText)
        {
            if (searchText.Length > 3 && searchText.StartsWith("@"))
            {
                int spacePos = searchText.IndexOf(' ');
                string searchDomain;
                if (spacePos > -1)
                    searchDomain = searchText.Substring(1, spacePos - 1);
                else
                    searchDomain = searchText.Substring(1);

                if (___Stack != null && ___Stack.Collectible != null)
                {
                    if (___Stack.Collectible.Code.Domain.StartsWith(searchDomain, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (spacePos > -1)
                        {
                            searchText = searchText.Substring(searchDomain.Length + 2);
                            return true;
                        } else
                        {
                            __result = 1;
                            return false;
                        }
                    }
                }

                __result = 0;
                return false;
            } else if (searchText.Length > 3 && searchText.StartsWith("#"))
            {
                int spacePos = searchText.IndexOf(' ');
                string searchCategory;
                if (spacePos > -1)
                    searchCategory = searchText.Substring(1, spacePos - 1);
                else
                    searchCategory = searchText.Substring(1);

                if (___Stack != null && ___Stack.Collectible != null && ___Stack.Collectible.CreativeInventoryTabs != null)
                {
                    foreach(string cat in ___Stack.Collectible.CreativeInventoryTabs)
                    {
                        if(cat.StartsWith(searchCategory, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (spacePos > -1)
                            {
                                searchText = searchText.Substring(searchCategory.Length + 2);
                                return true;
                            }
                            else
                            {
                                __result = 1;
                                return false;
                            }

                        }
                    }
                }

                __result = 0;
                return false;
            }
            else
                return true;
        }
    }

    [HarmonyPatch(typeof(GuiDialogHandbook), "FilterItems")]
    class PatcherFilterItems
    {
        public static bool Prefix(ref string ___currentCatgoryCode, ref string ___currentSearchText, GuiComposer ___overviewGui)
        {
            string searchText = ___overviewGui.GetTextInput("searchField").GetText();
            if (___currentCatgoryCode != null && ___currentCatgoryCode.StartsWith("#"))
            {
                if (searchText != "")
                    ___currentSearchText = ___currentCatgoryCode + " " + searchText;
                else
                    ___currentSearchText = ___currentCatgoryCode;
            } else
            {
                ___currentSearchText = searchText;
            }
            return true;
        }


        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo f_currentCatgoryCode = AccessTools.Field(typeof(GuiDialogHandbook), "currentCatgoryCode");
            MethodInfo m_startsWith = AccessTools.Method(typeof(string), "StartsWith", new Type[] { typeof(string) } );

            bool found = false, next = false;
            foreach(CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(f_currentCatgoryCode) || next)
                {
                    if (!found)
                    {
                        if(next)
                        {
                            found = true;
                            next = false;
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, f_currentCatgoryCode);
                            yield return new CodeInstruction(OpCodes.Ldstr, "#");
                            yield return new CodeInstruction(OpCodes.Callvirt, m_startsWith);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, instruction.operand);
                        }
                        else
                            next = true;
                    } 
                }
            }
        }
        
    }
}
