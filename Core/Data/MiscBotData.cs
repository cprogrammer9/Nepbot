using Discord.Commands;
using Discord.WebSocket;
using NepBot.Core.Commands;
using NepBot.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NepBot.Core.Data
{
    [Serializable]
    public class MiscBotData
    {
        [OptionalField]
        public List<Notes> channelNotes = new List<Notes>();
        [OptionalField]
        public ulong totalPudding = 0;
        [OptionalField]
        public ulong totalEggplant = 0;

        public string ListofNotes(ulong guildID, bool admin = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Notes g in channelNotes)
            {
                if(admin)
                {
                    sb.Append(g.noteName).Append("\n");
                    continue;
                }
                if (guildID == g.guildId)
                    sb.Append(g.noteName).Append("\n");
            }
            return sb.ToString();
        }

        public void AddToNotes(string name, string msg, ulong gID)
        {
            for (int i = 0; i < channelNotes.Count; i++)
            {
                if (channelNotes[i].noteName.ToLower() == name.ToLower())
                {
                    channelNotes[i].AddToNotes(msg);
                    if (channelNotes[i].guildId == 0)
                        channelNotes[i].guildId = gID;
                    return;
                }
            }
            channelNotes.Add(new Notes(name, gID));
            channelNotes[channelNotes.Count - 1].AddToNotes(msg);
        }

        public string GetNotes(string name)
        {
            for (int i = 0; i < channelNotes.Count; i++)
            {
                if (channelNotes[i].noteName.ToLower() == name.ToLower())
                {
                    return channelNotes[i].ShowNotes();
                }
            }
            return string.Empty;
        }

    }

    [Serializable]
    public class Notes
    {
        public readonly string noteName;
        StringBuilder allNotes = new StringBuilder();
        [OptionalField]
        public ulong guildId = 0;

        public string ShowNotes()
        {
            return allNotes.ToString();
        }

        public void AddToNotes(string no)
        {
            if (allNotes.ToString().Length + no.Length > 1950)
            {
                allNotes.Append("|*|");
            }
            allNotes.Append("- ").Append(no).Append("\n");
        }

        public Notes(string noteName, ulong guildId)
        {
            this.noteName = noteName;
            this.guildId = guildId;
            allNotes.Append($"Here are the notes for {noteName}:\n");
        }
    }
}
