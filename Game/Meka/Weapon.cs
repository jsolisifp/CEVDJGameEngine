using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Weapon : Component
    {
        public string name;

        public Vector3 canonOffset; 

        public string texture;
        public string shader;
        public string model;
        
        Transform transform;

        public Weapon()
        {
            name = "Pistola";
            canonOffset = Vector3.Zero;

            texture = "Gray.png";
            shader = "Default.shader";
            model = "ArmaPistola.obj";

            transform = new Transform();
        }

        public Transform GetTransform()
        {
            return transform;
        }
        public void SetTransform(Transform transform)
        {
            this.transform = transform;
        }

        public override void Render(float deltaTime)
        {
            Model m = Assets.GetLoadedAsset<Model>(model);
            Shader s = Assets.GetLoadedAsset<Shader>(shader);
            Texture t = Assets.GetLoadedAsset<Texture>(texture);

            if (m != null && s != null && t != null)
            {
                GameEngine.Render.DrawModel(transform.position,transform.rotation,transform.scale,m,s,t);
            }
        }
    }
}
