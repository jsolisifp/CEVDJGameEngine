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

        public Vector3 canonOffset = new Vector3(0,0.1f,0.25f);
        public Vector3 projectileScale = new Vector3(0.1f, 0.1f, 0.3f);

        public string texture;
        public string shader;
        public string model;

        public int maxAmmo = 10;
        public int ammo = 10;
        public int damage = 5;
        public int projectileSpeed = 100;
        public float fireRate = 0.1f;
        float time;

        public bool isReloading;
        public float reloadTime = 2;

        Transform transform;
        int teamId;
        bool shoot;
        Meka meka;
        
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

        public override void Start()
        {
            meka = gameObject.GetComponent<Meka>();
            time += fireRate;
        }

        public override void Update(float deltaTime)
        {
            if (shoot && time > fireRate && !isReloading && meka.isAiming)
            {
                ammo--;
                Transform t = new Transform();
                t.position = transform.TransformPosition(canonOffset);
                t.rotation = transform.rotation;
                t.scale = projectileScale;
                Renderer r = new Renderer();
                r.modelId = "UnitBox.obj";
                r.shaderId = "Default.shader";
                r.textureId = "Yellow.png";

                Projectile.CreateProjectile(t, r, Physics.ColliderType.box, teamId, damage, projectileSpeed, true,
                    new Vector3(0,0,0.15f),"Default.shader", "Yellow.png", 0, new Vector3(0.25f), 0.25f);
                time = 0;
                if(ammo <= 0) isReloading = true;
            }

            if(isReloading && time > reloadTime){ isReloading = false; ammo = maxAmmo; }
            time += deltaTime;
            shoot = false;
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

        public void Shoot()
        {
            shoot = true;
        }

        public void SetTeamId(int teamId)
        {
            this.teamId = teamId;
        }
    }
}
