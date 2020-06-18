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
using System.Timers;
using NepBot.Resources.Games;
using NepBot.Resources.Code_Implements;
using NepBot.Resources.Extensions;
using NepBot.Core.Data;
using Image = System.DrawingCore.Image;

namespace NepBot.Core
{
    public class UserStuff
    {
        public List<SocketGuildUser> previousList = new List<SocketGuildUser>();
        private int userNumberSnapshot = 0;
        private Timer _userCheckTimer;
        private SocketGuild sg;
        private List<DataClass> names = new List<DataClass>();
        public List<RPParticipants> rpParticipants = new List<RPParticipants>();
        public ulong channelID;
        

        /// <summary>
        /// Sets the data for the bot's "User has left the server" feature.
        /// </summary>
        /// <param name="sg"></param>
        /// <param name="channelID"></param>
        public void SetData(SocketGuild sg, ulong channelID)
        {
            this.sg = sg;
            this.channelID = channelID;
            userNumberSnapshot = sg.Users.Count;
            ExtensionMethods.WriteToLog(ExtensionMethods.LogType.GenericMessage, null, $"{userNumberSnapshot} SNAPSHOT.");
            previousList = sg.Users.ToList();
            SetDataClass();
            _userCheckTimer = new Timer(5000);
            _userCheckTimer.Elapsed += new ElapsedEventHandler(EndSession);
            _userCheckTimer.AutoReset = true;
            _userCheckTimer.Enabled = true;
        }

        /// <summary>
        /// Repopulates the list of users in the server after there has been a difference reported in the number of users.
        /// </summary>
        private void SetDataClass()
        {
            names.Clear();
            foreach (SocketGuildUser x in previousList)
            {
                names.Add(new DataClass(x.Username, x.Id));
            }
        }

        /// <summary>
        /// Announces who left the server.
        /// </summary>
        /// <returns></returns>
        private string Report()
        {
            List<SocketGuildUser> currentList = sg.Users.ToList();
            StringBuilder leavers = new StringBuilder();
            ulong userID = 0;
            for (int i = 0; i < names.Count; i++)
            {
                ulong id = names[i].userID; // gets the user ID in Discord
                if (id == 508129645854588928) // ignores if it's the bot.
                    continue;
                for (int x = 0; x < currentList.Count; x++)
                {
                    userID = id;
                    if (currentList[x].Id == id) // if the id of the snapshop of the users in the server is equal to the id of the current server user in the server
                    {
                        userID = 0;
                        break;
                    }
                }
                if(userID != 0)
                {
                    return leavers.Append(names[i].username).Append(" has left the server!\n").ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks if user count in the server is still the same. If it's less, it will report who is missing (left the server). If it is higher, it will adjust the count and not report anything.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void EndSession(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (userNumberSnapshot == sg.Users.Count)
                {
                    return;
                }
                if (sg.Users.Count > userNumberSnapshot)
                {
                    userNumberSnapshot = sg.Users.Count;
                    previousList = sg.Users.ToList();
                    SetDataClass();
                    return;
                }
                string f = Report();
                if (f == string.Empty)
                    return;                
                Program.SendMessage(f.ToString(), channelID, sg);
                userNumberSnapshot = sg.Users.Count;
                previousList = sg.Users.ToList();
                SetDataClass();
            }
            catch (Exception n)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, n, "from userstuff");
            }
        }

        /// <summary>
        /// For use in paragraph roleplay to create a turn order.
        /// </summary>
        /// <param name="rpName"></param>
        /// <param name="sgu"></param>
        public void CreateRPParticipants(ulong rpName, List<ulong> sgu)
        {
            rpParticipants.Add(new RPParticipants(sgu, rpName));
        }

        private class DataClass
        {
            public ulong userID;
            public string username;

            public DataClass(string username, ulong userID)
            {
                this.userID = userID;
                this.username = username;
            }
        }
    }
}

[Serializable]
public class RPParticipants
{
    public List<ulong> sgu;
    public readonly ulong rpChannel;

    public void AddToList(ulong s)
    {
        sgu.Add(s);
    }

    public RPParticipants(List<ulong> sgus, ulong rpChannels)
    {
        sgu = sgus;
        rpChannel = rpChannels;
    }
}