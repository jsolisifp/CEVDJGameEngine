using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace GameEngine
{
    internal class Preset
    {
        Component component;
        List<GameObject> gameObjects;
        bool isPrepared;
        Dictionary<Component, GameObject> componentToParent;
        List<int[]> idArraysList; //Component, Field, parent, targetComponent
        public Preset(Component component) { this.component = component; }
        public Preset(Scene scene) { this.gameObjects = scene.GetGameObjects(); }

        public bool isComponent()
        {
            return component != null;
        }

        public Component GetComponentCopy()
        {
            Type type = component.GetType();
            Component copy = (Component)Activator.CreateInstance(type);
            FieldInfo[] fields = component.GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                field.SetValue(copy, field.GetValue(component));
            }
            return copy;
        }

        public List<GameObject> GetGameObjectsCopies()
        {
            if (!isPrepared) PrepareCopy();

            List<GameObject> copyList = new List<GameObject>();
            
            List<Component> componentsToFix = new List<Component>();
            
            int fixCounter = 0;
            bool isFixed;


            GameObject go, goCopy;
            Component c, cCopy;
            List<Component> components;
            FieldInfo[] fields;
            FieldInfo field;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                go = gameObjects[i];
                goCopy = new GameObject();

                goCopy.name = go.name;
                goCopy.active = go.active;
                goCopy.@static = go.@static;
                goCopy.layer = go.layer;

                components = go.GetComponents();

                for (int j = 0; j < components.Count; j++)
                {
                    c = components[j];
                    cCopy = (Component)Activator.CreateInstance(c.GetType());
                    fields = c.GetType().GetFields();
                    isFixed = false;
                    for (int k = 0; k < fields.Length; k++)
                    {
                        field = fields[k];
                        Object value = field.GetValue(c);

                        if (value == null) continue;
                        else if(!value.GetType().IsSubclassOf(typeof(Component))) field.SetValue(cCopy, value);
                        else
                        {
                            GameObject parent = componentToParent[(Component)value];
                            isFixed = true;
                            if(componentsToFix.Count != fixCounter+1) componentsToFix.Add(cCopy);
                        }
                    }
                    if(isFixed) fixCounter++;
                    goCopy.AddComponent(cCopy);
                }
                copyList.Add(goCopy);
            }

            int[] ids;
            int lastCompId = -1;
            fields = null;
            for (int i = 0; i < idArraysList.Count; i++)
            {
                ids = idArraysList[i];
                Component target = componentsToFix[ids[0]];
                if(lastCompId != ids[0]){ fields = target.GetType().GetFields(); lastCompId = ids[0]; }
                field = fields[ids[1]];
                field.SetValue(target, copyList[ids[2]].GetComponents()[ids[3]]);
            }

            return copyList;
        }

        public void PrepareCopy()
        {
            if(component != null) return;

            componentToParent = new Dictionary<Component, GameObject>();
            List<Component> componentsToFix = new List<Component>();
            idArraysList = new List<int[]>();
            bool isFixed;
            int fixCounter = 0;

            GameObject go;
            Component c;
            List<Component> components;
            FieldInfo[] fields;
            FieldInfo field;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                go = gameObjects[i];
                components = go.GetComponents();
                
                for (int j = 0; j < components.Count; j++)
                {
                    c = components[j];
                    componentToParent[c] = go;

                    fields = c.GetType().GetFields();
                    isFixed = false;
                    for (int k = 0; k < fields.Length; k++)
                    {
                        field = fields[k];
                        Object value = field.GetValue(c);

                        if (value == null) continue;
                        else if (value.GetType().IsSubclassOf(typeof(Component))){
                            
                            isFixed = true;
                            if (componentsToFix.Count != fixCounter + 1) componentsToFix.Add(c);

                            idArraysList.Add([fixCounter,k,0,0]);
                        }

                    }
                    if (isFixed) fixCounter++;
                }
            }

            int[] ids;
            int lastCompId = -1;
            fields = null;
            Component target;
            GameObject parent;
            for (int i = 0; i < idArraysList.Count; i++)
            {
                ids = idArraysList[i];
                c = componentsToFix[ids[0]];
                if (lastCompId != ids[0]) { fields = c.GetType().GetFields(); lastCompId = ids[0]; }
                field = fields[ids[1]];
                target = (Component)field.GetValue(c);
                parent = componentToParent[target];
                ids[2] = gameObjects.IndexOf(parent);
                ids[3] = parent.GetComponents().IndexOf(target);
            }

            isPrepared = true;
        }
    }
}
