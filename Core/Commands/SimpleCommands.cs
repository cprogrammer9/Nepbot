﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NepBot.Data;
using NepBot.Resources.Database;
using NepBot.Resources.Extensions;

namespace NepBot.Core.Commands
{
    public class SimpleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ImageData _imageData = new ImageData();
        private static List<string> urlList = Program.DataPath("GiantUrlList", "txt").Split('|').ToList();

        [Command("tag active")]
        [Summary("Tags only users in active roles who, generally, are active on the server.")]
        public async Task TagActive()
        {
            List<ulong> sgur = new List<ulong>();
            await Context.Channel.SendMessageAsync($"<@&{556722030393950219}><@&{472552681374744578}><@&{472552616753364998}><@&{472552639763185667}><@&{648179361119207425}>");
        }

        [Command("strong")]
        [Summary("Creates text using the emoji letters and numbers")]

        public async Task Strong([Remainder] string Input = null)
        {
            if (Input == null)
            {
                await Context.Channel.SendMessageAsync("Please input text you want the bot to spell!");
                return;
            }
            Input = Input.ToLower();
            char[] g = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            string[] numberNames = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "keycap_ten" };
            List<char> build = new List<char>();
            foreach (char p in Input.ToCharArray())
            {
                bool addIt = false;
                foreach (char x in g)
                {
                    if (p == x || p == ' ')
                    {
                        addIt = true;
                        break;
                    }
                }
                if (addIt)
                    build.Add(p);
            }
            StringBuilder sb = new StringBuilder();
            List<string> additionalSends = new List<string>();
            int val = -1;
            foreach (char x in build)
            {
                if (sb.Length >= 1800 && x == ' ')
                {
                    additionalSends.Add(sb.ToString());
                    sb = new StringBuilder();
                }
                if (x == ' ')
                {
                    sb.Append("  ");
                    continue;
                }
                if (int.TryParse(x.ToString(), out val))
                {
                    //await Context.Channel.SendMessageAsync($"It's parsing anumber, true {x.ToString()}");
                    sb.Append($":{numberNames[val]}: ");
                    continue;
                }
                sb.Append($":regional_indicator_{x}: ");
                //:regional_indicator_k: :keycap_ten:
            }
            additionalSends.Add(sb.ToString());
            try
            {


                for (int i = 0; i < additionalSends.Count; i++)
                {
                    await Context.Channel.SendMessageAsync(additionalSends[i]);
                }
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
            //await Context.Channel.SendMessageAsync(sb.ToString());
        }


        [Command("Spaghet")]
        [Summary("Posts the spaghet meme video.")]
        public async Task Spaghet()
        {
            await Context.Channel.SendMessageAsync(urlList[0]);
        }

