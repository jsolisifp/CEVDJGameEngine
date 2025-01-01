using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BepuPhysics.Constraints.Contact;
using Silk.NET.Vulkan;

namespace GameEngine
{
    internal class Meka : Component
    {
        public Weapon leftWeapon;
        public Weapon rightWeapon;
        public Transform hitBox;

        public int hp;
        public int maxHp;

        public int energy;
        public int maxEnergy;
        public int energyRefillRate;

        public int dashCost;
        public float dashSpeed;
        public float dashDuration;

        public float jumpHeight;

        public float groundAcceleration;
        public float airAcceleration;
        public float jumpAcceleration;
        public float upwardsAcceleration;
        public float gravityAcceleration;

        public float groundDeceleration;
        public float airDeceleration;

        public float maxGroundSpeed;
        public float maxFlightSpeed;
        public float maxLiftSpeed;
        public float maxFallSpeed;

        public bool isAiming;
        public bool isFlying;
        public bool isTargetLock;

        public Vector3 currentAim;
        public float targetingSpeed;
        Target targetAim;

        public Vector3 armsRestingRotation;
        public Vector3 speed;

        public bool leftLeverIsGrabbed;
        public bool rightLeverIsGrabbed;

        public Vector3 hitboxOffset;

        //Offset relativos de los componentes
        public Vector3 torsoOffset;
        public Vector3 armsOffset; //BrazoIzquierdo, el derecho simplemente invertimos el offsetX
        public Vector3 pilotOffset;
        public Vector3 leverOffset; //Tomamos tambien como refrencia la palanca izquierda e invertimos la otra
        public Vector3 legOffset; //Por lo general sera un vector.Zero
        public Vector3 handOffset;

        public string[] models; //0 Torso, 1 BrazoIzq, 2 BrazoDer, 3 Piernas, 4 Piloto, 5 PalancaIzq, 6 PalancaDer
        public string shader;
        public string[] textures; //0 Torso, 1 BrazoIzq, 2 BrazoDer, 3 Piernas, 4 Piloto, 5 PalancaIzq, 6 PalancaDer



        Vector3 rightArmOffset;
        Vector3 rightLeverOffset;
        Vector3 rightArmRestingRotation;


        Transform[] transforms; //0 Torso, 1 BrazoIzq, 2 BrazoDer, 3 Piernas, 4 Piloto, 5 PalancaIzq, 6 PalancaDer

        static Vector3 vectorUp = Vector3.UnitY;

        public Meka()
        {
            hp = 100;
            maxHp = 100;

            energy = 100;
            maxEnergy = 100;

            groundAcceleration = 2;
            airAcceleration = 4;
            jumpAcceleration = 6;
            upwardsAcceleration = 2;
            gravityAcceleration = 1;

            groundDeceleration = 3;
            airDeceleration = 2;

            maxGroundSpeed = 15;
            maxFlightSpeed = 20;
            maxLiftSpeed = 10;
            maxFallSpeed = 15;

            targetAim = null;
            targetingSpeed = 20;

            currentAim = Vector3.Zero;
            armsRestingRotation = new Vector3(0.7f, 0.4f, 0.5f);
            speed = Vector3.Zero;

            hitboxOffset = new Vector3(0, 0.5f, 0);

            torsoOffset = new Vector3(0, 0.8f, 0);
            armsOffset = new Vector3(0.7f, 0.57f, 0);
            pilotOffset = new Vector3(0, 0.4f, -0.15f);
            leverOffset = new Vector3(0.2f, 0.35f, 0.2f);
            legOffset = Vector3.Zero;
            handOffset = new Vector3(0, -0.2f, 0.75f);

            models = ["CajaGatoTorso.obj", "CajaGatoBrazoIzq.obj", "CajaGatoBrazoDer.obj", "CajaGatoPiernas.obj", "CajaGatoPilotoTmp.obj", "CajaGatoPalanca.obj", "CajaGatoPalanca.obj"];
            shader = "Default.shader";
            textures = ["Cardboard.png", "Cardboard.png", "Cardboard.png", "Cardboard.png", "Blue.png", "Gray.png", "Gray.png"];

            rightArmOffset = Vector3.Zero;
            rightLeverOffset = Vector3.Zero;
            transforms = new Transform[7];
            for (int i = 0; i < 7; i++)
            {
                transforms[i] = new Transform();
            }

        }

