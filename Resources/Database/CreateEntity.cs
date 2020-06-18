using NepBot.Resources.Extensions;
using NepBot.Resources.Database.Characters;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using NepBot.Resources.Database.Enemies;
using NepBot.Resources.Code_Implements;
using NepBot.Data;
using Discord.WebSocket;
using System.IO;
using Discord.Commands;

namespace NepBot.Resources.Database
{
    [Serializable]
    public abstract class CreateEntity
    {
        public delegate int ChosenAtk(CreateEntity enemy, DamageType dt, CreateSkill.StatusEffect effects = CreateSkill.StatusEffect.none);
        public ChosenAtk chosenATK;
        protected BaseValues hp, mp, patk, matk, pdef, mdef, agi;
        protected RPGSaveData _allData;
        public enum DamageType { Magic, Physical, Buff, Passive, NA }
        protected List<CreateSkill> AllPlayerSkills = new List<CreateSkill>();
        public delegate bool SetImmuneTimer(bool toggle);
        public SetImmuneTimer physicalImmuneTimer;
        public SetImmuneTimer magicalImmuneTimer;
        protected JobsSuper currentJob;
        public int stunDuration = 0, poisonDuration = 0, blindDuration = 0, muteDuration = 0;
        public int poisonDamage = 0;
        public Combat combat;
        public bool physicalImmune = false;
        public bool magicalImmune = false;
        protected string aiName = string.Empty;
        protected int aiExpValue = 0, aiJobExpValue = 0;
        protected static List<CreateEntity> enemies = new List<CreateEntity>();
        protected MathCalculations _math = new MathCalculations();
        public int logDMG;
        protected List<JobsSuper> allJobs = new List<JobsSuper>();

        public virtual int MonsterPuddingGain(int playerLevel)
        {
            return 0;
        }

        /// <summary>
        /// Paladin 0
        /// Dark Knight 1
        /// </summary>
        /// <param name="job"></param>
        public string SwitchJobs(string job)
        {
            switch (job.ToLower())
            {
                case "paladin":
                    CurrentJob = AllJobs[0];
                    return "You changed your job to a Paladin!";
                case "dark knight":
                    CurrentJob = AllJobs[1];
                    return "You changed your job to a Dark Knight!";
                default:
                    return "Either this job doesn't exist yet, you typod something or you entered an invalid job!";
            }
            //CurrentJob = AllJobs[job];
        }

        public List<JobsSuper> AllJobs
        {
            get { return allJobs; }
        }

        public void AddToJobList(JobsSuper th)
        {
            allJobs.Add(th);
        }

        public int AIJobExpValue
        {
            get { return aiJobExpValue; }
        }