        private bool LoopSkip(string phraseChk)
        {
            string[] array = new string[]
            {
                "!nep",
                "randomlast",
                "http"
            };
            foreach (string value in array)
            {
                bool flag = phraseChk.Contains(value);
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }

        [Command("RandomLast")]
        [Summary("Grabs the last (x) number of messages sent in this channel and randomizes all words then reposts it. Type !nep randomlast (number of messages)")]
        public async Task RandomLast([Remainder] string Input = null)
        {
            int amt = 1;
            bool flag = !int.TryParse(Input, out amt);
            if (flag)
            {
                amt = 1;
            }
            IEnumerable<IMessage> enumerable = await base.Context.Channel.GetMessagesAsync(amt + 1, CacheMode.AllowDownload, null).FlattenAsync<IMessage>();
            IEnumerable<IMessage> messages = enumerable;
            enumerable = null;
            List<string> sg = new List<string>();
            foreach (IMessage x2 in messages)
            {
                if (!this.LoopSkip(x2.Content))
                {
                    foreach (string z in x2.Content.Split(new char[]
                    {
                        ' '
                    }))
                    {
                        sg.Add(z);
                    }
                }
            }
            IEnumerator<IMessage> enumerator = null;
            Random rnd = new Random();
            string[] randomMessages = (from x in sg
                                       orderby rnd.Next()
                                       select x).ToArray<string>();
            StringBuilder sb = new StringBuilder();
            string[] array2 = randomMessages;
            int j = 0;
            while (j < array2.Length)
            {
                string g = array2[j];
                if (sb.Length > 1900)
                {
                    break;
                }
                if (!g.Contains("@"))
                {
                    goto IL_39A;
                }
                StringBuilder sbbb = new StringBuilder();
                foreach (char ch in g.ToCharArray())
                {
                    if (char.IsDigit(ch))
                    {
                        sbbb.Append(ch);
                    }
                }
                char[] array3 = null;
                string sug = null;
                ulong ul = 0UL;
                if (!ulong.TryParse(sbbb.ToString(), out ul))
                {
                    sbbb = null;
                    sug = null;
                    goto IL_39A;
                }
                sug = ExtensionMethods.GetUsersName(ul.ToString(), Context, true);
                sb.Append(sug).Append(" ");
            IL_3BE:
                j++;
                continue;
            IL_39A:
                sb.Append(g).Append(" ");
                g = null;
                goto IL_3BE;
            }
            array2 = null;
            await base.Context.Channel.SendMessageAsync(sb.ToString(), false, null, null);
        }

        [Command("RandomText")]
        [Summary("Randomizes the text you input here. Type !nep randomtext (paste text here)")]
        public async Task RandomText([Remainder] string Input = null)
        {
            string[] f = ExtensionMethods.GenericSplit(Input, " ");
            Random rnd = new Random();
            string[] MyRandomArray = (from x in f
                                      orderby rnd.Next()
                                      select x).ToArray<string>();
            StringBuilder sb = new StringBuilder();
            foreach (string g in MyRandomArray)
            {
                sb.Append(g).Append(" ");
            }
            string[] array = null;
            await base.Context.Channel.SendMessageAsync(sb.ToString(), false, null, null);
        }

        [Command("lewd")]
        [Summary("Posts the nep lewd quote video from the Neptunia dub")]
        public async Task Lewd()
        {
            await base.Context.Channel.SendMessageAsync(urlList[1]);
        }

        [Command("just do it")]
        [Summary("Posts the just do it video")]
        public async Task JustDoIt()
        {
            await base.Context.Channel.SendMessageAsync(urlList[2]);
        }

        [Command("woah")]
        [Summary("Posts the nep woah meme image")]
        public async Task NepWoah()
        {
            await base.Context.Channel.SendMessageAsync(urlList[3]);
        }

        [Command("motivation")]
        [Summary("Just the meme image you need to motivate your friends!")]
        public async Task Motivation()
        {
            await base.Context.Channel.SendMessageAsync(urlList[4]);
        }

        [Command("give pudding")]
        [Summary("Gives Nep bot 1 pudding. The bot records and saves a record of all pudding given to it.")]
        public async Task GivePudding([Remainder] string Input = null)
        {
            SocketUser contUser = base.Context.User;
            UserData ud = ExtensionMethods.FindPerson(contUser.Id);
            bool flag = ud.Pudding < 1UL;
            if (flag)
            {
                await base.Context.Channel.SendMessageAsync("aww... quit your teasing. You don't even have any pudding to give me... #SadNepu.", false, null, null);
            }
            else
            {
                ud.Pudding -= 1UL;
                Program.miscBotData.totalPudding += 1UL;
                await base.Context.Channel.SendMessageAsync(string.Format($"You lost 1 pudding, but Neptune's happy because she's been given a total of {0} pudding! Give yourself a pat on the back!\n {urlList[8]}", Program.miscBotData.totalPudding), false, null, null);
            }
        }

        [Command("monika")]
        [Summary("Posts a random Monika (from DDLC) image to the channel.")]
        public async Task Monika()
        {
            string[] f = Program.imageData.monikaImages;
            int i = global::UtilityClass.ReturnRandom(0, f.Length);
            await base.Context.Channel.SendFileAsync(f[i], null, false, null, null);
        }

        [Command("give eggplant")]
        [Summary("Gives Nepbot an eggplant. Are you really this sadistic? It keeps a record of all eggplant given to it.")]
        public async Task GiveEggplant()
        {
            Program.miscBotData.totalEggplant += 1UL;
            await base.Context.Channel.SendMessageAsync(string.Format($"Total of {0} was given to me! You're satan! You evil! You... oh I think I'm going to be siiiiiick!!\n {urlList[6]}", Program.miscBotData.totalEggplant), false, null, null);
        }

        [Command("wtf")]
        [Summary("Perfect Neptunia image for when something said on the server deserves a \"wtf\".")]
        public async Task WTF()
        {
            await base.Context.Channel.SendMessageAsync(urlList[5], false, null, null);
        }

        [Command("dab")]
        [Summary("Chibi Neptune dabs ya")]
        public async Task Dab()
        {
            await base.Context.Channel.SendMessageAsync(urlList[7]);
        }

        [Command("gif")]
        [Summary("Posts a random Neptunia-related gif to chat.")]
        public async Task NepGifs()
        {
            int random = UtilityClass.ReturnRandom(0, this._imageData.gifurls.Length);
            SocketUser contUser = base.Context.User;
            UserData ud = ExtensionMethods.FindPerson(contUser.Id);
            ud.Pudding += 10UL;
            await base.Context.Channel.SendMessageAsync(this._imageData.gifurls[random], false, null, null);
        }
    }
}