        public override void Start()
        {
            int teamId = hitBox.GetGameObject().GetComponent<Target>().teamId;
            if(leftWeapon != null) leftWeapon.SetTeamId(teamId);
            if(rightWeapon != null) rightWeapon.SetTeamId(teamId);
        }

        Vector3 lookAtPosition;
        float lastDeltaTime;
        public override void Update(float deltaTime)
        {

            transforms[0].position = gameObject.transform.TransformPosition(torsoOffset); //Torso
            transforms[3].position = transforms[0].TransformPosition(legOffset); //Piernas

            if (speed != Vector3.Zero && !isFlying)
            {
                transforms[0].position.Y -= 0.15f;
                transforms[3].scale.Y = 0.8f;
            }
            else
            {
                transforms[3].scale.Y = 1;
            }

            AimControl(deltaTime);
            

            transforms[1].position = transforms[0].TransformPosition(armsOffset); //BrazoIzquierdo
            rightArmOffset = armsOffset * 1;
            rightArmOffset.X = -rightArmOffset.X;
            transforms[2].position = transforms[0].TransformPosition(rightArmOffset); //BrazoDerecho
            transforms[4].position = transforms[0].TransformPosition(pilotOffset); //Piloto
            transforms[5].position = transforms[0].TransformPosition(leverOffset); //PalancaIzq
            rightLeverOffset = leverOffset * 1;
            rightLeverOffset.X = -leverOffset.X;
            transforms[6].position = transforms[0].TransformPosition(rightLeverOffset); //PalancaDer

            float maxSpeed, marginX, marginY;

            if (!isFlying)
            {
                maxSpeed = maxGroundSpeed;
                marginX = 0.25f;
                marginY = 1.5f;
            }
            else
            {
                maxSpeed = maxFlightSpeed;
                marginX = 1;
                marginY = 2.5f;
            }

            lookAtPosition = new Vector3(0, 0, 5);

            if (isAiming) 
            {
                Vector3 tmpAim = currentAim;
                tmpAim.Y = transforms[0].position.Y;
                transforms[0].LookAt(transforms[0].position - (tmpAim - transforms[0].position), Meka.vectorUp);
            }
            else
            {
                transforms[0].rotation = gameObject.transform.rotation;
                transforms[0].LookAt(transforms[0].position - (transforms[0].TransformPosition(lookAtPosition) - transforms[0].position), Meka.vectorUp);
            }

            lookAtPosition = new Vector3(0, -speed.Z / maxSpeed * marginY, 5);
            lookAtPosition = transforms[0].TransformPosition(lookAtPosition);
            Vector3 up = transforms[0].TransformDirection(new(speed.X / maxSpeed * marginX, 1, 0));
            transforms[0].LookAt(transforms[0].position - (lookAtPosition - transforms[0].position), up);

            if (isAiming && isTargetLock)
            {
                Vector3 tmpAim = currentAim;
                tmpAim.Y = gameObject.transform.position.Y;
                gameObject.transform.LookAt(gameObject.transform.position - (tmpAim - gameObject.transform.position), Meka.vectorUp);
            }
            else
            {
                gameObject.transform.rotation.X = 0;
                gameObject.transform.rotation.Z = 0;
            }
            

            for (int i = 1; i < 7; i++)
            {
                transforms[i].rotation = transforms[0].rotation;
            }

            if (!isFlying)
            {
                transforms[3].rotation = gameObject.transform.rotation;
                if (isAiming) 
                {
                    Vector3 tmpAim = currentAim;
                    tmpAim.Y = transforms[3].position.Y;
                    transforms[3].LookAt(transforms[3].position - (tmpAim - transforms[3].position), Meka.vectorUp);
                }
                
            }

            if (isAiming && leftWeapon != null && !leftWeapon.isReloading)
            {
                transforms[1].LookAt(transforms[1].position - (currentAim - transforms[1].position), up);
            }
            else
            {
                transforms[1].LookAt(transforms[1].position - (transforms[0].TransformPosition(armsRestingRotation) - transforms[1].position), up);
            }

            if (isAiming && rightWeapon != null && !rightWeapon.isReloading)
            {
                transforms[2].LookAt(transforms[2].position - (currentAim - transforms[2].position), up);
            }
            else
            {
                rightArmRestingRotation = armsRestingRotation;
                rightArmRestingRotation.X = -armsRestingRotation.X;
                transforms[2].LookAt(transforms[2].position - (transforms[0].TransformPosition(rightArmRestingRotation) - transforms[2].position), up);
            }

            if (leftWeapon != null)
            {
                Transform leftWeapTransform = leftWeapon.GetTransform();
                leftWeapTransform.position = transforms[1].TransformPosition(handOffset);
                leftWeapTransform.rotation = transforms[1].rotation;
            }

            if (rightWeapon != null)
            {
                Transform rightWeapTransform = rightWeapon.GetTransform();
                rightWeapTransform.position = transforms[2].TransformPosition(handOffset);
                rightWeapTransform.rotation = transforms[2].rotation;
            }

            if (!leftLeverIsGrabbed)
            {
                transforms[5].LookAt(transforms[0].TransformPosition(leverOffset - Meka.vectorUp), Vector3.UnitZ);
            }


            if (!rightLeverIsGrabbed)
            {
                transforms[6].LookAt(transforms[0].TransformPosition(rightLeverOffset - Meka.vectorUp), Vector3.UnitZ);
            }

            if (speed != Vector3.Zero)
            {
                gameObject.transform.position += gameObject.transform.TransformDirection(speed) * deltaTime;
            }
            if (hitBox != null)
            {
                hitBox.position = transforms[0].TransformPosition(hitboxOffset);
                hitBox.rotation = transforms[0].rotation;
            }

            SpeedControl(deltaTime);
            CheckFloor();

            lastDeltaTime = deltaTime;
        }

