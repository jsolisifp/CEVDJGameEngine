namespace GameEngine
{
    internal class Assets
    {
        const string assetsFolder = "\\Assets";

        static Dictionary<string, AssetLoader > loaders;

        static Dictionary<string, object> assets;

        public static void Init()
        {
            loaders = new Dictionary<string, AssetLoader>();
            assets = new Dictionary<string, object>();

        }

        public static void LoadAssets()
        {
            string assetsPath = GetAssetsPath();
            string[] files = Directory.GetFiles(assetsPath);

            foreach(var path in files)
            {
                string extension = GetExtension(path).ToLower();
                if(loaders.ContainsKey(extension))
                {
                    assets[path] = loaders[extension].LoadAsset(path);
                }

            }

        }

        public static void UnloadAssets()
        {
            foreach (KeyValuePair<string, object> keyValue in assets)
            {
                string extension = GetExtension(keyValue.Key).ToLower();
                if (loaders.ContainsKey(extension))
                {
                    loaders[extension].UnloadAsset(keyValue.Value);
                }
            }

            assets.Clear();

        }

        public static void LoadAsset(string path, bool relative = true)
        {
            path = (relative ? GetAssetsPath() + "\\" : "") + path;
            string extension = GetExtension(path).ToLower();
            if (loaders.ContainsKey(extension))
            {
                AssetLoader loader = loaders[extension];
                assets[path] = loader.LoadAsset(path);
            }
        }

        public static void UnloadAsset(string path, bool relative = true)
        {
            path = (relative ? GetAssetsPath() + "\\" : "") + path;
            string extension = GetExtension(path).ToLower();
            if (loaders.ContainsKey(extension))
            {
                AssetLoader loader = loaders[extension];
                loader.UnloadAsset(assets[path]);
            }

            assets.Remove(path);
        }

        public static void ReloadAsset(string path, bool relative = true)
        {
            path = (relative ? GetAssetsPath() + "\\" : "") + path; 
            string extension = GetExtension(path).ToLower();
            if(loaders.ContainsKey(extension))
            {
                AssetLoader loader = loaders[extension];
                loader.UnloadAsset(assets[path]);

                assets[path] = loader.LoadAsset(path);
            }
        }

        public static void ReloadAssets()
        {
            UnloadAssets();
            LoadAssets();
        }

        public static void Update(float deltaTime)
        {
        }

        public static void Finish()
        {
        }

        public static void RegisterAssetLoader(string extension, AssetLoader loader)
        {
            loaders.Add(extension.ToLower(), loader);
        }

        public static bool IsAssetLoaded(string path, bool relative = true)
        {
            string key = (relative ? GetAssetsPath() + "\\" : "") + path;

            return assets.ContainsKey(key);
        }

        public static Type GetLoadedAssetType(string path, bool relative = true)
        {
            string key = (relative ? ToAbsolutePath(path) : path);

            if (assets.ContainsKey(key)) { return assets[key].GetType(); }
            else { return null; }
        }

        public static T GetLoadedAsset<T>(string path, bool relative = true)
        {
            string key = (relative ? GetAssetsPath() + "\\" : "") + path;

            if(assets.ContainsKey(key))
            {
                object a = assets[key];
                if(a.GetType() == typeof(T)) { return (T)assets[key]; }
                else { return default(T); }
            }
            else { return default(T); }
            
        }

        public static string GetAssetsPath()
        {
            return Directory.GetCurrentDirectory() + assetsFolder;
        }

        public static List<string> GetAssetPaths()
        {
            return assets.Keys.ToList();
        }

        public static string LoadText(string path)
        {
            return File.ReadAllText(path);
        }

        public static string ToRelativePath(string path)
        {
            return path.Substring(GetAssetsPath().Length + 1);
        }

        public static string ToAbsolutePath(string path)
        {
            return GetAssetsPath() + "\\" + path;
        }

        static string GetExtension(string path)
        {
            string result = "";
            int index = path.LastIndexOf('.');
            if(index >= 0) { result = path.Substring(index + 1); }
            return result;
        }

    }
}