using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class AudioClip
    {
        uint buffer;
        string path;
        public AudioClip(string path, uint buffer)
        {
            this.path = path;
            this.buffer = buffer;
        }

        public string GetPath()
        {
            return path;
        }

        public uint GetBuffer() { return buffer; }
    }
}