        public override void Render(float deltaTime)
        {
            Model m;
            Shader s;
            Texture t;

            s = Assets.GetLoadedAsset<Shader>(shader);
            if (s == null) return;

            for (int i = 0; i < transforms.Length; i++)
            {
                m = Assets.GetLoadedAsset<Model>(models[i]);
                t = Assets.GetLoadedAsset<Texture>(textures[i]);

                if (m != null && t != null)
                {
                    GameEngine.Render.DrawModel(transforms[i].position, transforms[i].rotation, transforms[i].scale, m, s, t);
                }
            }
        }

        private void AimControl(float deltaTime)
        {
            if (targetAim != null)
            {
                Vector3 position = targetAim.GetGameObject().transform.position;
                if (currentAim != position)
                {
                    if (position.X > currentAim.X)
                    {
                        currentAim.X += targetingSpeed * deltaTime;
                        if(currentAim.X > position.X) currentAim.X = position.X;
                    }
                    else if (position.X < currentAim.X)
                    {
                        currentAim.X -= targetingSpeed * deltaTime;
                        if (currentAim.X < position.X) currentAim.X = position.X;
                    }

                    if (position.Y > currentAim.Y)
                    {
                        currentAim.Y += targetingSpeed * deltaTime;
                        if (currentAim.Y > position.Y) currentAim.Y = position.Y;
                    }
                    else if (position.Y < currentAim.Y)
                    {
                        currentAim.Y -= targetingSpeed * deltaTime;
                        if (currentAim.Y < position.Y) currentAim.Y = position.Y;
                    }

                    if (position.Z > currentAim.Z)
                    {
                        currentAim.Z += targetingSpeed * deltaTime;
                        if (currentAim.Z > position.Z) currentAim.Z = position.Z;
                    }
                    else if (position.Z < currentAim.Z)
                    {
                        currentAim.Z -= targetingSpeed * deltaTime;
                        if (currentAim.Z < position.Z) currentAim.Z = position.Z;
                    }

                }
                isAiming = true;
            }
            else
            {
                currentAim = gameObject.transform.TransformPosition(new Vector3(0, 0, 6));
                isAiming = false;
            }
        }

        Vector3 input;
        float rotation;
        public void InputMovement(Vector3 input, float rotation, Target target)
        {
            this.input = input;
            this.rotation = rotation;
            targetAim = target;
        }

        public void Shoot(int weapon)
        {
            if (weapon == 0) leftWeapon.Shoot();
            else rightWeapon.Shoot();
        }

