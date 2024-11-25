namespace GameEngine
{
    internal class SceneLoader : AssetLoader
    {
        public override object LoadAsset(string path)
        {
            Scene scene;
            
            scene = SceneSerializer.Deserialize(path);

            return scene;

        }

        public override void UnloadAsset(object asset)
        {
        }

    }
}
