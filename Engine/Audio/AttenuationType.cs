using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public enum AttenuationType
    {
        none,
        inverseDistance,
        inverseDistanceClamped,
        lineatDistance,
        linearDistanceClamped,
        exponentDistance,
        exponentDistanceClamped
    }
}
