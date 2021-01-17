using Foundation.Extensions;
using HarmonyLib;
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

        public PostService()
        {

        }
        public PostService(string title, int X, int Y, int Z)
        {
            this.title = title;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }

    class Necessaries : ModSystem
    {
        Harmony harmony = new Harmony("com.cwelth.necessaries");

        ICoreServerAPI serverApi;
        public static List<PostService> postServicesServer;
        public static List<PostService> postServicesClient = new List<PostService>();
        public static Dictionary<string, ITreeAttribute> foodParametresOnDeath = new Dictionary<string, ITreeAttribute>();

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

            api.RegisterItemClass("branchcutter", typeof(ItemBranchcutter));
            api.RegisterItemClass("sharpener", typeof(Sharpener));

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
                bool found = false;
                for (int i = 0; i < postServicesClient.Count; i++)
                {
                    if (postServicesClient[i].X == msg.X &&
                        postServicesClient[i].Y == msg.Y &&
                        postServicesClient[i].Z == msg.Z)
                    {
                        postServicesClient[i].title = msg.title;
                        found = true;
                        break;
                    }
                }
                if(!found)
                    postServicesClient.Add(msg);
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
        }

        public static void AddMailbox(string title, int X, int Y, int Z)
        {
            postServicesServer.Add(new PostService(title, X, Y, Z));
            serverChannel.BroadcastPacket(new PostService()
            {
                title = title,
                X = X,
                Y = Y,
                Z = Z,
                doRemove = false
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

        public static void EditMailbox(string title, int X, int Y, int Z)
        {
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
        }

        public void PushPostServices(IServerPlayer serverPlayer)
        {
            for (int i = 0; i < postServicesServer.Count; i++)
                serverChannel.SendPacket(new PostService()
                {
                    title = postServicesServer[i].title,
                    X = postServicesServer[i].X,
                    Y = postServicesServer[i].Y,
                    Z = postServicesServer[i].Z,
                    doRemove = false
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
