using Foundation.Extensions;
using HarmonyLib;
using necessaries.src.LeadVessel;
using necessaries.src.Mailbox;
using necessaries.src.Parcel;
using necessaries.src.SharpenerStuff;
using necessaries.src.Spikes;
using necessaries.src.Trashcan;
using ProtoBuf;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace necessaries.src
{

    [ProtoContract]
    class PostService
    {
        [ProtoMember(1)]
        public string title;
        [ProtoMember(2)]
        public int X;
        [ProtoMember(3)]
        public int Y;
        [ProtoMember(4)]
        public int Z;
        [ProtoMember(5)]
        public bool doRemove = false;
        [ProtoMember(6)]
        public bool isValid = false;
        [ProtoMember(7)]
        public bool makeValid = false;
        [ProtoMember(8)]
        public string owner;

        public PostService()
        {

        }
        public PostService(string owner, string title, int X, int Y, int Z, bool isValid)
        {
            this.owner = owner;
            this.title = title;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.isValid = isValid;
        }
    }

    public class ModConfigFile
    {
        public static ModConfigFile Current { get; set; }

        public string mailEnabledDesc { get; set; } = "Enable mailboxes, flags and parcels";
        public bool mailEnabled { get; set; } = true;
        public string mailHardmodeEnabledDesc { get; set; } = "Enable \"Hard\" mode for post service. Each mailbox should be registered with Creative-Only Post Registry block before use.";
        public bool mailHardmodeEnabled { get; set; } = false;
        public string reducedParcelVolumeDesc { get; set; } = "Should parcel volume be reduced to fit only one attachment?";
        public bool reducedParcelVolume { get; set; } = false;
        public string mailboxesAllowedDesc { get; set; } = "Maximum number of mailboxes which each player may have (0 - no limit)";
        public int mailboxesAllowed { get; set; } = 0;
        public string trashcanEnabledDesc { get; set; } = "Enable trashcan";
        public bool trashcanEnabled { get; set; } = true;
        public string rustySpikesEnabledDesc { get; set; } = "Enable Rusty Spikes";
        public bool rustySpikesEnabled { get; set; } = true;
        public string trapdoorPatchEnabledDesc { get; set; } = "Enable trapdoor patch (trapdoors act like a ladder)";
        public bool trapdoorPatchEnabled { get; set; } = true;
        public string vinesPatchEnabledDesc { get; set; } = "Enable vines patch (vines act like a ladder)";
        public bool vinesPatchEnabled { get; set; } = true;
        public string branchcutterEnabledDesc { get; set; } = "Enable Branchcutter";
        public bool branchcutterEnabled { get; set; } = true;
        public string grindstoneEnabledDesc { get; set; } = "Enable Grindstone, Sulfuric Acid and Glue";
        public bool grindstoneEnabled { get; set; } = true;
        public string graniteDiskPropertiesDesc { get; set; } = "Granite Sharpener Disk properties";
        public int graniteDiskRepairPerCycle { get; set; } = 2;
        public int graniteDiskDamagePerCycle { get; set; } = 2;
        public string basaltDiskPropertiesDesc { get; set; } = "Basalt Sharpener Disk properties";
        public int basaltDiskRepairPerCycle { get; set; } = 2;
        public int basaltDiskDamagePerCycle { get; set; } = 1;
        public string sandstoneDiskPropertiesDesc { get; set; } = "Sandstone Sharpener Disk properties";
        public int sandstoneDiskRepairPerCycle { get; set; } = 3;
        public int sandstoneDiskDamagePerCycle { get; set; } = 1;
        public string obsidianDiskPropertiesDesc { get; set; } = "Obsidian Sharpener Disk properties";
        public int obsidianDiskRepairPerCycle { get; set; } = 5;
        public int obsidianDiskDamagePerCycle { get; set; } = 1;
        public string diamondDiskPropertiesDesc { get; set; } = "Obsidian-Diamond Sharpener Disk properties";
        public int diamondDiskRepairPerCycle { get; set; } = 15;
        public int diamondDiskDamagePerCycle { get; set; } = 1;

    }

    class Necessaries : ModSystem
    {
        Harmony harmony = new Harmony("com.cwelth.necessaries");

        ICoreServerAPI serverApi;
        public static List<PostService> postServicesServer;
        public static List<PostService> postServicesClient = new List<PostService>();

        public override void StartPre(ICoreAPI api)
        {
            ModConfigFile.Current = api.LoadOrCreateConfig<ModConfigFile>("NecessariesConfig.json");

            api.World.Config.SetBool("mailEnabled", ModConfigFile.Current.mailEnabled);
            api.World.Config.SetBool("mailHardmodeEnabled", ModConfigFile.Current.mailHardmodeEnabled);
            api.World.Config.SetBool("trashcanEnabled", ModConfigFile.Current.trashcanEnabled);
            api.World.Config.SetBool("rustySpikesEnabled", ModConfigFile.Current.rustySpikesEnabled);
            api.World.Config.SetBool("trapdoorPatchEnabled", ModConfigFile.Current.trapdoorPatchEnabled);
            api.World.Config.SetBool("vinesPatchEnabled", ModConfigFile.Current.vinesPatchEnabled);
            api.World.Config.SetBool("branchcutterEnabled", ModConfigFile.Current.branchcutterEnabled);
            api.World.Config.SetBool("grindstoneEnabled", ModConfigFile.Current.grindstoneEnabled);


            base.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("mailbox", typeof(MailBox));
            api.RegisterBlockEntityClass("bemailbox", typeof(BEMailBox));

            api.RegisterBlockClass("parcel", typeof(ParcelBlock));
            api.RegisterBlockEntityClass("beparcel", typeof(BEParcel));

            api.RegisterBlockClass("trashcan", typeof(TrashcanBlock));
            api.RegisterBlockEntityClass("betrashcan", typeof(BETrashcan));

            api.RegisterBlockClass("rustyspikes", typeof(RustySpikes));

            api.RegisterBlockClass("grindstone", typeof(Grindstone));
            api.RegisterBlockEntityClass("begrindstone", typeof(BEGrindstone));

            api.RegisterBlockClass("leadvessel", typeof(Leadvessel));
            api.RegisterBlockEntityClass("beleadvessel", typeof(BELeadvessel));

            api.RegisterItemClass("branchcutter", typeof(ItemBranchcutter));
            api.RegisterItemClass("sharpener", typeof(Sharpener));

            api.RegisterItemClass("regscroll", typeof(RegScroll));

            api.RegisterBlockClass("postregistry", typeof(PostRegistry));

        }

        public static IServerNetworkChannel serverChannel;
        IClientNetworkChannel clientChannel;

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientChannel =
                api.Network.RegisterChannel("necessaries")
                .RegisterMessageType(typeof(PostService))
                .SetMessageHandler<PostService>(OnServerMessage)
            ;
            //Harmony.DEBUG = true;
            harmony.PatchAll();
        }

        private void OnServerMessage(PostService msg)
        {
            if (!msg.doRemove)
            {
                // Edit?
                int found = -1;
                for (int i = 0; i < postServicesClient.Count; i++)
                {
                    if (postServicesClient[i].X == msg.X &&
                        postServicesClient[i].Y == msg.Y &&
                        postServicesClient[i].Z == msg.Z)
                    {
                        found = i;
                        break;
                    }
                }
                if (found == -1)
                {
                    if (ModConfigFile.Current.mailHardmodeEnabled)
                    {
                        if (!msg.makeValid) postServicesClient.Add(msg);
                        else return;
                    } else
                    {
                        postServicesClient.Add(msg);
                    }
                }
                else
                {
                    if (!msg.makeValid)
                        postServicesClient[found].title = msg.title;
                    else
                    {
                        postServicesClient[found].isValid = true;
                    }
                }
            } else
            {
                for (int i = 0; i < postServicesClient.Count; i++)
                {
                    if (postServicesClient[i].X == msg.X &&
                        postServicesClient[i].Y == msg.Y &&
                        postServicesClient[i].Z == msg.Z)
                    {
                        postServicesClient.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverApi = api;

            serverChannel =
            api.Network.RegisterChannel("necessaries")
                .RegisterMessageType(typeof(PostService))
            ;

            api.Event.SaveGameLoaded += LoadPostServices;
            api.Event.GameWorldSave += SavePostServices;
            api.Event.PlayerJoin += PushPostServices;

            if (ModConfigFile.Current.mailEnabled)
            {
                api.ChatCommands
                    .GetOrCreate("listmailboxes")
                    .WithDescription("Prints a list of all registered mailboxes on a server")
                    .RequiresPrivilege("readlists")
                    .HandleWith(ListMailboxes);
            }
        }

        private TextCommandResult ListMailboxes(TextCommandCallingArgs args)
        {
            var caller = args.Caller;
            var player = caller.Player as IServerPlayer;
            var groupId = caller.FromChatGroupId;
            serverApi.SendMessage(player, groupId, "List of all registered mailboxes:", EnumChatType.Notification);
            if (ModConfigFile.Current.mailHardmodeEnabled) serverApi.SendMessage(player, groupId, "Mail HARD MODE is enabled!", EnumChatType.Notification);
            serverApi.SendMessage(player, groupId, "------------------------------", EnumChatType.Notification);
            int centerX = player.WorldData.EntityPlayer.World.BlockAccessor.MapSizeX / 2;
            int centerZ = player.WorldData.EntityPlayer.World.BlockAccessor.MapSizeZ / 2;
            for (int i = 0; i < postServicesServer.Count; i++)
            {
                string mailbox = (i + 1) + ". [" + postServicesServer[i].title + "] at: " + (postServicesServer[i].X - centerX) + ", " + postServicesServer[i].Y + ", " + (postServicesServer[i].Z - centerZ);
                if (ModConfigFile.Current.mailHardmodeEnabled)
                    if (postServicesServer[i].isValid) mailbox += " [registered]";
                    else mailbox += "[UNregistered]";
                mailbox += " Owned by: " + postServicesServer[i].owner;
                serverApi.SendMessage(player, groupId, mailbox, EnumChatType.Notification);
            }
            if(postServicesServer.Count == 0)
                serverApi.SendMessage(player, groupId, "There are none.", EnumChatType.Notification);
            return TextCommandResult.Success("All mailboxes listed.");
        }

        public static int CountMailboxes(IPlayer player, ICoreAPI api)
        {
            int count = 0;
            if (api.Side == EnumAppSide.Client)
            {
                for (int i = 0; i < postServicesClient.Count; i++)
                {
                    if (postServicesClient[i].owner == player.PlayerName) count++;
                }
            }
            else
            {
                for (int i = 0; i < postServicesServer.Count; i++)
                {
                    if (postServicesServer[i].owner == player.PlayerName) count++;
                }
            }
            return count;
        }

        public static bool AbleToPlaceMailbox(IPlayer player, ICoreAPI api)
        {
            int count = CountMailboxes(player, api);
            if (count >= ModConfigFile.Current.mailboxesAllowed && ModConfigFile.Current.mailboxesAllowed != 0) return false;
            return true;
        }

        public static void AddMailbox(IServerPlayer player, string title, int X, int Y, int Z)
        {
            bool validate = !ModConfigFile.Current.mailHardmodeEnabled;
            postServicesServer.Add(new PostService(player.PlayerName, title, X, Y, Z, validate));
            serverChannel.BroadcastPacket(new PostService()
            {
                owner = player.PlayerName,
                title = title,
                X = X,
                Y = Y,
                Z = Z,
                doRemove = false,
                isValid = validate
            });
        }

        public static void RemoveMailbox(int X, int Y, int Z)
        {
            for(int i = 0; i < postServicesServer.Count; i++)
            {
                if(postServicesServer[i].X == X &&
                    postServicesServer[i].Y == Y &&
                    postServicesServer[i].Z == Z)
                {
                    postServicesServer.RemoveAt(i);
                    break;
                }
            }
            serverChannel.BroadcastPacket(new PostService()
            {
                title = "",
                X = X,
                Y = Y,
                Z = Z,
                doRemove = true
            });
        }

        public static void ValidateMailbox(int X, int Y, int Z)
        {
            for (int i = 0; i < postServicesServer.Count; i++)
            {
                if (postServicesServer[i].X == X &&
                    postServicesServer[i].Y == Y &&
                    postServicesServer[i].Z == Z)
                {
                    postServicesServer[i].isValid = true;
                    break;
                }
            }
            serverChannel.BroadcastPacket(new PostService()
            {
                title = "",
                X = X,
                Y = Y,
                Z = Z,
                makeValid = true
            });
        }

        public static bool EditMailbox(string title, int X, int Y, int Z)
        {
            for (int i = 0; i < postServicesServer.Count; i++)
            {
                if(postServicesServer[i].title.Equals(title, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    if (postServicesServer[i].X == X &&
                    postServicesServer[i].Y == Y &&
                    postServicesServer[i].Z == Z) continue;

                    return false;
                }
            }

            for (int i = 0; i < postServicesServer.Count; i++)
            {
                if (postServicesServer[i].X == X &&
                    postServicesServer[i].Y == Y &&
                    postServicesServer[i].Z == Z)
                {
                    postServicesServer[i].title = title;
                    break;
                }
            }
            serverChannel.BroadcastPacket(new PostService()
            {
                title = title,
                X = X,
                Y = Y,
                Z = Z,
                doRemove = false
            });
            return true;
        }

        public void PushPostServices(IServerPlayer serverPlayer)
        {
            for (int i = 0; i < postServicesServer.Count; i++)
                serverChannel.SendPacket(new PostService()
                {
                    owner = postServicesServer[i].owner,
                    title = postServicesServer[i].title,
                    X = postServicesServer[i].X,
                    Y = postServicesServer[i].Y,
                    Z = postServicesServer[i].Z,
                    isValid = postServicesServer[i].isValid,
                    doRemove = false,
                    makeValid = false
                }, serverPlayer);
        }
        public void SavePostServices()
        {
            serverApi.WorldManager.SaveGame.StoreData("postServices", SerializerUtil.Serialize(postServicesServer));
        }

        public void LoadPostServices()
        {
            byte[] data = serverApi.WorldManager.SaveGame.GetData("postServices");

            postServicesServer = data == null ? new List<PostService>() : SerializerUtil.Deserialize<List<PostService>>(data);
        }

        public override void Dispose()
        {
            base.Dispose();
            serverChannel = null;
            harmony.UnpatchAll("com.cwelth.necessaries");
        }
    }
}
