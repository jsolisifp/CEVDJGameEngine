using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal interface IMekaController
    {
        public void AddTarget(Target target);
        public void RemoveTarget(Target target);
    }
}
