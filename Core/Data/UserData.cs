﻿using Discord.Commands;
using Discord.WebSocket;
using NepBot.Core.Commands;
using NepBot.Core.Data;
using NepBot.Resources.Database;
using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NepBot.Data
{
    [Serializable]
    public class UserData
    {
        private int _casualLevel = 1;
        private int _paraLevel = 1;
        private int _NonLevel = 1;
        private ulong _currentCasualExp = 0;
        private ulong _currentParagraphExp = 0;
        private ulong _currentNonExp = 0;
        [OptionalField]
        private ulong _pudding = 0;
        [OptionalField]
        public static DateTime UsedDailyPudding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        [OptionalField]
        public bool usedDaily = false;
        [OptionalField]
        private Reminder _reminder = new Reminder(0, -1, 0, 0, 0, 0, "");
        [OptionalField]
        private DateTime _lastPosted = DateTime.Now;
        [OptionalField]
        [NonSerialized]
        private Timer timer = new Timer();
        [OptionalField]
        private int[] ownedCards = null;
        [OptionalField]
        [NonSerialized]
        private Timer _sessionTimer = null;
        [OptionalField]
        [NonSerialized]
        public bool sessionOn = false;
        [OptionalField]
        [NonSerialized]
        public List<CardTypes> onlyOwned = new List<CardTypes>();
        [OptionalField]
        public int dCountAmt = 0;
        [OptionalField]
        public int bCountAmt = 0;
        [NonSerialized]
        private int _c, _r, _sr, _ssr;

        public string ReturnDateTime
        {
            get { return UsedDailyPudding.ToString(); }
        }

        public int CardValueGains
        {
            get { return _c + _r + _sr + _ssr; }
        }

        public int OwnedCards
        {
            get { return onlyOwned.Count; }
        }

        /// <summary>
        /// This is a hack due to some weird null issues with the nonserialized fields.
        /// </summary>
        public void SetValues()
        {
            sessionOn = false;
            onlyOwned = new List<CardTypes>();
            if (ownedCards == null || ownedCards.Length == 0)
            {
                ownedCards = Enumerable.Repeat(-1, 500).ToArray();
                return;
            }

            foreach (int g in ownedCards)
            {
                if (g == -1)
                    continue;
                onlyOwned.Add(CharacterCards.allCards[g]);
                if (CharacterCards.allCards[g]._cardType == CharacterCards.Type.c)
                    _c++;
                else if (CharacterCards.allCards[g]._cardType == CharacterCards.Type.r)
                    _r++;
                else if (CharacterCards.allCards[g]._cardType == CharacterCards.Type.sr)
                    _sr += 2;
                else if (CharacterCards.allCards[g]._cardType == CharacterCards.Type.ssr)
                    _ssr += 3;
            }
        }

        public string CardNameCall(string comparison)
        {
            foreach (CardTypes ct in onlyOwned)
            {
                if (ct.name.ToLower() == comparison.ToLower())
                    return ct.imgurID;
            }
            return string.Empty;
        }

        public void StartSession()
        {
            _sessionTimer = new Timer(60000);
            _sessionTimer.Elapsed += new ElapsedEventHandler(EndSession);
            _sessionTimer.AutoReset = true;
            _sessionTimer.Enabled = true;
            sessionOn = true;
        }

        private void EndSession(Object source, ElapsedEventArgs e)
        {
            _sessionTimer.Enabled = false;
            _sessionTimer.AutoReset = false;
            _sessionTimer = null;
            sessionOn = false;
        }

        public List<CardTypes> PopulateList()
        {
            List<CardTypes> op = new List<CardTypes>();
            foreach (CardTypes g in onlyOwned)
            {
                foreach (CardTypes ct in CharacterCards.allCards)
                {
                    if (g.idNumber == ct.idNumber)
                    {
                        op.Add(ct);
                        break;
                    }
                }
            }
            return op;
        }

        public bool AddToList(int num)
        {
            if (ownedCards.Length == 0)
                SetValues();
            try
            {
                if (ownedCards[num] == num)
                    return false;
                ownedCards[num] = num;
                onlyOwned.Add(CharacterCards.allCards[num]);
                return true;
            }
            catch (Exception n)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, n, $"OC Length: {ownedCards.Length} || num: {num}Add To List UserData");
            }
            return false;
        }

        public DateTime ReturnLastPosted
        {
            get { return _lastPosted; }
            set { _lastPosted = value; }
        }

        public DateTime ReturnReminderDate
        {
            get { return _reminder.ReturnTime; }
        }

        public void SetReminder(int month, int day, int year, int hour, int minute, ulong userId, string reminder)
        {
            _reminder = new Reminder(month, day, year, hour, minute, UserID, reminder);
            StartReminderTimer();
        }

        private void StartReminderTimer()
        {
            if (_reminder.NotSet)
            {
                Console.WriteLine("Returned for null?");
                return;
            }
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(ReminderTimer);
            timer.AutoReset = true;
            timer.Enabled = true;
            Console.WriteLine("Timer Set?");
        }

        private async void ReminderTimer(Object source, ElapsedEventArgs e)
        {
            if (!_reminder.Comparison())
                return;
            timer.Enabled = false;
            //await HelloWorld.MessageUserReminder(UserID, _reminder);
        }

        public void ResetDailies()
        {
            for (int i = 0; i < Program.ExpPoints.Count; i++)
            {
                Program.ExpPoints[i].usedDaily = false;
            }
        }

        public bool UsedDailyPudd()
        {
            if (DateTime.Now.Day != UsedDailyPudding.Day)
            {
                ResetDailies();
                UsedDailyPudding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }
            if (!usedDaily)
            {
                usedDaily = true;
                return false;
            }
            return usedDaily;
        }

        public bool CanBuy(ulong val)
        {
            return _pudding >= val;
        }

        public ulong Pudding
        {
            get { return _pudding; }
            set
            {
                if (value > 0)
                {
                    _pudding = value;
                    return;
                }
                double d = _pudding;
                _pudding = (d - value < 0) ? 0 : value;
                if (_pudding >= ulong.MaxValue)
                    _pudding = ulong.MaxValue;
            }
        }

        public ulong TotalPosts { get; set; } = 0;

        public UserData(ulong _userID)
        {
            this.UserID = _userID;
        }

        public void SwapID(ulong newID)
        {
            UserID = newID;
        }

        public ulong UserID { get; set; }


        public int ParaLevel
        {
            get { return _paraLevel; }
            set
            {
                _paraLevel = value;
                if (_paraLevel > 100)
                    _paraLevel = 100;
                if (_paraLevel < 1)
                    _paraLevel = 1;
            }
        }

        public int NonLevel
        {
            get { return _NonLevel; }
            set
            {
                _NonLevel = value;
                if (_NonLevel > 100)
                    _NonLevel = 100;
                if (_NonLevel < 1)
                    _NonLevel = 1;
            }
        }

        public int CasualLevel
        {
            get { return _casualLevel; }
            set
            {
                _casualLevel = value;
                if (_casualLevel > 100)
                    _casualLevel = 100;
                if (_casualLevel < 1)
                    _casualLevel = 1;
            }
        }

        public ulong CurrentParaExp
        {
            get { return _currentParagraphExp; }
            set
            {
                _currentParagraphExp = value;
                if (_currentParagraphExp < 0)
                    _currentParagraphExp = 0;
                if (_currentParagraphExp >= ReqExp(ParaLevel))
                {
                    ulong ce = _currentParagraphExp - ReqExp(ParaLevel);
                    _currentParagraphExp = 0;
                    ParaLevel++;
                    CurrentParaExp += ce;
                }
                //TotalPosts++;
            }
        }

        public ulong CurrentNonExp
        {
            get { return _currentNonExp; }
            set
            {
                _currentNonExp = value;
                if (_currentNonExp < 0)
                    _currentNonExp = 0;
                if (_currentNonExp >= ReqExp(NonLevel))
                {
                    ulong ce = _currentNonExp - ReqExp(NonLevel);
                    _currentNonExp = 0;
                    NonLevel++;
                    CurrentNonExp += ce;
                }
                //TotalPosts++;
            }
        }

        /// <summary>
        /// casualrp and pararp are the spelling for the switch
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="reqLevel"></param>
        /// <returns></returns>
        public bool CheckLevels(string roleName, int reqLevel)
        {
            switch (roleName.ToLower())
            {
                case "casualrp":
                    if (CasualLevel >= reqLevel)
                        return true;
                    break;
                case "pararp":
                    if (ParaLevel >= reqLevel)
                        return true;
                    break;
            }
            return false;
        }

        public ulong CurrentCasualExp
        {
            get { return _currentCasualExp; }
            set
            {
                _currentCasualExp = value;
                if (_currentCasualExp < 0)
                    _currentCasualExp = 0;
                if (_currentCasualExp >= ReqExp(CasualLevel))
                {
                    ulong ce = _currentCasualExp - ReqExp(CasualLevel);
                    _currentCasualExp = 0;
                    CasualLevel++;
                    CurrentCasualExp += ce;
                }
                //TotalPosts++;
            }
        }

        public enum TypesOfExp { Casual, Paragraph, Non }
        public void GainExp(TypesOfExp typesOfExp, ulong amt)
        {
            switch (typesOfExp)
            {
                case TypesOfExp.Casual:
                    CurrentCasualExp += amt;
                    break;
                case TypesOfExp.Paragraph:
                    CurrentParaExp += amt;
                    break;
                case TypesOfExp.Non:
                    CurrentNonExp += amt;
                    break;
            }
        }

        public ulong ReqExp(int level)
        {
            return (ulong)level * 500;
        }
    }
}