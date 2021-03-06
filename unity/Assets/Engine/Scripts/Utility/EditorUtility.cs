﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace XEngine
{
    public class TransformRotationGUIWrapper
    {
        public Transform t;
        public System.Object guiObj;
        public MethodInfo mi;

        public void OnGUI()
        {
            if (guiObj != null && mi != null)
            {
                mi.Invoke(guiObj, null);
            }
        }
    }

    public static class EditorCommon
    {
        public delegate void EnumTransform(Transform t, object param);
        static Type transformRotationGUIType = null;
        static Assembly unityEditorAssembly = null;

        public static object CallInternalFunction(Type type, string function, bool isStatic, bool isPrivate, bool isInstance, object obj, object[] parameters)
        {
            System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Default;
            if (isStatic)
            {
                flag |= System.Reflection.BindingFlags.Static;
            }
            if (isPrivate)
            {
                flag |= System.Reflection.BindingFlags.NonPublic;
            }
            else
            {
                flag |= System.Reflection.BindingFlags.Public;
            }
            if (isInstance)
            {
                flag |= System.Reflection.BindingFlags.Instance;
            }
            System.Reflection.MethodInfo mi = type.GetMethod(function, flag);
            if (mi != null)
            {
                return mi.Invoke(obj, parameters);
            }
            return null;
        }

        public static TransformRotationGUIWrapper GetTransformRotatGUI(Transform tran)
        {
            if (transformRotationGUIType == null)
            {
                if (unityEditorAssembly == null)
                {
                    unityEditorAssembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
                }
                if (unityEditorAssembly != null)
                {
                    transformRotationGUIType = unityEditorAssembly.GetType("UnityEditor.TransformRotationGUI");
                }
            }
            TransformRotationGUIWrapper wrapper = null;
            if (transformRotationGUIType != null)
            {
                System.Object guiObj = Activator.CreateInstance(transformRotationGUIType);
                if (guiObj != null)
                {
                    wrapper = new TransformRotationGUIWrapper();
                    wrapper.t = tran;
                    wrapper.guiObj = guiObj;
                    CallInternalFunction(transformRotationGUIType, "OnEnable", false, false, true, guiObj, new object[] { new SerializedObject(tran).FindProperty("m_LocalRotation"), EditorGUIUtility.TrTextContent("Rotation", "The local rotation.") });

                    wrapper.mi = transformRotationGUIType.GetMethod("RotationField", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);
                }
            }
            return wrapper;
        }

        public static void CreateDir(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string name = Path.GetFileNameWithoutExtension(dir);
                dir = Path.GetDirectoryName(dir);
                CreateDir(dir);
                AssetDatabase.CreateFolder(dir, name);
            }
        }

        public static void EnumRootObject(EnumTransform cb, object param = null)
        {
            UnityEngine.SceneManagement.Scene s = SceneManager.GetActiveScene();
            GameObject[] roots = s.GetRootGameObjects();
            for (int i = 0, imax = roots.Length; i < imax; ++i)
            {
                Transform t = roots[i].transform;
                cb(t, param);
            }
        }

        public static void EnumTargetObject(string goPath, EnumTransform cb, object param = null)
        {
            GameObject go = GameObject.Find(goPath);
            if (go != null)
            {
                Transform t = go.transform;
                for (int i = 0, imax = t.childCount; i < imax; ++i)
                {
                    Transform child = t.GetChild(i);
                    cb(child, param);
                }
            }
        }

        public static void EnumChildObject(Transform t, object param, EnumTransform fun)
        {
            for (int i = 0; i < t.childCount; ++i)
            {
                Transform child = t.GetChild(i);
                fun(child, param);
            }
        }

        public static void DestroyChildObjects(GameObject go, string name = "")
        {
            if (go != null)
            {
                Transform t = go.transform;
                if (t.childCount > 0)
                {
                    List<GameObject> children = new List<GameObject>();
                    for (int i = 0; i < t.childCount; ++i)
                    {
                        Transform child = t.GetChild(i);
                        if (string.IsNullOrEmpty(name) || child.name.StartsWith(name))
                            children.Add(child.gameObject);
                    }

                    for (int i = 0; i < children.Count; ++i)
                    {
                        GameObject.DestroyImmediate(children[i]);
                    }
                }
            }
        }


    }
}

#endif
