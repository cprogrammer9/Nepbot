using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Discord;
using NepBot.Resources.Code_Implements;
using NepBot.Resources.Database;

namespace NepBot.Resources.Database.Characters
{
    [Serializable]
    public abstract class JobsSuper
    {        
        protected JobSave _jobSave = new JobSave();
        protected CreateEntity thePlayer;
        protected MathCalculations math = new MathCalculations();
        protected string jobName;
        protected BaseValues physicalWeaponValue, magicalWeaponValue, armorValue, magArmorValue;
        public string jobDescription = string.Empty;

        public EmbedBuilder GetJobSkillInfo(string jobName)
        {
            EmbedBuilder eb = new EmbedBuilder();
            switch(jobName.ToLower())
            {
                case "paladin":
                    eb.WithTitle("All Paladin Skills");
                    foreach (CreateSkill Q in thePlayer.AllJobs[0].ReturnAllJobSkills)
                        eb.AddField(Q.SkillName, Q.SkillDescription);
                    //g.Append(f.SkillName);
                    break;
                case "dark knight":
                    eb.WithTitle("All Dark Knight Skills");
                    foreach (CreateSkill Q in thePlayer.AllJobs[1].ReturnAllJobSkills)
                        eb.AddField(Q.SkillName, Q.SkillDescription);
                    break;
                default:
                    eb = null;
                    break;
            }
            return eb;
        }

        public string ReportGainsPerLevel()
        {
            return $"\nGains per upgrade point for {jobName}: \npatk: {physicalWeaponValue.IncreaseRate}\nmatk: {magicalWeaponValue.IncreaseRate}\npdef: {armorValue.IncreaseRate}\nmdef: {magArmorValue.IncreaseRate}\n";
        }

        public string ReportStat(string inp)
        {
            switch (inp)
            {
                case "patk":
                    return thePlayer.PATKValue.ToString();
                case "matk":
                    return thePlayer.MATKValue.ToString();
                case "pdef":
                    return thePlayer.PDefValue.ToString();
                case "mdef":
                    return thePlayer.MDefValue.ToString();
            }
            return string.Empty;
        }

        public int PhysicalWeaponValue
        { get { return (physicalWeaponValue.Current + physicalWeaponValue.IncreaseRate); } }// * thePlayer.WeaponLevel; } }

        public int MagicalWeaponValue
        { get { return (magicalWeaponValue.Current + magicalWeaponValue.IncreaseRate); } }// * thePlayer.MagicWeaponLevel; } }

        public int ArmorValue
        { get { return (armorValue.Current + armorValue.IncreaseRate); } }// * thePlayer.ArmorLevel; } }

        public int MagArmorValue
        { get { return (magArmorValue.Current + magArmorValue.IncreaseRate); } }// * thePlayer.MagicArmorLevel; } }

        public virtual void AttackPatterns()
        {

        }

        protected virtual void NormalAttack()
        {

        }

        public List<CreateSkill> ReturnOwnedJobSkills
        {

            get
            {
                var f = new List<CreateSkill>();
                foreach (CreateSkill g in JobSkills)
                {
                    if (g.Owned)
                        f.Add(g);
                }
                return f;
            }
        }

        public List<CreateSkill> ReturnAllJobSkills
        {
            get
            {
                return JobSkills;
            }
            set { JobSkills = value; }
        }

        public List<CreateSkill> UnownedJobSkills
        {

            get
            {
                var f = new List<CreateSkill>();
                foreach (CreateSkill g in JobSkills)
                {
                    if (!g.Owned)
                        f.Add(g);
                }
                return f;
            }
        }

        public string JobName
        {
            get { return jobName; }
        }

        public int JobLevel
        {
            get { return _jobSave.jobLevel; }
            protected set
            {
                _jobSave.jobLevel = value;
                if (_jobSave.jobLevel > 99)
                    _jobSave.jobLevel = 99;
                if (_jobSave.jobLevel < 1)
                    _jobSave.jobLevel = 1;
            }
        }

        /// <summary>
        /// Current job experience.
        /// </summary>
        public int EXP
        {
            get { return _jobSave.currentExp; }
            set { _jobSave.currentExp = value; }
        }

        /// <summary>
        /// Job exp required to level up
        /// </summary>
        public int LevelExp
        {
            get { return JobLevel * 25; }
        }

        public void GainExp(int totalExp)
        {
            EXP = ExpCycle(totalExp + EXP);
        }

        protected int ExpCycle(int totalExp)
        {
            int totalXP = totalExp;
            int newLvls = JobLevel;
            while (totalXP >= LevelExp)
            {
                totalXP -= LevelExp;
                GainLevel();
            }
            newLvls = JobLevel - newLvls;
            if (newLvls != 0)
                thePlayer.combat.AddToLog($"You gained {newLvls} job level(s)! Type !nep job to check it out!");
            return totalXP;
        }

        protected void GainLevel()
        {
            JobLevel++;
        }

        protected List<CreateSkill> JobSkills;

        protected virtual List<CreateSkill> CreateSkills()
        {
            List<CreateSkill> skills = new List<CreateSkill>
            {
            
            };
            return skills;
        }

        public JobSave SaveData
        {
            get { return _jobSave; }
        }

        public void LoadData(JobSave js)
        {
            _jobSave = js;
        }
    }   
}

[Serializable]
public class JobSave
{
    public int jobLevel = 1;
    public int currentExp = 0;    
}
