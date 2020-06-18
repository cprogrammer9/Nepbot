using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database.Characters
{
    [Serializable]
    public class Paladin : JobsSuper
    {
        public Paladin(PlayerData thePlayer)
        {
            JobSkills = CreateSkills();
            jobName = "Paladin";
            this.thePlayer = thePlayer;
            physicalWeaponValue = new BaseValues(9999, 50, 0, 0);
            magicalWeaponValue = new BaseValues(9999, 25, 0, 0);
            armorValue = new BaseValues(9999, 75, 0, 0);
            magArmorValue = new BaseValues(9999, 35, 0, 0);
            AdjustStats();
            jobDescription = "Defensive warrior with a fairly potent self heal. Can stun enemies and become immune to damage, but trades raw attack power in exchange for sustainability.";
        }

        private void AdjustStats()
        {
            thePlayer.HP.IncreaseRate = 200;
            thePlayer.MP.IncreaseRate = 50;
            thePlayer.AGI.IncreaseRate = 25;
            thePlayer.HP.LevelUp(thePlayer.Level);
            thePlayer.MP.LevelUp(thePlayer.Level);
            thePlayer.AGI.LevelUp(thePlayer.Level);
        }


        public override void AttackPatterns()
        {
            //NormalAttack();
            decimal hpPercent = (decimal)thePlayer.HP.Current / (decimal)thePlayer.HP.Max;
            int mp = thePlayer.MP.Current;

            
            thePlayer.physicalImmune = JobSkills[3].GetDuration > 0;
            int rand = UtilityClass.ReturnRandom(0, ReturnOwnedJobSkills.Count + 1);

            if (mp < 2 || rand == JobSkills.Count)
            {
                NormalAttack();
                return;
            }
            if (ReturnOwnedJobSkills.Count != 0 && ReturnOwnedJobSkills[rand].UseSkill())
            {

            }
            else
                NormalAttack();
        }

        protected override void NormalAttack()
        {
            if (thePlayer.combat.ReturnEnemy.AttackEvaded(thePlayer))
            {
                thePlayer.combat.AddToLog($"{thePlayer.combat.ReturnEnemy} dodged {thePlayer.combat.PlayerName}'s attack!");
                return;
            }
            thePlayer.logDMG = thePlayer.combat.ReturnEnemy.TakeDMG(thePlayer, CreateEntity.DamageType.Physical, 1, CreateSkill.StatusEffect.none);
            thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} slashes the {thePlayer.combat.ReturnEnemy.ReturnAIName} for {thePlayer.logDMG} physical damage");
        }

        private bool MinorHeal(int skillPotency)
        {
            if (JobSkills[0].Owned && ((decimal)thePlayer.HP.Current / (decimal)thePlayer.HP.Max) <= .50m && thePlayer.MP.Current >= JobSkills[0].MPCost)
            {
                thePlayer.AdjustHP(skillPotency);
                thePlayer.MP.Current -= JobSkills[0].MPCost;
                thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} uses Minor Healing to recover {skillPotency} hp.");
                return true;
            }

            return false;
        }

        private bool StunFoe(int skillPotency)
        {
            if (JobSkills[1].Owned && ((decimal)thePlayer.HP.Current / (decimal)thePlayer.HP.Max) <= .75m && thePlayer.MP.Current >= JobSkills[1].MPCost && thePlayer.combat.ReturnEnemy.stunDuration <= 0)
            {
                thePlayer.logDMG = thePlayer.combat.ReturnEnemy.TakeDMG(thePlayer, CreateEntity.DamageType.Physical, skillPotency, CreateSkill.StatusEffect.stun);
                thePlayer.MP.Current -= JobSkills[1].MPCost;
                thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} attacks the {thePlayer.combat.ReturnEnemy.ReturnAIName} for {thePlayer.logDMG} physical damage and stuns it for 2 turns");
                return true;
            }
            return false;
        }

        private bool SmittenByLight(int skillPotency)
        {
            if (JobSkills[2].Owned && thePlayer.MP.Current >= JobSkills[2].MPCost && thePlayer.PATK.Temporary < skillPotency)
            {
                thePlayer.PATK.Temporary += skillPotency;
                thePlayer.MP.Current -= JobSkills[2].MPCost;
                thePlayer.logDMG = thePlayer.combat.ReturnEnemy.TakeDMG(thePlayer, CreateEntity.DamageType.Magic, skillPotency, CreateSkill.StatusEffect.blind);
                thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} attacks the {thePlayer.combat.ReturnEnemy.ReturnAIName} for {thePlayer.logDMG} magic damage and gains +{skillPotency} to physical attack damage.");

                return true;
            }

            return false;
        }

        private bool BlockAttack(int skillPotency)
        {
            if (JobSkills[3].Owned && thePlayer.MP.Current >= JobSkills[3].MPCost && thePlayer.combat.ReturnEnemy.stunDuration <= 0 && !thePlayer.physicalImmune)
            {
                int timerval = 2;
                if (JobSkills[3].SkillLevel >= 5 && JobSkills[3].SkillLevel < 10)
                    timerval = 3;
                else if (JobSkills[3].SkillLevel == 10)
                    timerval = 4;
                JobSkills[3].AddTimer(timerval);
                thePlayer.physicalImmune = true;
                thePlayer.MP.Current -= JobSkills[3].MPCost;
                thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} braces their shield for impact! Immune to physical damage for 2 turns.");
                return true;
            }
            return false;
        }

        public bool Slash(int skillPotency)
        {
            if (JobSkills[4].Owned && thePlayer.MP.Current >= JobSkills[4].MPCost)
            {
                thePlayer.logDMG = thePlayer.combat.ReturnEnemy.TakeDMG(thePlayer, CreateEntity.DamageType.Physical, JobSkills[4].SkillPotency, CreateSkill.StatusEffect.none);
                thePlayer.MP.Current -= JobSkills[4].MPCost;
                thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} slashes the {thePlayer.combat.ReturnEnemy.ReturnAIName} carefully targeting its weak points for {thePlayer.logDMG} physical damage.");
                return true;
            }
            return false;
        }

        protected override List<CreateSkill> CreateSkills()
        {
            List<CreateSkill> skills = new List<CreateSkill>
            {
                new CreateSkill("Minor Healing", 1, 50, 5, 0, "Heals for a small amount of damage.", CreateEntity.DamageType.Magic, new CreateSkill.voidaction(MinorHeal)), // 5000
                new CreateSkill("Shield Bash", 2, 115, 10, 0, "Stuns enemy for 2 turns.", CreateEntity.DamageType.Physical, new CreateSkill.voidaction(StunFoe)), // 15000
                new CreateSkill("Smitten by Light", 5, 200, 10, 0, $"Blinds enemy, dealing damage and increasing your attack power by 200 per skill level for the rest of the battle.", CreateEntity.DamageType.Magic, new CreateSkill.voidaction(SmittenByLight)), // 25000
                new CreateSkill("Block attack", 3, 0, 0, 0, $"Take no physical damage for 2 turns. An additional 1 turn at skill level 5, and another at 10.", CreateEntity.DamageType.NA, new CreateSkill.voidaction(BlockAttack)), // 25000
                new CreateSkill("Slash", 1, 100, 2, 0, "Slash the enemy dealing extra damage on top of your normal attack damage", CreateEntity.DamageType.Physical, new CreateSkill.voidaction(Slash)) // 10000
            };
            return skills;
        }
    }
}
