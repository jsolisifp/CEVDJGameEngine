using Silk.NET.OpenGL;

namespace GameEngine
{
    internal class ModelLoader : AssetLoader
    {
        GL openGL;

        public ModelLoader(GL gl)
        {
            openGL = gl;
        }

        public override object LoadAsset(string path)
        {
            return new Model(openGL, path);
        }

        public override void UnloadAsset(object asset)
        {
            Model m = (Model)asset;
            m.Dispose();
        }
    }
}
