using Discord.Commands;
using Discord.WebSocket;
using NepBot.Core.Commands;
using NepBot.Core.Data;
using NepBot.Resources.Database;
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
        [OptionalField]
        public DateTime UsedDailyPudding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        [OptionalField]
        public DateTime channelResetCounter = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        [OptionalField]
        [NonSerialized]
        public static ChannelCounter[] channelCounters = new ChannelCounter[]
        {
            new ChannelCounter("suggestions", 472557745069883402),
            new ChannelCounter("introduce yourself", 642586427250704424),
            new ChannelCounter("general", 474802733438861312),
            new ChannelCounter("general 2", 632370311198801921),
            new ChannelCounter("japanese talk", 630578449391681557),
            new ChannelCounter("emotional support", 695374494616911922),
            new ChannelCounter("gaming night", 694249823989268620),
            new ChannelCounter("free stuff", 694249870256635935),
            new ChannelCounter("bot spam", 559866304643727367),
            new ChannelCounter("fox and nekos", 583444802612494336),
            new ChannelCounter("images and videos", 472551865989464094),
            new ChannelCounter("memes", 472551853364871180),
            new ChannelCounter("fanfictions and art sharing", 565366019368026133),
            new ChannelCounter("music and stuff idk", 675341851196129313),
            new ChannelCounter("archive of lols and shame", 610949484280676538),
            new ChannelCounter("pointless roleplays", 642621866195943454),
            new ChannelCounter("characters", 472557467495170048),
            new ChannelCounter("rp requests", 594421141825781770),
            new ChannelCounter("request discussions", 608020590338637876),            
            new ChannelCounter("character creation 1", 726271454831968337),
            new ChannelCounter("character creation 2", 726271475065159721),
            new ChannelCounter("character creation 3", 726271494904217620),
            new ChannelCounter("channel 1 casual", 472551756656672798),
            new ChannelCounter("channel 2 casual", 472551762990071828),
            new ChannelCounter("channel 3 casual", 472551769336184842),
            new ChannelCounter("channel 4 casual", 472551775178850310),
            new ChannelCounter("channel 5 casual", 472551782548242442),
            new ChannelCounter("channel 6 casual", 643261156206575626),
            new ChannelCounter("channel 7 casual", 643261181368205333),
            new ChannelCounter("channel 8 casual", 697568823825531011),
            new ChannelCounter("ooc", 472570210180923412),
            new ChannelCounter("ooc", 645487693266157588),
            new ChannelCounter("first person casual roleplay", 558458110168137748),
            new ChannelCounter("channel 1 para", 472551695432286240),
            new ChannelCounter("channel 2 para", 472551726910668800),
            new ChannelCounter("channel 3 para", 472551715766403072),
            new ChannelCounter("channel 4 para", 472551737761202177),
            new ChannelCounter("channel 5 para", 472551745994620949),
            new ChannelCounter("channel 6 para", 643264985731956766),
            new ChannelCounter("channel 7 para", 643265020280569876),
            new ChannelCounter("ooc", 472570224088973384),
            new ChannelCounter("ooc2", 645487751713521664),
            new ChannelCounter("first-person-paragraph-roleplay", 558458080547962883),
            new ChannelCounter("dnd room 1", 574126117368365066),
            new ChannelCounter("dnd room 2", 574126158484865025),
            new ChannelCounter("dnd character sheets", 574137157485068298),
            new ChannelCounter("dnd ooc chat", 574474515854262272),
            new ChannelCounter("stream 1 chat", 611699820934987789),
            new ChannelCounter("stream 2 chat", 617529680714792960),
            new ChannelCounter("streamer announcements", 617530793862103046),
        };

        public bool SendActivityReport()
        {
            return DateTime.Now > channelResetCounter;
        }

        public void AdvanceDailyDay()
        {
            UsedDailyPudding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 0, 0, 0);
            channelResetCounter = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 0, 0, 0);
        }

        public string ListofNotes(ulong guildID, bool admin = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Notes g in channelNotes)
            {
                if (admin)
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
