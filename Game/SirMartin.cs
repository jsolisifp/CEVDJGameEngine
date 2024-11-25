using System.Numerics;

namespace GameEngine
{
    internal class SirMartin : Component
    {
        public int bullets;
        public bool isNear;

        public override void Update(float deltaTime)
        {
            Vector3 p = gameObject.transform.position;

            p += new Vector3(0, 0, 2) * deltaTime;

            gameObject.transform.position = p;


        }

        public override void OnTriggerEnter(Rigidbody other)
        {
            //Console.WriteLine("SirMartin TriggerEnter: " + other.GetGameObject().name);

            //other.AddForce(Vector3.UnitY * 10, Physics.ForceMode.impulse);
            other.AddForce(Vector3.UnitY * 10, Physics.ForceMode.velocityChange);
            //other.AddTorque(Vector3.UnitY * 10, Physics.ForceMode.impulse);
            other.AddTorque(Vector3.UnitY * 10, Physics.ForceMode.velocityChange);
        }
        public override void OnTriggerStay(Rigidbody other)
        {
            //Console.WriteLine("SirMartin TriggerStay: " + other.GetGameObject().name);
        }

        public override void OnTriggerExit(Rigidbody other)
        {
            //Console.WriteLine("SirMartin TriggerExit: " + other.GetGameObject().name);
        }

    }
}
