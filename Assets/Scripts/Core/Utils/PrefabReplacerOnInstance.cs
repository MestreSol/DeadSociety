﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Utils
{
    [ExecuteInEditMode]
    public class PrefabReplacerOnInstance : MonoBehaviour
    {
        public GameObject TargetPrefab;

        void Awake()
        {
#if UNITY_EDITOR
            List<GameObject> allPrefabObjectsInScene = new List<GameObject>();
            foreach (Transform t in GameObject.FindObjectsOfType<Transform>())
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject))
                {
                    allPrefabObjectsInScene.Add(t.gameObject);
                }
            }

            foreach (GameObject go in allPrefabObjectsInScene)
            {
                GameObject instanceSource = PrefabUtility.GetCorrespondingObjectFromSource(go);

                if (instanceSource == TargetPrefab)
                {
                    transform.SetParent(go.transform.parent);
                    transform.position = go.transform.position;
                    transform.rotation = go.transform.rotation;
                    transform.localScale = go.transform.localScale;

                    // Undo.Register
                    Undo.DestroyObjectImmediate(go);

                    Debug.Log("Replaced prefab in scene");
                    DestroyImmediate(this);
                    break;
                }
            }
#endif
        }
    }
}