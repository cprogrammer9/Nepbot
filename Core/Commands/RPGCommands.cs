using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.IO;
using ImgFlip;
using System.Net.Http;
using System.DrawingCore;
using ConvenienceMethods;
using Discord.WebSocket;
using System.Linq;
using NepBot.Resources.Database;
using NepBot.Data;
using System.Reflection;
using Passive.Services.DatabaseService;
using System.Text;
using NepBot.Resources.Games;
using NepBot.Resources.Code_Implements;
using System.Timers;
using NepBot.Resources.Database.Characters;
using NepBot.Resources.Extensions;

namespace NepBot.Core.Commands
{
    public class RPGCommands : ModuleBase<SocketCommandContext>
    {
        #region Misc Helpful Methods
        private List<CreateEntity> PlayerData
        {
            get { return Program.PlayerData; }
        }

        private PlayerData PlayerD(ulong findMatch)
        {
            PlayerData g = null;
            foreach (PlayerData s in PlayerData)
            {
                if (s.GetPlayerID == findMatch)
                {
                    g = s;
                    break;
                }
            }
            return g;
        }

        private UserData FindPerson(ulong UserID)
        {
            UserData udd = new UserData(UserID);
            foreach (UserData ud in Program.ExpPoints)
            {
                if (ud.UserID == UserID)
                {
                    udd = ud;
                    break;
                }
            }
            return udd;
        }

        #endregion

        [Command("Get Job Skills")]
        [Summary("Sends a list of all skills you can buy (or use if you already own) for that job. Type !nep get job skills (job name).")]

        public async Task JobSkills([Remainder] string Input = null)
        {
            ulong su = Context.User.Id;
            PlayerData pd = PlayerD(su);
            EmbedBuilder eb = pd.CurrentJob.GetJobSkillInfo(Input);
            if (eb == null)
            {
                await Context.User.SendMessageAsync("Either that job doesn't exist or you typod! Type !nep available jobs to see a list of jobs.");
                return;
            }
            await Context.User.SendMessageAsync("", false, eb.Build());
        }

        [Command("Available Jobs")]
        [Summary("Gives you a list of all jobs available in the RPG at this time. The list is everchanging and growing while the bot is still in its infancy.")]

        public async Task AvailJobs()
        {
            ulong su = Context.User.Id;
            PlayerData pd = PlayerD(su);
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("All available jobs.");
            foreach (JobsSuper Q in pd.AllJobs)
            {
                eb.AddField(Q.JobName, Q.jobDescription);

            }
            await Context.User.SendMessageAsync("", false, eb.Build());
        }

        [Command("Switch Job")]
        [Alias("Change job")]
        [Summary("Change your job. Type !nep change job (or switch job) and the job name.")]

        public async Task ChangeJob([Remainder]string Input = null)
        {
            ulong su = Context.User.Id;
            PlayerData pd = PlayerD(su);
            await Context.Channel.SendMessageAsync(pd.SwitchJobs(Input));
        }

        [Command("Buy Skill")]
        [Summary("Spend pudding to buy skills for your current job. Type !nep ")]

        public async Task BuySkill([Remainder]string Input = null)
        {
            if (Input == null)
            {
                await Context.Channel.SendMessageAsync("Gotta tell me what kind of skill you want to buy first. !nep Buy Skill (Skill name)");
                return;
            }
            ulong su = Context.User.Id;
            PlayerData pd = PlayerD(su);
            UserData ud = FindPerson(su);
            CreateSkill cs = pd.FindSkillByName(Input);
            if (ud.Pudding < (ulong)cs.PuddingCost)
            {
                await Context.Channel.SendMessageAsync("Ehh you don't have enough pudding to buy this skill! Earn more and come back later!");
                return;
            }
            //ud.Pudding -= (ulong)cs.PuddingCost;
            if (cs.BuySkill(ud.Pudding, pd.CurrentJob.JobLevel) == 1)
            {
                await Context.Channel.SendMessageAsync($"Your job level needs to be at {cs.requiredJobLevel} to buy this skill!");
                return;
            }
            await Context.Channel.SendMessageAsync(string.Concat("Congratulations! You are the proud owner of the skill: ", cs.SkillName, ". Just to remind you what this skill does: ", cs.SkillDescription));
        }

        [Command("Job Info")]
        [Summary("This displays your job info! The higher your job level, the higher you can upgrade your job skills!" +
                    "So if your job is level 5, you can upgrade your skills to a maximum of 5 levels too! Some skills only unlock at certain job levels too.")]

