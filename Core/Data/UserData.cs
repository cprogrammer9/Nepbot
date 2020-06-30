using Discord.Commands;
using Discord.Rest;
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
        private Timer _sessionTimer = new Timer();
        [OptionalField]
        [NonSerialized]
        public List<CardTypes> onlyOwned = new List<CardTypes>();
        [OptionalField]
        public int dCountAmt = 0;
        [OptionalField]
        public int bCountAmt = 0;
        [NonSerialized]
        private int _c, _r, _sr, _ssr;
        [OptionalField]
        [NonSerialized]
        public int siftThroughCreation = 0;
        [OptionalField]
        [NonSerialized]
        Timer _characterCreation = null;
        [OptionalField]
        public List<ChannelCharacter> roleplayCharacterChannelUsage = null;
        [OptionalField]
        public static int characterIDNumbers = 0;
        [OptionalField]
        public List<CharacterData> characterDataList = new List<CharacterData>();


        [Serializable]
        public class ChannelCharacter
        {
            //public CharacterData CD { get; set; } = new CharacterData();
            public int idNumber = -1;

            public CharacterData CD(List<CharacterData> cdl)
            {
                foreach (var p in cdl)
                    if (p.ReturnIDNumber == idNumber)
                        return p;
                return null;
            }
            public ulong ChannelId { get; }
            public string CategoryName { get; }

            public ChannelCharacter(ulong channel, string categoryName)
            {
                ChannelId = channel;
                CategoryName = categoryName;
            }
        }

        public ulong RPCharacterExp(int wordCount)
        {
            decimal multiplier = 0;
            switch (wordCount)
            {
                case 250:
                    multiplier = .25m;
                    break;
                case 500:
                    multiplier = .5m;
                    break;
                case 750:
                    multiplier = .75m;
                    break;
                case 1000:
                    multiplier = 1m;
                    break;
            }
            ulong levels = (ulong)(OwnedCards + ParaLevel + CasualLevel);
            ulong total = 100 + levels * 2;
            return (ulong)(total + total * multiplier);
        }

        public string AddChannel(SocketGuild sg, string channelName, CharacterData characterToAdd)
        {
            ChannelCharacter cc = null;
            //Find the channel by name
            foreach (var channelId in sg.Channels)
            {
                if (channelId.Name.ToLower() == channelName.ToLower())
                {
                    // See if that channel ID is in the list
                    foreach (var g in roleplayCharacterChannelUsage)
                    {
                        if (channelId.Id == g.ChannelId)
                        {
                            cc = g;
                            goto LoopEnd;
                        }
                    }
                }
            }
        LoopEnd:;
            if (cc == null)
                return "Channel either not found or you chose an invalid channel. Type the channel as it appears on the server. Only use casual or para non-ooc channels.";
            if (cc.CD(characterDataList).NameOfCharacter.ToLower() != characterToAdd.NameOfCharacter.ToLower())
            {
                //cc.CD(characterDataList) = characterToAdd;
                cc.idNumber = characterToAdd.ReturnIDNumber;
                return $"Your character {cc.CD(characterDataList).NameOfCharacter} has been swapped out with {characterToAdd.NameOfCharacter}";
            }
            cc.idNumber = characterToAdd.ReturnIDNumber;
            return $"Your character {cc.CD(characterDataList).NameOfCharacter} has been added to the channel {channelName}. Only one character can exist in a channel at a time. You will only get experience in that channel for this character.";
        }

        public ulong CasualRPExp(int wordCount)
        {
            return 250 + (ulong)wordCount;
        }

        public ulong ParaRPExp(int wordCount)
        {

            return 500 + (ulong)wordCount;
        }

        public void DisableCharacterCreation()
        {
            _characterCreation.Enabled = false;
            _characterCreation = null;
            for (int i = 0; i < Program.characterCreationSessions.Count; i++)
            {
                if (Program.characterCreationSessions[i].ud.UserID == UserID)
                {
                    Program.characterCreationSessions.RemoveAt(i);
                    break;
                }
            }
        }

        public void StartCharacterCreationSession()
        {
            if (_characterCreation == null)
                _characterCreation = new Timer(1800000 * 2);
            _characterCreation.Elapsed += new ElapsedEventHandler(SessionForCharacterCreation);
            _characterCreation.AutoReset = false;
            _characterCreation.Enabled = true;
        }

        private void SessionForCharacterCreation(Object source, ElapsedEventArgs e)
        {
            _characterCreation.Enabled = false;
            _characterCreation = null;
        }

        public CharacterData FindCharacter(string name)
        {
            name = name.ToLower();
            foreach (var p in characterDataList)
            {
                if (p.NameOfCharacter.ToLower() == name)
                    return p;
            }
            return null;
        }

        public string ReturnDateTime
        {
            get { return Program.miscBotData.UsedDailyPudding.ToString(); }
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
            if (roleplayCharacterChannelUsage == null)
                roleplayCharacterChannelUsage = new List<ChannelCharacter>()
        {
            //Casual Channels
            new ChannelCharacter(472551756656672798, "Casual Roleplay"),
            new ChannelCharacter(472551762990071828, "Casual Roleplay"),
            new ChannelCharacter(472551769336184842, "Casual Roleplay"),
            new ChannelCharacter(472551775178850310, "Casual Roleplay"),
            new ChannelCharacter(472551782548242442, "Casual Roleplay"),
            new ChannelCharacter(643261156206575626, "Casual Roleplay"),
            new ChannelCharacter(643261181368205333, "Casual Roleplay"),
            new ChannelCharacter(697568823825531011, "Casual Roleplay"),
            new ChannelCharacter(558458110168137748, "Casual Roleplay"),
            //Para Channels
            new ChannelCharacter(472551695432286240, "Paragraph Roleplay"),
            new ChannelCharacter(472551726910668800, "Paragraph Roleplay"),
            new ChannelCharacter(472551715766403072, "Paragraph Roleplay"),
            new ChannelCharacter(472551737761202177, "Paragraph Roleplay"),
            new ChannelCharacter(472551745994620949, "Paragraph Roleplay"),
            new ChannelCharacter(643264985731956766, "Paragraph Roleplay"),
            new ChannelCharacter(643265020280569876, "Paragraph Roleplay"),
            new ChannelCharacter(558458080547962883, "Paragraph Roleplay")
        };
            if (characterDataList == null)
                characterDataList = new List<CharacterData>();
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
            characterIDNumbers += characterDataList.Count;
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
        }

        // For card sessions
        private void EndSession(Object source, ElapsedEventArgs e)
        {
            _sessionTimer.Enabled = false;
            _sessionTimer.AutoReset = false;
            _sessionTimer = null;
            for (int i = 0; i < Program.cardSessions.Count; i++)
            {
                if (Program.cardSessions[i].ud.UserID == UserID)
                {
                    Program.cardSessions.RemoveAt(i);
                    break;
                }
            }
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
                Console.WriteLine("Start Reminder Timer Returned for null?");
                return;
            }
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(ReminderTimer);
            timer.AutoReset = true;
            timer.Enabled = true;
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
            if (DateTime.Now.Day != Program.miscBotData.UsedDailyPudding.Day)
            {
                ResetDailies();
                Program.miscBotData.UsedDailyPudding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
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
            if (roleplayCharacterChannelUsage == null)
                roleplayCharacterChannelUsage = new List<ChannelCharacter>()
        {
            //Casual Channels
            new ChannelCharacter(472551756656672798, "Casual Roleplay"),
            new ChannelCharacter(472551762990071828, "Casual Roleplay"),
            new ChannelCharacter(472551769336184842, "Casual Roleplay"),
            new ChannelCharacter(472551775178850310, "Casual Roleplay"),
            new ChannelCharacter(472551782548242442, "Casual Roleplay"),
            new ChannelCharacter(643261156206575626, "Casual Roleplay"),
            new ChannelCharacter(643261181368205333, "Casual Roleplay"),
            new ChannelCharacter(697568823825531011, "Casual Roleplay"),
            new ChannelCharacter(558458110168137748, "Casual Roleplay"),
            //Para Channels
            new ChannelCharacter(472551695432286240, "Paragraph Roleplay"),
            new ChannelCharacter(472551726910668800, "Paragraph Roleplay"),
            new ChannelCharacter(472551715766403072, "Paragraph Roleplay"),
            new ChannelCharacter(472551737761202177, "Paragraph Roleplay"),
            new ChannelCharacter(472551745994620949, "Paragraph Roleplay"),
            new ChannelCharacter(643264985731956766, "Paragraph Roleplay"),
            new ChannelCharacter(643265020280569876, "Paragraph Roleplay"),
            new ChannelCharacter(558458080547962883, "Paragraph Roleplay")
        };
            if (characterDataList == null)
                characterDataList = new List<CharacterData>();
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

        public string GainExp(string words)
        {
            string sb = string.Empty;
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            int wordCount = words.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;

            if(wordCount >= 250)
            {
                PuddingGainsPerPost(100 + (ulong)ParaLevel * 10);
            }

            return sb;
        }

        private void PuddingGainsPerPost(ulong val)
        {
            Pudding += val;
        }

        public ulong CurrentParaExp
        {
            get { return _currentParagraphExp; }
            set
            {
                decimal p = _currentParagraphExp;
                p = value;
                if (p < 0)
                {
                    Console.WriteLine($"Current Para Exp became negative. Check your math. Value: {value}, Current Para Exp: {_currentParagraphExp}");
                    p = 0;
                }
                _currentParagraphExp = (ulong)p;
                //TotalPosts++;
                Pudding += (ulong)(100 + ParaLevel * 10);
                while (_currentParagraphExp >= ReqExp(ParaLevel))
                {
                    ulong ce = _currentParagraphExp - ReqExp(ParaLevel);
                    _currentParagraphExp -= ReqExp(ParaLevel);
                    ParaLevel++;
                    CurrentParaExp += ce;
                    Pudding += 2500;
                }
                CurrentParaExp += _currentParagraphExp;
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
                TotalPosts++;
                Pudding += (ulong)(5 + NonLevel / 5);
                while (_currentNonExp >= ReqExp(NonLevel))
                {
                    ulong ce = _currentNonExp - ReqExp(NonLevel);
                    _currentNonExp -= ReqExp(NonLevel);
                    NonLevel++;
                    CurrentNonExp += ce;
                    Pudding += 2500;
                }
                CurrentNonExp += _currentNonExp;
                //TotalPosts++;
            }
        }

        public ulong CurrentCasualExp
        {
            get { return _currentCasualExp; }
            set
            {
                _currentCasualExp = value;
                if (_currentCasualExp < 0)
                    _currentCasualExp = 0;
                TotalPosts++;
                Pudding += (ulong)(25 + CasualLevel / 2);
                while (_currentCasualExp >= ReqExp(CasualLevel))
                {
                    ulong ce = _currentCasualExp - ReqExp(CasualLevel);// level * 500
                    _currentCasualExp -= ReqExp(CasualLevel);
                    CasualLevel++;
                    CurrentCasualExp += ce;
                    Pudding += 2500;
                }
                CurrentCasualExp += _currentCasualExp;
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