        public static void AddEnemies()
        {
            try
            {
                enemies.Clear();
                enemies.Add(new Wolf(0));
                enemies.Add(new Bear(0));
            }
            catch (Exception m)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, string.Empty);
            }

        }

        public virtual CreateEntity SpawnEnemy
        {
            get { return null; }
        }

        public static List<CreateEntity> Enemies
        {
            get { return enemies; }
        }

        public int ExpValue
        {
            get { return aiExpValue; }
        }

        public string ReturnAIName
        {
            get { return aiName; }
        }

        public virtual void AiResetValues()
        {

        }

        public virtual string Log()
        {
            return string.Empty;
        }

        public JobsSuper CurrentJob
        {
            get { return currentJob; }
            set { currentJob = value; }
        }

        public EmbedBuilder RPGStats(SocketCommandContext context)
        {
            EmbedBuilder g = new EmbedBuilder();
            string sockUser = ExtensionMethods.GetUsersName(GetPlayerID.ToString(), context, true);
            g.WithTitle($"Current Stats for: {sockUser}");
            g.AddField("Player Exp", $"Player Level: {Level}\nPlayer XP Current: {EXP} / {LevelExp}");
            g.AddField("Current Job", string.Concat(CurrentJob.JobName, ".\nJob Level: ", CurrentJob.JobLevel, "\nCurrent Job Exp: ", CurrentJob.EXP, " / ", CurrentJob.LevelExp));
            //g.AddField("Job Exp", $"Job XP Current: {EXP} Job XP Until Level {LevelExp}");
            g.AddField("Total Hit Points", HP.Max);
            g.AddField("total Mana Points", MP.Max);
            g.AddField("Total Physical Attack", PATKValue);
            g.AddField("Total Magic Attack", MATKValue);
            g.AddField("Total Defense", PDefValue);
            g.AddField("Total Magic Defense", MDefValue);
            g.AddField("Total Agility", AGI.Current);
            g.AddField("Physical Weapon", $"Level: {_allData.weaponLevel} - Power Value: {PATKValue}");
            g.AddField("Magical Weapon", $"Level: {_allData.magicWeaponLevel} - Power Value: {MATKValue}");
            g.AddField("Physical Armor", $"Level: {_allData.armorLevel} - Defense Value: {PDefValue}");
            g.AddField("Magical Barrier Armor", $"Level: {_allData.magArmorLevel} - Defense Value: {MDefValue}");

            foreach (CreateSkill v in CurrentJob.ReturnOwnedJobSkills)
                g.AddField(v.SkillName, v.SkillDescriptionFull(new string[] { $"Skill Potency: {PotencyFormula(v)}" }, this));

            return g;
        }

        public bool PhysicalImmune(bool toggle)
        {
            return physicalImmuneTimer.Invoke(toggle);
        }

        public bool MagicalImmune(bool toggle)
        {
            return magicalImmuneTimer.Invoke(toggle);
        }

        public void EndCombat()
        {
            combat = null;
            HP.Current = HP.Max;
            MP.Current = MP.Max;
            SetTemporaryToZero();
            SetDurationsToZero();
        }

        public void StartedCombat(Combat c)
        {
            combat = c;
        }

        #region vitals
        public BaseValues HP
        {
            get
            {
                return hp;
            }
        }

        public decimal GetMPPercent
        {
            get { return (decimal)MP.Current / (decimal)MP.Max; }
        }

        public decimal GetHPPercent
        {
            get { return (decimal)HP.Current / (decimal)HP.Max; }
        }

        public BaseValues MP
        {
            get
            {
                return mp;
            }
        }

        public BaseValues PATK
        {
            get
            {
                patk.Current = PATKValue;
                return patk;
            }
        }

        public BaseValues MATK
        {
            get
            {
                matk.Current = MATKValue;
                return matk;
            }
        }

        public BaseValues PDEF
        {
            get
            {
                pdef.Current = PDefValue;
                return pdef;
            }
        }

        public BaseValues MDEF
        {
            get
            {
                mdef.Current = MDefValue;
                return mdef;
            }
        }

        public BaseValues AGI
        {
            get
            {
                return agi;
            }
        }
        #endregion

        public void RandomAttack()
        {
            CurrentJob.AttackPatterns();
        }

        public void SetTemporaryToZero()
        {
            hp.Temporary = 0;
            mp.Temporary = 0;
            patk.Temporary = 0;
            matk.Temporary = 0;
            pdef.Temporary = 0;
            mdef.Temporary = 0;
            agi.Temporary = 0;
        }

        public List<CreateSkill> OwnedSkills
        {
            get
            {
                List<CreateSkill> cc = new List<CreateSkill>();
                foreach (CreateSkill x in AllPlayerSkills)
                {
                    if (x.Owned)
                        cc.Add(x);
                }
                return cc;
            }
        }

        /// <summary>
        /// can't figure out the math for this. Deal with it another time
        /// </summary>
        /// <param name="attacker"></param>
        /// <returns></returns>
        public bool AttackEvaded(CreateEntity attacker)
        {
            return false;
            int g = (int)(100m * ((decimal)AGI.Current / (decimal)AGI.Max));
            int accuracy = (int)(100m * ((decimal)attacker.AGI.Current / (decimal)attacker.AGI.Max));
            g -= accuracy;
            if (g < 0)
                g = 0;
            Random rnd = new Random();
            int total = rnd.Next(0, 101 - g);
            return total >= g;
        }

        public CreateSkill FindSkillByName(string query)
        {
            CreateSkill p = null;
            foreach (CreateSkill x in CurrentJob.ReturnOwnedJobSkills)
            {
                if (x.SkillName.ToLower() == query.ToLower())
                {
                    p = x;
                    break;
                }
            }
            return p;
        }

        public string ListUNOwnedSkills()
        {
            StringBuilder b = new StringBuilder();
            foreach (CreateSkill g in AllPlayerSkills)
            {
                if (!g.Owned)
                {
                    b.Append("Name: ").Append(g.SkillName).Append("\nDescription: ").Append(g.SkillDescription);
                }
            }
            return b.ToString();
        }

        public void AdjustHP(int valu)
        {
            HP.Current += valu;
        }

        public string ListOwnedSkills()
        {
            StringBuilder b = new StringBuilder();
            foreach (CreateSkill g in AllPlayerSkills)
            {
                if (g.Owned)
                {
                    b.Append("Name: ").Append(g.SkillName).Append("\nDescription: ").Append(g.SkillDescription);
                }
            }
            return b.ToString();
        }

        /// <summary>
        /// Current Experience
        /// </summary>
        public int EXP
        {
            get { return _allData.exp; }
        }

        /// <summary>
        /// Experience required to level
        /// </summary>
        public int LevelExp
        {
            get { return Level * 50; }
        }

        /// <summary>
        /// Adds exp to the current experience. Automatically triggers level up.
        /// </summary>
        /// <param name="totalExp"></param>
        public void GainExp(int totalExp)
        {
            _allData.exp = ExpCycle(totalExp + _allData.exp);
        }

        protected int ExpCycle(int totalExp)
        {
            int totalXP = totalExp;
            int newLvls = Level;
            while (totalXP >= LevelExp)
            {
                totalXP -= LevelExp;
                GainLevel();
            }
            newLvls = Level - newLvls;
            if (newLvls != 0)
                combat.afterLogPlayer = $"\nYou gained {newLvls} level(s)! Type !nep my rpg stats to check it out!";
            return totalXP;
        }

        protected void GainLevel()
        {
            Level++;
            HP.LevelUp(Level);
            MP.LevelUp(Level);
            agi.LevelUp(Level);
        }

        public int PotencyFormula(CreateSkill sp)
        {
            double potencyTotal = (double)sp.SkillPotency * (double)sp.SkillPotency.ToString().Length;
            double total = (sp.dmg_type == DamageType.Magic) ? (double)matk.Current + potencyTotal : (double)patk.Current + potencyTotal;
            return (int)total;
        }

        public virtual void AIAttack()
        {

        }

        public virtual decimal DealNormalDMG(int modifier)
        {
            return _math.PotencyCalculations(modifier, PATKValue);
        }

        public virtual decimal DealNormalMagicDMG(int modifier)
        {
            return _math.PotencyCalculations(modifier, MATKValue);
        }

        public void SetDurationsToZero()
        {
            stunDuration = 0;
            poisonDuration = 0;
            muteDuration = 0;
            blindDuration = 0;
            poisonDamage = 0;
            physicalImmune = false;
            magicalImmune = false;
        }

        public void ReduceDuration()
        {
            stunDuration--;
            poisonDuration--;
            muteDuration--;
            blindDuration--;
            if (stunDuration < 0)
                stunDuration = 0;
            if (poisonDuration < 0)
                poisonDuration = 0;
            if (muteDuration < 0)
                muteDuration = 0;
            if (blindDuration < 0)
                blindDuration = 0;
        }

        public void Poisoned(int amt, int duration)
        {
            poisonDuration = duration;
            poisonDamage = amt;
        }

        public void DoTs()
        {
            if (poisonDuration > 0)
            {
                bool isAI = ReturnAIName != string.Empty;
                AdjustHP(-poisonDamage);
                if (!isAI)
                    combat.AddToLog($"{combat.PlayerName} takes {poisonDamage} poison damage!");
                else
                    combat.AddToLog($"{ReturnAIName} takes {poisonDamage} poison damage!");
            }
        }

        public int TakeDMG(CreateEntity attacker, DamageType dt, int skillDamage, CreateSkill.StatusEffect effects = CreateSkill.StatusEffect.none)
        {
            if (skillDamage <= 0)
                skillDamage = 1;
            decimal total = (dt == DamageType.Physical) ? attacker.DealNormalDMG(skillDamage) : attacker.DealNormalMagicDMG(skillDamage);

            if (total == 0 || physicalImmune || magicalImmune)
            {
                return 0;
            }

            decimal reduction = 0;
            int currentTurnDamage = 0;
            if (dt == DamageType.Physical)//FIX AFTER
            {
                reduction = (decimal)PDefValue / 9999m;
            }
            else
            {
                reduction = (decimal)MDefValue / 9999m;
            }

            if (effects != CreateSkill.StatusEffect.none)
                switch (effects)
                {
                    case CreateSkill.StatusEffect.stun:
                        stunDuration += 2;
                        break;
                    case CreateSkill.StatusEffect.mute:
                        muteDuration += 4;
                        break;
                    case CreateSkill.StatusEffect.blind:
                        blindDuration += 4;
                        break;
                    case CreateSkill.StatusEffect.poison:
                        Poisoned((int)total, 4);
                        break;
                }
            currentTurnDamage = (int)(total - total * reduction);
            if (currentTurnDamage < 0)
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.CustomMessage, null, "Current Turn Damage variable is 0 or lower. It should never equal this. In CreateEntity > TakeDMG");


            int one = 0;
            int per = (int)((decimal)currentTurnDamage - (decimal)currentTurnDamage * .1m);
            if (per > 1)
                one = UtilityClass.ReturnRandom(per, ++currentTurnDamage);
            else
                one = 1;
            //ExtensionMethods.WriteToLog($"{two} || ");
            HP.Current -= one;
            return one;
        }

        public static string Error = "";

        public int Level
        {
            get { return _allData._level; }
            protected set
            {
                _allData._level = value;
                if (_allData._level > 9999)
                    _allData._level = 9999;
            }
        }

        public enum UpgradeType { matk, patk, pdef, mdef, all }

        public string UpgradeCost(UpgradeType TypeofUpgrade)
        {
            string val = string.Empty;
            switch (TypeofUpgrade)
            {
                case UpgradeType.matk:
                    val = string.Concat("It costs ", _allData.magicWeaponLevel * 1000, " pudding to upgrade your magic weapon!");
                    break;
                case UpgradeType.patk:
                    val = string.Concat("It costs ", _allData.weaponLevel * 1000, " pudding to upgrade your physical weapon!");
                    break;
                case UpgradeType.pdef:
                    val = string.Concat("It costs ", _allData.armorLevel * 1000, " pudding to upgrade your physical defense armor!");
                    break;
                case UpgradeType.mdef:
                    val = string.Concat("It costs ", _allData.magArmorLevel * 1000, " pudding to upgrade your magical barrier armor!");
                    break;
                case UpgradeType.all:
                    val = $"Costs:\nPhysical Weapon: {_allData.weaponLevel * 1000}\nMagical Weapon: {_allData.magicWeaponLevel * 1000}\nArmor: {_allData.armorLevel * 1000}\nMagical BarrierArmor: {_allData.magArmorLevel * 1000}";
                    break;
            }
            return val;
        }

        public int MATKValue
        {
            get
            {
                int g = _allData.magicWeaponLevel * (CurrentJob.MagicalWeaponValue + matk.Temporary);
                if (g > matk.Max)
                    g = matk.Max;
                return g;
            }
        }

        public int MDefValue
        {
            get
            {
                int g = _allData.magArmorLevel * (CurrentJob.MagArmorValue + mdef.Temporary);
                if (g > mdef.Max)
                    g = mdef.Max;
                return g;

            }
        }

        public int PDefValue
        {
            get
            {
                int g = _allData.armorLevel * (CurrentJob.ArmorValue + pdef.Temporary);
                if (g > pdef.Max)
                    g = pdef.Max;
                return g;

            }
        }

        public int PATKValue
        {
            get
            {
                int g = _allData.weaponLevel * (CurrentJob.PhysicalWeaponValue + patk.Temporary);
                if (g > patk.Max)
                    g = patk.Max;

                return g;
            }
        }

        public int WeaponLevel
        {
            get { return _allData.weaponLevel; }
            private set { _allData.weaponLevel = value; }
        }

        public int MagicWeaponLevel
        {
            get { return _allData.magicWeaponLevel; }
            private set { _allData.magicWeaponLevel = value; }
        }

        public int ArmorLevel
        {
            get { return _allData.armorLevel; }
            private set { _allData.armorLevel = value; }
        }

        public int MagicArmorLevel
        {
            get { return _allData.magArmorLevel; }
            private set { _allData.magArmorLevel = value; }
        }

        public ulong GetPlayerID
        {
            get { return _allData.playerID; }
        }

        protected bool CanAfford(int gearLevel, ulong puddingLevel)
        {
            return (ulong)GearCost(gearLevel) <= puddingLevel;
        }

        protected int GearCost(int gearLevel)
        {
            return gearLevel * 5000;
        }

        /// <summary>
        /// --- Submit userdata and use this method to subtract from total pudding. If the value is 0, then you cannot afford to buy it.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public ulong LevelGear(string type, UserData d, int amt = 1)
        {
            ulong am = (ulong)amt;
            switch (type)
            {
                case "patk": // weapon
                    if (CanAfford(WeaponLevel + amt, d.Pudding))
                    {
                        int costAmt = GearCost(WeaponLevel) * amt;
                        //d.Pudding -= (ulong)(ulong)WeaponLevel * 1000u;
                        //canAfford = (ulong)(ulong)WeaponLevel * 1000u * am;
                        WeaponLevel += amt;
                        return (ulong)costAmt;
                    }
                    break;
                case "matk": // magic weapon
                    if (CanAfford(MagicWeaponLevel * amt, d.Pudding))
                    {
                        int costAmt = GearCost(MagicWeaponLevel) * amt;
                        //d.Pudding -= (ulong)MagicWeaponLevel * 1000u;
                        //canAfford = (ulong)MagicWeaponLevel * 1000u * am;
                        MagicWeaponLevel += amt;
                        return (ulong)costAmt;
                    }
                    break;
                case "pdef": // armor
                    if (CanAfford(ArmorLevel * amt, d.Pudding))
                    {
                        int costAmt = GearCost(ArmorLevel) * amt;
                        //d.Pudding -= (ulong)ArmorLevel * 1000u;
                        //canAfford = (ulong)ArmorLevel * 1000u;
                        ArmorLevel += amt;
                        return (ulong)costAmt;
                    }
                    break;
                case "mdef": // magic armor
                    if (CanAfford(MagicArmorLevel * amt, d.Pudding))
                    {
                        int costAmt = GearCost(MagicArmorLevel) * amt;
                        //d.Pudding -= (ulong)MagicArmorLevel * 1000u;
                        //canAfford = (ulong)MagicArmorLevel * 1000u;
                        MagicArmorLevel += amt;
                        return (ulong)costAmt;
                    }
                    break;
            }
            return 0;
        }

        public RPGSaveData SaveControl()
        {
            _allData.saveData.Clear();
            _allData.currentJob = currentJob.JobName;
            foreach (JobsSuper g in allJobs)
            {
                _allData.AddToSaveList(g.SaveData);
                foreach (CreateSkill x in g.ReturnAllJobSkills)
                {
                    _allData.AddToSaveList(x.SaveData());
                }
            }
            return _allData;
        }

        public void LoadData(RPGSaveData j)
        {
            List<object> f = j.saveData;
            ExtensionMethods.WriteToLog(ExtensionMethods.LogType.CustomMessage, null, j.playerID.ToString() + " from LoadData rpg stats");
            _allData.ReloadData(j._level, j.exp, j.armorLevel, j.magArmorLevel, j.weaponLevel, j.magicWeaponLevel, j.playerID, j.currentJob);
            int jobListBeginning = 0;
            int allJobsSifter = 0;
            bool toggle = false;
            try
            {
                for (int i = 0; i < f.Count; i++)
                {
                    object x = f[i];
                    if (i == allJobs[allJobsSifter].ReturnAllJobSkills.Count + 1 || i == 0)
                    {
                        if (toggle)
                        {
                            allJobsSifter++;
                            toggle = false;
                        }
                        allJobs[allJobsSifter].LoadData((JobSave)x);
                        jobListBeginning = 0;
                    }
                    else
                    {
                        allJobs[allJobsSifter].ReturnAllJobSkills[jobListBeginning++].LoadData((SeriBool)x);
                        toggle = true;
                    }
                }
                SwitchJobs(j.currentJob);
            }
            catch (Exception m)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, string.Empty);
            }
            hp.LevelUp(Level);
            mp.LevelUp(Level);
            agi.LevelUp(Level);
        }
    }
}

[Serializable]
public class RPGSaveData
{
    public int _level = 1;
    public int exp = 0;
    public int armorLevel = 1;
    public int magArmorLevel = 1;
    public int weaponLevel = 1;
    public int magicWeaponLevel = 1;
    public ulong playerID;
    public string currentJob;
    public List<Object> saveData = new List<object>();

    public void ReloadData(int lvl, int xp, int arml, int magarml, int weaplvl, int magweaplvl, ulong playerD, string jobs)
    {
        _level = lvl;
        exp = xp;
        armorLevel = arml;
        magArmorLevel = magarml;
        weaponLevel = weaplvl;
        magicWeaponLevel = magweaplvl;
        playerID = playerD;
        currentJob = jobs;
    }

    public string ReportAllStats
    {
        get
        {
            string f = $"\n{_level}\n{exp}\n{armorLevel}\n{magArmorLevel}\n{weaponLevel}\n{magicWeaponLevel}\n{playerID}\n{currentJob}\n";
            return f;
        }
    }

    public void AddToSaveList(object o)
    {
        saveData.Add(o);
    }

    public object LoadData(int pos)
    {
        return saveData[pos];
    }
}