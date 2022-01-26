using Cairo;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace survivalcats.src
{
    [HarmonyPatch(typeof(GuiDialogHandbook), "loadEntries")]
    class PatcherLoadEntries
    {

        public static void Postfix(List<string> ___categoryCodes, List<GuiHandbookPage> ___allHandbookPages, ICoreClientAPI ___capi)
        {
            List<string> creativeTabs = new List<string>();
            List<GuiHandbookBookmarkedItemStackPage> bookmarkedPages = new List<GuiHandbookBookmarkedItemStackPage>();
            List<string> bookmarks = new List<string>();

            ___categoryCodes.Add("bookmark");

            string path = System.IO.Path.Combine(GamePaths.DataPath, "ModData", "bookmarks." + ___capi.World.Seed.ToString() + ".json");
            PatcherinitDetailGui.bookmarksPath = path;

            if (File.Exists(path))
            {
                foreach (string bookmark in File.ReadLines(path))
                    bookmarks.Add(bookmark);
            }

            
            CreativeTabsConfig creativeTabsConfig = ___capi.Assets.TryGet("config/creativetabs.json").ToObject<CreativeTabsConfig>();

            foreach (TabConfig tab in creativeTabsConfig.TabConfigs)
            {
                if (tab.code == "general" || tab.code == "meta") continue;
                creativeTabs.Add(tab.code);
                if (ModConfigFile.Current.displayVanillaTabsAsCategories) ___categoryCodes.Add("#" + tab.code);
            }

            if (ModConfigFile.Current.displayModsAsCategories)
            {
                foreach (GuiHandbookPage page in ___allHandbookPages)
                {
                    if (!(page is GuiHandbookItemStackPage)) continue;
                    GuiHandbookItemStackPage isp = (GuiHandbookItemStackPage)page;

                    foreach (string category in isp.Stack.Collectible.CreativeInventoryTabs)
                    {
                        if (!creativeTabs.Contains(category) && category != "general" && category != "meta")
                        {
                            creativeTabs.Add(category);
                            ___categoryCodes.Add("#" + category);
                        }
                    }

                    foreach (string bookmark in bookmarks)
                        if (isp.PageCode.Equals(bookmark))
                            bookmarkedPages.Add(new GuiHandbookBookmarkedItemStackPage(___capi, isp.Stack));
                }
            }
            else
            {
                foreach (GuiHandbookPage page in ___allHandbookPages)
                {
                    if (!(page is GuiHandbookItemStackPage)) continue;
                    GuiHandbookItemStackPage isp = (GuiHandbookItemStackPage)page;

                    foreach (string bookmark in bookmarks)
                        if (isp.PageCode.Equals(bookmark))
                            bookmarkedPages.Add(new GuiHandbookBookmarkedItemStackPage(___capi, isp.Stack));
                }
            }

            foreach (GuiHandbookBookmarkedItemStackPage elem in bookmarkedPages)
                ___allHandbookPages.Add(elem);
        }

        private class CreativeTabsConfig
        {
            public TabConfig[] TabConfigs = null;
        }

        private class TabConfig
        {
            public string code = "";
            public float listOrder = 0;
            public int paddingTop = 0;
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
                        }
                        else
                        {
                            __result = 1;
                            return false;
                        }
                    }
                }

                __result = 0;
                return false;
            }
            else if (searchText.Length > 3 && searchText.StartsWith("#"))
            {
                int spacePos = searchText.IndexOf(' ');
                string searchCategory;
                if (spacePos > -1)
                    searchCategory = searchText.Substring(1, spacePos - 1);
                else
                    searchCategory = searchText.Substring(1);

                if (___Stack != null && ___Stack.Collectible != null && ___Stack.Collectible.CreativeInventoryTabs != null)
                {
                    foreach (string cat in ___Stack.Collectible.CreativeInventoryTabs)
                    {
                        if (cat.StartsWith(searchCategory, StringComparison.InvariantCultureIgnoreCase))
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
            else if (searchText.Length > 3 && searchText.StartsWith("$"))
            {
                int spacePos = searchText.IndexOf(' ');
                string searchCode;
                if (spacePos > -1)
                    searchCode = searchText.Substring(1, spacePos - 1);
                else
                    searchCode = searchText.Substring(1);

                if (___Stack != null && ___Stack.Collectible != null)
                {
                    if (___Stack.Collectible.Code.ToString().CaseInsensitiveContains(searchCode))
                    {
                        if (spacePos > -1)
                        {
                            searchText = searchText.Substring(searchCode.Length + 2);
                            return true;
                        }
                        else
                        {
                            __result = 1;
                            return false;
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
            }
            else
            {
                ___currentSearchText = searchText;
            }
            return true;
        }



        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo f_currentCatgoryCode = AccessTools.Field(typeof(GuiDialogHandbook), "currentCatgoryCode");
            MethodInfo m_startsWith = AccessTools.Method(typeof(string), "StartsWith", new Type[] { typeof(string) });

            bool found = false, next = false;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(f_currentCatgoryCode) || next)
                {
                    if (!found)
                    {
                        if (next)
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

    [HarmonyPatch(typeof(GuiDialogHandbook), "OnTabClicked")]
    class PatcherOnTabClicked
    {
        public static bool Prefix(GuiComposer ___overviewGui)
        {
            ___overviewGui.GetTextInput("searchField").SetValue("");
            return true;
        }
    }

    [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), "GetHandbookInfo")]
    class PatcherGetHandbookInfo
    {
        static string BuildQuantities(List<GridRecipe> recipes)
        {
            List<int> added = new List<int>();
            string ret = "";
            foreach (GridRecipe recipe in recipes)
            {
                int q = recipe.Output.Quantity;
                if (!added.Contains(q))
                {
                    added.Add(q);
                    if (ret == "") ret = "x" + q;
                    else ret += ", x" + q;
                }
            }
            return ret;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo m_conCatOriginal = AccessTools.Method(typeof(string), "Concat", new Type[] { typeof(string), typeof(string), typeof(string) });
            MethodInfo m_conCatReplacement = AccessTools.Method(typeof(string), "Concat", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
            MethodInfo m_BuildQuantities = AccessTools.Method(typeof(PatcherGetHandbookInfo), "BuildQuantities");

            byte foundStage = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr && instruction.operand.Equals("Crafting"))
                {
                    if (foundStage == 0) foundStage = 1;
                }
                if (foundStage == 1 && instruction.opcode == OpCodes.Ldstr && instruction.operand.Equals("\n"))
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, " (");
                    yield return new CodeInstruction(OpCodes.Ldloc, 19); //gridRecipeList1
                    yield return new CodeInstruction(OpCodes.Call, m_BuildQuantities);
                    yield return new CodeInstruction(OpCodes.Ldstr, ")");
                    yield return new CodeInstruction(OpCodes.Call, m_conCatOriginal);
                    foundStage = 2;
                }
                if (foundStage == 2 && instruction.Calls(m_conCatOriginal))
                {
                    yield return new CodeInstruction(OpCodes.Call, m_conCatReplacement);
                    foundStage = 3;
                    continue;
                }
                yield return instruction;
            }
        }
    }
    
    [HarmonyPatch(typeof(GuiDialogHandbook), "initDetailGui")]
    static class PatcherinitDetailGui
    {

        public static string bookmarksPath = "";
        
        static string patchButtonIn(ref ElementBounds bounds, ElementBounds parentBounds, BrowseHistoryElement element)
        {
            bounds = ElementBounds
                .FixedSize(0, 0)
                .FixedUnder(parentBounds, 2 * 5 + 5)
                .WithAlignment(EnumDialogArea.LeftFixed)
                .WithFixedPadding(20, 4)
                .WithFixedAlignmentOffset(120, 1);

            bool found = false;
            if (File.Exists(bookmarksPath))
            {
                foreach (string bookmark in File.ReadLines(bookmarksPath))
                    if (bookmark.Equals(element.Page.PageCode)) found = true;
            }

            if (!found)
            {
                return Lang.Get("game:button-bookmark");
            }
            else
                return Lang.Get("game:button-bookmark-remove");
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            LocalBuilder l_bookmarkBounds = generator.DeclareLocal(typeof(ElementBounds));

            MethodInfo m_AddSmallButton = AccessTools.Method(typeof(Vintagestory.API.Client.GuiComposerHelpers), "AddSmallButton");
            MethodInfo m_patchButtonIn = AccessTools.Method(typeof(PatcherinitDetailGui), "patchButtonIn");
            MethodInfo m_ButtonCallback = AccessTools.Method(typeof(GuiDialogHandbookBookmarks), "OnBookmarkClick");
            ConstructorInfo m_ActionConsumable = AccessTools.Constructor(typeof(ActionConsumable), new Type[] { typeof(object), typeof(IntPtr) });

            MethodInfo m_FixedUnder = AccessTools.Method(typeof(ElementBounds), "FixedUnder");
            MethodInfo m_WithFixedPadding = AccessTools.Method(typeof(ElementBounds), "WithFixedPadding", new Type[] { typeof(double), typeof(double) } );

            FieldInfo f_BrowseHistoryElement = AccessTools.Field(typeof(GuiDialogHandbook), "browseHistory");
            MethodInfo m_Peek = AccessTools.Method(typeof(Stack<BrowseHistoryElement>), "Peek");

            bool found = false;
            int foundOverview = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if(instruction.Calls(m_FixedUnder) && foundOverview == 0)
                {
                    foundOverview = 1;
                }
                if (instruction.LoadsConstant(8) && foundOverview == 1)
                {
                    foundOverview = 2;
                    yield return new CodeInstruction(OpCodes.Ldc_I4_4);
                    continue;
                }
                if(instruction.Calls(m_WithFixedPadding) && foundOverview == 2)
                {
                    foundOverview = 3;
                }
                if (instruction.LoadsConstant(0D) && foundOverview == 3)
                {
                    foundOverview = 4;
                    yield return new CodeInstruction(OpCodes.Ldc_R8, 280D);
                    continue;
                }

                yield return instruction;

                if (instruction.Calls(m_AddSmallButton) && !found)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloca, l_bookmarkBounds.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_BrowseHistoryElement);
                    yield return new CodeInstruction(OpCodes.Callvirt, m_Peek);

                    yield return new CodeInstruction(OpCodes.Call, m_patchButtonIn);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldftn, m_ButtonCallback);
                    yield return new CodeInstruction(OpCodes.Newobj, m_ActionConsumable);
                    yield return new CodeInstruction(OpCodes.Ldloc, l_bookmarkBounds.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    yield return new CodeInstruction(OpCodes.Ldstr, "btn-bookmark");
                    yield return new CodeInstruction(OpCodes.Call, m_AddSmallButton);
                }
            }
        }

        public static void Postfix(GuiComposer ___detailViewGui, Stack<BrowseHistoryElement> ___browseHistory)
        {
            BrowseHistoryElement element = ___browseHistory.Peek();
            if(!(element.Page is GuiHandbookItemStackPage))
            {
                ___detailViewGui.GetButton("btn-bookmark").Enabled = false;
            } else
                ___detailViewGui.GetButton("btn-bookmark").Enabled = true;
        }

    }

    [HarmonyPatch(typeof(ModSystemHandbook), "Event_LevelFinalize")]
    class PatcherEvent_LevelFinalize
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ConstructorInfo m_Constructor = AccessTools.Constructor(typeof(GuiDialogHandbookBookmarks), new Type[] { typeof(ICoreClientAPI) });

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj)
                {
                    yield return new CodeInstruction(OpCodes.Newobj, m_Constructor);
                } else
                    yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(ItemStack), "MatchesSearchText")]
    class PatcherMatchesSearchText
    {
        public static bool Prefix(ref bool __result, ItemStack __instance, ref string searchText)
        {
            if (searchText.Length > 3 && searchText.StartsWith("@"))
            {
                int spacePos = searchText.IndexOf(' ');
                string searchDomain;
                if (spacePos > -1)
                    searchDomain = searchText.Substring(1, spacePos - 1);
                else
                    searchDomain = searchText.Substring(1);

                if (__instance.Collectible.Code.Domain.StartsWith(searchDomain, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (spacePos > -1)
                    {
                        searchText = searchText.Substring(searchDomain.Length + 2);
                        return true;
                    }
                    else
                    {
                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return false;
            }
            else if (searchText.Length > 3 && searchText.StartsWith("$"))
            {
                int spacePos = searchText.IndexOf(' ');
                string searchCode;
                if (spacePos > -1)
                    searchCode = searchText.Substring(1, spacePos - 1);
                else
                    searchCode = searchText.Substring(1);

                if (__instance.Collectible.Code.ToString().CaseInsensitiveContains(searchCode))
                {
                    if (spacePos > -1)
                    {
                        searchText = searchText.Substring(searchCode.Length + 2);
                        return true;
                    }
                    else
                    {
                        __result = true;
                        return false;
                    }

                }

                __result = false;
                return false;
            }
            else
                return true;
        }
    }

    [HarmonyPatch(typeof(GuiElementItemSlotGridBase), "FilterItemsBySearchText")]
    class PatcherFilterItemsBySearchText
    {
        public static bool Prefix(ref string text, ref Dictionary<int, string> searchCache)
        {
            if(text.StartsWith("@") || text.StartsWith("$"))
                searchCache = null;
            return true;
        }
    }

    /*
    [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), "GetHandbookInfo")]
    class PatcherGetHandbookInfo2
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(0) && !patched)
                {
                    if(ClientSettings.ExtendedDebugInfo)
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    else
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    patched = true;
                }
                else
                    yield return instruction;
            }
        }
    }
    */

    [HarmonyPatch(typeof(CollectibleObject), "GetHeldItemInfo")]
    class PatcherGetHeldItemInfo
    {
        public static void Postfix(ICoreAPI ___api, ref ItemSlot inSlot, ref StringBuilder dsc)
        {
            if (!ModConfigFile.Current.addModNameToInfo) return;
            Mod stackMod = ___api.ModLoader.GetMod(inSlot.Itemstack.Collectible.Code.Domain);
            if (stackMod != null)
            {
                dsc.Insert(0, "<font color=\"#88FF88\"><strong>Mod: " + stackMod.Info.Name + " (@" + stackMod.Info.ModID + ")</strong></font>\n");
            }
        }
    }

    [HarmonyPatch(typeof(GuiDialogHandbook), "genTabs")]
    class PatchergenTabs
    {

        static string GetProperTranslation(string langCode)
        {
            if (Lang.GetIfExists(langCode) == null)
                return Lang.Get("game:tabname-" + langCode.Substring(19));
            else
                return Lang.GetIfExists(langCode);
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo m_GetProperTranslation = AccessTools.Method(typeof(PatchergenTabs), "GetProperTranslation");

            bool found = false, next = false;
            int count = 14, skipnext = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (skipnext == 0) yield return instruction;
                else skipnext--;
                if (instruction.opcode == OpCodes.Ldloc_0) 
                    next = true;
                else
                {
                    if (next && instruction.opcode == OpCodes.Ldloc_1)
                    {
                        //Found LdLoc_0 LdLoc_1
                        found = true;
                        next = false;
                    }
                    else next = false;
                }
                if(found)
                {
                    count--;
                    if(count == 0)
                    {
                        yield return new CodeInstruction(OpCodes.Call, m_GetProperTranslation);
                        skipnext = 2;
                        found = false;
                    }
                }
            }
        }

    }

}