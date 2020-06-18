using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Database.Characters
{
    public class EnemyGeneric : JobsSuper
    {
        public EnemyGeneric(BaseValues patk, BaseValues matk, BaseValues armor, BaseValues magarmor, CreateEntity th)
        {
            thePlayer = th;
            jobName = "Enemies";
            physicalWeaponValue = patk;
            magicalWeaponValue = matk;
            armorValue = armor;
            magArmorValue = magarmor;
        }

    }
}