using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
//using UnityEditor;

namespace MILab
{

    // Attach this script to any object that should have its transform saved
    // Its data will be saved in Unity's persistent data path
    // Filepath: <persistent data path>/<scene name>/<game object hierarchy path>.dat
    // Each game object with this script should end up with its own save file
    // Conflict could occur if multiple objects have the same path in the hierarchy and name

    public class SaveableTransformObject : MonoBehaviour
    {

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save(gameObject);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                Load(gameObject);
            }

        }


        // Save each object recursively
        [Button]
        public void EditorSave()
        {
            Save(gameObject);
        }

        // Save each object and its descendants recursively
        public void Save(GameObject go)
        {
          
            // Save the data
            SaveTransformData sd = new SaveTransformData();
            PopulateSaveData(sd, go);

            if (FileManager.WriteToFile(GetFileName(go), sd.ToJson()))
            {
                Debug.Log("Saved: " + GetFileName(go));
            }

            // Loop for all children
            var allDescendants = go.GetComponentsInChildren<Transform>();

            foreach (Transform descendant in allDescendants)
            {
                // Save the data
                sd = new SaveTransformData();
                PopulateSaveData(sd, descendant.gameObject);

                if (FileManager.WriteToFile(GetFileName(descendant.gameObject), sd.ToJson()))
                {
                    Debug.Log("Saved: " + GetFileName(descendant.gameObject));
                }
            }
        }


        [Button]
        public void EditorLoad()
        {
            Load(gameObject);
            //EditorUtility.SetDirty(gameObject);
        }

        // Load each object and its descendants recursively
        public void Load(GameObject go)
        {
            // Load the data
            if (FileManager.LoadFromFile(GetFileName(go), out var json))
            {
                SaveTransformData sd = new SaveTransformData();
                sd.LoadFromJson(json);

                LoadFromSaveData(sd, go);
                Debug.Log("Loaded: " + GetFileName(go));
            }

            // Loop for all children
            var allDescendants = go.GetComponentsInChildren<Transform>();

            foreach (Transform descendant in allDescendants)
            {
                if (FileManager.LoadFromFile(GetFileName(descendant.gameObject), out var childJson))
                {
                    SaveTransformData sd = new SaveTransformData();
                    sd.LoadFromJson(childJson);

                    LoadFromSaveData(sd, descendant.gameObject);
                    Debug.Log("Loaded: " + GetFileName(descendant.gameObject));
                }
            }

        }

        public void LoadFromSaveData(SaveTransformData saveTransformData, GameObject go)
        {
            go.transform.localPosition = saveTransformData.localPosition;
            go.transform.localRotation = saveTransformData.localRotation;
            go.transform.localScale = saveTransformData.localScale;
        }

        public void PopulateSaveData(SaveTransformData saveTransformData, GameObject go)
        {
            saveTransformData.localPosition = go.transform.localPosition;
            saveTransformData.localRotation = go.transform.localRotation;
            saveTransformData.localScale = go.transform.localScale;
        }

        public string GetFileName(GameObject obj)
        {
            String path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = Path.Combine(obj.name, path);
            }

            String fileName = path + ".json";
            fileName = Path.Combine(gameObject.scene.name, fileName);
            return fileName;
        }

        /*
        public string GetGameObjectPath(GameObject obj)
        {
            //string path = "/" + obj.name;
            String path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                //path = "/" + obj.name + path;
                path = Path.Combine(obj.name, path);
            }
            return path;
        }
        */

    }
}