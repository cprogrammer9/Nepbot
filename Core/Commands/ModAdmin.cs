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
using static NepBot.Data.GuildData;

namespace NepBot.Core.Commands
{
    [RequireUserPermission(GuildPermission.BanMembers)]
    public class ModAdmin : ModuleBase<SocketCommandContext>
    {
        public static bool disableLeftServerMessage = false;

        [Command("Unmute")]

        public async Task Unmute([Remainder] string Input = null)
        {
            try
            {
                GuildData getMyGuild = Program.AllGuildData.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                Console.WriteLine(getMyGuild.GuildId);
                SocketGuildUser contUser = (ExtensionMethods.GetSocketUser(Input, Context, false) as SocketGuildUser);
                ulong user = contUser.Id;
                MutePeople getUser = new MutePeople();
                foreach (var g in getMyGuild.mutedPeople)
                {
                    if (g.userId == user)
                    {
                        getUser = g;
                        break;
                    }
                }
                Console.WriteLine(getMyGuild.mutedPeople.Count);
                var getGuilduser = Context.Guild.GetUser(user);
                for (int i = 0; i < getUser.roleIds.Length; i++)
                {
                    await getGuilduser.AddRoleAsync(Context.Guild.GetRole(getUser.roleIds[i]));
                }

                await Context.Channel.SendMessageAsync($"Unmuted {Input}");
            }
            catch (Exception i)
            {
                Console.WriteLine(string.Format("From Unmute>>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
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

        [Command("Mute")]

        public async Task Mute([Remainder] string Input = null)
        {
            try
            {
                if (Input == null)
                {
                    await Context.Channel.SendMessageAsync("it's !mute (name)&(number of hours to mute)");
                    return;
                }
                var data = ExtensionMethods.GenericSplit(Input, "&", "|");
                SocketGuildUser contUser = (ExtensionMethods.GetSocketUser(data[0], Context, false) as SocketGuildUser);
                ulong userName = contUser.Id;
                if (!int.TryParse(data[1], out int hours))
                {
                    await Context.Channel.SendMessageAsync("it's !mute (name)&(number of hours to mute). Please enter only a whole number for hours to mute");
                    return;
                }
                GuildData getMyGuild = Program.AllGuildData.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
                var curroles = Context.Guild.GetUser(userName).Roles.ToList();
                List<ulong> roles = new List<ulong>();

                for (int i = 0; i < curroles.Count; i++)
                {
                    if (curroles[i].Id == 472551343148761090)
                        continue;
                    roles.Add(curroles[i].Id);
                }

                for (int i = 0; i < curroles.Count; i++)
                {
                    if (curroles[i].Id == 472551343148761090)
                        continue;
                    await Context.Guild.GetUser(userName).RemoveRoleAsync(curroles[i]);
                }


                getMyGuild.mutedPeople.Add(new GuildData.MutePeople(DateTime.Now, DateTime.Now.AddHours(hours), userName, roles.ToArray()));

                await Context.Channel.SendMessageAsync($"user has been muted for {hours} hours.");
            }
            catch (Exception i)
            {
                Console.WriteLine(string.Format("From Mute>>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
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

        [Command("NukeRaid")]
        [Alias("Purge", "Nuke Raid", "PurgeRaid", "Purge Raid", "Anti Raid")]

        public async Task NukeRaid()
        {
            disableLeftServerMessage = true;
            var pp = Program.AllGuildData.First(x => x.GuildId == Context.Guild.Id);
            pp.enableAutoInviteDeletions = true;
            for (int p = 0; p < Program.newMemberList.Count; p++)
            {
                await Context.Guild.GetUser(Program.newMemberList[p].userID).BanAsync();
                Program.newMemberList.Remove(Program.newMemberList[p]);
            }
            for(int i = 0; i < Program.newMemberMessages.Count; i++)
            {
                await Program.newMemberMessages[i].DeleteAsync();
            }
            disableLeftServerMessage = false;
        }

        [Command("clean")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task CleanChat([Remainder] string Input = null)
        {
            try
            {
                int amt = 0;
                if (!int.TryParse(Input, out amt))
                    amt = 0;
                IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amt + 1).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                const int delay = 10000;
                IUserMessage m = await ReplyAsync($"Deleted {amt} message for ya... phew time for some pudding and a nap!");
                GuildCommands.AddTaskControl.Add(new Resources.TaskControl(null, delay));
                GuildCommands.AddTaskControl[GuildCommands.AddTaskControl.Count - 1].AddDeletion(m);
                //await m.DeleteAsync();
            }
            catch (Exception m)
            {
                await Context.Channel.SendMessageAsync($">>> {m.Message}\n{m.TargetSite}\n{m.Source}\n{m.InnerException}\n{m.StackTrace}\n{m.HResult}\n{m.Data}\n{m.HelpLink}");
            }
        }

        [Command("Issue Warning")]
        public async Task ModeratorWarningList([Remainder] string Input = null)
        {
            try
            {
                var data = ExtensionMethods.GenericSplit(Input, "&", "|");
                ulong p = ExtensionMethods.FindUserIdByName(data[0], Context);
                UserData ud = ExtensionMethods.FindPerson(p);
                ud.IssueWarning(data[1]);
                await Context.Channel.SendMessageAsync($"Warning issued for {Context.Guild.GetUser(p).Username}.\n{ud.warningData[ud.warningData.Count - 1]}\n Sad day for server...");
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

        [Command("Display Warnings")]
        public async Task DisplayWarningList([Remainder] string Input = null)
        {
            try
            {
                ulong p = ExtensionMethods.FindUserIdByName(Input, Context);
                UserData ud = ExtensionMethods.FindPerson(p);

                StringBuilder sb = new StringBuilder();
                sb.Append($"Total warnings issued for {Input}:\n");
                sb.Append(ud.ReturnWarnings);
                await Context.Channel.SendMessageAsync(sb.ToString());
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
    }
}
