using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Discord.WebSocket;

namespace NepBot.Data
{
	[Serializable]
	public class GuildData
	{
		private readonly ulong _guildID;

		private List<GuildData.AutoPromotions> _autoPromotions = new List<GuildData.AutoPromotions>();

		[OptionalField]
		public List<GuildData.Bets> bets = new List<GuildData.Bets>();

		[OptionalField]
		public DateTime weeklyLovers = DateTime.Now;

		[OptionalField]
		public List<GuildData.UserPicks> up = new List<GuildData.UserPicks>();

		private static int disable = 0;

		public ulong GuildId
		{
			get
			{
				return this._guildID;
			}
		}

		public bool WeeklyLoversReset
		{
			get
			{
				return DateTime.Now >= this.weeklyLovers;
			}
		}

		public void SetWeeklyLovers(int day)
		{
			this.weeklyLovers = DateTime.Now.AddDays((double)day);
		}

		public void SaveData()
		{
			Program.AllGuildData.Add(this);
		}

		public void LoadData(GuildData guildData)
		{
			bool flag = this._guildID != guildData._guildID;
			if (!flag)
			{
				foreach (GuildData.AutoPromotions item in guildData._autoPromotions)
				{
					this._autoPromotions.Add(item);
				}
				this.weeklyLovers = guildData.weeklyLovers;
			}
		}

		public void SetAutoPromotion(string roleName, string xpCategory, int level)
		{
			this._autoPromotions.Add(new GuildData.AutoPromotions(roleName, xpCategory, level));
		}

		public GuildData(ulong guildID)
		{
			this._guildID = guildID;
		}

		[Serializable]
		public class UserPicks
		{
			public List<ulong> IDNum = new List<ulong>();

			public string pingListName;

			public UserPicks(string n)
			{
				this.pingListName = n;
			}

			public void AddToPingList(ulong id)
			{
				this.IDNum.Add(id);
			}
		}

		[Serializable]
		public class Bets
		{
			public readonly string betName;

			public List<ulong> users = new List<ulong>();

			public readonly ulong creator;

			public readonly ulong guild;

			public readonly int betAmt;

			public int pool;

			public string BetInfo()
			{
				return string.Format("{0} - total pool amt {1}", this.betName, this.pool);
			}

			public int PayOut(ulong userId)
			{
				bool flag = this.creator != userId;
				int result;
				if (flag)
				{
					result = 0;
				}
				else
				{
					result = this.pool;
				}
				return result;
			}

			public Bets(string betName, ulong creator, ulong guild, int betAmt)
			{
				this.betName = betName;
				this.creator = creator;
				this.guild = guild;
				bool flag = betAmt > 5000;
				if (flag)
				{
					betAmt = 5000;
				}
				this.betAmt = betAmt;
			}

			public int AddBet(string bn, ulong userCheck)
			{
				bool flag = bn.ToLower() != this.betName.ToLower();
				int result;
				if (flag)
				{
					result = 1;
				}
				else
				{
					foreach (ulong num in this.users)
					{
						bool flag2 = userCheck == num;
						if (flag2)
						{
							return 2;
						}
					}
					this.pool += this.betAmt;
					result = 0;
				}
				return result;
			}
		}

		[Serializable]
		private class AutoPromotions
		{
			private readonly string _roleName;

			private readonly string _associatedExpName;

			private readonly int _level;

			public AutoPromotions(string roleName, string xpName, int level)
			{
				this._roleName = roleName;
				this._level = level;
				this._associatedExpName = xpName;
			}

			public void Promote(ref SocketGuildUser user, SocketGuild sr, UserData ud)
			{
				bool flag = !ud.CheckLevels(this._roleName, this._level);
				if (!flag)
				{
					foreach (SocketRole socketRole in user.Roles)
					{
						bool flag2 = this._roleName == socketRole.Name;
						if (flag2)
						{
							return;
						}
					}
					foreach (SocketRole socketRole2 in sr.Roles)
					{
						bool flag3 = socketRole2.Name == this._roleName;
						if (flag3)
						{
							user.AddRoleAsync(socketRole2, null);
						}
					}
				}
			}
		}
	}
}
