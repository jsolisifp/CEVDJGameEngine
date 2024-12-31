using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class TargetingZone : Component
    {
        public Transform controller;
        IMekaController m_controller;
        public override void Start()
        {
            m_controller = controller.GetGameObject().GetComponent<MekaPlayerController>();
            if(m_controller != null ) return;
            //Añadir los siguientes tipos de controlador aqui.
        }

        public override void OnTriggerEnter(Rigidbody other)
        {
            Target target = other.GetGameObject().GetComponent<Target>();
            if( target != null ) m_controller.AddTarget( target );
        }

        public override void OnTriggerExit(Rigidbody other)
        {
            Target target = other.GetGameObject().GetComponent<Target>();
            if (target != null) m_controller.RemoveTarget(target);
        }
    }
}
