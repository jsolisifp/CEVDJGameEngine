using Silk.NET.OpenGL;

namespace GameEngine
{
    internal class Renderer : Component
    {
        public string modelId;
        public string shaderId ;
        public string textureId;

        public float opacity = 1;

        public override void Render(float deltaTime)
        {
            Model m = Assets.GetLoadedAsset<Model>(modelId);
            Texture t = Assets.GetLoadedAsset<Texture>(textureId);
            Shader s = Assets.GetLoadedAsset<Shader>(shaderId);

            if(m != null && t != null && s != null)
            {
                Shader.BlendType blendType = s.GetBlendType();
                if (blendType == Shader.BlendType.transparent || blendType == Shader.BlendType.additive)
                {
                    GameEngine.Render.SetOpacity(opacity);
                }

                Transform tf = gameObject.transform;
                GameEngine.Render.DrawModel(tf.position, tf.rotation, tf.scale, m, s, t);
            }
        }
    }
}
