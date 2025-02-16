using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console.Utility
{
    internal static class MyUtils
    {
        /**
         * formula taken from StackOverflow https://stackoverflow.com/questions/218060/random-gaussian-variables
         * @return random number with distribution N(mean, stdDev^2)
         */
        public static decimal NormalDistribution(decimal mean, decimal stdDev)
        {
            Random rand = new Random(); //reuse this if you are generating many
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + (decimal)randStdNormal * stdDev;
        }

        /**
         * converts string to double
         * @return the double, -1 is failed
         */
        public static decimal StringToDecimal(string str)
        {
            try
            {
                return Convert.ToDecimal(str);
            }
            catch
            {
                return -1;
            }
        }

        /**
         * @return 0 if failed
         */
        public static uint StringToUInt(string str)
        {
            try
            {
                return Convert.ToUInt32(str);
            }
            catch
            {
                return 0;
            }
        }

        public static string StringAfterCommand(string input, string command)
        {
            if (input.Length <= command.Length) return "";
            return input.Substring(command.Length + 1);
        }
    }
}
