namespace GameEngine
{
    internal class GameObject
    {
        public string name;
        public bool active;
        public bool @static;
        public int layer;

        List<Component> components;

        public Transform transform;

        public GameObject()
        {
            name = "";
            active = false;

            components = new List<Component>();

        }

        public void Start()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Start();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Stop();
            }
        }

        public void Update(float deltaTime)
        {
            for(int i = 0; i < components.Count; i++)
            {
                components[i].Update(deltaTime);
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].FixedUpdate(deltaTime);
            }
        }

        public void OnCollisionEnter(Physics.Collision collision)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnCollisionEnter(collision);
            }

        }

        public void OnCollisionStay(Physics.Collision collision)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnCollisionStay(collision);
            }
        }

        public void OnCollisionExit(Physics.Collision collision)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnCollisionExit(collision);
            }
        }

        public void OnTriggerEnter(Rigidbody other)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnTriggerEnter(other);
            }
        }

        public void OnTriggerStay(Rigidbody other)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnTriggerStay(other);
            }
        }

        public void OnTriggerExit(Rigidbody other)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnTriggerExit(other);
            }
        }

        public void Render(float deltaTime)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Render(deltaTime);
            }
        }

        public void AddComponent(Component c)
        {
            if(c.GetType().Equals(typeof(Transform)))
            {
                transform = (Transform)c;
            }

            components.Add(c);
            c.SetGameObject(this);
        }

        public void RemoveComponent(Component c)
        {
            if (c.GetType().Equals(typeof(Transform)))
            {
                transform = null;
            }

            components.Remove(c);
            c.SetGameObject(null);
        }

        public List<Component> GetComponents()
        {
            return components;
        }

        public T GetComponent<T>() where T: Component
        {
            T c = (T)components.Find((Component c) => c.GetType() == typeof(T));

            return c;
        }
    }
}
