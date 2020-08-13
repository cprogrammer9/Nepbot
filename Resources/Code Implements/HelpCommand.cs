using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Imaging;
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

namespace NepBot.Resources.Code_Implements
{
    /// <summary>
    /// Total embed length can equal a total of 6000 in length.
    /// Total field length can equal a total of 1024
    /// Total amount of fields can equal a total of 25
    /// </summary>
    public class HelpCommand
    {

        readonly SocketCommandContext scc;
        readonly CommandService database;

        public HelpCommand(SocketCommandContext scc, CommandService database)
        {
            this.scc = scc;
            this.database = database;
        }

        private bool OverflowExists(EmbedBuilder eb)
        {
            return eb.Length >= 5500 || eb.Fields.Count >= 25;
        }

        public List<EmbedBuilder> GrabCommandInfo(string moduleName)
        {
            List<EmbedBuilder> eb = new List<EmbedBuilder>();
            eb.Add(new EmbedBuilder());
            string swapped = SwapInputText(moduleName);
            ModuleInfo chosenModule = database.Modules.FirstOrDefault(x => x.Name.ToLower() == SwapInputText(moduleName));
            if (chosenModule == null)
            {
                return null;
            }
            int ebIndex = 0;
            eb[0].Color = Discord.Color.Purple;
            foreach (var commands in chosenModule.Commands)
            {
                if (OverflowExists(eb[ebIndex]))
                {
                    eb.Add(new EmbedBuilder());
                    ebIndex++;
                    eb[ebIndex].Color = Discord.Color.Purple;
                }
                eb[ebIndex].Description = "Type !nep (command name) followed by any additional rules. Read the summaries for details!";
                eb[ebIndex].AddField(commands.Name, (commands.Summary != null) ? commands.Summary : "needs summary", true);
            }
            return eb;
        }

        private string SwapInputText(string info)
        {
            info = info.ToLower();
            info = info.Replace("main", "ComplexCommands");
            info = info.Replace("games", "Games");
            info = info.Replace("simple", "SimpleCommands");
            info = info.Replace("server", "GuildCommands");
            info = info.Replace("roleplay", "RoleplayingCommands");

            if (info == "modadmin")
                return string.Empty;
            if (info == "mycommands")
                return string.Empty;
            return info.ToLower();
        }

        public EmbedBuilder BaseInformation()
        {
            EmbedBuilder ebb = new EmbedBuilder();
            ebb.Color = Discord.Color.Blue;
            ebb.Description = "Please select a help category for more information! Type !nep help (one of the options below)";
            //ebb.AddField("Please select a help category for more information!", "type !nep help (one of the options below)", false);
            ebb.AddField("Main", "Commands for making memes, check profiles and other stuff!", false);
            ebb.AddField("Games", "Games you can play with the bot such as Blackjack! (work in progress)", false);
            ebb.AddField("Simple", "Simple commands to just have fun with!", false);
            ebb.AddField("Roleplay", "Commands for things you can do with roleplaying on the server such as creating a character.", false);
            ebb.AddField("Server", "Commands for a few things you can do with the server. Not much here right now", false);
            return ebb;
        }
    }
}
