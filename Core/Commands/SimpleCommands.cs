﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using NepBot.Data;
using NepBot.Resources.Database;
using NepBot.Resources.Extensions;

namespace NepBot.Core.Commands
{
    public class SimpleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ImageData _imageData = new ImageData();
        private static List<string> urlList = File.ReadAllText(Program.DataPath("GiantUrlList", "txt")).Split('|').ToList();

        [Command("random anime image")]
        [Alias("rai", "random anime", "random image", "anime image", "ai")]
        [Summary("Grabs a random image from Safebooru and posts it to chat")]

        public async Task RandomAI([Remainder] string Input = null)
        {
            var myClient = new WebClient();
            var response = myClient.DownloadStringTaskAsync("https://safebooru.org/index.php?page=post&s=random").Result;
            var charArray = response.ToCharArray();
            StringBuilder sb = new StringBuilder();
            string searchTerm = "<meta property=\"og:image\" itemprop=\"image\" content=";
            int length = "\"https://safebooru.org//images/511/14532ea3c5ab53eedf99f2327cfe97e4e5b052b2.jpg\" />".Length + searchTerm.Length;
            bool buildUrl = false;
            try
            {
            errorFound:;
                for (int i = 0; i < charArray.Length - 25; i++)
                {
                    if (!buildUrl)
                    {
                        for (int lengthTerm = 0; lengthTerm < searchTerm.Length; lengthTerm++)
                            sb.Append($"{charArray[i + lengthTerm]}");
                        if (sb.ToString() == searchTerm)
                        {
                            buildUrl = true;
                        }
                        sb = new StringBuilder();
                    }
                    if (buildUrl)
                    {
                        sb.Append(charArray[i]);
                        if (sb.Length >= length)
                            break;
                        continue;
                    }
                    sb = new StringBuilder();
                }               
                if (sb.ToString().Length == 0)
                {
                    length++;
                    goto errorFound;
                }
                string v = sb.ToString();
                v = v.Replace(searchTerm, string.Empty);
                v = v.Replace("\"", string.Empty);
                v = v.Replace(" /", string.Empty);
                v = v.Replace(">", string.Empty);
                v = v.Replace(" <div", string.Empty);
                v = v.Replace("<div", string.Empty);
                v = v.Replace("\n<div", string.Empty);
                sb = new StringBuilder(v);
                HttpClient cw = new HttpClient();
                Stream stream2 = await cw.GetStreamAsync(sb.ToString());
                MemoryStream ms = new MemoryStream();
                stream2.CopyTo(ms);
                stream2.Close();
                Bitmap bitMap = (Bitmap)System.Drawing.Image.FromStream(ms);
                ms.Position = 0;
                if (Input != null)
                {
                    ms = new MemoryStream();
                    List<string> blankList = new List<string>();
                    var splitter = ExtensionMethods.GenericSplit(Input, "&", "|");
                    blankList.Add("");
                    blankList.Add(splitter[0]);
                    if (splitter.Length > 1)
                        blankList.Add(splitter[1]);
                    ExtensionMethods.TextToImage(blankList, ref bitMap, ref ms, false);
                    ms.Position = 0;
                }
                //await Context.Channel.SendMessageAsync($"URL: <{v}>");
                string tag = (!v.Contains("gif")) ? bitMap.ImageFormatType().ToString().ToLower() : "gif";
                await Context.Channel.SendFileAsync(ms, $"Pudding.{tag}", "Pudding Delivery!~Nepu", false, null, null);
                bitMap.Dispose();
                ms.Close();
                cw.Dispose();
            }
            catch (Exception i)
            {
                await Context.Channel.SendMessageAsync(sb.ToString());
            }
        }

        [Command("promote me")]
        [Summary("Gives you your deserved roleplay role based on your level if the bot didn't do it automatically")]

