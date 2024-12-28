using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace GameEngine
{
    internal class Meka : Component
    {
        public Weapon leftWeapon;
        public Weapon rightWeapon;

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
        public Vector3 currentAim;
        public Vector3 armsRestingRotation;
        public Vector3 speed;

        public bool leftLeverIsGrabbed;
        public bool rightLeverIsGrabbed;

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


            currentAim = Vector3.Zero;
            armsRestingRotation = new Vector3(39,-10,20);
            speed = Vector3.Zero;

            torsoOffset = new Vector3(0,0.8f,0);
            armsOffset = new Vector3(0.7f,0.57f,0);
            pilotOffset = new Vector3(0,0.4f,-0.15f);
            leverOffset = new Vector3(0.2f,0.35f,0.2f);
            legOffset = Vector3.Zero;
            handOffset = new Vector3(0,-0.2f,0.75f);

            models = ["CajaGatoTorso.obj", "CajaGatoBrazoIzq.obj", "CajaGatoBrazoDer.obj", "CajaGatoPiernas.obj", "CajaGatoPilotoTmp.obj", "CajaGatoPalanca.obj", "CajaGatoPalanca.obj"];
            shader = "Default.shader";
            textures = ["Cardboard.png", "Cardboard.png", "Cardboard.png", "Cardboard.png", "Blue.png", "Gray.png", "Gray.png"];

            rightArmOffset = Vector3.Zero;
            rightLeverOffset = Vector3.Zero;
            transforms = new Transform[7];
            for(int i = 0; i < 7; i++)
            {
                transforms[i] = new Transform();
            }
            
        }

        public override void Update(float deltaTime)
        {
            transforms[0].position = gameObject.transform.TransformPosition(torsoOffset); //Torso
            transforms[3].position = transforms[0].TransformPosition(legOffset); //Piernas

            if (speed != Vector3.Zero && !isFlying)
            {
                transforms[0].position.Y -= 0.1f;
                transforms[3].scale.Y = 0.875f;
            }
            else
            {
                transforms[3].scale.Y = 1;
            }

            transforms[1].position = transforms[0].TransformPosition(armsOffset); //BrazoIzquierdo
            rightArmOffset = armsOffset * 1;
            rightArmOffset.X = -rightArmOffset.X;
            transforms[2].position = transforms[0].TransformPosition(rightArmOffset); //BrazoDerecho
            transforms[4].position = transforms[0].TransformPosition(pilotOffset); //Piloto
            transforms[5].position = transforms[0].TransformPosition(leverOffset); //PalancaIzq
            rightLeverOffset = leverOffset * 1;
            rightLeverOffset.X = -leverOffset.X;
            transforms[6].position = transforms[0].TransformPosition(rightLeverOffset); //PalancaDer

            if (isAiming && isFlying)
            {
                transforms[0].LookAt(transforms[0].position - (currentAim - transforms[0].position), Meka.vectorUp);
            }
            else if (isAiming && !isFlying)
            {
                Vector3 tmpAim = currentAim;
                tmpAim.Y = transforms[0].position.Y;
                transforms[0].LookAt(transforms[0].position - (tmpAim - transforms[0].position), Meka.vectorUp);
            }
            else if(!isAiming && isFlying)
            {
                transforms[0].rotation.Y = gameObject.transform.rotation.Y;
                transforms[0].rotation.X = speed.Z / maxFlightSpeed * 50 + gameObject.transform.rotation.X;
                transforms[0].rotation.Z = -speed.X / maxFlightSpeed * 50 + gameObject.transform.rotation.Z;
            }
            else
            {
                transforms[0].rotation = gameObject.transform.rotation;
                transforms[0].rotation.X = speed.Z / maxGroundSpeed * 25 + gameObject.transform.rotation.X;
            }

            for (int i = 1; i < 7; i++) {
                transforms[i].rotation = transforms[0].rotation;
            }

            if (!isFlying && !isAiming)
            {
                transforms[3].rotation = gameObject.transform.rotation;
            }

            if (isAiming)
            {
                transforms[1].LookAt(transforms[1].position - (currentAim - transforms[1].position),Meka.vectorUp);
                transforms[2].LookAt(transforms[2].position - (currentAim - transforms[2].position), Meka.vectorUp);
            }
            else
            {
                transforms[1].rotation += armsRestingRotation;
                rightArmRestingRotation = armsRestingRotation * -1;
                rightArmRestingRotation.X *= -1;
                transforms[2].rotation += rightArmRestingRotation;
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
                transforms[5].LookAt(transforms[0].TransformPosition(leverOffset- Meka.vectorUp),Vector3.UnitZ);
            }


            if (!rightLeverIsGrabbed)
            {
                transforms[6].LookAt(transforms[0].TransformPosition(rightLeverOffset - Meka.vectorUp), Vector3.UnitZ);
            }

            if (speed != Vector3.Zero)
            {
                gameObject.transform.position += gameObject.transform.TransformDirection(speed)*deltaTime;
            }
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

                if (m!=null && t!=null) {
                    GameEngine.Render.DrawModel(transforms[i].position, transforms[i].rotation, transforms[i].scale,m,s,t);
                }
            }
        }

        public void InputMeka(float deltaTime, Vector3 input, float rotation)
        {

            gameObject.transform.rotation.Y += rotation * 100 * deltaTime;

            if (!isFlying)
            {
                if (input.X != 0 || input.Z != 0)
                {
                    speed += input * groundAcceleration;
                }
                if (input.X == 0)
                {
                    if (speed.X > 0)
                    {
                        speed.X -= groundDeceleration;
                        if (speed.X < 0) speed.X = 0;
                    }
                    else if (speed.X < 0)
                    {
                        speed.X += groundDeceleration;
                        if (speed.X > 0) speed.X = 0;
                    }
                }
                if(input.Z == 0) {
                    if (speed.Z > 0)
                    {
                        speed.Z -= groundDeceleration;
                        if (speed.Z < 0) speed.Z = 0;
                    }
                    else if (speed.Z < 0)
                    {
                        speed.Z += groundDeceleration;
                        if (speed.Z > 0) speed.Z = 0;
                    }
                }

                if (speed.X > maxGroundSpeed) { speed.X = maxGroundSpeed; }
                else if (speed.X < -maxGroundSpeed) { speed.X = -maxGroundSpeed; }

                if (speed.Z > maxGroundSpeed) { speed.Z = maxGroundSpeed; }
                else if (speed.Z < -maxGroundSpeed) { speed.Z = -maxGroundSpeed; }

            }
            else
            {

            }
        }
    }
}
