using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace GameEngine
{
    internal class SceneSerializer
    {
        public static void Serialize(Scene scene, string path)
        {
            int lastId = 0;

            var componentToId = new Dictionary<Component, int>();

            List<GameObject> gameObjects = scene.GetGameObjects();

            // Generate ids for components

            for (int i = 0; i < gameObjects.Count; i++)
            {
                List<Component> components = gameObjects[i].GetComponents();

                for (int j = 0; j < components.Count; j++)
                {
                    componentToId[components[j]] = lastId;
                    lastId++;
                }
            }

            // Write file

            StreamWriter writer = new StreamWriter(path);

            writer.WriteLine("name:" + scene.name);

            writer.WriteLine("gameObjects:" + SerializeInt32(gameObjects.Count));

            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject g = gameObjects[i];
                writer.WriteLine("name:" + g.name);
                writer.WriteLine("active:" + SerializeBool(g.active));
                writer.WriteLine("static:" + SerializeBool(g.@static));
                writer.WriteLine("layer:" + SerializeInt32(g.layer));


                List<Component> components = g.GetComponents();

                writer.WriteLine("components:" + SerializeInt32(components.Count));

                for (int j = 0; j < components.Count; j++)
                {
                    Component component = components[j];

                    writer.WriteLine("___id:" + SerializeInt32(componentToId[component]));
                    writer.WriteLine("___type:" + component.GetType().Name);

                    FieldInfo[] fields = component.GetType().GetFields();

                    writer.WriteLine("fields:" + SerializeInt32(fields.Length));

                    for (int k = 0; k < fields.Length; k++)
                    {
                        FieldInfo field = fields[k];
                        Type type = field.GetValue(component).GetType();
                        string typeName = type.Name;
                        Object value = field.GetValue(component);

                        writer.WriteLine("name:" + field.Name);
                        writer.WriteLine("type:" + type.Name);

                        string valueString;
                        if (typeName == "Single" || typeName == "Int32" ||
                            typeName == "String" || typeName == "Boolean")
                        {
                            valueString = value.ToString();
                        }

                        if (typeName == "String")
                        {
                            valueString = SerializeString((string)value);
                        }
                        else if (typeName == "Single")
                        {
                            valueString = SerializeSingle((Single)value);
                        }
                        else if (typeName == "Int32")
                        {
                            valueString = SerializeInt32((Int32)value);
                        }
                        else if(typeName == "Boolean")
                        {
                            valueString = SerializeBool((Boolean)value);
                        }
                        else if (typeName == "Vector3")
                        {
                            Vector3 v = (Vector3)value;
                            valueString = SerializeSingle(v.X) + "," + SerializeSingle(v.Y) + "," + SerializeSingle(v.Z);
                        }
                        else if (typeName == "Vector4")
                        {
                            Vector4 v = (Vector4)value;
                            valueString = SerializeSingle(v.X) + "," + SerializeSingle(v.Y) + "," + SerializeSingle(v.Z) + "," + SerializeSingle(v.W);
                        }
                        else if (type.IsSubclassOf(typeof(Component)))
                        {
                            valueString = componentToId[(Component)value].ToString();
                        }
                        else
                        {
                            Console.WriteLine("Warning: Unrecognized field type " + typeName);
                            valueString = "";
                        }

                        writer.WriteLine("value:" + valueString);

                    }
                }


            }

            writer.Close();

        }

        public static Scene Deserialize(string path)
        {
            var idToComponent = new Dictionary<int, Component>();
            var scene = new Scene();

            StreamReader reader = null;
            string line = "";

            for (int pass = 0; pass < 2; pass++)
            {
                reader = new StreamReader(path);

                line = reader.ReadLine();
                if (pass == 0) { scene.name = line.Split(':')[1]; }

                line = reader.ReadLine();
                int gameObjectCount = Int32.Parse(line.Split(':')[1], CultureInfo.InvariantCulture);

                for (int i = 0; i < gameObjectCount; i++)
                {
                    GameObject gameObject = null;

                    if (pass == 0) { gameObject = new GameObject(); }
                    else { gameObject = scene.GetGameObjects()[i]; }

                    line = reader.ReadLine();

                    if (pass == 0) { gameObject.name = line.Split(':')[1]; }

                    line = reader.ReadLine();
                    if (pass == 0) { gameObject.active = DeserializeBool(line.Split(':')[1]); }

                    line = reader.ReadLine();
                    if (pass == 0) { gameObject.@static = DeserializeBool(line.Split(':')[1]); }

                    line = reader.ReadLine();
                    if (pass == 0) { gameObject.layer = DeserializeInt32(line.Split(':')[1]); }

                    line = reader.ReadLine();
                    int componentCount = DeserializeInt32(line.Split(':')[1]);

                    for (int j = 0; j < componentCount; j++)
                    {
                        line = reader.ReadLine();
                        int id = DeserializeInt32(line.Split(':')[1]);

                        line = reader.ReadLine();
                        string typeName = line.Split(':')[1];
                        Type type = Type.GetType("GameEngine." + typeName);

                        Object component = null;

                        if (pass == 0)
                        {
                            component = Activator.CreateInstance(type);
                            idToComponent[id] = (Component)component;

                            gameObject.AddComponent((Component)component);
                        }
                        else
                        {
                            component = scene.GetGameObjects()[i].GetComponents()[j];
                        }

                        line = reader.ReadLine();
                        int numFields = DeserializeInt32(line.Split(':')[1]);

                        for (int k = 0; k < numFields; k++)
                        {
                            line = reader.ReadLine();

                            string fieldName = line.Split(':')[1];

                            line = reader.ReadLine();
                            string fieldTypeName = line.Split(':')[1];

                            line = reader.ReadLine();
                            string fieldValueString = line.Split(':')[1];

                            FieldInfo field = type.GetField(fieldName);
                            Object value = null;

                            if (fieldTypeName == "Single")
                            {
                                if (pass == 0) { value = DeserializeSingle(fieldValueString); }
                            }
                            else if (fieldTypeName == "Int32")
                            {
                                if (pass == 0) { value = DeserializeInt32(fieldValueString); }
                            }
                            else if (fieldTypeName == "Boolean")
                            {
                                if (pass == 0) { value = DeserializeBool(fieldValueString); }
                            }
                            else if (fieldTypeName == "Vector3")
                            {
                                if (pass == 0)
                                {
                                    string[] parts = fieldValueString.Split(',');
                                    value = new Vector3(DeserializeSingle(parts[0]),
                                                        DeserializeSingle(parts[1]),
                                                        DeserializeSingle(parts[2]));
                                }
                            }
                            else if (fieldTypeName == "Vector4")
                            {
                                if (pass == 0)
                                {
                                    string[] parts = fieldValueString.Split(',');
                                    value = new Vector4(DeserializeSingle(parts[0]),
                                                        DeserializeSingle(parts[1]),
                                                        DeserializeSingle(parts[2]),
                                                        DeserializeSingle(parts[3]));
                                }
                            }
                            else if (fieldTypeName == "String")
                            {
                                if (pass == 0) { value = fieldValueString; }
                            }
                            else if (type.IsSubclassOf(typeof(Component)))
                            {
                                if (pass == 1)
                                {
                                    int n = DeserializeInt32(fieldValueString);
                                    value = idToComponent[n];
                                }
                            }

                            if (value != null)
                            {
                                field.SetValue(component, value);
                            }
                        }

                    }

                    if (pass == 0)
                    {
                        scene.AddGameObject(gameObject);
                    }

                }

                reader.Close();

            }

            return scene;

        }

        static string SerializeString(string s)
        {
            // easy
            return s;
        }

        static string SerializeBool(bool b)
        {
            return b.ToString(CultureInfo.InvariantCulture);
        }

        static string SerializeSingle(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }

        static string SerializeInt32(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }
        static string DeserializeString(string s)
        {
            // easy
            return s;
        }

        static bool DeserializeBool(string s)
        {
            return Boolean.Parse(s);
        }

        static float DeserializeSingle(string s)
        {
            return Single.Parse(s, CultureInfo.InvariantCulture);
        }

        static int DeserializeInt32(string s)
        {
            return Int32.Parse(s, CultureInfo.InvariantCulture);
        }

    }
}
