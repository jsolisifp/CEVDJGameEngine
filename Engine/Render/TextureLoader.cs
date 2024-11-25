using Silk.NET.OpenGL;

namespace GameEngine
{
    internal class TextureLoader : AssetLoader
    {
        GL openGL;

        public TextureLoader(GL gl)
        {
            openGL = gl;
        }

        public override object LoadAsset(string path)
        {
            return new Texture(openGL, path);
        }

        public override void UnloadAsset(object asset)
        {
            Texture t = (Texture)asset;
            t.Dispose();
        }
    }
}
