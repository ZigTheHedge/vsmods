using System;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using ProtoBuf;
using Foundation.Extensions;

namespace streamdc.src
{
    public class SDCFileConfig
    {
        public static SDCFileConfig Current { get; set; }

        public bool AddDeathWaypoint { get; set; } = true;
        public string WaypointIconVariants { get; set; } = "Variants for WaypointIcon: circle, bee, cave, home, ladder, pick, rocks, ruins, spiral, star1, star2, trader, vessel";
        public string WaypointIcon { get; set; } = "circle";
        public string WaypointColorVariants { get; set; } = "Variants for WaypointColor: https://www.99colors.net/dot-net-colors";
        public string WaypointColor { get; set; } = "crimson";
        public bool PinWaypoint { get; set; } = true;
    }
    public class SDCConfigItem
    {
        public string UUID = "";
        public string Name = "";
        public int deathCount = 0;

        public void Set(string UUID, string Name, int deathCount)
        {
            this.UUID = UUID;
            this.Name = Name;
            this.deathCount = deathCount;
        }

        public void Set(SDCConfigItem src)
        {
            this.UUID = src.UUID;
            this.Name = src.Name;
            this.deathCount = src.deathCount;
        }
    }

    public class SDCConfig
    {
        private int entries = 0;
        private SDCConfigItem[] contents = new SDCConfigItem[0];
        private string dcPath = "";

        public int FindByUUID(string UUID)
        {
            for(int i = 0; i < entries; i++)
            {
                if (contents[i].UUID.Equals(UUID)) return i;
            }
            return -1;
        }

        public void AddDeath(string UUID, string Name)
        {
            int exister = FindByUUID(UUID);
            if(exister == -1)
            {
                var backup = new SDCConfigItem[entries + 1];
                for (int i = 0; i < entries; i++)
                {
                    backup[i] = new SDCConfigItem();
                    backup[i].Set(contents[i].UUID, contents[i].Name, contents[i].deathCount);
                }
                entries++;
                contents = new SDCConfigItem[entries];
                for (int i = 0; i < entries - 1; i++)
                {
                    contents[i] = new SDCConfigItem();
                    contents[i].Set(backup[i].UUID, backup[i].Name, backup[i].deathCount);
                }
                contents[entries - 1] = new SDCConfigItem();
                contents[entries - 1].Set(UUID, Name, 1);
            } else
            {
                contents[exister].deathCount++;
            }
            SortDeaths();
            SaveToFile();
        }

        public int GetDCForPlayer(string UUID)
        {
            int exister = FindByUUID(UUID);
            if (exister == -1) return 0;
            else return contents[exister].deathCount;
        }

        public int GetRankForPlayer(string UUID)
        {
            int exister = FindByUUID(UUID);
            if (exister == -1) return 0;
            else return exister + 1;
        }

        public void LoadFromFile(string FileName)
        {
            dcPath = FileName;
            try
            {
                using (StreamReader configFile =
                        new StreamReader(FileName))
                {
                    int numEntries = Int32.Parse(configFile.ReadLine());
                    contents = null;
                    contents = new SDCConfigItem[entries = numEntries];

                    for (int i = 0; i < numEntries; i++)
                    {
                        contents[i] = new SDCConfigItem();
                        contents[i].Set(configFile.ReadLine(), configFile.ReadLine(), Int32.Parse(configFile.ReadLine()));
                    }
                }
            } catch
            {

            }
        }

        public void SaveToFile()
        {
            using (StreamWriter configFile =
                    new StreamWriter(dcPath))
            {
                configFile.WriteLine(entries);
                for (int i = 0; i < entries; i++)
                {
                    configFile.WriteLine(contents[i].UUID);
                    configFile.WriteLine(contents[i].Name);
                    configFile.WriteLine(contents[i].deathCount);
                }
            }
        }

        private void SortDeaths()
        {
            if (entries == 0) return;
            for (int mIdx = 0; mIdx < entries - 1; mIdx++)
            {
                SDCConfigItem Max = new SDCConfigItem();
                Max.Set(contents[mIdx]);
                for (int iCur = mIdx + 1; iCur < entries; iCur++)
                { 
                    if (contents[iCur].deathCount > Max.deathCount)
                    {
                        SDCConfigItem tmp = new SDCConfigItem();
                        tmp.Set(contents[iCur]);
                        contents[iCur].Set(Max);
                        contents[mIdx].Set(tmp);
                        Max.Set(contents[mIdx]);
                    }
                }
            }
        }

