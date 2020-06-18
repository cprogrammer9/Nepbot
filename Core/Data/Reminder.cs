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
using System.Timers;

namespace NepBot.Core.Data
{
    [Serializable]
    public class Reminder
    {
        readonly DateTime _reminder;
        public readonly string _message;
        public readonly ulong userID;
        readonly int notSet = -1;

        public bool NotSet
        {
            get { return notSet == -1; }
        }

        public bool Comparison()
        {
            return
            DateTime.Now.Minute == _reminder.Minute &&
            DateTime.Now.Hour == _reminder.Hour &&
            DateTime.Now.Day == _reminder.Day &&
            DateTime.Now.Month == _reminder.Month &&
            DateTime.Now.Year == _reminder.Year;
        }

        private int ReturnMonthNumber(string match)
        {
            string m = match.ToLower();
            string[] months = new string[] {
            "january",
            "february",
            "march",
            "april",
            "may",
            "june",
            "july",
            "august",
            "september",
            "october",
            "november",
            "december",
        };
            for (int i = 0; i < months.Length; i++)
            {
                if (months[i] == match)
                    return i;
            }
            return 0;
        }

        private int ReturnDayNumber(string match)
        {
            string m = match.ToLower();
            string[] days = new string[]{
            "",
            "monday",
            "tuesday",
            "wednesday",
            "thursday",
            "friday",
            "saturday",
            "sunday",
        };
            for (int i = 1; i < days.Length; i++)
            {
                if (days[i] == m)
                    return i;
            }
            return 1;
        }

        public Reminder(int month, int day, int year, int hour, int minute, ulong userId, string reminder)
        {
            if (day == -1)
                return;
            else
                notSet = 0;
            userID = userId;
            if (month == 0)
                month = DateTime.Now.Month;
            _message = reminder;
            _reminder = new DateTime(DateTime.Now.Year, month, day, hour, minute, DateTime.Now.Second);
        }

        public DateTime ReturnTime
        {
            get { return _reminder; }
        }
    }

}
