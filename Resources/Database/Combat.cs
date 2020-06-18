using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NepBot.Resources.Code_Implements;
using NepBot.Core.Commands;
using System.Reflection;
using System.IO;
using Discord.Commands;
using Discord;
using NepBot.Resources.Extensions;
using NepBot.Data;
using System.Threading;

namespace NepBot.Resources.Database
{
    [Serializable]
    public class Combat
    {
        private CreateEntity _enemy, _player;
        private readonly string _discordName;
        MathCalculations _math = new MathCalculations();
        public int turnNumber = 1;
        public bool inCombat = true;
        public int multipleAttack { get; private set; } = 0;

        public string PlayerName
        {
            get { return _discordName; }
        }

        public CreateEntity ReturnPlayer
        {
            get { return _player; }
            set { _player = value; }
        }

        public CreateEntity ReturnEnemy
        {
            get { return _enemy; }
            set { _enemy = value; }
        }

        public bool PlayerGoesFirst
        {
            get { return _enemy.AGI.Current <= _player.AGI.Current; }
        }

        private bool PlayerWins
        {
            get { return _enemy.HP.Current == 0; }
        }

        private bool PlayerLoses
        {
            get { return _player.HP.Current == 0; }
        }

        SocketCommandContext p;
        UserData _userData;

        public Combat(CreateEntity enemy, CreateEntity player, string discordName, SocketCommandContext pal, UserData userData, int multipleAttacks = 0)
        {
            _userData = userData;
            multipleAttack = (multipleAttacks > 1) ? multipleAttacks : 0;
            _enemy = enemy;
            _enemy.StartedCombat(this);
            _player = player;
            _player.StartedCombat(this);
            _discordName = discordName;
            p = pal;
            multipleAttackSnapshot = multipleAttacks;
            _player.HP.Current = _player.HP.Max;
            Thread t = new Thread(InitiateCombat);
            t.Start();
            //InitiateCombat();
        }

        public string ReportLog
        {
            get { return log.ToString(); }
        }

        private StringBuilder log = new StringBuilder();
        bool toggleLog = true;

        public void AddToLog(string details)
        {
            if (toggleLog && turnNumber % 2 == 0)
                log.Append("\n").Append(details)
                    .Append("\nPlayer HP: ")
                    .Append(_player.HP.Current)
                    .Append(" / ")
                    .Append(_player.HP.Max)
                    .Append($" <-> MP: {_player.MP.Current} / {_player.MP.Max}")
                    .Append("\nEnemy HP: ")
                    .Append(_enemy.HP.Current)
                    .Append(" / ")
                    .Append(_enemy.HP.Max)
                    .Append($" <-> MP: {_enemy.MP.Current} / {_enemy.MP.Max}");
            else if (toggleLog)
            {
                log.Append("\n").Append(details);
            }
        }

        int turnSwitch = -1;

        private void DPSLoop()
        {
            while (true)
            {
                if (_player.HP.Current <= 0)
                    break;
                else if (_enemy.HP.Current <= 0)
                    break;
                switch (turnSwitch)
                {
                    case (-1):
                        if (PlayerGoesFirst)
                        {
                            _player.DoTs();
                            _player.RandomAttack();
                            _player.ReduceDuration();
                            turnSwitch = 1;
                        }
                        else
                        {
                            _enemy.DoTs();
                            _enemy.AIAttack();
                            _enemy.ReduceDuration();
                            turnSwitch = 0;
                        }
                        break;
                    case 0:
                        _player.DoTs();
                        if (_player.stunDuration > 0)
                        {
                            _player.ReduceDuration();
                            goto case (1);
                        }
                        _player.RandomAttack();
                        turnSwitch = 1;
                        break;
                    case 1:
                        _enemy.DoTs();
                        if (_enemy.stunDuration > 0)
                        {
                            _enemy.ReduceDuration();
                            goto case (0);
                        }
                        _enemy.AIAttack();

                        turnSwitch = 0;
                        break;
                }
                turnNumber++;
            }
        }

        private async void InitiateCombat()
        {
            if (multipleAttack == 0)
            {
                DPSLoop();
                tempExp += _enemy.ExpValue;
                tempJobExp += _enemy.AIJobExpValue;
                await EndCombat();
                return;
            }
            toggleLog = false;
            for (int i = 0; i < multipleAttack; i++)
            {
                if (i + 1 == multipleAttack)
                    toggleLog = true;
                _enemy.HP.Current = _enemy.HP.Max;
                _enemy.SetDurationsToZero();
                _enemy.SetTemporaryToZero();
                DPSLoop();
                if (_player.HP.Current <= 0)
                    break;
                tempExp += _enemy.ExpValue;
                tempJobExp += _enemy.AIJobExpValue;
                multipleAttackWins++;
            }
            await EndCombat();
        }

        public string afterLogPlayer = string.Empty;
        int tempExp = 0;
        int tempJobExp = 0;
        int multipleAttackWins = 0;
        int multipleAttackSnapshot = 0;

        public async Task EndCombat()
        {
            Console.WriteLine(_player.HP.Current);
            bool playerLoses = _player.HP.Current <= 0;
            inCombat = false;

            if (PlayerWins)
            {
                _player.GainExp(tempExp);
                _player.CurrentJob.GainExp(tempJobExp);
                _userData.Pudding += (ulong)_enemy.MonsterPuddingGain(_player.Level);
            }
            _player.EndCombat();
            _enemy.EndCombat();
            if (playerLoses)
            {
                if (multipleAttackSnapshot == 0)
                    multipleAttackSnapshot = 1;
                log.Append($"\nThe {_enemy.ReturnAIName} smacked the crap out of you! Better luck next time!\n You attacked {multipleAttackSnapshot} times but failed to win all battles! You won {multipleAttackWins} out of {multipleAttackSnapshot} battles and failed to gain {tempExp} exp and {tempJobExp} job exp. Level up and try again, or attack less times.");
                await p.User.SendMessageAsync("Here are your combat results:\n" + ReportLog);
                return;
            }
            else
                await p.User.SendMessageAsync($"Here are your combat results. If you did multiple battles. It will only display the last battle due to the character limit:\n{ReportLog}");
            if (multipleAttackSnapshot == 0)
                multipleAttackSnapshot = 1;
            log = new StringBuilder();
            log.Append(afterLogPlayer);
            log.Append(($"\nFinal Fight in Battle Chain Result:\nYou gain {tempExp} exp and {tempJobExp} job exp!\nYou need {_player.LevelExp - _player.EXP} more exp to level up and {_player.CurrentJob.LevelExp - _player.CurrentJob.EXP} to job level up!\nYou gained {_enemy.MonsterPuddingGain(_player.Level) * multipleAttackSnapshot} pudding!"));
            await p.User.SendMessageAsync("Here are your spoils:\n" + ReportLog);
        }
    }
}
