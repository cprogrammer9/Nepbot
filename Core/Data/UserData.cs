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
        private ulong _wordCountHigh = 0, _channelId = 0;
        [OptionalField]
        [NonSerialized]
        private ulong _wordCountGhost = 0;
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
        [OptionalField]
        public List<string> warningData = new List<string>();
        [OptionalField]
        public bool pingForInactivity = false;
        [OptionalField]
        [NonSerialized]
        public bool disablePingUntilBotRestart = false;

        public string ReturnWarnings
        {
            get
            {
                if (warningData == null || warningData.Count == 0)
                    warningData = new List<string>();
                StringBuilder sb = new StringBuilder();
                foreach (string p in warningData)
                {
                    sb.Append(p).Append("\n");
                }
                //var x = sb.ToString().CharacterLimit().ToList();
                return sb.ToString();
            }
        }

        public void IssueWarning(string warningInformation)
        {
            if (warningData == null || warningData.Count == 0)
                warningData = new List<string>();
            warningData.Add(warningInformation);
        }

        public void ResetChannelCharacters()
        {
            roleplayCharacterChannelUsage = new List<ChannelCharacter>()
            {
            new ChannelCharacter(472551756656672798, "Creative Roleplay"),
            new ChannelCharacter(472551769336184842, "Creative Roleplay"),
            new ChannelCharacter(472551782548242442, "Creative Roleplay"),
            new ChannelCharacter(643261156206575626, "Creative Roleplay"),
            new ChannelCharacter(472551737761202177, "Creative Roleplay"),
            new ChannelCharacter(643261181368205333, "Creative Roleplay"),
            new ChannelCharacter(697568823825531011, "Creative Roleplay"),
            new ChannelCharacter(123456789, "The Land of the Kingdoms")
            };
        }

        [Serializable]
        public class ChannelCharacter
        {
            //public CharacterData CD { get; set; } = new CharacterData();
            public int idNumber = -1;

            /// <summary>
            /// Grabs the character in the channel by its ID number.
            /// </summary>
            /// <param name="cdl"></param>
            /// <returns></returns>
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

        public bool SetNewWorldCount(ulong compare)
        {
            return WordCountRecord < compare;
        }

        public ulong WordCountRecordGhost
        {
            get { return _wordCountGhost; }
            set { _wordCountGhost = value; }
        }

        public ulong WordCountRecord
        {
            get { return _wordCountHigh; }
            set { _wordCountHigh = value; }
        }

        public ulong ChannelIdGhost
        {
            get { return _channelId; }
            set { _channelId = value; }
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
            if (warningData == null || warningData.Count == 0)
                warningData = new List<string>();
            if (roleplayCharacterChannelUsage == null)
                ResetChannelCharacters();
            paraGhost = ParaLevel;
            casualGhost = CasualLevel;
            nonGhost = NonLevel;
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
            }
            RefreshCardValueData();
            characterIDNumbers += characterDataList.Count;
        }

        private void RefreshCardValueData()
        {
            _c = 0;
            _r = 0;
            _sr = 0;
            _ssr = 0;
            foreach (int g in ownedCards)
            {
                if (g == -1)
                    continue;
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
            _sessionTimer = new Timer(new TimeSpan(0,1,0).TotalMilliseconds);
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
                RefreshCardValueData();
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
                Program.miscBotData.AdvanceDailyDay();
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
            if (warningData == null || warningData.Count == 0)
                warningData = new List<string>();
            if (roleplayCharacterChannelUsage == null)
                ResetChannelCharacters();
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

        [OptionalField]
        [NonSerialized]
        int casualGhost = 1;
        [OptionalField]
        [NonSerialized]
        int paraGhost = 1;
        [OptionalField]
        [NonSerialized]
        int nonGhost = 1;

        public string LevelupMessage(SocketCommandContext scc)
        {
            string msg = string.Empty;
            string GetUsername;
            if (nonGhost != NonLevel)
            {
                GetUsername = ExtensionMethods.GetUsersName(UserID.ToString(), scc);
                msg = $"{GetUsername} has reached Non-roleplay level {NonLevel} YAY! Pudding for everyone!! " +
                    $"I'm gonna give you *{2500 * (NonLevel - nonGhost)}* pudding per level gained... *gulp* y - yes... {2500 * (NonLevel - nonGhost)}...";
                nonGhost = NonLevel;
                return msg;
            }

            if (casualGhost != CasualLevel)
            {
                GetUsername = ExtensionMethods.GetUsersName(UserID.ToString(), scc);
                msg = $"{GetUsername} has reached Roleplaying level {CasualLevel} YAY! Pudding for everyone!! " +
                    $"I'm gonna give you *{2500 * (CasualLevel - casualGhost)}* pudding per level gained... *gulp* y - yes... {2500 * (CasualLevel - casualGhost)}...";
                if (paraGhost != ParaLevel)
                {
                    msg = string.Concat(msg, $"\nLiterate Roleplay leveled up to {ParaLevel} too! {10000 * (ParaLevel - paraGhost)} pudding gained!");
                    paraGhost = ParaLevel;
                }
                casualGhost = CasualLevel;
                return msg;
            }

            if (paraGhost != ParaLevel)
            {
                GetUsername = ExtensionMethods.GetUsersName(UserID.ToString(), scc);
                msg = $"{GetUsername} has reached Literate Roleplaying level {ParaLevel} YAY! Pudding for everyone!! " +
                    $"I'm gonna give you *{10000 * (ParaLevel - paraGhost)}* pudding per level gained... *gulp* y - yes... {10000 * (ParaLevel - paraGhost)}...";
                paraGhost = ParaLevel;
                return msg;
            }

            return msg;
        }

        /// <summary>
        /// Gain exp the proper way. It prints out 2 strings. One to adjust if para exp was earned and one normal to post to the exp channel.
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public string GainExp(SocketCommandContext scc, int wordCount, string characterExpMessage, bool rpExpBool)
        {
            try
            {
                if (!rpExpBool)
                {
                    nonGhost = NonLevel;
                    ulong p = (ulong)Math.Ceiling(25 + (25 * (OwnedCards * .01m)));
                    CurrentNonExp += p;
                    return string.Empty;
                }

                ulong gainParaExp = (wordCount >= 250) ? ParaRPExp(wordCount) : 0;
                ulong gainCasualExp = CasualRPExp(wordCount);

                casualGhost = CasualLevel;
                CurrentCasualExp += gainCasualExp;
                PuddingGainsPerPost(25 + (ulong)CasualLevel / 2);

                if (wordCount >= 250)
                {
                    PuddingGainsPerPost(100 + (ulong)ParaLevel * 10);
                    paraGhost = ParaLevel;
                    CurrentParaExp += gainParaExp;
                }

                string sb = $"**{ExtensionMethods.NameGetter(scc.User, scc.Guild)}** has gained __{gainCasualExp} exp__ from their reply in " +
                    $"__{scc.Channel.Name}__!{characterExpMessage} Total word count: __{wordCount}__" +
                    $"\n{gainParaExp} Literate Roleplay exp earned. (250 words or more to earn lit rp exp)";

                return sb;
            }
            catch (Exception i)
            {
                Console.WriteLine(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
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
            return string.Empty;
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
                //var tester = value;
                // if (tester.ToString().Length >= ulong.MaxValue.ToString().Length - 3)
                //     _currentParagraphExp = 0;
                // else
                _currentParagraphExp = value;
                while (_currentParagraphExp >= ReqExp(ParaLevel))
                {
                    _currentParagraphExp -= ReqExp(ParaLevel);
                    ParaLevel++;
                    Pudding += 10000;
                }
            }
        }

        public ulong CurrentNonExp
        {
            get { return _currentNonExp; }
            set
            {
                var tester = value;
                if (tester.ToString().Length >= ulong.MaxValue.ToString().Length - 3)
                    _currentNonExp = 0;
                else
                    _currentNonExp = value;
                TotalPosts++;
                Pudding += (ulong)(5 + NonLevel / 5);
                while (_currentNonExp >= ReqExp(NonLevel))
                {
                    _currentNonExp -= ReqExp(NonLevel);
                    NonLevel++;
                    Pudding += 2500;
                }
            }
        }

        public ulong CurrentCasualExp
        {

            get { return _currentCasualExp; }
            set
            {
                var tester = value;
                if (tester.ToString().Length >= ulong.MaxValue.ToString().Length - 3)
                    _currentCasualExp = 0;
                else
                    _currentCasualExp = value;
                while (_currentCasualExp >= ReqExp(CasualLevel))
                {
                    _currentCasualExp -= ReqExp(CasualLevel);
                    CasualLevel++;
                    Pudding += 2500;
                }
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

        public ulong ReqExp(int level)
        {
            return (ulong)level * 500;
        }
    }
}