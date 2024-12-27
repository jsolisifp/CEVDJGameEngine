using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Meka : Component
    {
        public Weapon leftWeapon;
        public Weapon rightWeapon;

        public bool isAiming;
        public Vector3 currentAim;
        public Vector3 armsRestingRotation;

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


        Transform[] transforms; //0 Torso, 1 BrazoIzq, 2 BrazoDer, 3 Piernas, 4 Piloto, 5 PalancaIzq, 6 PalancaDer

        public Meka()
        {

            currentAim = Vector3.Zero;

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
            transforms[1].position = transforms[0].TransformPosition(armsOffset); //BrazoIzquierdo
            rightArmOffset = armsOffset * 1;
            rightArmOffset.X = -rightArmOffset.X;
            transforms[2].position = transforms[0].TransformPosition(rightArmOffset); //BrazoDerecho
            transforms[3].position = transforms[0].TransformPosition(legOffset); //Piernas
            transforms[4].position = transforms[0].TransformPosition(pilotOffset); //Piloto
            transforms[5].position = transforms[0].TransformPosition(leverOffset); //PalancaIzq
            rightLeverOffset = leverOffset * 1;
            rightLeverOffset.X = -leverOffset.X;
            transforms[6].position = transforms[0].TransformPosition(rightLeverOffset); //PalancaDer

            for (int i = 0; i < 7; i++) {
                transforms[i].rotation.Y = gameObject.transform.rotation.Y;
            }
            if (isAiming)
            {
                transforms[1].LookAt(transforms[1].position - (currentAim - transforms[1].position),Vector3.UnitY);
                transforms[2].LookAt(transforms[2].position - (currentAim - transforms[2].position),Vector3.UnitY);
            }
            else
            {
                transforms[1].rotation = armsRestingRotation;
                transforms[2].rotation = armsRestingRotation;
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
                transforms[5].LookAt(transforms[0].TransformPosition(leverOffset-Vector3.UnitY),Vector3.UnitZ);
            }


            if (!rightLeverIsGrabbed)
            {
                transforms[6].LookAt(transforms[0].TransformPosition(rightLeverOffset - Vector3.UnitY), Vector3.UnitZ);
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

    }
}