        public async Task JobInfo([Remainder] string Input = null)
        {
            StringBuilder g = new StringBuilder();
            PlayerData pd = PlayerD(Context.User.Id);
            if (pd.CurrentJob.ReturnOwnedJobSkills.Count > 0)
                foreach (CreateSkill f in pd.CurrentJob.ReturnOwnedJobSkills)
                {
                    g.Append($"{f.SkillName}");
                }
            else
                g.Append("You don't own any skills!");

            EmbedBuilder eb = new EmbedBuilder();
            EmbedBuilder be = new EmbedBuilder();
            eb.AddField("Hail Great Pudding Warrior!", $"You're {Context.User.Username}, a level {pd.CurrentJob.JobLevel} {pd.CurrentJob.JobName}.");
            eb.AddField("Exp Points", $"\nExp Points: {pd.CurrentJob.EXP} / {pd.CurrentJob.LevelExp}");
            eb.WithTitle("Per-Point Values for Enhancing Equipment");
            eb.AddField("Weapon Level and Patk", $"\npatk weapon Level:\n{pd.WeaponLevel}\nTotal patk:\n{pd.CurrentJob.PhysicalWeaponValue}");
            eb.AddField("Magic Level and Matk", $"\nmatk weapon Level:\n{pd.MagicWeaponLevel} Total matk:\n{pd.CurrentJob.MagicalWeaponValue}");
            eb.AddField("Armor level and Pdef", $"\npdef armor Level:\n{pd.ArmorLevel} Total pdef:\n{pd.CurrentJob.ArmorValue}");
            eb.AddField("Magic Armor Level and Mdef", $"\nmdef barrier Level:\n{pd.MagicArmorLevel} Total matk:\n{pd.CurrentJob.MagArmorValue}");
            eb.AddField("Pdef/Mdef/Matk/Patk gained per equipment upgrade as this job", $"{pd.CurrentJob.ReportGainsPerLevel()}");
            eb.AddField("Skills you own as this job", $"{g}");

            await Context.User.SendMessageAsync("", false, eb.Build());
        }

        [Command("Upgrade")]
        [Summary("Spend pudding to upgrade patk, matk, pdef and mdef!It can be upgraded a total of 100 times!Every level adds 1 point to the respective modifier for your job class. type !nep job info or !nep job for more information about your job and stats! You can perform multiple upgrades in a single command by typing !nep upgrade(skill name) (amount) if you got the pudding for it!")]

        public async Task Upgrade([Remainder] string Input = null)
        {
            string[] sp = ExtensionMethods.GenericSplit(Input, " ");
            if (sp[0].ToLower() == "help")
            {
                await Context.Channel.SendMessageAsync("Spend pudding to upgrade patk, matk, pdef and mdef! It can be upgraded a total of 100 times! Every level adds 1 point to the respective modifier for your job class. type !nep job info or !nep job for more information about your job and stats! You can perform multiple upgrades in a single command by typing !nep upgrade (skill name) (amount) if you got the pudding for it!");
                return;
            }
            int cycles = (sp.Length > 1) ? int.Parse(sp[1]) : 1;
            PlayerData pd = PlayerD(Context.User.Id);
            UserData ud = FindPerson(Context.User.Id);
            ulong val = pd.LevelGear(sp[0].ToLower(), ud, cycles);
            ulong totalPaid = val;
            int totalGained = pd.WeaponLevel;
            await Context.Channel.SendMessageAsync(val.ToString());
            //if (cycles > 1 && val != 0 && val < ud.Pudding)
            //{
            //  for (int i = 1; i < cycles; i++)
            //                {
            //if (val > ud.Pudding)
            //break;
            //ud.Pudding -= val;
            //val = pd.LevelGear(sp[0].ToLower(), ud, cycles);
            //totalPaid += val;
            //totalGained += cycles;
            //              }
            //        }

            await Context.Channel.SendMessageAsync($"You spent {totalPaid} pudding and upgraded {sp[0]} by {cycles} point(s)! Total pudding now {ud.Pudding}. Total {sp[0]} now {pd.CurrentJob.ReportStat(sp[0])}");
            return;

            if (val > ud.Pudding)
            {
                await Context.Channel.SendMessageAsync($"Ehhh... not enough pudding to upgrade {sp[0]}. You have {ud.Pudding} and need {val}. Just need {val - ud.Pudding} more!");
                return;
            }
            if (val == 0)
            {
                await Context.Channel.SendMessageAsync("Psst, hey. You need to type either patk, matk, pdef or mdef after Upgrade to upgrade these stats!");
                return;
            }


            await Context.Channel.SendMessageAsync($"You spent {totalPaid} pudding and upgraded {sp[0]} by {totalGained} point(s)! Total pudding now {ud.Pudding}. Total {sp[0]} now {pd.CurrentJob.ReportStat(sp[0])}");
        }

        [Command("List Enemies")]
        [Summary("Gives you a list of all enemies you can attack!")]

