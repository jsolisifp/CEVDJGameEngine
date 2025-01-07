using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class AudioClip
    {
        public uint buffer;
        public AudioClip(uint buffer)
        { 
            this.buffer = buffer;
        }
    }
}
