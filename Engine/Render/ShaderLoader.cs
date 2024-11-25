using Silk.NET.OpenGL;
using static GameEngine.Shader;

namespace GameEngine
{
    internal class ShaderLoader : AssetLoader
    {
        GL openGL;

        public ShaderLoader(GL gl)
        {
            openGL = gl;
        }

        public override object LoadAsset(string path)
        {
            string shader = Assets.LoadText(path);
            string[] areas = shader.Split("====");
            string blending = areas[0].Split(" ")[1].Trim();
            Shader.BlendType blendType;
            if (blending == "transparent") { blendType = Shader.BlendType.transparent; }
            else if (blending == "additive") { blendType = Shader.BlendType.additive; }
            else { blendType = BlendType.opaque; }
            return new Shader(openGL, areas[1], areas[2], blendType);
        }

        public override void UnloadAsset(object asset)
        {
            Shader s = (Shader)asset;
            s.Dispose();
        }
    }
}
