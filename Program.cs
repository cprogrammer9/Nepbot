using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using NepBot.Core.Commands;
using System.Collections.Generic;
using NepBot.Data;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Passive.Services.DatabaseService;
using System.Timers;
using System.Runtime.InteropServices;
using System.Threading;
using NepBot.Resources.Database;
using NepBot.Resources.Extensions;
using NepBot.Core.Data;
using System.Text;
using NepBot.Resources.Code_Implements;
using NepBot.Core;
using System.Linq;
using Discord.Rest;

namespace NepBot
{
    public class Program
    {
        public static CharacterCards characterCards = new CharacterCards();
        public static ImageData imageData = new ImageData();
        private CommandService _commands;
        private DiscordSocketClient _client;
        public static SocketGuild currentGuild;
        private static List<UserData> expPoints = new List<UserData>();
        public static MiscBotData miscBotData = new MiscBotData();
        public static List<CreateEntity> _playerData = new List<CreateEntity>();
        public static List<GuildData> AllGuildData = new List<GuildData>();
        public static List<ulong> _playersIDs = new List<ulong>();
        public static bool continuously = false;
        public static ulong[] excludePeople = new ulong[] { 138085604280893440, 698768363232428104, 394458969932038144UL, 513059797201846273UL, 289913686867443713UL, 362023139103735826UL, 130085308036284416UL, 508129645854588928UL, 338277916360048640UL, 401728599821910017UL, 433127658230906890UL, 109848271505080320UL };
        private DateTime loserGuy = DateTime.Now;
        public static ulong includeMessage = 0UL;
        private readonly string imgurls = Program.DataPath("imgurls", "txt");
        public static SocketCommandContext _contextDelete;
        public const ulong myGuildId = 472551343148761090;
        public static SocketGuild myGuildSocket;
        //public static DateTime dailyPudding;

        /// <summary>
        /// Returns the list of participants in the RPG game. RPG game is unfinished.
        /// </summary>  
        public static List<CreateEntity> PlayerData
        {
            get { return _playerData; }
            set { _playerData = value; }
        }

        public static List<UserData> ExpPoints => expPoints;
        private static string _dataPath = string.Empty;


        /// <summary>
        /// Make sure you include the file extension if it has one (JPG, EXE, etc). Do not include the.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string DataPath(string fileName, string fileExtension = null)
        {
            return Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\NepBot.exe", string.Concat(@"Data\", fileName, (fileExtension == null) ? "" : ".", fileExtension));
        }


        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
        .AddSingleton(_client)
        .AddSingleton(_commands)
        .AddSingleton<DatabaseService>()
        //.AddSingleton<CommandHandler>()
        //.AddSingleton(expPoints)
        .BuildServiceProvider();

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        static bool exitSystem = false;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private bool Handler(CtrlType sig)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            //do your cleanup here
            SerializeUserData(null, null);
            Thread.Sleep(5000); //simulate some cleanup delay

            Console.WriteLine("Cleanup complete");

            //allow main to run off
            exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }

        private void ShutDownData()
        {
            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
        }

        #endregion

        #region Save Data
        private System.Timers.Timer saveTimer;

        private void SaveTimer()
        {
            saveTimer = new System.Timers.Timer();
            saveTimer.Interval = 300000;

            saveTimer.Elapsed += SerializeUserData;
            saveTimer.AutoReset = true;
            saveTimer.Enabled = true;
        }

