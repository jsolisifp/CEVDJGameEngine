using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BepuPhysics.Collidables;

namespace GameEngine
{
    internal class Projectile : Component
    {
        public int teamId;
        public int damage;
        public float speed;

        public bool createsExplosion;
        public Vector3 explosionOriginOffset;

        public string explosionShader;
        public string explosionTexture;

        public int explosionDamage;
        public Vector3 explosionFinalScale;
        public float explosionDuration;

        Vector3 direction;
        bool hit;
        float time;
        float lifetime = 5;
        public Projectile()
        {
            createsExplosion = false;

            explosionShader = "";
            explosionTexture = "";
        }


        public override void Start()
        {
            direction = gameObject.transform.GetForward();
            hit = false;
        }
        public override void Update(float deltaTime)
        {
            gameObject.transform.position += direction * speed * deltaTime;
            if (time > lifetime) Stop();
            else time += deltaTime;
        }
        public override void OnCollisionEnter(Physics.Collision collision)
        {
            GameObject go = collision.transform.GetGameObject();
            Trigger trigger = go.GetComponent<Trigger>();
            Projectile projectile = go.GetComponent<Projectile>();
            Target target = go.GetComponent<Target>();
            if (trigger != null || projectile != null || target != null && target.teamId == teamId) return;
            if (target != null) target.Damage(damage);
            hit = true;
            Stop();
        }

        public override void Stop()
        {
            if (!hit) return;
            List<Component> components = gameObject.GetComponents();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] != this) components[i].Stop();
            }

            SceneManager.GetActiveScene().RemoveGameObject(gameObject);

            if (createsExplosion)
            {
                Explosion.CreateExplosion(
                    gameObject.transform.TransformPosition(explosionOriginOffset), teamId, explosionDamage,
                    explosionDuration, explosionFinalScale, explosionShader, explosionTexture);
            }
        }
        static int projectileCount = 0;
        public static void CreateProjectile(Transform transform, Renderer renderer, Physics.ColliderType colliderType, int teamId, int damage, float speed, bool createsExplosion, Vector3 explosionOriginOffset, string explosionShader, string explosionTexture, int explosionDamage, Vector3 explosionFinalScale, float explosionDuration)
        {
            GameObject go = new GameObject();
            go.name = "Projectile "+projectileCount++;
            go.AddComponent(transform);
            go.AddComponent(renderer);

            if (colliderType == Physics.ColliderType.box)
            {
                BoxCollider boxC = new BoxCollider();
                boxC.size = new Vector3(1, 1, 1);

                go.AddComponent(boxC);
            }
            else
            {
                SphereCollider sphereC = new SphereCollider();
                sphereC.radius = 0.5f;

                go.AddComponent(sphereC);
            }

            Rigidbody rigidC = new Rigidbody();
            rigidC.isKinematic = true;

            go.AddComponent(rigidC);

            Projectile p = new Projectile();
            p.teamId = teamId;
            p.damage = damage;
            p.speed = speed;
            p.createsExplosion = createsExplosion;
            p.explosionOriginOffset = explosionOriginOffset;
            p.explosionShader = explosionShader;
            p.explosionTexture = explosionTexture;
            p.explosionDamage = explosionDamage;
            p.explosionFinalScale = explosionFinalScale;
            p.explosionDuration = explosionDuration;
            go.AddComponent(p);

            SceneManager.GetActiveScene().AddGameObject(go);
            go.Start();
        }
    }
}
