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

namespace NepBot.Core.Commands
{
    public class ModAdmin : ModuleBase<SocketCommandContext>
    {
        [Command("clean")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task CleanChat([Remainder]string Input = null)
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
    }
}
