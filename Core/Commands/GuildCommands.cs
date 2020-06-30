using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.IO;
using ImgFlip;
using System.Net.Http;
using System.DrawingCore;
using ConvenienceMethods;
using Discord.WebSocket;
using System.Linq;
using NepBot.Resources.Database;
using NepBot.Data;
using System.Reflection;
using Passive.Services.DatabaseService;
using System.Text;
using NepBot.Resources.Games;
using NepBot.Resources.Code_Implements;
using NepBot.Resources.Extensions;
using NepBot.Core.Data;
using Discord.Rest;
using System.Timers;
using NepBot.Resources;


namespace NepBot.Core.Commands
{
    public class GuildCommands : ModuleBase<SocketCommandContext>
    {
        static int sifter = 0;
        List<SocketGuildUser> primaryUsers;
        string messages = string.Empty;
        static Timer t;

        [Command("Rename Channel")]
        [Summary("Any roleplayer can use this to rename a roleplay channel. Type !nep rename channel (channel name)|(new name). Don't use this to troll or I'll ban you from using the command.")]
        public async Task RenameChannel([Remainder] string Input = null)
        {
            ulong[] roles = new ulong[]
            {
                472552681374744578UL,
                472552616753364998UL,
                472552639763185667UL,
                682766159027896508UL
            };
            bool found = false;
            foreach (SocketRole x2 in base.Context.Guild.GetUser(base.Context.User.Id).Roles)
            {
                bool flag = found;
                if (flag)
                {
                    break;
                }
                foreach (ulong z in roles)
                {
                    bool flag2 = x2.Id == z;
                    if (flag2)
                    {
                        found = true;
                        //await base.Context.Channel.SendMessageAsync("Found a matching ID", false, null, null);
                        break;
                    }
                }
                ulong[] array = null;
            }
            IEnumerator<SocketRole> enumerator = null;
            if (!found)
            {
                await base.Context.Channel.SendMessageAsync("returned with !found", false, null, null);
            }
            else
            {
                string[] f = ExtensionMethods.GenericSplit(Input, "|", "&");
                string previous = f[0];
                string renamed = f[1];
                SocketGuildChannel sc = null;
                foreach (SocketGuildChannel g in base.Context.Guild.Channels)
                {
                    if (g.Name.ToLower() == previous.ToLower())
                    {
                        sc = g;
                        break;
                    }
                }
                IEnumerator<SocketGuildChannel> enumerator2 = null;
                ulong? p = base.Context.Guild.GetTextChannel(sc.Id).CategoryId;
                ulong? num = p;
                ulong num2 = 472551467212079114UL;
                bool flag3;
                if (!(num.GetValueOrDefault() == num2 & num != null))
                {
                    num = p;
                    num2 = 472551492377903115UL;
                    flag3 = !(num.GetValueOrDefault() == num2 & num != null);
                }
                else
                {
                    flag3 = false;
                }
                if (flag3)
                {
                    await base.Context.Channel.SendMessageAsync(string.Format("returned with category ID mismatch: Category ID: {0} ==> Looking for 472551467212079114 or 472551467212079115", p), false, null, null);
                }
                else
                {
                    await sc.ModifyAsync(delegate (GuildChannelProperties x)
                    {
                        x.Name = renamed;
                    }, null);
                }
            }
        }

        public static void DeleteFromArray(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (AddTaskControl.Count < 1)
                return;
            List<int> removeAt = new List<int>();
            for (int i = 0; i < AddTaskControl.Count; i++)
            {
                if (!AddTaskControl[i].IsEnabled)
                    removeAt.Add(i);
            }
            foreach (var g in removeAt)
            {
                AddTaskControl.RemoveAt(g);
            }
        }

        public GuildCommands()
        {
            //Console.WriteLine("Before: " + AddTaskControl.Count);
            TaskControl.AddToElapsed(DeleteFromArray);
            //Console.WriteLine("After: " + AddTaskControl.Count);
        }

        public static List<TaskControl> AddTaskControl { get; set; } = new List<TaskControl>();

        public async static Task MessageChannel(SocketCommandContext scc, SocketUser sgu, string lvlName, string levelNumber, string customMsg = null, ulong channelID = 0)
        {
            try
            {
                if (customMsg != null && channelID != 0)
                {
                    await scc.Guild.GetTextChannel(channelID).SendMessageAsync(customMsg);
                    return;
                }
                string puddingValue = (lvlName == "Paragraph Roleplay Level") ? "5000" : "2500";
                IUserMessage m = await scc.Channel.SendMessageAsync($"{ExtensionMethods.NameGetter(sgu, scc.Guild)} has reached {lvlName} {levelNumber} YAY! Pudding for everyone!! I'm gonna give you {puddingValue} pudding per level gained... *gulp* y-yes... {puddingValue}...");
                AddTaskControl.Add(new TaskControl(null, 25000, false));
                AddTaskControl[AddTaskControl.Count - 1].AddDeletion(m);
                //await Task.Delay(25000);
                //await m.DeleteAsync();
            }
            catch (Exception x)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, x, "Level Up Message Error");
            }
        }

        /// <summary>
        /// To allow users to pin messages without having moderator status. Work in progress.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Command("Pin Message")]
        [Alias("Pin", "pinmessage")]
        [Summary("Anyone can pin a message, but you need to enable developer mode, right click on message and choose (copy ID). Then type !nep pin message (id number).")]
        public async Task PinMessage([Remainder] string id = null)
        {
            ulong idn = ulong.Parse(id);
            try
            {
                var f = (RestUserMessage)Context.Channel.GetMessageAsync(idn).Result;
                await f.PinAsync();
            }
            //(RestUserMessage)
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

        private GuildData GetGuildId(ulong Id)
        {
            foreach (GuildData gd in Program.AllGuildData)
            {
                if (gd.GuildId == Id)
                    return gd;
            }
            return null;
        }

        /// <summary>
        /// Assigns the reader of roleplays role or removes it from the person calling this command.
        /// </summary>
        /// <returns></returns>
        [Command("read")]
        [Summary("Gives you or takes away your ability to see roleplay channels if you are not a roleplayer. You cannot interact with those channels, however.")]
        public async Task Read()
        {
            var g = Context.Guild.Roles.ToList();

            bool has = false;
            int find = 0;
            foreach (var z in Context.Guild.Roles)
            {
                if (z.Name.ToLower() == "reader of roleplays")
                {
                    break;
                }
                find++;
            }

            foreach (var p in Context.Guild.GetUser(Context.User.Id).Roles)
                if (p.Name.ToLower() == "reader of roleplays")
                    has = true;

            if (!has)
            {
                await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(g[find]);
                await Context.Channel.SendMessageAsync($"{Context.User.Username} you're all set to read roleplays! If you want to disable this, just type the command again!");
                return;
            }
            await Context.Guild.GetUser(Context.User.Id).RemoveRoleAsync(g[find]);
            await Context.Channel.SendMessageAsync($"{Context.User.Username} you will no longer see roleplay channels. Type the command again if you want to see them!");
        }

        public void SetTimer()
        {
            t = new Timer(5000);
            t.Elapsed += DoStuff;
            t.AutoReset = true;
            t.Enabled = true;
        }
        static StringBuilder sb = new StringBuilder();
        public async void DoStuff(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (sifter >= primaryUsers.Count)
            {
                sifter = 0;
                //await Context.User.SendMessageAsync(sb.ToString());
                t.Enabled = false;
                return;
            }
            //sb.Append(primaryUsers[sifter].Username).Append("\n");
            await primaryUsers[sifter].SendMessageAsync(messages);
            sifter++;
        }

    }
}
