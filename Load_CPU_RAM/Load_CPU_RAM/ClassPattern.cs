using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Load_CPU_RAM
{
    internal class ClassPattern
    {
        private static readonly Random random = new Random(DateTime.Now.Millisecond); // генератор случайных чисел

       // static int multiplier = Convert.ToInt32(MainForm.multiplier);
        static int length = Convert.ToInt32(MainForm.multiplier) * 8 * 1024 * 1024 / 24;


        private readonly int[] FillPattern = new int[length];



        public ClassPattern()
        {
           

            for (int i = 0; i < length; i++)
            {
                FillPattern[i] = (int)random.Next(minValue: 0, maxValue: 2);
            }
        }
    }
}
