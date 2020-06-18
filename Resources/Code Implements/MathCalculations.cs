using NepBot.Data;
using NepBot.Resources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepBot.Resources.Code_Implements
{
    [Serializable]
    public class MathCalculations
    {

        public ulong DailyPudding(UserData ud)
        {
            //public enum Type { c, r, sr, ssr };
            ulong formula = (ulong)(1000 + ((ud.ParaLevel * 5 + ud.CasualLevel * 2 + ud.NonLevel) * (25 + ud.CardValueGains)));            
            return formula;
        }

        public bool CanBet(ulong betAmt, ulong playerTotal)
        {
            return playerTotal > betAmt;
        }

        public ulong TotalBet(int playerLevel, UserData ud)
        {
            return (ulong)playerLevel * (ulong)(25 + ud.CardValueGains);
        }

        public double SecondsToMilliseconds(double seconds)
        {
            return 1000d * seconds;
        }

        //backup of previous math
        /*public ulong PotencyCalculations(ulong potency, ulong atk)
        {
            decimal pot = (decimal)potency / 999m;
            decimal at = (decimal)atk;

            return (ulong)(at + (at * pot));
        }*/

        /// <summary>
        /// Calculates skill potency in the unfinished RPG game.
        /// </summary>
        /// <param name="potency"></param>
        /// <param name="atk"></param>
        /// <returns></returns>
        public decimal PotencyCalculations(int potency, int atk)
        {
            decimal pot = (decimal)potency;
            decimal at = (decimal)atk;
            decimal total = at + at * pot / 100m;
            //ExtensionMethods.WriteToLog($"\nPotency Modifier: {pot}\nAttack: {atk}\nTotal: {total}");
            return total;
        }
    }
}

public static class UtilityClass
{
    private static Random rnd = new Random();
    public static int ReturnRandom(int min, int max)
    {
        return rnd.Next(min, max);
    }
}
