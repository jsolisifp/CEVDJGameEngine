using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class PresetLoader : AssetLoader
    {
        public override object LoadAsset(string path)
        {
            Preset p;

            if (path.EndsWith(".component"))
            {
                p = new Preset(SceneSerializer.DeserializeComponent(path));
            }
            else
            {
                p = new Preset(SceneSerializer.Deserialize(path));
            }

            return p;
        }

        public override void UnloadAsset(object asset)
        {
        }
    }
}
