namespace GameEngine
{
    internal class Scene
    {
        List<GameObject> gameObjects;

        public string name;

        public Scene()
        {
            gameObjects = new List<GameObject>();
        }

        public void AddGameObject(GameObject go)
        {
            gameObjects.Add(go);
        }

        public void RemoveGameObject(GameObject go)
        {
            gameObjects.Remove(go);
        }

        public void Start()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Start();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Stop();
            }
        }

        public void Update(float deltaTime)
        {
            for(int i = 0; i < gameObjects.Count; i ++)
            {
                gameObjects[i].Update(deltaTime);
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].FixedUpdate(deltaTime);
            }
        }

        public void Render(float deltaTime)
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Render(deltaTime);
            }
        }

        public List<GameObject> GetGameObjects()
        {
            return gameObjects;
        }

    }
}