        public void PrintTopFive(ICoreServerAPI api, IServerPlayer player, int gId)
        {
            int maxEnt = (entries > 5) ? 5 : entries;
            for (int i = 0; i < maxEnt; i++)
            {
                if (player.PlayerName.Equals(contents[i].Name))
                    api.SendMessage(player, gId, string.Format("[ -&gt; ] {0} - {1}", contents[i].Name, contents[i].deathCount), EnumChatType.Notification);
                else
                    api.SendMessage(player, gId, string.Format("[    ] {0} - {1}", contents[i].Name, contents[i].deathCount), EnumChatType.Notification);
            }
        }

    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NetworkSendCurrentDC
    {
        public int currentDC;
        public int X;
        public int Y;
        public int Z;
    }

    public class StreamDC : ModSystem
    {
        #region Client
        IClientNetworkChannel clientChannel;
        ICoreClientAPI clientApi;

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            clientChannel =
                api.Network.RegisterChannel("streamdc")
                .RegisterMessageType(typeof(NetworkSendCurrentDC))
                .SetMessageHandler<NetworkSendCurrentDC>(OnServerMessage)
            ;
        }

        private void OnServerMessage(NetworkSendCurrentDC msg)
        {
            string dcPath = "deathcounter.txt";
            
            var timeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (SDCFileConfig.Current.AddDeathWaypoint)
            {
                clientApi.SendChatMessage(string.Format("/waypoint addati " + SDCFileConfig.Current.WaypointIcon + " ={0} ={1} ={2} " + SDCFileConfig.Current.PinWaypoint.ToString() + " " + SDCFileConfig.Current.WaypointColor + " Death: {3}", msg.X, msg.Y, msg.Z, timeString));
            }

            using (StreamWriter dcFile =
                    new StreamWriter(Path.Combine(clientApi.DataBasePath, dcPath)))
            {
                dcFile.WriteLine(msg.currentDC);
            }
        }

        #endregion

        public override void StartPre(ICoreAPI api)
        {
            SDCFileConfig.Current = api.LoadOrCreateConfig<SDCFileConfig>("StreamDCConfig.json");
        }

        #region Server
        IServerNetworkChannel serverChannel;
        ICoreServerAPI serverApi;
        SDCConfig deathData = new SDCConfig();

        public override void StartServerSide(ICoreServerAPI api)
        {
            serverApi = api;
            deathData.LoadFromFile(Path.Combine(api.DataBasePath, "streamdc.data"));

            api.Event.PlayerDeath += OnDeath;
            serverChannel =
                api.Network.RegisterChannel("streamdc")
                .RegisterMessageType(typeof(NetworkSendCurrentDC))
            ;

            api.RegisterCommand("dc", "displays the top five of deaths on this server", "", ProcessDCCommand, Privilege.chat);

        }

        private void ProcessDCCommand(IServerPlayer player, int groupId, CmdArgs args)
        {
            serverApi.SendMessage(player, groupId, string.Format("Your Death Count is: {0}, Your Rank is: {1}", deathData.GetDCForPlayer(player.PlayerUID), deathData.GetRankForPlayer(player.PlayerUID)), EnumChatType.Notification);
            serverApi.SendMessage(player, groupId, "------------------------------", EnumChatType.Notification);
            deathData.PrintTopFive(serverApi, player, groupId);
        }

        private void OnDeath(IServerPlayer byPlayer, DamageSource damageSource)
        {

            deathData.AddDeath(byPlayer.PlayerUID, byPlayer.PlayerName);
            serverChannel.SendPacket(new NetworkSendCurrentDC()
            {
                currentDC = deathData.GetDCForPlayer(byPlayer.PlayerUID),
                X = (int)byPlayer.Entity.ServerPos.X,
                Y = (int)byPlayer.Entity.ServerPos.Y,
                Z = (int)byPlayer.Entity.ServerPos.Z
            }, byPlayer);
            ProcessDCCommand(byPlayer, 0, null);
        }

        #endregion
    }
}
