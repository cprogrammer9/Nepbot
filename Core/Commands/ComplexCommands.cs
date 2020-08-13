using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using ConvenienceMethods;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImgFlip;
using NepBot.Core.Data;
using NepBot.Data;
using NepBot.Resources.Code_Implements;
using NepBot.Resources.Extensions;
using Discord.Rest;
using NepBot.Resources;
using static NepBot.Resources.Extensions.ExtensionMethods;
using NepBot.Resources.Database;

namespace NepBot.Core.Commands
{
    [Serializable]
    public sealed class ComplexCommands : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _database;
        private MathCalculations _math = new MathCalculations();
        private readonly string imgurls = Program.DataPath("imgurls", "txt");
        public readonly string Testttt = "";
        static TaskControl tk;
        public static List<ToReplace<RestUserMessage>> rum = new List<ToReplace<RestUserMessage>>();
        public static List<ToReplace<SocketUserMessage>> sum = new List<ToReplace<SocketUserMessage>>();
        public const int timerDuration = 40000;

        [Command("imageOverimage")]
        [Alias("ioi")]
        [Summary("Places an image over another image. Type !nep ioi {name of image}&{image url}&x offset&y offset\nJust enter a number for the offsets if necessary. This is in case the image isn't centered properly it gives you some manual control.")]

