using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Foundation.Extensions
{
    public static class ApiExtensions
    {
        public static TConfig LoadOrCreateConfig<TConfig>(this ICoreAPI api, string filename) where TConfig : new()
        {
            try
            {
                var loadedConfig = api.LoadModConfig<TConfig>(filename);
                if (loadedConfig != null)
                {
                    api.StoreModConfig(loadedConfig, filename);
                    return loadedConfig;
                }
            }
            catch (Exception e)
            {
                api.World.Logger.Error("{0}", $"Failed loading file ({filename}), error {e}. Will initialize new one");
            }

            var newConfig = new TConfig();
            api.StoreModConfig(newConfig, filename);
            return newConfig;
        }

        public static string GetWorldId(this ICoreAPI api) => api?.World?.Seed.ToString();

        /// <summary>
        /// These data files are per world 
        /// </summary>
        public static TData LoadOrCreateDataFile<TData>(this ICoreAPI api, string filename) where TData : new()
        {
            var path = Path.Combine(GamePaths.DataPath, "ModData", GetWorldId(api), filename);
            try
            {
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    return JsonUtil.FromString<TData>(content);
                }
            }
            catch (Exception e)
            {
                api.World.Logger.Log(EnumLogType.Error, $"Failed loading file ({path}), error {e}. Will initialize new one");
            }
            var newData = new TData();
            SaveDataFile(api, filename, newData);
            return newData;
        }

        /// <summary>
        /// These data files are per world 
        /// </summary>
        public static void SaveDataFile<TData>(this ICoreAPI api, string filename, TData data) where TData : new()
        {
            var path = Path.Combine(GamePaths.DataPath, "ModData", GetWorldId(api), filename);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var content = JsonUtil.ToString(data);
                File.WriteAllText(path, content);
            }
            catch (Exception e)
            {
                api.World.Logger.Log(EnumLogType.Error, $"Failed loading file ({path}), error {e}. Will initialize new one");
            }
        }

    }
}
