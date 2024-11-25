namespace GameEngine
{
    internal abstract class AssetLoader
    {
        public abstract object LoadAsset(string path);
        public abstract void UnloadAsset(object asset);
    }
}