        public async Task Ioi([Remainder] string Input = null)
        {
            try
            {
                if (!HasSeparator(Context.Message.Content))
                {
                    await Context.Channel.SendMessageAsync("It's !nep ioi (image name)&bottom text&top text");
                    return;
                }
                var split = ExtensionMethods.GenericSplit(Input, "|", "&");
                ImageData id = new ImageData();
                GraphicsMethods gm = new GraphicsMethods();
                MemeStorage p = new MemeStorage();
                int offsetX = 0;
                int offsetY = 0;
                if (split.Length > 2)
                    offsetX = int.Parse(split[2]);
                if (split.Length > 3)
                    offsetY = int.Parse(split[3]);
                foreach (var x in id.memeStorage)
                {
                    if (x.memeName == split[0])
                    {
                        p = x;
                        break;
                    }
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    Stream response = null;
                    Bitmap bitMap = null;
                    HttpClient cw = new HttpClient();
                    Stream stream2 = await cw.GetStreamAsync(split[1]);
                    response = stream2;
                    bitMap = (Bitmap)System.Drawing.Image.FromStream(response);
                    Bitmap x = gm.MemeCaption(bitMap, new Size(p.sizeWidth, p.sizeHeight), p.offsetX, p.offsetY, p.imgPath, offsetX, offsetY) as Bitmap;
                    x.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0L, SeekOrigin.Begin);
                    x.Dispose();
                    await Context.Channel.SendFileAsync(stream, "Pudding.png", "Pudding Delivery!~Nepu", false, null, null);
                }

            }
            catch (Exception i)
            {
                await base.Context.Channel.SendMessageAsync(string.Format("From Ioi>>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                }), false, null, null);
            }

        }

        [Command("vote")]
        [Summary("Create a react vote for funsies or realzies. You can use your own emoji or let the bot pick randomly.\nType !nep (question or comment)|Choice1(emoji if any)|Choice2(emoji if any) etc")]

        public async Task Vote([Remainder] string Input = null)
        {
            if (!HasSeparator(Context.Message.Content))
            {
                await Context.Channel.SendMessageAsync("It's !nep vote (thing being asked to vote for)&(vote option 1)&(vote option 2) etc");
                return;
            }
            var split = ExtensionMethods.GenericSplit(Input, "|", "&");
            var emojis = Context.Guild.Emotes.ToList();
            List<GuildEmote> emojiIds = new List<GuildEmote>();
            for (int i = 1; i < split.Length; i++)
            {
                int picker = UtilityClass.ReturnRandom(0, emojis.Count);
                bool continueBaseLoop = true;
                foreach (var x in emojis)
                {
                    if (split[i].ToLower().Contains(x.Name.ToLower()))
                    {//<:perpell:571875093870018576>
                        //await Context.Channel.SendMessageAsync("INSIDE FIRST IF STATEMENT: " + split[i] + " | " + x.Name);
                        continueBaseLoop = false;
                        emojiIds.Add(x);
                        split[i] = split[i].Replace($"<:{x.Name}:{x.Id.ToString()}>", string.Empty);
                        break;
                    }
                }
                if (continueBaseLoop)
                    emojiIds.Add(emojis[picker]);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"Cast your votes for: **__{split[0]}__**\n");
            for (int i = 1; i < split.Length; i++)
            {
                int emojiSift = i - 1;
                if (i + 1 < split.Length)
                    sb.Append($"{split[i]}: {"".MakeCustomEmoji(emojiIds[emojiSift].Name, emojiIds[emojiSift].Id.ToString())}, ");
                else
                    sb.Append($"{split[i]}: {"".MakeCustomEmoji(emojiIds[emojiSift].Name, emojiIds[emojiSift].Id.ToString())}");
            }
            var g = await Context.Channel.SendMessageAsync($"{sb.ToString()}");
            foreach (var p in emojiIds)
            {
                //await new Task()
                await g.AddReactionAsync(p);
            }
            //await g.AddReactionsAsync(emojiIds.ToArray());
        }

        [Command("listcustomemojis")]
        [Alias("ListCustomEmoji", "CustomEmojiList", "ListEmojis", "ListEms", "EmojiList", "GifList", "emoji list")]
        [Summary("Gets a list of custom gif emojis you can use in chat. Type !(emojiname) to use them!")]
        public async Task AddIMG()
        {
            StringBuilder sb = new StringBuilder();
            string[] g = ExtensionMethods.GenericSplit(File.ReadAllText(this.imgurls), "|");
            for (int i = 1; i < g.Length; i += 3)
            {
                sb.Append(g[i]).Append((i + 3 >= g.Length) ? string.Empty : ", ");
            }
            await base.Context.Channel.SendMessageAsync(sb.ToString(), false, null, null);
        }

        [Command("random emoji")]
        [Summary("Posts a random gif emoji from the bot list into the chat.")]
        public async Task RandomEmoji()
        {
            string[] g = ExtensionMethods.GenericSplit(File.ReadAllText(this.imgurls), "|");
            List<string> emNames = new List<string>();
            List<string> emUrls = new List<string>();
            for (int i = 1; i < g.Length; i += 3)
            {
                emNames.Add(g[i]);
                emUrls.Add(g[i + 1]);
            }
            int finder = UtilityClass.ReturnRandom(0, emNames.Count);
            string user = ExtensionMethods.GetUsersName(Context.User.Id.ToString(), Context, false);
            string tots = string.Concat("**__", user, "__** replies with: ", emNames[finder], " ", emUrls[finder]);
            var f = await base.Context.Channel.SendMessageAsync(tots);
            //rum.Add(new ToReplace(f, tots.Replace(emUrls[finder], string.Empty)));
            //tk = new TaskControl(EndTimer, timerDuration);
        }

        public struct ToReplace<T>
        {
            public T originalMessage;
            public string removal;
            public ToReplace(T originalMessage, string removal)
            {
                this.originalMessage = originalMessage;
                this.removal = removal;
            }
        }



        private void EndTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                foreach (var f in rum)
                {
                    f.originalMessage.ModifyAsync(x => x.Content = f.removal);
                }
                rum.Clear();
                foreach (var f in sum)
                {
                    f.originalMessage.ModifyAsync(x => x.Content = f.removal);
                }
                sum.Clear();
            }
            catch (Exception i)
            {
                Console.WriteLine(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                }), false, null, null);
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("addimg")]
        [Summary("(gifs only please) Adds an image to the bot's emoji list to be used by others. Format: !nep addimg (name)|(url) (no parenthesis)")]
        public async Task AddIMG([Remainder] string Input = null)
        {
            if (Context.Guild.Id != Program.myGuildId)
                return;
            bool flag = Input.Contains("help");
            if (flag)
            {
                await base.Context.Channel.SendMessageAsync("Format is !nep addimg (name)|(url)\nDo not include the parenthesis. Don't be that guy.", false, null, null);
            }
            else
            {
                string[] g = ExtensionMethods.GenericSplit(Input, "|", "&");
                string user = ExtensionMethods.GetUsersName(Context.User.Id.ToString(), Context);
                g[0] = g[0].Replace(" ", string.Empty);
                g[1] = g[1].Replace(" ", string.Empty);
                //g[2] = g[2].Replace(" ", string.Empty);
                string[] list = ExtensionMethods.GenericSplit(File.ReadAllText(this.imgurls), "|");
                for (int i = 1; i < list.Length; i += 3)
                {
                    list[i] = list[i].Replace(" ", string.Empty);
                    if (g[0] == list[i])
                    {
                        await base.Context.Channel.SendMessageAsync("The name " + g[0] + " is already being used for another emoji. Try again.", false, null, null);
                        return;
                    }
                }
                for (int j = 2; j < list.Length; j += 3)
                {
                    list[j] = list[j].Replace(" ", string.Empty);
                    if (g[1] == list[j])
                    {
                        await base.Context.Channel.SendMessageAsync("The URL for this image has already been added to this bot. Try again.", false, null, null);
                        return;
                    }
                }
                File.AppendAllText(this.imgurls, string.Concat(new string[]
                {
                    user,
                    "|",
                    g[0].ToLower(),
                    "| ",
                    g[1],
                    " |\n"
                }));
                await base.Context.Channel.SendMessageAsync(string.Concat(new string[]
                {
                    user,
                    " Added image name: ",
                    g[0],
                    "\nURL: ",
                    g[1]
                }), false, null, null);
            }
        }

        [Command("em")]
        [Alias(new string[]
        {
            "emoji",
            "e"
        })]
        [Summary("Uses an emoji from the bot's emoji list. !nep em (name)|(custom message)\nCustom message is optional. Can also just type !(name|(custom message).")]
        public async Task EmojiUse([Remainder] string Input = null)
        {
            bool flag = Input.Contains("help");
            if (flag)
            {
                await base.Context.Channel.SendMessageAsync("Format is !nep em (name)|(custom message)\nDo not add spaces for any of the image names. None of them have spaces.", false, null, null);
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
                    string[] g = ExtensionMethods.GenericSplit(File.ReadAllText(this.imgurls), "|");
                    string[] content = ExtensionMethods.GenericSplit(Input, "|", "&");
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

                            string tots = string.Concat(msg, "\n**__", user, "__** replies with: ", g[i], " ", g[++i], youCan);
                            var f = await base.Context.Channel.SendMessageAsync(tots);
                            //rum.Add(new ToReplace(f, tots.Replace(g[++i], string.Empty)));
                            //tk = new TaskControl(EndTimer, timerDuration);
                            await Program._contextDelete.Message.DeleteAsync(null);
                            break;
                        }
                    }
                    youCan = null;
                    g = null;
                    content = null;
                    msg = null;
                    user = null;
                }
                catch (Exception i)
                {
                    await base.Context.Channel.SendMessageAsync(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                    {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                    }), false, null, null);
                }
            }
        }

        [Command("pat")]
        [Alias("pet", "pats", "headpat", "head pat")]
        [Summary("Sends a random gif patting the person. !nep pat (name)")]

        public async Task Pat([Remainder] string Input = null)
        {
            string p = File.ReadAllText(Program.DataPath("pats", "txt"));
            string[] pats = p.Split('|');
            string pick = pats[UtilityClass.ReturnRandom(0, pats.Length)];
            SocketUser name = ExtensionMethods.GetSocketUser(Input, Context, false);
            string tots = string.Format("<@{0}> pats <@{1}>!\n{2}", base.Context.User.Id, name.Id, pick);
            var f = await base.Context.Channel.SendMessageAsync(tots);
            rum.Add(new ToReplace<RestUserMessage>(f, tots.Replace(pick, string.Empty)));
            tk = new TaskControl(EndTimer, timerDuration);
        }

        [Command("captiongif")]
        [Summary("Captions a gif with the text of your choice. You can actually caption the gif with multiple kinds of text, just add blanks to skip a frame. Type !captiongif (url)|get frames if you want to get the total frames so you know how much text you can add")]

        public async Task CaptionGif([Remainder] string Input = null)
        {
            try
            {
                if (Input == null)
                {
                    await Context.Channel.SendMessageAsync("You need to post a gif URL and text! !nep captiongif (url)&(text) etc.");
                    return;
                }
                if (Input.ToLower() == "help")
                {
                    await Context.Channel.SendMessageAsync("Captions a gif with the text of your choice. You can actually caption the gif with multiple kinds of text, just add blanks to skip a frame. " +
                        "Type !captiongif (url)|get frames if you want to get the total frames so you know how much text you can add. |is a frame skip. Each| you add after text skips the frame");
                    return;
                }
                if (!HasSeparator(Context.Message.Content))
                {
                    await Context.Channel.SendMessageAsync("It's !nep caption gif (gif url)&(frame 1)&(frame 2) etc");
                    return;
                }
                bool hasAttachment = Context.Message.Attachments.ToArray().Length > 0;
                var f = ExtensionMethods.GenericSplit(Input, "|", "&");
                HttpClient cw = new HttpClient();
                Stream stream2 = await cw.GetStreamAsync((hasAttachment) ? Context.Message.Attachments.ToArray()[0].Url : f[0]);
                Stream response = stream2;
                MemoryStream finalOutput = new MemoryStream();
                response.CopyTo(finalOutput);
                finalOutput.Position = 0;
                var bitMap = System.Drawing.Image.FromStream(finalOutput);
                GifInfo gi = new GifInfo(bitMap);
                Console.WriteLine(Input);
                if (f[1].ToLower() == "get frames")
                {
                    await Context.Channel.SendMessageAsync(gi.FrameCount.ToString() + " total frames in this gif");
                    return;
                }

                GraphicsMethods gr = new GraphicsMethods();
                int x = 1;
                string txt = string.Empty;

                for (int i = 0; i < gi.Frames.Count; i++)
                {
                    txt = (f[x] != "") ? f[x] : txt;
                    gr.InsertTextOnImage("Impact", 150f, (Bitmap)gi.Frames[i], txt, true, false, true);
                    if (x + 1 < f.Length)
                        x++;
                }
                byte[] poi = gi.Ff();
                gi.debugMsg = gi.FileTooBig(bitMap, gi.BytesToString(poi.Length));
                if (gi.debugMsg != string.Empty)
                {
                    await Context.Channel.SendMessageAsync(gi.debugMsg);
                    cw.Dispose();
                    bitMap.Dispose();
                    response.Dispose();
                    gi = null;
                    return;
                }
                using (MemoryStream stream5 = new MemoryStream(poi))
                {
                    stream5.Position = 0;
                    SocketUser contUser = Context.User;
                    UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                    ud.Pudding += (ulong)(Input.Length * 25);
                    var xx = await Context.Channel.SendFileAsync(stream5, "Pudding.gif", $"<@{Context.User.Id}> has added text to this animation!~Nepu. Type !nep captiongif help to learn how to do this! Each | you add before the next text skips a frame. Use this to create delays before the text changes!", false, null, null);
                }
                //
                GuildCommands.AddTaskControl.Add(new TaskControl(null, 5000));
                GuildCommands.AddTaskControl[GuildCommands.AddTaskControl.Count - 1].AddDeletion(Context.Message);
                gi = null;
                finalOutput.Close();
                response.Dispose();
                bitMap.Dispose();
                cw.Dispose();
            }
            catch (Exception m)
            {
                await Context.Channel.SendMessageAsync($">>> {m.Message}\n{m.TargetSite}\n{m.Source}\n{m.InnerException}\n{m.StackTrace}\n{m.HResult}\n{m.Data}\n{m.HelpLink}");
            }

        }

        [Command("slap")]
        [Alias("♪")]
        [Summary("Posts a random slap gif. Type !nep slap (person's name)")]
        public async Task Hit([Remainder] string Input = null)
        {
            try
            {
                string p = File.ReadAllText(Program.DataPath("slaps", "txt"));
                string[] slaps = p.Split('|');
                string pick = slaps[UtilityClass.ReturnRandom(0, slaps.Length)];
                SocketUser name = ExtensionMethods.GetSocketUser(Input, Context, false);
                if (name == null)
                {
                    await Context.Channel.SendMessageAsync("User not found. Did you type name correctly?");
                    return;
                }
                string tots = $"{Context.User.Id.FormatTag(TagT.user)} hits {name.Id.FormatTag(TagT.user)}!\n{pick}";
                var f = await base.Context.Channel.SendMessageAsync(tots);
                rum.Add(new ToReplace<RestUserMessage>(f, tots.Replace(pick, string.Empty)));
                tk = new TaskControl(EndTimer, timerDuration);
            }
            catch (Exception i)
            {
                await Context.Channel.SendMessageAsync(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                }));
            }
        }
        //ミ(ノ￣^￣)ノ≡≡≡≡  ☆()￣□￣)/
        [Command("hug")]
        [Alias("huggu")]
        [Summary("Hugs a fellow server member! Type !nep hug (username)")]
        public async Task Hug([Remainder] string Input = null)
        {
            string url = File.ReadAllText(Program.DataPath("nuzes", "txt"));
            SocketUser name = ExtensionMethods.GetSocketUser(Input, Context, false);
            string tots = $"<@{Context.User.Id}> gives <@{name.Id}> a big hug!\n{url}";
            var f = await base.Context.Channel.SendMessageAsync(tots);
            rum.Add(new ToReplace<RestUserMessage>(f, tots.Replace(url, string.Empty)));
            tk = new TaskControl(EndTimer, timerDuration);
        }

        [Command("pick")]
        [Alias(
            "Bot of the server, what is your wisdom",
            "Bot of the server, what is your wisdom?",
            "Bot of the server what is your wisdom",
            "Bot of the server what is your wisdom?",
            "Oh bot of the server, what is your wisdom",
            "Oh bot of the server, what is your wisdom?",
            "Oh bot of the server what is your wisdom?",
            "Oh bot of the server, what is your wisdom?"
        )]
        [Summary("Have the bot pick something for you! Type !nep pick (thing)|(thing2) etc, supports unlimited things to pick!")]
        public async Task Pick([Remainder] string Input = null)
        {
            string[] f = ExtensionMethods.GenericSplit(Input, "|", "&");
            int g = UtilityClass.ReturnRandom(0, f.Length);
            string[] contains = new string[] { "awesome", "best girl", "beautiful", "cute" };
            foreach (string x in f)
            {
                if (x.ToLower().Contains("strang") || x.ToLower().Contains("sam"))
                {
                    foreach (string cont in contains)

                        if (x.ToLower().Contains(cont))
                        {
                            await base.Context.Channel.SendMessageAsync(x, false, null, null);
                            return;
                        }
                }
            }
            string choice = "";
            choice = f[g];
            await base.Context.Channel.SendMessageAsync(choice, false, null, null);
        }

        [Command("my rate")]
        [Summary("Check your pudding gains per post! This value increases depending on your levels!")]
        public async Task MyRate([Remainder] string Input = null)
        {
            SocketGuildUser contUser = Context.Guild.GetUser(Context.User.Id);
            bool flag = Input != null;
            if (flag)
            {
                contUser = (ExtensionMethods.GetSocketUser(Input, Context, false) as SocketGuildUser);
            }
            UserData ud = ExtensionMethods.FindPerson(contUser.Id);
            await base.Context.Channel.SendMessageAsync(string.Concat(
                ExtensionMethods.NameGetter(contUser, base.Context.Guild),
                " you gain pudding at this rate (based off your level):\nNon-Roleplay channel posts: ",
                ((ulong)(5L + (ulong)ud.NonLevel / 5L)).ToString(),
                $"\nCasual Roleplay Posts: {ud.CasualRPExp(0)} + word count",
                $"\nParagraph Roleplay Posts: {ud.ParaRPExp(0)} + word count\n",
                $"Your daily pudding gains are at **{_math.DailyPudding(ud)}** related to ALL levels gained everywhere. Casual and Paragraph roleplay levels have higher multipliers with Paragraph having the highest!\nYou also gain additional pudding like +50, +75 etc for using bot commands like the meme commands!",
                $"The amount you can bet in games which is (25 + amount of cards owned (from the nepbot card collection game!)) * your non-RP level which is: **{_math.TotalBet(ud.NonLevel, ud)}**"));
        }

        public ComplexCommands(CommandService database)
        {
            this._database = database;
            tk = new TaskControl(EndTimer, timerDuration);
        }

        public static async Task MessageUserReminder(ulong userID, Reminder reminder, SocketCommandContext Context)
        {
            SocketUser g = ExtensionMethods.GetSocketUser(userID.ToString(), Context, false);
            await g.SendMessageAsync("Here's the reminder you requested! ~Nepu ~Nepu\n" + reminder._message, false, null, null);
        }

        [Command("allnotes")]
        [Alias(new string[]
        {
            "alln",
            "all notes",
            "allnote",
            "all note"
        })]
        [Summary("Posts all notes made by people of this server. You can use the note name to view its content.")]
        public async Task AllNotes()
        {
            string b = Program.miscBotData.ListofNotes(base.Context.Guild.Id, false);
            bool flag = b.Length <= 0;
            if (flag)
            {
                await base.Context.User.SendMessageAsync("Nothing here for your guild!", false, null, null);
            }
            else
            {
                await base.Context.User.SendMessageAsync(b, false, null, null);
            }
        }

        [Command("getnote")]
        [Alias("gn", "getnotes", "get note", "get notes")]
        [Summary("Gets a note saved to the bot! Type !nep getnote then the name of the note!")]
        public async Task GetNotes([Remainder] string Input = null)
        {
            string notes = Program.miscBotData.GetNotes(Input);
            bool flag = notes == string.Empty;
            if (flag)
            {
                await base.Context.Channel.SendMessageAsync("The note " + Input + " doesn't exist! Did you spell it wrong? Searching for notes is not case sensitive by the way :)", false, null, null);
            }
            else
            {
                string[] split = ExtensionMethods.GenericSplit(notes, "|*|");
                foreach (string g in split)
                {
                    await base.Context.User.SendMessageAsync(g, false, null, null);
                }
                string[] array = null;
            }
        }

        [Command("addnote")]
        [Alias("an", "addnotes", "add note", "add notes")]
        [Summary("Adds a note to the bot. Type !nep addnote (note name)|(message)")]
        public async Task AddToNotes([Remainder] string Input = null)
        {
            string[] g = ExtensionMethods.GenericSplit(Input, "|", "&");
            bool flag = g.Length < 2 || g.Length > 2;
            if (flag)
            {
                await base.Context.Channel.SendMessageAsync("it's !nep addnote (name)|message did you do it correctly?", false, null, null);
            }
            else
            {
                Program.miscBotData.AddToNotes(g[0], g[1], base.Context.Guild.Id);
                await base.Context.Channel.SendMessageAsync(string.Concat(new string[]
                {
                    "You got it boss! Added that message to note ",
                    g[0],
                    "! type !nep getnote ",
                    g[0],
                    " to read all notes on this file!"
                }), false, null, null);
            }
        }

        [Command("bot meme list")]
        [Summary("Gives the image URL and command names for the images I have stored in my personal meme folder.")]
        public async Task CustomMemeHelp([Remainder] string Input = null)
        {
            int num = 0;
            try
            {
                bool flag = Input != null && Input.ToLower() == "help";
                if (flag)
                {
                    await base.Context.Channel.SendMessageAsync("Gives the image URL and command names for the images I have stored in my personal meme folder.", false, null, null);
                    return;
                }
                EmbedBuilder b = new EmbedBuilder();
                List<EmbedBuilder> f = new List<EmbedBuilder>();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < DataFiles.imageNames.Count; i++)
                {
                    b.AddField(DataFiles.imageNames[i], (i < DataFiles.imageURLs.Count) ? DataFiles.imageURLs[i] : "No URL right now", false);
                }
                b.AddField("URL To the full album", $"{File.ReadAllText(Program.DataPath("imgur url to meme photos", "txt"))}", false);
                await base.Context.Channel.SendMessageAsync("", false, b.Build(), null);
                b = null;
            }
            catch (Exception i)
            {
                await Context.Channel.SendMessageAsync(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                }));
            }
        }
        /*
        [Command("remind me")]
        [Summary("(Disabled, doesn't work for now) Set a reminder (it uses my server's timezone of EST). Bot will message you about doing the thing! Format is !nep remind me at/on ##/##/#### #:## pm/am to Message here. Example of use !nep remind me on 5/15/2019 at 6:00 to Dentist appointment in 1 hour!")]
        public async Task Reminder([Remainder] string Input = null)
        {
            return;
            bool flag = Input.ToLower() == "help";
            if (flag)
            {
                await base.Context.Channel.SendMessageAsync("Set a reminder (it uses my server's timezone of EST). Bot will message you about doing the thing! Format is !nep remind me (month #) (day) (hour 24 hour clock) (minute) (message). Example of use !nep remind me 6 16 11 0 Dentist appointment in 1 hour!", false, null, null);
            }
            else
            {
                int num = 0;
                try
                {
                    UserData ud = ExtensionMethods.FindPerson(base.Context.User.Id);
                    List<string> dates = ExtensionMethods.GenericSplit(Input, "/", " ", ":").ToList();
                    dates.RemoveAt(0);
                    dates.RemoveAt(3);
                    int month = int.Parse(dates[0]);
                    int day = int.Parse(dates[1]);
                    int year = int.Parse(dates[2]);
                    int hour = int.Parse(dates[3]).ConvertTo24HourTime(dates[5]);
                    int minute = int.Parse(dates[4]);
                    StringBuilder p = new StringBuilder();
                    bool on = false;
                    foreach (string g in dates)
                    {
                        if (on)
                        {
                            p.Append(g).Append(" ");
                        }
                        if (g == "to")
                        {
                            on = true;
                        }
                    }
                    List<string>.Enumerator enumerator = default(List<string>.Enumerator);
                    ud.SetReminder(month, day, year, hour, minute, ud.UserID, p.ToString());
                    await base.Context.Channel.SendMessageAsync(string.Format("You will be reminded on {0} to:\n{1}.\nNow give your ol' pal Nep a pudding won'cha?!", ud.ReturnReminderDate, p.ToString()), false, null, null);
                    ud = null;
                    dates = null;
                    p = null;
                }
                catch (Exception i)
                {
                    num = 1;
                    if (num == 1)
                    {
                        await base.Context.Channel.SendMessageAsync(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                        {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                        }), false, null, null);
                    }
                }
            }
        }*/

        [Command("dnddice")]
        [Alias("dd")]
        [Summary("Does a random dice roll.Type!nep dnddice How many dice How many sides each dice has yes(If you write yes at the end, the bot will PM you the results.Otherwise leave it blank.\nExample: !nep dd 2 6 yes(or just y)")]
        public async Task DNDDice([Remainder] string Input = null)
        {
            string[] diceValues = ExtensionMethods.GenericSplit(Input, " ");
            if (diceValues.Length >= 2)
            {
                int total = 0;
                StringBuilder b = new StringBuilder();
                for (int i = 0; i < int.Parse(diceValues[0]); i++)
                {
                    int rand = UtilityClass.ReturnRandom(1, int.Parse(diceValues[1]) + 1);
                    total += rand;
                    b.Append("[").Append(rand).Append("] ");
                }
                bool isYes = false;
                if (diceValues.Length >= 3)
                {
                    isYes = (diceValues[2].ToLower() == "yes" || diceValues[2].ToLower() == "y");
                    if (isYes)
                    {
                        await base.Context.User.SendMessageAsync(string.Concat(new object[]
                        {
                                total,
                                " rolled! woohoo! ",
                                diceValues[0],
                                " dice, each die has ",
                                int.Parse(diceValues[1]),
                                " sides: ",
                                b.ToString()
                        }), false, null, null);
                    }
                }
                else if (!isYes)
                {
                    await base.Context.Channel.SendMessageAsync(string.Concat(new object[]
                    {
                            total,
                            " rolled! woohoo! ",
                            diceValues[0],
                            " dice, each die has ",
                            int.Parse(diceValues[1]),
                            " sides: ",
                            b.ToString()
                    }), false, null, null);
                }
            }
        }

        [Command("rolldice")]
        [Summary("Does a random dice roll.Type!nep dice and either 1 - 75.")]
        public async Task DiceRoll([Remainder] string Input = null)
        {
            string[] diceStrings = new string[]
            {
                    "",
                    "<:1_:574108098424471572>",
                    "<:2_:574108098462089216>",
                    "<:3_:574108098508357632>",
                    "<:4_:574108098441248768>",
                    "<:5_:574108098353037312>",
                    "<:6_:574108098386460672>"
            };
            int val = int.TryParse(Input, out val) ? int.Parse(Input) : -9999;
            if (val != -9999)
            {
                if (val > 75)
                {
                    val = 75;
                }
                if (val < 1)
                {
                    val = 1;
                }
                int total = 0;
                StringBuilder b = new StringBuilder();
                for (int i = 0; i < val; i++)
                {
                    int rand = UtilityClass.ReturnRandom(1, 7);
                    total += rand;
                    b.Append(diceStrings[rand]).Append(" ");
                }
                await base.Context.Channel.SendMessageAsync(total + " rolled! woohoo!" + b.ToString(), false, null, null);
            }
        }

        [Command("help")]
        [Summary("Gives list of commands. Can be used in DMs with the bot.")]
        public async Task HelpCommand([Remainder] string Input = null)
        {
            HelpCommand hc = new HelpCommand(Context, _database);
            try
            {
                if (Input == null)
                {
                    await Context.Channel.SendMessageAsync("", false, hc.BaseInformation().Build());
                    return;
                }
                if (hc.GrabCommandInfo(Input) == null)
                {
                    await Context.Channel.SendMessageAsync("You entered an invalid choice! Type !nep help to get a list of possible help topics and try again!");
                    return;
                }

                foreach (var g in hc.GrabCommandInfo(Input))
                {
                    await Context.Channel.SendMessageAsync("", false, g.Build());
                }
            }
            catch (Exception i)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, i, "From Help Command ");
            }
        }

        [Command("profile")]
        [Summary("Checks your profile. Experience, levels, pudding amounts, post count etc. You can check another person's profile too, just enter their name after profile!")]
        public async Task EarnExp([Remainder] string Input = null)
        {
            try
            {
                SocketGuildUser contUser = Context.Guild.GetUser(Context.User.Id);
                bool flag = Input != null;
                if (flag)
                {
                    contUser = (ExtensionMethods.GetSocketUser(Input, Context, false) as SocketGuildUser);
                }
                Stream response = null;
                Bitmap bitMap = null;
                HttpClient cw = new HttpClient();
                bool flag2 = contUser.GetAvatarUrl(Discord.ImageFormat.Auto, 128) != null;
                if (flag2)
                {
                    Stream stream = await cw.GetStreamAsync(contUser.GetAvatarUrl(Discord.ImageFormat.Auto, 128));
                    response = stream;
                    stream = null;
                    bitMap = (Bitmap)System.Drawing.Image.FromStream(response);
                }
                else
                {
                    response = null;
                    bitMap = null;
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    GraphicsMethods gm = new GraphicsMethods();
                    UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                    bitMap = gm.ProfileArt(ExtensionMethods.NameGetter(contUser, Context.Guild), bitMap, ud);
                    string path = Program.DataPath("heysup", "jpg");
                    bitMap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    stream.Seek(0L, SeekOrigin.Begin);
                    bitMap.Dispose();
                    await Context.Channel.SendFileAsync(stream, "Pudding.jpg", "Pudding Delivery~ Nepu!", false, null, null);
                    //GraphicsMethods gr = new GraphicsMethods();
                    //tester = gr.InsertTextOnImage("Impact", 150f, tester, splitting[1], false, splitting[0] == "optimus wtf");
                    //tester = gr.InsertTextOnImage("Impact", 150f, tester, splitting[2], true, false);
                    //tester.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);                    
                    //tester.Dispose();
                    //await base.Context.Channel.SendFileAsync(stream, "Pudding.jpg", "Pudding Delivery~ Nepu!", false, null, null);
                    //gr = null;
                }

                contUser = null;
                response = null;
                bitMap = null;
                cw = null;
            }
            catch (Exception i)
            {
                await base.Context.Channel.SendMessageAsync(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                }), false, null, null);
            }
        }

        [Command("meme")]
        [Summary("Captions a meme. !nep meme (top text)|(bottom text). Type !nep list all memes to view what's available")]
        public async Task CreateMeme([Remainder] string Input = null)
        {
            try
            {
                bool flag = Input.ToLower() == "help";
                if (flag)
                {
                    StringBuilder b = new StringBuilder();
                    foreach (string x in DataFiles.imageNames)
                    {
                        b.Append("\n").Append(x);
                    }
                    await base.Context.Channel.SendMessageAsync("Captions a meme from a meme of your choice using the !nep list all memes to view the memes hosted by imgflip. Or use these images to caption from my personal folder: " + b.ToString() + "\nSyntax: !nep meme MemeName.TopText.BottomText", false, null, null);
                }
                else
                {
                    if (!HasSeparator(Context.Message.Content))
                    {
                        await Context.Channel.SendMessageAsync("It's !nep meme (meme name)&bottom text&top text");
                        return;
                    }
                    Bitmap tester = null;
                    List<string> splitting = ExtensionMethods.RecreateMemeTextList(new List<string>(ExtensionMethods.GenericSplit(Input, "|", "&")));
                    tester = DataFiles.ReturnImage(splitting[0]);
                    if (tester != null)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            GraphicsMethods gr = new GraphicsMethods();
                            tester = gr.InsertTextOnImage("Impact", 150f, tester, splitting[1], false, splitting[0] == "optimus wtf");
                            tester = gr.InsertTextOnImage("Impact", 150f, tester, splitting[2], true, false);
                            tester.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            stream.Seek(0L, SeekOrigin.Begin);
                            tester.Dispose();
                            await base.Context.Channel.SendFileAsync(stream, "Pudding.jpg", "Pudding Delivery~ Nepu!", false, null, null);
                            gr = null;
                        }
                    }
                    else
                    {
                        var g = File.ReadAllText(Program.DataPath("imgflip userid", "txt"));
                        var info = g.Split('|');
                        Console.WriteLine(info[0]);
                        Console.WriteLine(info[1]);
                        ImgFlipApi a = ImgFlipApi.Create(Utilities.MakeSecureStringFromString(info[0]), Utilities.MakeSecureStringFromString(info[1]));
                        string abc = a.Generate(splitting[0], splitting[1], splitting[2], splitting[3]).Result;
                        SocketUser contUser = base.Context.User;
                        UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                        ud.Pudding += 50UL;
                        await base.Context.Channel.SendMessageAsync(abc, false, null, null);
                        tester = null;
                        splitting = null;
                        a = null;
                        abc = null;
                        contUser = null;
                        ud = null;
                    }
                }
            }
            catch (Exception i)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, i, "From Memes ");
            }
        }

        [Command("find meme")]
        [Summary("Searches the imgflip API database for all available memes. If you're looking if a specific one is there, use this command.")]
        public async Task FindMeme([Remainder] string Input = null)
        {
            string[] acctInfo = ExtensionMethods.GenericSplit(File.OpenText(Program.DataPath("imgflip userid", "txt")).ReadToEnd(), "|");
            ImgFlipApi a = ImgFlipApi.Create(Utilities.MakeSecureStringFromString(acctInfo[0] ?? ""), Utilities.MakeSecureStringFromString(acctInfo[1] ?? ""));
            List<string> memes = a.GetMemeNameMatches(Input).Result;
            if (memes.Count == 0)
            {
                await base.Context.Channel.SendMessageAsync("Sorry, no results were found, nepu~.", false, null, null);
            }
            else
            {
                await base.Context.Channel.SendMessageAsync("Here are the a list of meme names matching your search, nepu~:\n", false, null, null);
                foreach (string g in memes)
                {
                    await base.Context.Channel.SendMessageAsync(g, false, null, null);
                }
            }
        }

        [Command("list all memes")]
        [Summary("Displays all custom meme images available. Also posts a pastebin file of all imgflip API memes available.")]
        public async Task ListMemes()
        {
            StringBuilder b = new StringBuilder();
            foreach (string x in DataFiles.imageNames)
            {
                b.Append("\n").Append(x);
            }
            await base.Context.Channel.SendMessageAsync($"Custom meme images {b.ToString()}\n**OR** Click here for a list of imgflip memes I will accept, nepu~" + "https://pastebin.com/vfE9Hfv0", false, null, null);
        }

        [Command("caption image")]
        [Summary("Takes an image from a URL and captions it with whatever you tell it to. (You can either add a URL, or skip the url and upload the image while using the bot command)  There are three ways you can use this:\n1: !nep caption image URL | TopText | BottomText(Writes all text)\n2: !nep caption image URL | bottomText(Writes only top text)\n3: !nep caption image | URL | topText(Writes only bottom text)")]
        public async Task MakeMemeURL([Remainder] string Input = null)
        {
            var splitting = ExtensionMethods.GenericSplit(Input, "|", "&");
            Stream response;
            Bitmap bitMap;
            bool hasAttachment = Context.Message.Attachments.Count > 0;
            if (!HasSeparator(Context.Message.Content))
            {
                await Context.Channel.SendMessageAsync("It's !nep caption image (url)&bottom text&top text");
                return;
            }
            HttpClient cw = new HttpClient();
            try
            {
                Stream stream2 = await cw.GetStreamAsync((hasAttachment) ? Context.Message.Attachments.ToArray()[0].Url : splitting[0]);
                response = stream2;
                stream2 = null;
                bitMap = (Bitmap)System.Drawing.Image.FromStream(response);
                MemoryStream stream = new MemoryStream();
                ExtensionMethods.TextToImage(splitting.ToList(), ref bitMap, ref stream, Context.Message.Attachments.Count > 0);
                SocketUser contUser = base.Context.User;
                UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                ud.Pudding += 75UL;
                bitMap.Dispose();
                response.Close();
                await Context.Channel.SendFileAsync(stream, "Pudding.jpg", "Pudding Delivery!~Nepu", false, null, null);
                stream.Close();
            }
            catch (Exception j)
            {
                string mainMsg = "Command entered incorrectly. Attach an image then (top text)&(bottom text) OR use an image URL (URL)&(top text)&(bottom text)";
                /*string mainMsg = string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}", new object[]
                {
                                j.Message,
                                j.TargetSite,
                                j.Source,
                                j.InnerException,
                                j.StackTrace,
                                j.HResult
                });
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, j, "CPP problem");*/
                await base.Context.Channel.SendMessageAsync(mainMsg);
            }
        }

        [Command("caption profile pic")]
        [Alias(new string[] { "cpp" })]
        [Summary("Takes a person's avatar and captions it. There are three ways you can use this:\n1: !nep caption profile pic topTextbottomText.Username (Writes all text)\n2: !nep caption profile pic .bottomText.Username (Writes only top text)\n3: !nep caption profile pic .topText. .Username (Writes only bottom text)")]
        public async Task MakeMeme([Remainder] string Input = null)
        {
            try
            {
                if (Input == null)//!HasSeparator(Context.Message.Content))
                {
                    await Context.Channel.SendMessageAsync("It's !nep cpp (name)&bottom text&top text");
                    return;
                }
                var splitting = ExtensionMethods.GenericSplit(Input, "|", "&");
                Stream response = null;
                Bitmap bitMap = null;
                HttpClient cw = new HttpClient();
                SocketUser User = null;
                string tag = string.Empty;
                string defaultToYou = string.Empty;
                User = ExtensionMethods.GetSocketUser(splitting[0], Context, false);
                if (User == null)
                {
                    defaultToYou = "User not found on server, defaulting to you\n";
                    User = Context.User;
                }
                try
                {
                    string pic = User.GetAvatarUrl(Discord.ImageFormat.Auto, 2048);
                    Stream stream2 = await cw.GetStreamAsync(pic);
                    response = stream2;
                    MemoryStream streamtest = new MemoryStream();
                    response.CopyTo(streamtest);
                    streamtest.Position = 0;
                    bitMap = (Bitmap)System.Drawing.Image.FromStream(streamtest);
                    tag = (!pic.Contains("gif")) ? bitMap.ImageFormatType().ToString().ToLower() : "gif";

                    if (splitting.Length == 1)
                    {
                        streamtest.Position = 0;
                        await Context.Channel.SendFileAsync(streamtest, $"{defaultToYou}Pudding.{tag}", "Pudding Delivery!~Nepu", false, null, null);
                        streamtest.Close();
                        bitMap.Dispose();
                        stream2.Close();
                        response.Close();
                        return;
                    }
                    streamtest.Close();
                    stream2.Close();
                    response.Close();
                    if (bitMap == null)
                    {
                        await base.Context.Channel.SendMessageAsync("Problem with the request boss image is le' null or whatever.", false, null, null);
                    }
                    else
                    {
                        if (tag == "gif")
                        {
                            Input = Input.Replace($"{splitting[0]}&", string.Empty);
                            var x = pic.Split('?');
                            if (x.Length > 1)
                            {
                                pic = pic.Replace($"{x[1]}", string.Empty);
                                pic = pic.Replace($"?", string.Empty);
                            }
                            Console.WriteLine(Input);
                            Console.WriteLine(pic);
                            await CaptionGif($"{pic}&{Input}");
                            streamtest.Close();
                            bitMap.Dispose();
                            stream2.Close();
                            response.Close();
                            return;
                        }
                        MemoryStream stream = new MemoryStream();
                        ExtensionMethods.TextToImage(splitting.ToList(), ref bitMap, ref stream, Context.Message.Attachments.Count > 0);
                        SocketUser contUser = base.Context.User;
                        UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                        ud.Pudding += 50UL;
                        await base.Context.Channel.SendFileAsync(stream, $"Pudding.{tag}", "Pudding Delivery!~Nepu", false, null, null);
                        stream.Close();
                        bitMap.Dispose();
                    }
                }
                catch (Exception j)
                {
                    string mainMsg = string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}", new object[]
                    {
                                j.Message,
                                j.TargetSite,
                                j.Source,
                                j.InnerException,
                                j.StackTrace,
                                j.HResult
                    });
                    ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, j, "CPP problem");
                    await base.Context.Channel.SendMessageAsync(mainMsg, false, null, null);
                }
            }

            catch (Exception j)
            {
                string mainMsg = string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}", new object[]
                {
                                j.Message,
                                j.TargetSite,
                                j.Source,
                                j.InnerException,
                                j.StackTrace,
                                j.HResult
                });
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, j, "CPP problem");
                await base.Context.Channel.SendMessageAsync(mainMsg, false, null, null);
            }
        }

        [Command("nep's pudding")]
        [Alias("neps pudding", "daily")]
        [Summary("Gives you a lot of pudding (you type !nep daily instead). Can do this once per day. The amount you gain is based off your level in both roleplay types and your non roleplay level. Default is 1000")]
        public async Task GetPudding()
        {
            SocketUser contUser = base.Context.User;
            UserData ud = ExtensionMethods.FindPerson(contUser.Id);
            bool usedDailyPudding = ud.UsedDailyPudd();
            ulong p = _math.DailyPudding(ud);
            Console.Write($"{contUser.Username}: {p}");
            if (!usedDailyPudding)
            {
                ud.Pudding += p;
                await Context.Channel.SendMessageAsync($"WAIT NOT THE PUDDING! ANYTHING BUT MY PUDDING!! Aww... I can't believe you took {p} bites of pudding! " +
                    $"You savage!\n{File.ReadAllText(Program.DataPath("dailypuddingURL", "txt"))}");
            }
            else
            {
                DateTime f = Program.miscBotData.UsedDailyPudding.AddDays(1);
                //Console.WriteLine("f: " + f);
                //Console.WriteLine("USED DAILY PUDDING: " + UserData.UsedDailyPudding);
                await Context.Channel.SendMessageAsync($"NOPE! Pudding's all mine! You already had your share today... all {p} of them! You'll have to wait until {f} ~nepu");
            }
        }

        private bool HasSeparator(string msg)
        {
            return msg.Contains("&") || msg.Contains("|");
        }
    }
}