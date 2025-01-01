using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Explosion : Component
    {
        int teamId;
        int damage;
        float duration;
        Vector3 finalScale;
        float time;

        public override void Start()
        {
            time = 0;
        }
        public override void Update(float deltaTime)
        {
            gameObject.transform.scale = finalScale * time/duration;
            if (time < duration) time += deltaTime;
            else Stop();
        }

        public override void OnTriggerEnter(Rigidbody other)
        {
            Target target = other.GetGameObject().GetComponent<Target>();
            if (target != null && target.teamId != teamId) target.Damage(damage);
        }

        public override void Stop()
        {
            List<Component> components = gameObject.GetComponents();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] != this) components[i].Stop();
            }

            SceneManager.GetActiveScene().RemoveGameObject(gameObject);
        }

        static int explosionCount = 0;
        public static void CreateExplosion(Vector3 pos, int teamId, int dmg, float dur, Vector3 fScale, string shader, string texture)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Explosion " + explosionCount++;
            gameObject.AddComponent(new Transform());
            gameObject.transform.position = pos;
            gameObject.transform.scale = Vector3.Zero;

            SphereCollider collider = new SphereCollider();
            collider.radius = 0.5f;
            gameObject.AddComponent(collider);

            Renderer renderer = new Renderer();
            renderer.modelId = "UnitSphere.obj";
            renderer.shaderId = shader;
            renderer.textureId = texture;
            gameObject.AddComponent(renderer);

            Explosion explosion = new Explosion();
            explosion.teamId = teamId;
            explosion.damage = dmg;
            explosion.duration = dur;
            explosion.finalScale = fScale;
            gameObject.AddComponent(explosion);

            SceneManager.GetActiveScene().AddGameObject(gameObject);
            gameObject.Start();
        }
    }
}
