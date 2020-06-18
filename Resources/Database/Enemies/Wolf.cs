using NepBot.Resources.Database.Characters;
using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database.Enemies
{
    public class Wolf : CreateEntity
    {
        public Wolf(ulong playerid)
        {
            hp = new BaseValues(150, 0, 150, 0);
            mp = new BaseValues(25, 0, 25, 0);
            patk = new BaseValues(9999, 10, 5);
            matk = new BaseValues(9999, 1, 1);
            pdef = new BaseValues(9999, 10, 5);
            mdef = new BaseValues(9999, 1, 1);
            agi = new BaseValues(9999, 150, 25);
            aiName = "Wolf";
            aiExpValue = 10;
            aiJobExpValue = 1;
            _allData = new RPGSaveData()
            {
                weaponLevel = 2,
                magicWeaponLevel = 1,
                armorLevel = 2,
                magArmorLevel = 1,
                _level = 1
            };
            CurrentJob = new EnemyGeneric(patk, matk, pdef, mdef, this);
        }

        public override CreateEntity SpawnEnemy => new Wolf(0);

        public override int MonsterPuddingGain(int playerLevel)
        {
            return 1;
        }

        public override void AiResetValues()
        {

        }

        public override void AIAttack()
        {
            try
            {
                if (combat.ReturnEnemy.AttackEvaded(this))
                {
                    combat.AddToLog($"{combat.ReturnEnemy} dodged {combat.PlayerName}'s attack!");
                    return;
                }
                logDMG = combat.ReturnPlayer.TakeDMG(this, DamageType.Physical, 1);
                combat.AddToLog(Log());
            }
            catch (NullReferenceException e)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, e, string.Empty);
            }

        }

        public override string Log()
        {
            return $"The {ReturnAIName} bites you for {logDMG} damage.";
        }
    }
}
