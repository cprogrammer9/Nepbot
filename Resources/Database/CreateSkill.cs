using NepBot.Resources.Code_Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database
{
    [Serializable]
    public class CreateSkill
    {
        public int _skillPotency;
        public string SkillDescription { get; }
        public string SkillName { get; }
        public int PuddingCost { get; }
        public int MPCost { get; }
        private SeriBool _owned = new SeriBool();
        public delegate int ulongaction(int poten);
        public delegate decimal decimalaction(decimal poten);
        public delegate bool voidaction(int poten);
        private decimalaction _decimalSkill = null;
        private ulongaction _ulongSkill = null;
        private voidaction _voidSkill = null;
        public enum StatusEffect { none, poison, stun, blind, mute }
        public CreateEntity.DamageType dmg_type;
        public readonly int requiredJobLevel;
        MathCalculations _math = new MathCalculations();

        public int GetDuration { get; private set; } = 0;

        public bool DurationCountdown()
        {
            return GetDuration > 0;
        }

        public void AddTimer(int amt = 2)
        {
            if (!_owned.Owned)
                return;
            GetDuration = amt;
        }

        public void ReduceTimer()
        {
            if (!_owned.Owned)
                return;
            GetDuration--;
            if (GetDuration < 0)
                GetDuration = 0;
        }

        public int SkillPotency
        {
            get { return _skillPotency * _owned.skillLevel; }
        }

        public string SkillDescriptionFull(string[] args, CreateEntity ent)
        {
            StringBuilder b = new StringBuilder();
            b.Append(SkillDescription);
            if (dmg_type == CreateEntity.DamageType.Magic)
                b.Append($" Potency Amount: {ent.DealNormalMagicDMG(SkillPotency)}.");
            else if (dmg_type == CreateEntity.DamageType.Physical)
                b.Append($" Potency Amount: {ent.DealNormalDMG(SkillPotency)}.");
            else if (dmg_type == CreateEntity.DamageType.Buff)
                b.Append($" Potency Amount: {SkillPotency}.");
            else if (dmg_type == CreateEntity.DamageType.Passive)
                b.Append($" Potency Amount: {SkillPotency * .001m}.");

            foreach (string g in args)
            {
                b.Append(g);
            }
            return b.ToString();
        }

        /// <summary>
        /// Use this in the RPGCommands to check if the skill can be upgraded. It returns the upgrade message. Compare it to string.Empty to know if it can be upgraded.
        /// </summary>
        /// <param name="playerPudding"></param>
        /// <param name="jobLevel"></param>
        /// <returns></returns>
        public string CanUpgradeSkill(ulong playerPudding, int jobLevel)
        {
            if (!CanUpgradeSkillCheck(playerPudding, jobLevel))
                return $"Not enough pudding to buy skill! You need {UpgradeCost} pudding!";
            else if (jobLevel <= SkillLevel)
                return $"Your job level is {jobLevel} and your current skill level is {SkillLevel}. Your job level must be high than your skill level to upgrade it!";
            return string.Empty;
        }

        public ulong BuySkill(ulong playerPudding, int jobLevel)
        {
            if (jobLevel < requiredJobLevel && !Owned)
                return 1;
            if (CanUpgradeSkillCheck(playerPudding, jobLevel))
            {
                ulong g = UpgradeCost;
                SkillLevel++;
                Owned = true;
                return g;
            }
            return 0;
        }

        public ulong UpgradeCost
        {
            get
            {
                return (ulong)((decimal)SkillLevel * (decimal)PuddingCost);
            }
        }

        public int SkillLevel
        {
            get { return _owned.skillLevel; }
            private set
            {
                _owned.skillLevel = value;
                if (_owned.skillLevel > 10)
                    _owned.skillLevel = 10;
                if (_owned.skillLevel < 1)
                    _owned.skillLevel = 1;
            }
        }

        private bool CanUpgradeSkillCheck(ulong totalPudding, int jobLvl)
        {
            return UpgradeCost <= totalPudding && SkillLevel < jobLvl;
        }

        public bool Owned
        {
            get { return true; }// _owned.Owned; }
            private set { _owned.Owned = value; }
        }

        public CreateSkill(string nam, int requiredJobLvl, int poten, int mpCost, int puddingCost, string skillDescription, CreateEntity.DamageType dmgT, voidaction a)
        {
            requiredJobLevel = requiredJobLvl;
            SkillName = nam;
            _skillPotency = poten;
            SkillDescription = skillDescription;
            PuddingCost = puddingCost;
            MPCost = mpCost;
            _voidSkill += a;
            dmg_type = dmgT;
        }

        public CreateSkill(string nam, int requiredJobLvl, int poten, int mpCost, int puddingCost, string skillDescription, CreateEntity.DamageType dmgT, ulongaction a)
        {
            requiredJobLevel = requiredJobLvl;
            SkillName = nam;
            _skillPotency = poten;
            SkillDescription = skillDescription;
            PuddingCost = puddingCost;
            MPCost = mpCost;
            _ulongSkill += a;
            dmg_type = dmgT;
        }

        public CreateSkill(string nam, int requiredJobLvl, int poten, int mpCost, int puddingCost, string skillDescription, CreateEntity.DamageType dmgT, decimalaction a)
        {
            requiredJobLevel = requiredJobLvl;
            SkillName = nam;
            _skillPotency = poten;
            SkillDescription = skillDescription;
            PuddingCost = puddingCost;
            MPCost = mpCost;
            _decimalSkill += a;
            dmg_type = dmgT;
        }

        public decimal UseSkill(decimal reduc = .001m)
        {
            return _decimalSkill.Invoke(SkillPotency * .001m);
        }

        public bool UseSkill()
        {
            if (_ulongSkill != null)
                _ulongSkill.Invoke(SkillPotency);
            else if (_voidSkill != null)
                return _voidSkill.Invoke(SkillPotency);

            return false;
        }

        public SeriBool SaveData()
        {
            return _owned;
        }

        public void LoadData(SeriBool loadedData)
        {
            _owned = (SeriBool)loadedData;
        }
    }
}

[Serializable]
public class SeriBool
{
    public bool Owned { get; set; } = false;
    public int skillLevel = 1;
}