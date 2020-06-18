using NepBot.Resources.Database.Characters;
using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database.Enemies
{
    public class Bear : CreateEntity
    {
        public Bear(ulong playerid)
        {
            hp = new BaseValues(800, 0, 800, 0);
            mp = new BaseValues(25, 0, 25, 0);
            patk = new BaseValues(9999, 50, 150);
            matk = new BaseValues(9999, 1, 0);
            pdef = new BaseValues(9999, 250, 200);
            mdef = new BaseValues(9999, 2, 0);
            agi = new BaseValues(9999, 15, 150);
            aiName = "Bear";
            aiExpValue = 25;
            aiJobExpValue = 1;
            _allData = new RPGSaveData()
            {
                weaponLevel = 1,
                magicWeaponLevel = 1,
                armorLevel = 3,
                magArmorLevel = 1,
                _level = 2
            };
            CurrentJob = new EnemyGeneric(patk, matk, pdef, mdef, this);
        }

        public override CreateEntity SpawnEnemy => new Bear(0);

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
                logDMG = combat.ReturnPlayer.TakeDMG(this, DamageType.Physical, 25);
                combat.AddToLog(Log());
            }
            catch (NullReferenceException e)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, e, string.Empty);
            }
        }

        public override string Log()
        {
            return $"The {ReturnAIName} claws you for {logDMG} damage.";
        }
    }
}
