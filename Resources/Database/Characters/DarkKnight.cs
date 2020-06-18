using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database.Characters
{
    [Serializable]
    public class DarkKnight : JobsSuper
    {
        public DarkKnight(PlayerData thePlayer)
        {
            JobSkills = CreateSkills();
            jobName = "Dark Knight";
            this.thePlayer = thePlayer;
            physicalWeaponValue = new BaseValues(9999, 100, 0, 0);
            magicalWeaponValue = new BaseValues(9999, 50, 0, 0);
            armorValue = new BaseValues(9999, 25, 0, 0);
            magArmorValue = new BaseValues(9999, 25, 0, 0);
            AdjustStats();
            jobDescription = "In-between defensive and glass cannon. Uses necromantic powers to recover health, increase defenses and poison enemies. Powerful physical attacks.";
        }

        private void AdjustStats()
        {
            thePlayer.HP.IncreaseRate = 100;
            thePlayer.MP.IncreaseRate = 30;
            thePlayer.AGI.IncreaseRate = 15;
            thePlayer.HP.LevelUp(thePlayer.Level);
            thePlayer.MP.LevelUp(thePlayer.Level);
            thePlayer.AGI.LevelUp(thePlayer.Level);
        }

        public override void AttackPatterns()
        {
            //NormalAttack();
            decimal hpPercent = (decimal)thePlayer.HP.Current / (decimal)thePlayer.HP.Max;
            int mp = thePlayer.MP.Current;

            JobSkills[3].ReduceTimer();
            thePlayer.physicalImmune = JobSkills[3].GetDuration > 0;
            int rand = UtilityClass.ReturnRandom(1, ReturnOwnedJobSkills.Count + 1);

            if (mp < 10 || rand == JobSkills.Count)
            {
                NormalAttack();
                return;
            }
            if (ReturnOwnedJobSkills.Count != 0 && ReturnOwnedJobSkills[rand].UseSkill())
            {
                thePlayer.MP.Current -= ReturnOwnedJobSkills[rand].MPCost;
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
            if (!JobSkills[0].Owned)
                thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} slashes the {thePlayer.combat.ReturnEnemy.ReturnAIName} for {thePlayer.logDMG} physical damage");
            else
            {
                int hpGain = (int)JobSkills[0].UseSkill(.001m);
                if (hpGain <= 0)
                    hpGain = 1;

                thePlayer.AdjustHP(hpGain);
                thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} slashes the {thePlayer.combat.ReturnEnemy.ReturnAIName} for {thePlayer.logDMG} physical damage and you leech {hpGain} hp!");
            }
        }

        public decimal SiphonStrike(decimal skillPotency = .001m)
        {
            if (JobSkills[0].Owned)
            {
                decimal val = (decimal)skillPotency * (decimal)thePlayer.logDMG;
                Console.WriteLine($"Potency: {(decimal)JobSkills[0].SkillPotency}\nDMG: {(decimal)thePlayer.logDMG}");
                return val;
            }
            else return 0;
        }

        public bool DarkArmor(int skillPotency)
        {
            if (JobSkills[1].Owned && JobSkills[1].MPCost <= thePlayer.MP.Current && thePlayer.combat.turnNumber < 5 && armorValue.Temporary <= 0)
            {
                armorValue.Temporary += JobSkills[1].SkillPotency;
                magArmorValue.Temporary += JobSkills[1].SkillPotency;
                thePlayer.combat.AddToLog($"An unholy aura permeates a dark glow around you adding {JobSkills[1].SkillPotency} bonus armor and magical armor defense to you until the end of combat!");
                return true;
            }
            return false;
        }

        public bool ClawsofHell(int skillPotency)
        {
            if (JobSkills[2].Owned && JobSkills[2].MPCost <= thePlayer.MP.Current && thePlayer.combat.ReturnEnemy.poisonDuration <= 0)
            {
                thePlayer.logDMG = thePlayer.combat.ReturnEnemy.TakeDMG(thePlayer, CreateEntity.DamageType.Magic, skillPotency, CreateSkill.StatusEffect.poison);
                thePlayer.combat.AddToLog($"Claws arise from hell and crush the {thePlayer.combat.ReturnEnemy.ReturnAIName} for {thePlayer.combat.ReturnEnemy.poisonDamage} damage and poisons it doing {skillPotency} per turn");
                return true;
            }
            return false;
        }

        public bool StealEssense(int skillPotency)
        {
            if (JobSkills[3].Owned && JobSkills[3].MPCost <= thePlayer.MP.Current && thePlayer.GetMPPercent <= .30m)
            {
                if (thePlayer.GetHPPercent > .50m)
                    return false;
                thePlayer.logDMG = thePlayer.combat.ReturnEnemy.TakeDMG(thePlayer, CreateEntity.DamageType.Magic, skillPotency);
                int recoveryAmt = thePlayer.logDMG * (int)((decimal)thePlayer.logDMG * .001m);
                thePlayer.MP.Current += recoveryAmt;
                thePlayer.HP.Current += recoveryAmt;
                thePlayer.combat.AddToLog($"the {thePlayer.combat.ReturnEnemy.ReturnAIName} writhes in agony as its very essence is drained. You recover {recoveryAmt} mp and hp.");
                return true;
            }
            return false;
        }

        public bool GrandSlash(int skillPotency)
        {
            if (JobSkills[3].Owned && JobSkills[3].MPCost <= thePlayer.MP.Current)
            {
                thePlayer.logDMG = thePlayer.combat.ReturnEnemy.TakeDMG(thePlayer, CreateEntity.DamageType.Physical, JobSkills[3].SkillPotency, CreateSkill.StatusEffect.none);
                if (!JobSkills[0].Owned)
                    thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} raises their large greatsword high into the air and smashes the ground beneath the {thePlayer.combat.ReturnEnemy.ReturnAIName}. The {thePlayer.combat.ReturnEnemy.ReturnAIName} takes {thePlayer.logDMG} physical damage");
                else
                {
                    int hpGain = (int)JobSkills[0].UseSkill(.001m);
                    if (hpGain <= 0)
                        hpGain = 1;
                    thePlayer.AdjustHP(hpGain);
                    thePlayer.combat.AddToLog($"{thePlayer.combat.PlayerName} raises their large greatsword high into the air and smashes the ground beneath the {thePlayer.combat.ReturnEnemy.ReturnAIName}. The {thePlayer.combat.ReturnEnemy.ReturnAIName} takes {thePlayer.logDMG} physical damage and you leech {hpGain} hp!");
                }
                return true;
            }
            return false;
        }

        protected override List<CreateSkill> CreateSkills()
        {
            List<CreateSkill> skills = new List<CreateSkill>
            {
                new CreateSkill("Siphon Strike", 1, 30, 0, 0, $"All physical damage restores HP by 3% per skill level of the damage dealt rounded down.", CreateEntity.DamageType.Passive, new CreateSkill.decimalaction(SiphonStrike)),
                new CreateSkill("Dark Armor", 2, 250, 15, 0, $"Increases pdef and mdef by +250 per skill level. This skill will really shine once the bonus reaches 1000+", CreateEntity.DamageType.Buff, new CreateSkill.voidaction(DarkArmor)),
                new CreateSkill("Claws of Hell", 5, 50, 20, 0, "Skeletal hands reach out from the ground dealing damage and poisoning the enemy.", CreateEntity.DamageType.Magic, new CreateSkill.voidaction(ClawsofHell)),
                new CreateSkill("Steal Essence", 3, 100, 15, 0, "Steals MP and HP from the enemy and gives it to you.", CreateEntity.DamageType.Magic, new CreateSkill.voidaction(StealEssense)),
                new CreateSkill("Grand Slash", 1, 100, 10, 0, "Slash the enemy dealing extra damage on top of your normal attack damage compatible with Siphon Strike.", CreateEntity.DamageType.Physical, new CreateSkill.voidaction(GrandSlash))
            };
            return skills;
        }
    }
}

