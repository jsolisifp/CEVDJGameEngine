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
        public string explosionClipId;
        public float explosionOpacity;

        Vector3 direction;
        bool hit;
        float time;
        float lifetime = 3;
        public Projectile()
        {
            createsExplosion = false;

            explosionShader = "";
            explosionTexture = "";
            explosionClipId = "";
            explosionOpacity = 1;
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
            List<Component> components = gameObject.GetComponents();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] != this) components[i].Stop();
            }

            SceneManager.GetActiveScene().RemoveGameObject(gameObject);

            if (hit && createsExplosion)
            {
                Explosion.CreateExplosion(
                    gameObject.transform.TransformPosition(explosionOriginOffset), teamId, explosionDamage,
                    explosionDuration, explosionFinalScale, explosionShader, explosionTexture, explosionClipId, explosionOpacity);
            }
        }
    }
}
