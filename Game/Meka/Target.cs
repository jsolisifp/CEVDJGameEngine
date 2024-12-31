using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Target : Component
    {
        public Transform mekaTransform;
        public int teamId;
        Meka meka;

        public override void Start()
        {
            if(mekaTransform == null) return;
            meka = mekaTransform.GetGameObject().GetComponent<Meka>();
        }

        public override void OnCollisionEnter(Physics.Collision collision)
        {
            if (meka == null) return;
            meka.OnCollisionEnter(collision);
        }

        public override void OnCollisionStay(Physics.Collision collision)
        {
            if (meka == null) return;
            meka.OnCollisionStay(collision);
        }
    }
}
