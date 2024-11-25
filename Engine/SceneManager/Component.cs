namespace GameEngine
{
    internal abstract class Component
    {
        public bool active;

        protected GameObject gameObject;

        public virtual void Start() { }
        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float deltaTime) { }
        public virtual void OnCollisionEnter(Physics.Collision collision) { }
        public virtual void OnCollisionStay(Physics.Collision collision) { }
        public virtual void OnCollisionExit(Physics.Collision collision) { }
        public virtual void OnTriggerEnter(Rigidbody other) { }
        public virtual void OnTriggerStay(Rigidbody other) { }
        public virtual void OnTriggerExit(Rigidbody other) { }
        public virtual void Render(float deltaTime) { }
        public virtual void Stop() { }


        public void SetGameObject(GameObject go)
        {
            gameObject = go;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

    }
}
