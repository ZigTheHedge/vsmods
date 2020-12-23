using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace survivalcats.src
{
    class GuiDialogHandbookBookmarks : GuiDialogHandbook
    {
        public GuiDialogHandbookBookmarks(ICoreClientAPI capi) : base(capi)
        {
            capi.Logger.Debug("GuiDialogHandbookBookmarks injected!");
        }

        public bool OnBookmarkClick()
        {
            Stack<BrowseHistoryElement> browseHistory = AccessTools.Field(typeof(GuiDialogHandbook), "browseHistory").GetValue(this) as Stack<BrowseHistoryElement>;
            List<GuiHandbookPage> allHandbookPages = AccessTools.Field(typeof(GuiDialogHandbook), "allHandbookPages").GetValue(this) as List<GuiHandbookPage>;
            MethodInfo m_initDetailGui = AccessTools.Method(typeof(GuiDialogHandbook), "initDetailGui");


            BrowseHistoryElement element = browseHistory.Peek();

            if (!(element.Page is GuiHandbookItemStackPage)) return true;

            bool found = false;
            if (File.Exists(PatcherinitDetailGui.bookmarksPath))
            {
                foreach (string bookmark in File.ReadLines(PatcherinitDetailGui.bookmarksPath))
                    if (bookmark.Equals(element.Page.PageCode)) found = true;
            }

            if(found)
            {
                // Remove bookmark from list
                int i = allHandbookPages.Count;
                while (i >= 0)
                {
                    i--;
                    if (allHandbookPages[i].PageCode.Equals(element.Page.PageCode))
                    {
                        allHandbookPages.RemoveAt(i);
                        break;
                    }
                }

                // Remove bookmark from File
                foreach (string bookmark in File.ReadLines(PatcherinitDetailGui.bookmarksPath))
                {
                    if (!bookmark.Equals(element.Page.PageCode))
                    {
                        File.AppendAllText(PatcherinitDetailGui.bookmarksPath + ".tmp", bookmark + "\n");
                    }
                }
                if (File.Exists(PatcherinitDetailGui.bookmarksPath + ".tmp"))
                {
                    File.Delete(PatcherinitDetailGui.bookmarksPath);
                    File.Move(PatcherinitDetailGui.bookmarksPath + ".tmp", PatcherinitDetailGui.bookmarksPath);
                }
                else
                    File.Delete(PatcherinitDetailGui.bookmarksPath);
            } else
            {
                if (!Directory.Exists(Path.GetDirectoryName(PatcherinitDetailGui.bookmarksPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(PatcherinitDetailGui.bookmarksPath));
                File.AppendAllText(PatcherinitDetailGui.bookmarksPath, element.Page.PageCode + "\n");
                allHandbookPages.Add(new GuiHandbookBookmarkedItemStackPage(capi, ((GuiHandbookItemStackPage)element.Page).Stack));
            }

            m_initDetailGui.Invoke(this, null);
            return true;
        }
    }
}
