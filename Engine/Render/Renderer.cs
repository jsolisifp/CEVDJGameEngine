namespace GameEngine
{
    internal class Renderer : Component
    {
        public string modelId;
        public string shaderId ;
        public string textureId;

        public override void Render(float deltaTime)
        {
            Model m = Assets.GetLoadedAsset<Model>(modelId);
            Texture t = Assets.GetLoadedAsset<Texture>(textureId);
            Shader s = Assets.GetLoadedAsset<Shader>(shaderId);

            if(m != null && t != null && s != null)
            {
                Transform tf = gameObject.transform;
                GameEngine.Render.DrawModel(tf.position, tf.rotation, tf.scale, m, s, t);
            }
        }
    }
}
