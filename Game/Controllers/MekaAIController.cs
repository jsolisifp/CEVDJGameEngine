using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class MekaAIController : Component, IMekaController
    {
        public Transform mekaTransform;
        public Transform targetingZone;

        public Vector3 targetingOffset;

        public bool isTurret;

        public float preferedDistanceFromTarget;

        Meka meka;
        Target mekaTarget;

        Target currentTarget;
        List<Target> targets;

        public override void Start()
        {
            targets = new List<Target>();
            if (mekaTransform == null) return;
            meka = mekaTransform.GetGameObject().GetComponent<Meka>();
            mekaTarget = meka.hitBox.GetGameObject().GetComponent<Target>();
            
        }

        public override void Update(float deltaTime)
        {
            if (mekaTransform == null || targetingZone == null) return;
            TargetingSystem();

            if(isTurret) TurretBehaviour();

            if (meka.hp < 0) Stop();
            
        }

        public override void Stop()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (meka.hp<=0 && !scene.GetGameObjects().Contains(gameObject))
            {
                GameObject targeting = targetingZone.GetGameObject();
                targeting.Stop();
                scene.RemoveGameObject(targeting);
            }
        }

        private void TargetingSystem()
        {
            targetingZone.rotation = mekaTransform.rotation;
            targetingZone.position = mekaTransform.TransformPosition(targetingOffset);

            if (targets.Count == 0 || currentTarget != null && currentTarget.teamId == -1) { currentTarget = null; return; }
            else if (meka.isTargetLock && currentTarget != null && currentTarget.teamId != -1) return;
            meka.isTargetLock = false;
            Target tmpTarget = null;
            Vector3 tmpVector;
            float distance = -1;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].teamId == -1)
                {
                    targets.RemoveAt(i);
                    if (i >= targets.Count) continue;
                }
                tmpVector = mekaTransform.InverseTransformPosition(targets[i].GetGameObject().transform.position);
                if (i == 0 || tmpVector.Length() < distance)
                {
                    tmpTarget = targets[i];
                    distance = tmpVector.Length();
                }
            }
            currentTarget = tmpTarget;
        }

        public void TurretBehaviour()
        {
            if (meka == null) return;

            if (currentTarget!=null)
            {
                meka.InputMovement(Vector3.Zero, 0, currentTarget);
                Vector3 distance = currentTarget.GetGameObject().transform.position - meka.currentAim;
                if (distance.Length() < 2)
                {
                    meka.Shoot(0);
                    meka.Shoot(1);
                }
            }
            
        }

        public void AddTarget(Target target)
        {
            if (target.teamId == -1 || target.teamId == mekaTarget.teamId || targets.Contains(target)) return;
            targets.Add(target);
        }

        public void RemoveTarget(Target target)
        {
            if (currentTarget == target && targets.Count > 1) currentTarget = null;
            else if (currentTarget == target && targets.Count == 1) return;
            targets.Remove(target);
        }


    }
}