        private void LoadData()
        {
            string fileName = "User Data List";
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter n = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            string pat = DataPath(fileName);
            using (FileStream stream = new FileStream(pat, FileMode.Open))
            {
                //n.Serialize(stream, savedObj); expPoints
                //n.Serialize(stream, rpgSaveObj); rpSaveData
                //n.Serialize(stream, AllGuildData); guildSaveData
                //n.Serialize(stream, miscBotData); miscBotData
                try
                {
                    expPoints = (List<UserData>)n.Deserialize(stream);
                    foreach (UserData ep in expPoints)
                    {
                        ep.SetValues();
                    }
                    //List<RPGSaveData> rpSaveData = (List<RPGSaveData>)n.Deserialize(stream);                        
                    List<GuildData> guildSaveData = (List<GuildData>)n.Deserialize(stream);
                    miscBotData = (MiscBotData)n.Deserialize(stream);

                    //                    if (rpSaveData == null)
                    //{
                    //  Console.WriteLine("RP Save Data is null");
                    //}
                    //RepopulatePlayerDataList(rpSaveData);
                    AllGuildData.Clear();
                    for (int i = 0; i < guildSaveData.Count; i++)
                    {
                        AllGuildData.Add(guildSaveData[i]);
                    }
                }

                catch (Exception m)
                {
                    ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, "from LoadData in Program.cs");
                    return;
                }
            }
        }

        private void SerializeUserData(Object source, System.Timers.ElapsedEventArgs e)
        {
            string fileName = "User Data List";
            //string rpgFilename = "RPG Data";
            object savedObj = expPoints;
            List<RPGSaveData> rpgObj = new List<RPGSaveData>();
            foreach (CreateEntity g in _playerData)
            {
                rpgObj.Add(g.SaveControl());
            }
            object rpgSaveObj = rpgObj;
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter n = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            string pat = DataPath(fileName);
            using (FileStream stream = new FileStream(pat, FileMode.Create))
            {
                try
                {
                    n.Serialize(stream, savedObj);
                    //n.Serialize(stream, rpgSaveObj);
                    n.Serialize(stream, AllGuildData);
                    n.Serialize(stream, miscBotData);
                }
                catch (Exception nm)
                {
                    ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, nm, "Error saving data");
                    return;
                }
                //Console.WriteLine($"{DateTime.Now} <> Successful Data Save");
            }
        }

        /// <summary>
        /// repopulates the RPG save data list
        /// </summary>
        /// <param name="rpg"></param>
        public void RepopulatePlayerDataList(List<RPGSaveData> rpg)
        {
            try
            {
                for (int i = 0; i < rpg.Count; i++)
                {
                    _playerData.Add(new PlayerData(rpg[i].playerID));
                    _playerData[i].LoadData(rpg[i]);
                }
            }
            catch (Exception m)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, "Problem populating RPG list or loading data");
                throw;
            }
        }

        private bool SaveExists()
        {
            string fileName = "User Data List";
            string pat = DataPath(fileName);
            try
            {
                using (FileStream stream = new FileStream(pat, FileMode.Open))
                {

                }

            }
            catch (FileNotFoundException e)
            {
                return false;
            }
            return true;
        }

        private async Task AddAllGuilds()
        {
            StringBuilder tester = new StringBuilder();
            foreach (var g in _client.Guilds)
            {
                AllGuildData.Add(new GuildData(g.Id));
                tester.Append($"{g.Name}\n");
            }
        }
        #endregion

        public async Task Client_UserLeft(SocketGuildUser sgu)
        {
            await this._client.GetGuild(sgu.Guild.Id).GetTextChannel(643769622453288970).SendMessageAsync(sgu.Username + " has left the server.");
        }

        public async Task Client_UserJoined(SocketGuildUser sgu)
        {
            var sent = await this._client.GetGuild(sgu.Guild.Id).GetTextChannel(643769622453288970).SendMessageAsync($"Welcome to the server <@{sgu.Id}> ! Roleplay channels are hidden by default, we are both a roleplay and conversational server. If you want to roleplay, react with <:perpell:571875093870018576>, if you don't want to roleplay, reaction with <:nepswoon:591333460820361244>. Check out <#472552180235370517> for details on rules. A few things: We are laid back here. I don't care too much if you do things like talk in image channels or post images in other channels (not post a lot in a row). ERP NOT allowed. Don't ask about it and even making it obvious you are taking an ERP to DMs is not allowed. Keep it all private no one wants to know. Other than that, check the rules out for the other details and have fun!");
            await sent.AddReactionAsync(_client.GetGuild(472551343148761090).GetEmoteAsync(571875093870018576).Result);
            await sent.AddReactionAsync(_client.GetGuild(472551343148761090).GetEmoteAsync(591333460820361244).Result);
            //roleSet.Add(new RoleSet(sgu.Id, sent.Id, 571875093870018576, 591333460820361244));
        }

        private List<RoleSet> roleSet = new List<RoleSet>() { };

        struct RoleSet
        {
            public readonly ulong userID;
            public readonly ulong msgID;
            readonly ulong[] reactionID;

            public RoleSet(ulong userID, ulong msgID, params ulong[] reactionID)
            {
                this.userID = userID;
                this.msgID = msgID;
                this.reactionID = reactionID;
            }

            public ulong GetEmojis(int pos)
            {
                return reactionID[pos];
            }
        }

        public async Task Client_Reaction(Cacheable<IUserMessage, ulong> cm, ISocketMessageChannel ism, SocketReaction sr)
        {
            //await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync(sr.Emote.Name);
            try
            {
                if (ism.Id != 643769622453288970)
                {
                    //await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync("Returned due to wrong channel");
                    return;
                }

                //int getPOS = 0;

                //foreach (RoleSet rs in roleSet)
                //{
                //  if (rs.userID == sr.UserId)
                //    break;
                //getPOS++;
                //}

                if (!ism.GetMessageAsync(sr.MessageId).Result.Content.Contains("A few things: W")) //(roleSet[getPOS].msgID != sr.MessageId)
                {
                    //await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync("Returned due to wrong message");
                    return;
                }
                if (sr.Emote.Name.Contains("perpell"))//assign roleplay(roleSet[getPOS].GetEmojis(0) == 571875093870018576)
                {
                    //await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync("Supposed to assign perpell role");
                    await _client.GetGuild(472551343148761090).GetUser(sr.UserId).RemoveRoleAsync(_client.GetGuild(472551343148761090).GetRole(556722030393950219));
                    await _client.GetGuild(472551343148761090).GetUser(sr.UserId).AddRoleAsync(_client.GetGuild(472551343148761090).GetRole(472552681374744578));
                    return;
                }
                if (sr.Emote.Name.Contains("nepswoon"))// don't assign roleplay(roleSet[getPOS].GetEmojis(0) == 591333460820361244)
                {
                    //await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync("Supposed to assign nepswoon role");
                    await _client.GetGuild(472551343148761090).GetUser(sr.UserId).RemoveRoleAsync(_client.GetGuild(472551343148761090).GetRole(472552681374744578));
                    await _client.GetGuild(472551343148761090).GetUser(sr.UserId).AddRoleAsync(_client.GetGuild(472551343148761090).GetRole(556722030393950219));
                    return;
                }
                //  await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync("No if statement used");
            }
            catch (Exception m)
            {
                await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync($">>> {m.Message}\n{m.TargetSite}\n{m.Source}\n{m.InnerException}\n{m.StackTrace}\n{m.HResult}\n{m.Data}\n{m.HelpLink}");
            }
            //await _client.GetGuild(472551343148761090).GetTextChannel(643769622453288970).SendMessageAsync("After the try catch");
        }

        private async Task MainAsync()
        {
            try
            {
                this._client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Critical
                });
                this._commands = new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Critical
                });
                _client.MessageReceived += Client_MessageReceived;
                _client.UserLeft += Client_UserLeft;
                _client.UserJoined += Client_UserJoined;
                _client.ReactionAdded += Client_Reaction;
                bool flag = this.SaveExists();
                if (flag)
                {
                    this.LoadData();
                }
                this.SaveTimer();
                await this._commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
                this._client.Ready += this.Client_Ready;
                this._client.Log += this.Client_Log;
                string rep = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Token.txt");
                string Token = File.ReadAllText(rep);
                this.ShutDownData();
                await this._client.LoginAsync(TokenType.Bot, Token, true);
                await this._client.StartAsync();
                await Task.Delay(-1);
                myGuildSocket = _client.GetGuild(myGuildId);
                //  temporaryTimer = new System.Timers.Timer(500);
                //temporaryTimer.Elapsed += DeleteFromArray;
                //temporaryTimer.Enabled = true;                
                //temporaryTimer.Start();
            }
            catch (Exception i)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, i, "");
            }
        }

        public static UserStuff myGuild = new UserStuff();

        private async Task Client_Log(LogMessage Message)
        {
            Console.WriteLine($"{DateTime.Now} at {Message.Source}] {Message.Message}");
        }

        private async Task Client_Ready()
        {
            CreateEntity.AddEnemies();
            DataFiles.LoadMemeDirectory();
            //await ImgurImplement.ImgurDataSetup();
            await _client.SetGameAsync("Eating Pudding", "https://Google.com");
            await AddAllGuilds();
            //await this._client.GetGuild(472551343148761090).GetTextChannel(559866304643727367).SendMessageAsync("<:perpell:571875093870018576>, if you don't want to roleplay, reaction with <:nepswoon:591333460820361244>");
            //<:perpell:571875093870018576>, if you don't want to roleplay, reaction with <:nepswoon:591333460820361244>
            //myGuild.SetData(_client.GetGuild(472551343148761090), 643769622453288970);
            myGuild.rpParticipants.Add(new RPParticipants(new List<ulong>() { 187273824176177152, 152548979085672448, 400678973639753728, 308540989952360448, 401728599821910017 }, 472551695432286240));
            //await WeeklyLoversHack(new TimeSpan(0, 0, 5), new CancellationToken(false));
            //myGuild.rpParticipants.Add(new RPParticipants(new List<ulong>() { 289913686867443713, 423329319503396875, 308540989952360448, 401728599821910017 }, 472551715766403072));
            //await _client.GetGuild(472551343148761090).GetTextChannel(474802733438861312).SendMessageAsync("Cute Totori has left the server!");

        }

        private bool IsOnList()
        {
            if (expPoints.Count == 0)
                return false;
            bool t = false;
            foreach (UserData x in expPoints)
            {
                if (x.UserID == Context.User.Id)
                {
                    t = true;
                    break;
                }
            }
            return t;
        }

        public static bool IsOnPlayerData(SocketCommandContext scc)
        {
            if (expPoints.Count == 0)
                return false;
            bool t = false;
            foreach (PlayerData x in _playerData)
            {
                if (x.GetPlayerID == scc.User.Id)
                {
                    t = true;
                    break;
                }
            }
            return t;
        }


        ulong snapshot = 0;
        private bool IsParagraph(ulong ID)
        {
            List<ulong> Categories = new List<ulong>();
            Categories.Add(472551695432286240);
            Categories.Add(472551726910668800);
            Categories.Add(472551715766403072);
            Categories.Add(472551737761202177);
            Categories.Add(472551745994620949);
            Categories.Add(558458080547962883);
            bool np = false;

            foreach (ulong ul in Categories)
            {
                if (ul == ID)
                {
                    np = true;
                    break;
                }
            }

            if (np && Context.User.Id != snapshot)
            {
                var pp = Context.Message.Content.Split(' ');
                if (pp.Length >= 249)
                {
                    RPParticipants rpp = null;
                    StringBuilder sb = new StringBuilder();
                    foreach (RPParticipants g in myGuild.rpParticipants)
                    {
                        sb.Append(g.rpChannel + " " + Context.Channel.Id);
                        if (Context.Channel.Id == g.rpChannel)
                        {
                            rpp = g;
                            break;
                        }
                    }
                    SocketUser sgu = null;
                    if (rpp != null)
                        for (int i = 0; i < rpp.sgu.Count; i++)
                        {
                            if (Context.User.Id == rpp.sgu[i] && i + 1 < rpp.sgu.Count)
                            {
                                sgu = ExtensionMethods.GetSocketUser(rpp.sgu[i + 1].ToString(), Context, false);
                                break;
                            }
                            else if (Context.User.Id == rpp.sgu[i] && i + 1 >= rpp.sgu.Count)
                            {
                                sgu = ExtensionMethods.GetSocketUser(rpp.sgu[0].ToString(), Context, false);
                                break;
                            }
                        }
                    if (sgu != null)
                    {
                        SendMessage($"<@{sgu.Id}> your turn to reply in <#{rpp.rpChannel}>", 622988672739966996, Context.Guild);
                    }
                    snapshot = Context.User.Id;
                }
            }

            return np;
        }

        public bool IsCasual(ulong ID)
        {
            List<ulong> Categories = new List<ulong>();
            Categories.Add(472551756656672798);
            Categories.Add(472551762990071828);
            Categories.Add(472551769336184842);
            Categories.Add(472551775178850310);
            Categories.Add(472551782548242442);
            Categories.Add(558458110168137748);
            bool np = false;
            foreach (ulong ul in Categories)
            {
                if (ul == ID)
                {
                    np = true;
                    break;
                }
            }

            return np;
        }

        public UserData ReturnPositionInList(ulong authorID)
        {
            int x = -1;
            for (int i = 0; i < expPoints.Count; i++)
            {
                if (authorID == expPoints[i].UserID)
                {
                    x = i;
                    break;
                }
            }
            return (x != -1) ? expPoints[x] : null;
        }

        public async Task AddExp(int length)
        {
            //ExtensionMethods.WriteToLog(ExtensionMethods.LogType.CustomMessage, null, v.ToString());            
            UserData user = ReturnPositionInList(Context.User.Id);
            //ExtensionMethods.WriteToLog(ExtensionMethods.LogType.Debug, null, Message.Author.Id.ToString());            
            if (user == null)
            {
                expPoints.Add(new UserData(Context.User.Id));
                user = expPoints[expPoints.Count - 1];
            }
            if (user.sessionOn)
                tData = user;
            else
                tData = null;

            user.ReturnLastPosted = DateTime.Now;
            if (!IsOnPlayerData(Context))
            {
                _playerData.Add(new PlayerData(Context.User.Id));
            }
            if (IsParagraph(Context.Channel.Id))
            {
                int snapshop = user.ParaLevel;
                user.CurrentParaExp += 500;
                user.TotalPosts++;
                user.Pudding += 100 + (ulong)user.ParaLevel * 10;
                if (user.ParaLevel != snapshop)
                {
                    user.Pudding += 2500;
                    await GuildCommands.MessageChannel(Context, Context.User, "Paragraph Roleplay Level", user.ParaLevel.ToString());
                }
                int amt = Context.Message.Content.Split(' ').Length;
                //$"<@{sgu.Id}> your turn to reply in <#{rpp.rpChannel}>"
                await GuildCommands.MessageChannel(Context, Context.User, "", "", $"{ExtensionMethods.NameGetter(Context.User, Context.Guild)}'s word count is {amt} in <#{Context.Channel.Id}>", 632720832455639089);
                return;
            }
            else if (IsCasual(Context.Channel.Id))
            {
                int snapshop = user.CasualLevel;
                user.CurrentCasualExp += 250;
                user.TotalPosts++;
                user.Pudding += 25 + (ulong)user.CasualLevel / 2;
                if (user.CasualLevel != snapshop)
                {
                    user.Pudding += 2500;
                    await GuildCommands.MessageChannel(Context, Context.User, "Casual Roleplay Level", user.CasualLevel.ToString());
                }
                return;
            }
            if (Context.Channel.Id != 559866304643727367)
            {
                int snapshop = user.NonLevel;
                user.CurrentNonExp += 25;
                user.TotalPosts++;
                user.Pudding += 5 + (ulong)user.NonLevel / 5;
                if (user.NonLevel != snapshop)
                {
                    user.Pudding += 2500;
                    await GuildCommands.MessageChannel(Context, Context.User, "Non-roleplay Level", user.NonLevel.ToString());
                }
            }
        }

        public static async Task SendMessage(string message, ulong channelID, SocketGuild sg)
        {
            try
            {
                if (sg == null)
                    sg = myGuildSocket;
                await sg.GetTextChannel(channelID).SendMessageAsync(message);
            }
            catch (Exception n)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, n, "From sendmessage");
            }
        }

        public static async Task MessageUser(string message, ulong userID, SocketGuild sg)
        {
            //await sg.GetTextChannel(559866304643727367).SendMessageAsync(message); sends to bot spam channel
            await sg.GetUser(userID).SendMessageAsync(message);
        }

        private static SocketUserMessage Message = null;
        SocketCommandContext Context = null;
        static string word = string.Empty;
        private UserData tData = null;

        private bool ThankYou(string msg)
        {
            string[] ty = new string[] { "thanks", "thank you", "thankyou", "thanks a lot", "thanks alot", "thank", "ty" };
            string[] nb = new string[] { "nep bot", "nepbot", "nep" };
            var msgList = msg.ToLower().Split(' ').ToList();
            for (int i = 0; i < msgList.Count; i++)
            {
                foreach (string thank in ty)
                {
                    if (msgList[i] == thank)
                    {
                        i++;
                        if (i >= msgList.Count)
                            return false;
                        foreach (string x in nb)
                        {
                            if (msgList[i] == x)
                                return true;
                        }
                    }

                }
            }
            return false;
        }

        private bool HasMessagePrefix(string msg, ref int argPOS)
        {
            if (msg[0] != '!')
                return false;
            string[] prefixes = new string[] { "!nep " };
            if (msg.Length <= prefixes[0].Length)
                return false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < prefixes[0].Length; i++)
            {
                argPOS++;
                sb.Append(msg[i]);
            }

            if (sb.ToString() != prefixes[0])
            {
                sb = new StringBuilder();
                argPOS = 1;
                msg = msg.Remove(4, 1);
                for (int i = 0; i < prefixes[0].Length; i++)
                {
                    argPOS++;
                    sb.Append(msg[i]);
                }
            }

            return sb.ToString() == prefixes[0];
        }

        private int FindInstances(string[] searchTerm, string text)
        {
            string[] source = text.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int totalCount = 0;

            for (int i = 0; i < searchTerm.Length; i++)
            {
                var matchQuery = from word in source
                                 where word.ToLowerInvariant() == searchTerm[i].ToLowerInvariant()
                                 select word;
                totalCount += matchQuery.Count();
            }
            return totalCount;
        }

        public async Task WeeklyLoversHack(TimeSpan interval, CancellationToken cancellationToken)
        {
            int counter = 0;
            foreach (var f in AllGuildData)
            {
                if (f.GuildId == 472551343148761090)
                {
                    break;
                }
                counter++;
            }
            try
            {
                while (true)
                {
                    if (AllGuildData[counter].WeeklyLoversReset)
                    {
                        AllGuildData[counter].weeklyLovers.AddDays(7);
                        await PickLovers();
                        break;
                    }
                }
            }
            catch (Exception m)
            {
                await Context.Client.GetGuild(472551343148761090).GetTextChannel(559866304643727367).SendMessageAsync($">>> {m.Message}\n{m.TargetSite}\n{m.Source}\n{m.InnerException}\n{m.StackTrace}\n{m.HResult}\n{m.Data}\n{m.HelpLink}");
            }
        }

        public async Task PickLovers()
        {
            SocketRole sr = null;
            try
            {
                var g = Context.Guild.Roles.ToList();
                var users = Context.Guild.Users.ToList();
                foreach (var x in users)
                {
                    foreach (var q in x.Roles)
                        if (q.Id == 660938379499929601)
                        {
                            await x.RemoveRoleAsync(q);
                        }
                }
                int snapshot = -10;
                List<ulong> chosenMembers = new List<ulong>();
                foreach (var x in users)
                {
                    bool add = true;
                    foreach (var s in x.Roles)
                        if (s.Id == 556268989219602442 || s.Id == 557012017773543445)
                        {
                            add = false;
                            break;
                        }
                    if (add)
                        chosenMembers.Add(x.Id);
                }
                for (int i = 0; i < 2; i++)
                {
                    int chooser = UtilityClass.ReturnRandom(0, chosenMembers.Count);
                    string isChos = ChooseLover(chosenMembers[chooser], chooser == snapshot).Result;
                    if (isChos != "ready")
                    {
                        i--;
                        continue;
                    }

                    snapshot = chooser;
                    await Context.Guild.GetUser(chosenMembers[chooser]).AddRoleAsync(Context.Guild.GetRole(660938379499929601));
                }

            }
            catch (Exception m)
            {
                await Context.Channel.SendMessageAsync($">>> {m.Message}\n{m.TargetSite}\n{m.Source}\n{m.InnerException}\n{m.StackTrace}\n{m.HResult}\n{m.Data}\n{m.HelpLink}");
            }
        }

        private async Task<string> ChooseLover(ulong chosenPerson, bool sn)
        {
            try
            {
                if (sn)
                    return "snapshop = chooser";
                if (Context.Guild.GetUser(chosenPerson).IsBot)
                    return "Bot chosen";
                foreach (ulong ul in excludePeople)
                    if (chosenPerson == ul)
                        return "Excluded people chosen";
            }
            catch (Exception m)
            {
                await Context.Channel.SendMessageAsync($">>> {m.Message}\n{m.TargetSite}\n{m.Source}\n{m.InnerException}\n{m.StackTrace}\n{m.HResult}\n{m.Data}\n{m.HelpLink}");
                return "catch";
            }
            return "ready";
        }

        public async Task EmojiUse([Remainder] string Input = null)
        {
            bool flag = Input.Contains("help");
            if (flag)
            {
                await Context.Channel.SendMessageAsync("Format is !nep em (name)|(custom message)\nDo not add spaces for any of the image names. None of them have spaces.", false, null, null);
            }
            else
            {
                int num = 0;
                try
                {
                    string youCan = string.Empty;
                    if (Program.includeMessage % 5UL == 0UL)
                    {
                        youCan = "You can do this too! Type !nep em (image name)|(custom message if you want to add one)";
                    }
                    Program.includeMessage += 1UL;
                    Input = Input.ToLower();
                    string[] g = File.ReadAllText(this.imgurls).Split(new char[]
                    {
                        '|'
                    });
                    string[] content = Input.Split(new char[]
                    {
                        '|'
                    });
                    string msg = string.Empty;
                    if (content.Length > 1)
                    {
                        msg = content[1];
                    }
                    string user = ExtensionMethods.GetUsersName(Context.User.Id.ToString(), Context, true);
                    for (int i = 1; i < g.Length; i += 3)
                    {
                        if (content[0] == g[i])
                        {
                            await Context.Channel.SendMessageAsync(string.Concat(new string[]
                            {
                                msg,
                                "\n**__",
                                user,
                                "__** replies with: ",
                                g[i],
                                " ",
                                g[++i],
                                youCan
                            }), false, null, null);
                            break;
                        }
                    }
                    youCan = null;
                    g = null;
                    content = null;
                    msg = null;
                    user = null;
                }
                catch (Exception j)
                {
                    num = 1;
                    if (num == 1)
                    {
                        await Context.Channel.SendMessageAsync(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}<<<", new object[]
                        {
                        j.Message,
                        j.TargetSite,
                        j.Source,
                        j.InnerException,
                        j.StackTrace,
                        j.HResult,
                        j.Data,
                        j.HelpLink
                        }), false, null, null);
                        j = null;
                    }
                }
            }
        }

        private bool ContainsChecker(params string[] t)
        {
            string z = Message.Content.ToLower().Replace("gae", "gay");
            if (!Message.Content.ToLower().Contains("strangey"))
                z = z.Replace("gey", "gay");
            foreach (string f in t)
            {
                if (z.Contains(f.ToLower()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Used to automatically delete invite links. Remove the commenting out in the main method.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        //System.Timers.Timer temporaryTimer = new System.Timers.Timer();

        public void DeleteFromArray(Object source, System.Timers.ElapsedEventArgs e)
        {
            var ppp = _client.GetGuild(472551343148761090).GetInvitesAsync().Result.ToList();
            for (int i = 0; i < ppp.Count; i++)
            {
                Console.Write($"Deleted: {ppp[i].Inviter} || {ppp[i].CreatedAt}");
                ppp[i].DeleteAsync();
            }
        }

        StringBuilder tg = new StringBuilder();

        private async Task Client_MessageReceived(SocketMessage MessageParam)
        {
            try
            {
                Program.Message = (SocketUserMessage)MessageParam;
                this.Context = new SocketCommandContext(this._client, Program.Message);
                Program.currentGuild = this.Context.Guild;
                if (Context.User.Id == 187273824176177152 && Context.Channel.Id == 472570932498661386)
                {
                    Console.WriteLine(Message.Content);
                }

                if (Context.Channel.Id == 565366019368026133 && !Context.User.IsBot)
                {
                    if (Context.Message.Attachments.Count <= 0)
                    {
                        if (!Context.Message.Content.ToLower().Contains("http"))// || !Context.Message.Content.ToLower().Contains(".com"))
                        {
                            var asyncDelete = await Context.Channel.SendMessageAsync("Message deleted because it's not a link to a work of art or fanfiction or an image upload. Please tag the person in one of the general chat channels to talk about their art");
                            await Message.DeleteAsync();
                            GuildCommands.AddTaskControl.Add(new Resources.TaskControl(null, 10000, false));
                            GuildCommands.AddTaskControl[GuildCommands.AddTaskControl.Count - 1].AddDeletion(asyncDelete);
                        }
                    }
                }
                string f = Message.Content.Split(' ')[0];

                
                if (this.Context.Message != null && !(this.Context.Message.Content == ""))
                {
                    if (!this.Context.User.IsBot)
                    {
                        if (this.ThankYou(Program.Message.Content))
                        {
                            if (this.ThankYou(Program.Message.Content))
                            {
                                await this.Context.Channel.SendMessageAsync("You're welcome kiddo! I think I've earned some pudding *nudge nudge*", false, null, null);
                            }
                        }
                        await this.AddExp(Program.Message.Content.ToString().Length);
                        int ArgPos = 0;
                        if (!Program.Message.Content.Contains("!") && this.tData != null)
                        {
                            string ct = this.tData.CardNameCall(Program.Message.Content);
                            if (ct != string.Empty)
                            {
                                await this.Context.Channel.SendMessageAsync(ImgurImplement.GetImage(ct).Result, false, null, null);
                            }
                            ct = null;
                        }
                        if (!this.HasMessagePrefix(Program.Message.Content.ToLower(), ref ArgPos))
                        {
                            if (Program.Message.Content[0] == '!' && !(Program.Message.Content.ToLower() == "!d bump") && Program.Message.Content[1] != ' ' && !Message.Content.ToLower().Contains("!nep"))
                            {
                                //string g = Program.Message.Content.Replace("!", string.Empty);
                                //await this.Context.Message.DeleteAsync(null);
                                //await this.EmojiUse(g);
                            }
                        }
                        else
                        {
                            IResult result = await this._commands.ExecuteAsync(this.Context, ArgPos, null, MultiMatchHandling.Exception);
                            IResult Result = result;
                            result = null;
                            bool mngMsg = this.Context.Message.Content.ToLower().Contains("!nep em") || this.Context.Message.Content.ToLower().Contains("!nep e") || this.Context.Message.Content.ToLower().Contains("!nep emoji");
                            if (mngMsg)
                            {
                                Program._contextDelete = this.Context;
                            }
                            else
                            {
                                Program._contextDelete = null;
                            }
                            if (!Result.IsSuccess)
                            {
                                Console.WriteLine(string.Format("{0} at Commands] Something went wrong with executing a command. Text: {1} | Error: {2}", DateTime.Now, this.Context.Message.Content, Result.ErrorReason));
                            }
                            Result = null;
                        }
                    }
                }
            }
            catch (Exception i)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, i, "Client_MessageReceived problem");
                throw;
            }
        }
    }
}