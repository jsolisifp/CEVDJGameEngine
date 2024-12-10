using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class RandUtils
    {
        public static float Range(Random r, float min, float max)
        {
            float result = min + r.NextSingle() * (max - min);

            return result;
        }
    }
}