        public async Task Enemies()
        {
            StringBuilder b = new StringBuilder();
            PlayerData pd = PlayerD(Context.User.Id);
            foreach (CreateEntity r in CreateEntity.Enemies)
            {
                b.Append(r.ReturnAIName).Append("\n");
            }
            await Context.Channel.SendMessageAsync($"Here is a list of enemies you can attack from weakest to hardest!\n{b}");
        }

        [Command("Attack")]
        [Summary("Attack an enemy! Type !nep enemies to get a list of what you can attack. Example type !nep attack wolf (number). You can perform multiple attacks in a row (up to 20 attacks) but your HP will not recover after each fight. If you die, you lose all gained exp so " +
                    "use this wisely!")]

        public async Task Attack([Remainder] string Input = null)
        {
            try
            {
                string[] lol = ExtensionMethods.GenericSplit(Input, " ");
                int val = 0;
                if (lol.Length > 1 && int.Parse(lol[1]) > 0)
                    val = int.Parse(lol[1]);
                if (lol[0].ToLower() == "help")
                {
                    await Context.User.SendMessageAsync("Attack an enemy! Type !nep enemies to get a list of what you can attack. Example type !nep attack wolf (number). You can perform multiple attacks in a row (up to 10 attacks) but your HP will not recover after each fight. If you die, you lose all gained exp so " +
                        "use this wisely!");
                    return;
                }
                if (!Program.IsOnPlayerData(Context))
                    Program.PlayerData.Add(new PlayerData(Context.User.Id));
                PlayerData pd = PlayerD(Context.User.Id);
                UserData ud = ExtensionMethods.FindPerson(Context.User.Id);
                int atkAmt = ud.NonLevel / 2;
                if (atkAmt < 1)
                    atkAmt = 1;
                if (val > atkAmt)
                {
                    await Context.Channel.SendMessageAsync($"You can only attack {atkAmt} times in a row. This is your ooc chat level / 2 rounded down.");
                    return;
                }
                lol[0] = lol[0].ToLower();
                CreateEntity enemy = null;
                foreach (CreateEntity x in CreateEntity.Enemies)
                {
                    if (x.ReturnAIName.ToLower() == lol[0])
                    {
                        enemy = x.SpawnEnemy;
                        break;
                    }
                }
                if (enemy == null)
                    await Context.Channel.SendMessageAsync("Null");
                if (pd.combat != null && pd.combat.inCombat)
                {
                    await Context.User.SendMessageAsync("You're already in a battle. Please wait until it's done.");
                    return;
                }
                Combat c = new Combat(enemy, pd, Context.User.Username, Context, ud, val);
                c = null;
            }
            catch (Exception m)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, "from attack method");
            }

            //await Context.User.SendMessageAsync("Here are your combat results:\n" + c.ReportLog);
        }

        public async Task EndCombat(Combat c)
        {
            await Context.User.SendMessageAsync("Here are your combat results:\n" + c.ReportLog);
        }

        [Command("My RPG Stats")]
        [Alias("rpg stats", "rp stats", "stats rpg", "roleplay stats")]
        [Summary("Gives you a list of non-job specific player stats containing your total HP, MP, Agility, Exp Points etc.")]

        public async Task MyStats([Remainder] string Input = null)
        {
            SocketUser verify = Context.User;
            if (Input != null)
                verify = ExtensionMethods.GetSocketUser(Input, Context);
            PlayerData pd = PlayerD(verify.Id);
            EmbedBuilder eb = pd.RPGStats(Context);
            await Context.User.SendMessageAsync("", false, eb.Build());
        }

        [Command("List Owned Job Skills")]
        [Summary("Lists the skills you own for your current job. You can get this information with !nep job info, but this contains ONLY the job skill information.")]

        public async Task OwnedJobSkills()
        {
            PlayerData pd = PlayerD(Context.User.Id);
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("Owned Job Skills Skills");
            foreach (CreateSkill Q in pd.CurrentJob.ReturnOwnedJobSkills)
            {
                eb.AddField(Q.SkillName, Q.SkillDescription);

            }
            await Context.User.SendMessageAsync("", false, eb.Build());
        }

        [Command("List Job Skills")]
        [Summary("Lists all skills available for your current job. Useful to quickly show you what the skills are so you can buy them.")]

        public async Task AllJobSkills()
        {
            PlayerData pd = PlayerD(Context.User.Id);
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("All Job Skills");
            foreach (CreateSkill Q in pd.CurrentJob.ReturnAllJobSkills)
            {
                eb.AddField(Q.SkillName, Q.SkillDescriptionFull(new string[] { $" Skill Power: {pd.PotencyFormula(Q)}" }, pd));
            }
            await Context.User.SendMessageAsync("", false, eb.Build());
        }
    }
}
