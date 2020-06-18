using NepBot.Resources.Database.Characters;
using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database
{
    [Serializable]
    public class PlayerData : CreateEntity
    {
        public PlayerData(ulong playerid)
        {
            hp = new BaseValues(100, 100, 100, 0);
            mp = new BaseValues(25, 25, 25, 0);
            patk = new BaseValues(9999, 0);
            matk = new BaseValues(9999, 0);
            pdef = new BaseValues(9999, 0);
            mdef = new BaseValues(9999, 0);
            agi = new BaseValues(50, 25, 5);
            _allData = new RPGSaveData
            {
                playerID = playerid
            };

            AddToJobList(new Paladin(this));
            AddToJobList(new DarkKnight(this));

            CurrentJob = AllJobs[0];

            patk.Current = PATKValue;
            matk.Current = MATKValue;
            pdef.Current = PDefValue;
            mdef.Current = MDefValue;
        }
    }
}