        private async Task PromoteUser()
        {
            try
            {
                if (Context.Guild.Id != Program.myGuildId)
                    return;
                UserData ud = ExtensionMethods.FindPerson(Context.User.Id);
                int arrayIndex = ud.CasualLevel / 10; // automatically finds the array index based on the server's level guaranteed to find their maximum deserved role
                for (int i = arrayIndex; i > 0; i--)
                {
                    if (Program.casualLevels[i].roleNumber == 0)
                    {
                        continue;
                    }
                    await Context.Guild.GetUser(ud.UserID).AddRoleAsync(Context.Guild.GetRole(Program.casualLevels[i].roleNumber));
                }
                arrayIndex = ud.ParaLevel / 10; // automatically finds the array index based on the server's level guaranteed to find their maximum deserved role
                for (int i = arrayIndex; i > 0; i--)
                {
                    if (Program.paraLevels[i].roleNumber == 0)
                    {
                        continue;
                    }
                    await Context.Guild.GetUser(ud.UserID).AddRoleAsync(Context.Guild.GetRole(Program.paraLevels[i].roleNumber));
                }
                arrayIndex = ud.NonLevel / 10;
                for (int i = arrayIndex; i > 0; i--)
                {
                    if (Program.nonLevels[i].roleNumber == 0)
                    {
                        continue;
                    }
                    await Context.Guild.GetUser(ud.UserID).AddRoleAsync(Context.Guild.GetRole(Program.nonLevels[i].roleNumber));
                }
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
                }), false, null, null);
            }
        }

        [Command("Random Profile Pic")]
        [Alias("RandomProfilePic", "RandomProfilePicture", "Random Profile Picture", "rpp", "r p p")]
        [Summary("Posts the profile picture of a random user. You can add a top and bottom caption if you want !nep rpp (top text)&(bottom text). Or just !nep rpp for no caption.")]

        public async Task RPP([Remainder] string Input = null)
        {
            try
            {
                SocketGuildUser User = Context.Guild.Users.ToList()[UtilityClass.ReturnRandom(0, Context.Guild.Users.Count)];
                Stream response = null;
                Bitmap bitMap = null;
                HttpClient cw = new HttpClient();
                string tag = string.Empty;
                string defaultToYou = string.Empty;
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
                    streamtest.Close();
                    stream2.Close();
                    response.Close();
                    MemoryStream stream = new MemoryStream();
                    List<string> blankList = new List<string>();
                    if (Input != null)
                    {
                        var splitter = ExtensionMethods.GenericSplit(Input, "&", "|");
                        blankList.Add("");
                        blankList.Add(splitter[0]);
                        if (splitter.Length > 1)
                            blankList.Add(splitter[1]);
                    }
                    ExtensionMethods.TextToImage(blankList, ref bitMap, ref stream, Context.Message.Attachments.Count > 0);
                    string aRand = "A random profile pic!";
                    aRand += (blankList.Count == 0) ? "" : " With captions!";
                    SocketUser contUser = base.Context.User;
                    UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                    string userName = (string.IsNullOrEmpty(User.Nickname) || string.IsNullOrWhiteSpace(User.Nickname)) ? User.Username : User.Nickname;
                    ud.Pudding += 50UL;
                    await base.Context.Channel.SendFileAsync(stream, $"Pudding.{tag}", $"Pudding Delivery!~Nepu\n{aRand}! It's {userName}'s profile picture!", false, null, null);
                    stream.Close();
                    bitMap.Dispose();
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

        [Command("youtube")]
        [Summary("Performs a youtube search and grabs the first video on the list. Try to be specific in your search for maximum efficiency.")]
        public async Task Youtube([Remainder] string Input = null)
        {
            var myClient = new WebClient();
            Input = Input.Replace(' ', '+');
            string response = myClient.DownloadString($"https://www.youtube.com/results?search_query={Input}");
            var charArray = response.ToCharArray();
            StringBuilder sb = new StringBuilder();
            string searchTerm = "/watch?v=";
            int length = "/watch?v=TKTIfXTd32M".Length;
            bool buildUrl = false;
            for (int i = 0; i < charArray.Length - 13; i++)
            {
                if (!buildUrl)
                {
                    for (int lengthTerm = 0; lengthTerm < searchTerm.Length; lengthTerm++)
                        sb.Append($"{charArray[i + lengthTerm]}");
                    //sb.Append($"{charArray[i]}{charArray[i + 1]}{charArray[i + 2]}{charArray[i + 3]}{charArray[i + 4]}{charArray[i + 5]}{charArray[i + 6]}{charArray[i + 7]}{charArray[i + 8]}");
                    if (sb.ToString() == searchTerm)
                    {
                        buildUrl = true;
                    }
                    sb = new StringBuilder();
                }
                if (buildUrl)
                {
                    sb.Append(charArray[i]);
                    if (sb.Length >= length)
                        break;
                    continue;
                }
                sb = new StringBuilder();
            }
            await Context.Channel.SendMessageAsync(sb.ToString().Insert(0, "https://www.youtube.com"));
        }

        [Command("find ping")]
        [Summary("Finds the last ping someone sent you that wasn't sent by the bot.")]
        public async Task FP([Remainder] string Input = null)
        {
            var p = Context.Channel.GetMessagesAsync(20000).FlattenAsync().Result.ToList();
            bool custom = ulong.TryParse(Input, out ulong customSearch);
            if (!custom)
                customSearch = Context.User.Id;
            string url = string.Empty;
            for (int i = 0; i < p.Count; i++)
            {
                if (p[i].Author.IsBot)
                    continue;
                if (p[i].Content.Contains("@") && p[i].Content.Contains($"{customSearch}"))
                {
                    url = p[i].GetJumpUrl();
                    break;
                }
            }
            if (url == string.Empty)
            {
                await Context.Channel.SendMessageAsync("I scanned 20000 messages and found no ping!");
                return;
            }
            await Context.Channel.SendMessageAsync(url);
        }

        [Command("pi")]
        [Summary("Prints out as many characters of pi as Discord will allow in a single message.")]
        public async Task PI()
        {
            string pi = "3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067982148086513282306647093844609550582231725359408128481117450284102701938521105559644622948954930381964428810975665933446128475648233786783165271201909145648566923460348610454326648213393607260249141273724587006606315588174881520920962829254091715364367892590360011330530548820466521384146951941511609433057270365759591953092186117381932611793105118548074462379962749567351885752724891227938183011949129833673362440656643086021394946395224737190702179860943702770539217176293176752384674818467669405132000568127145263560827785771342757789609173637178721468440901224953430146549585371050792279689258923542019956112129021960864034418159813629774771309960518707211349999998372978049951059731732816096318595024459455346908302642522308253344685035261931188171010003137838752886587533208381420617177669147303598253490428755468731159562863882353787593751957781857780532171226806613001927876611195909216420198938095257201065485863278865936153381827968230301952035301852968995773622599413891249721775283479131515574857242454150695950829533116861727855889075098381754637464939319255060400927701671139009848824012858361603563707660104710181942955596198946767837449448255379774726847104047534646208046684259069491293313677028989152104752162056966024058038150193511253382430035587640247496473263914199272604269922796782354781636009341721641219924586315030286182974555706749838505494588586926995690927210797509302955321165344987202755960236480665499119881834797753566369807426542527862551818417574672890977772793800081647060016145249192173217214772350141441973568548161361157352552133475741849468438523323907394143334547762416862518983569485562099219222184272550254256887671790494601653466804988627232791786085784383827967976681454100953883786360950680064225125205117392984896084128488626945604241965285022210661186306744278622039194945047123713786960956364371917287467764657573962413890865832645995813390478027590";
            await Context.Channel.SendMessageAsync(pi);
        }

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


        [Command("spaghet")]
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

        [Command("randomlast")]
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

        [Command("pingme")]
        [Summary("Have the bot ping you if you are inactive for a week or longer")]
        public async Task PingMe()
        {
            UserData ud = ExtensionMethods.FindPerson(Context.User.Id);
            ud.pingForInactivity = !ud.pingForInactivity;
            if (ud.pingForInactivity)
                await Context.Channel.SendMessageAsync("pinging you if you are inactive for 2 days or longer");
            if (!ud.pingForInactivity)
                await Context.Channel.SendMessageAsync("No longer pinging you if you are inactive for 2 days or longer");
        }

        [Command("randomtext")]
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
            bool flag = ud.Pudding < 1;
            if (flag)
            {
                await base.Context.Channel.SendMessageAsync("aww... quit your teasing. You don't even have any pudding to give me... #SadNepu.", false, null, null);
            }
            else
            {
                ud.Pudding -= 1UL;
                await base.Context.Channel.SendMessageAsync($"You lost 1 pudding, but Neptune's happy because she's been given a total of {++Program.miscBotData.totalPudding} pudding! Give yourself a pat on the back!\n {urlList[8]}");
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
            try
            {

                await Context.Channel.SendMessageAsync($"Total of {++Program.miscBotData.totalEggplant} was given to me! You're satan! You evil! " +
                    $"You... oh I think I'm going to be siiiiiick!!\n {urlList[6]} ");

            }
            catch (Exception i)
            {
                await Context.Channel.SendMessageAsync(string.Format($"{urlList.Count}\n>>>" + "{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
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