        private void SpeedControl(float deltaTime)
        {
            gameObject.transform.rotation.Y += rotation * 100 * deltaTime;
            float acceleration = isFlying ? airAcceleration : groundAcceleration;
            float deceleration = isFlying ? airDeceleration : groundDeceleration;
            float maxSpeed = isFlying ? maxFlightSpeed : maxGroundSpeed;

            if (input.X == 0)
            {
                if (speed.X > 0)
                {
                    speed.X -= deceleration;
                    if (speed.X < 0) speed.X = 0;
                }
                else if (speed.X < 0)
                {
                    speed.X += deceleration;
                    if (speed.X > 0) speed.X = 0;
                }
            }
            else
            {
                speed.X += input.X * acceleration;
            }

            if (input.Z == 0)
            {
                if (speed.Z > 0)
                {
                    speed.Z -= deceleration;
                    if (speed.Z < 0) speed.Z = 0;
                }
                else if (speed.Z < 0)
                {
                    speed.Z += deceleration;
                    if (speed.Z > 0) speed.Z = 0;
                }
            }
            else
            {
                speed.Z += input.Z * acceleration;
            }

            if (input.Y == 0 && isFlying) speed.Y -= gravityAcceleration;
            else if (input.Y == 1 && !isFlying) speed.Y += jumpAcceleration;
            else if (input.Y == 1 && isFlying) speed.Y += upwardsAcceleration;

            if (speed.X > maxSpeed) { speed.X = maxSpeed; }
            else if (speed.X < -maxSpeed) { speed.X = -maxSpeed; }

            if (speed.Z > maxSpeed) { speed.Z = maxSpeed; }
            else if (speed.Z < -maxSpeed) { speed.Z = -maxSpeed; }

            if (speed.Y > maxLiftSpeed) { speed.Y = maxLiftSpeed; }
            else if (speed.Y < -maxFallSpeed) { speed.Y = -maxFallSpeed; }
        }

        float margin = 0.5f;
        public void CheckFloor()
        {
            bool floor = false;
            Vector3 position;
            Physics.RaycastHit hit;
            for (int i = -1; i < 4 && speed.Y <= 0 && !floor; i++)
            {
                position = new Vector3(i % 2 == 0 ? -margin : margin, 0, i >= 2 ? -margin : margin);
                floor = Physics.Raycast(gameObject.transform.TransformPosition(i!=-1?position:Vector3.Zero), -Vector3.UnitY, 0.1f, out hit);
                if (floor)
                {
                    GameObject go = hit.transform.GetGameObject();
                    Rigidbody rigidbody = go.GetComponent<Rigidbody>();
                    Projectile p = go.GetComponent<Projectile>();
                    if (rigidbody != null && p==null)
                    {
                        gameObject.transform.position.Y -= hit.distance;
                        speed.Y = 0;
                        isFlying = false;
                    }
                    else
                    {
                        floor = false;
                    }
                }
            }

            if (!floor || speed.Y > 0)
            {
                isFlying = true;
            }
        }

        Vector3 enterPosition;
        Vector3 enterDirection;
        public override void OnCollisionEnter(Physics.Collision collision)
        {
            GameObject go = collision.transform.GetGameObject();
            Projectile p = go.GetComponent<Projectile>();
            if (p != null) return;
            if (collision.rigidbody.isKinematic || go.@static){
                float deltaTime = lastDeltaTime > 0.01f ? lastDeltaTime : 0.01f;

                enterDirection = speed != Vector3.Zero ?Vector3.Normalize(speed) : Vector3.Zero;
                enterPosition = gameObject.transform.position - gameObject.transform.TransformDirection(enterDirection) * 0.1f * (deltaTime);
                
            }
            
        }

        public override void OnCollisionStay(Physics.Collision collision)
        {
            GameObject go = collision.transform.GetGameObject();
            Projectile p = go.GetComponent<Projectile>();
            if (p != null) return ;
            if (collision.rigidbody.isKinematic || go.@static)
            {
                
                gameObject.transform.position = enterPosition;
                enterPosition -= gameObject.transform.TransformDirection(enterDirection) * 0.01f;
            }
        }

        public void Damage(int damage)
        {
            hp-=damage;
        }
    }
}
