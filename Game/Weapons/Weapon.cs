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
        public bool showCanonPosition;

        public string texture;
        public string shader;
        public string model;

        public int projectilesPerShoot = 1;
        public int maxAmmo = 10;
        public int ammo = 10;
        public float spread = 15;

        public string projectilePreset = "Proyectil.preset";

        public int damage = 5;
        public int explosionDamage = 0;
        public int projectileSpeed = 100;
        public float fireRate = 0.1f;

        public bool isReloading;
        public float reloadTime = 2;

        
       
        float time;
        Preset projectile;

        Transform transform;
        int teamId;
        bool shoot;
        Meka meka;
        Random r;
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
            r = new Random();
            projectile = Assets.GetLoadedAsset<Preset>(projectilePreset);
            if (projectile != null) projectile.PrepareCopy();
        }

        public override void Update(float deltaTime)
        {
            if (shoot && time > fireRate && !isReloading && meka.isAiming)
            {
                for (int i = 0; i < projectilesPerShoot; i++)
                {
                    GameObject go = projectile.GetGameObjectsCopies().First();
                    go.transform.position = transform.TransformPosition(canonOffset);
                    go.transform.rotation = transform.rotation;
                    if(spread > 0)
                    {
                        go.transform.rotation.X += RandUtils.Range(r, -spread, +spread);
                        go.transform.rotation.Y += RandUtils.Range(r, -spread, +spread);
                        go.transform.rotation.Z += RandUtils.Range(r, -spread, +spread);
                    }

                    Projectile p = go.GetComponent<Projectile>();
                    p.damage = damage;
                    p.explosionDamage = explosionDamage;
                    p.speed = projectileSpeed;
                    p.teamId = teamId;

                    SceneManager.GetActiveScene().AddGameObject(go);
                    go.Start();
                }

                ammo--;
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

            if (showCanonPosition) {
                m = Assets.GetLoadedAsset<Model>("UnitBox.obj");
                s = Assets.GetLoadedAsset<Shader>(shader);
                t = Assets.GetLoadedAsset<Texture>("Purple.png");
                if (m != null && s != null && t != null)
                {
                    GameEngine.Render.DrawModel(transform.TransformPosition(canonOffset), transform.rotation, new Vector3(0.2f), m, s, t);
                }
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
