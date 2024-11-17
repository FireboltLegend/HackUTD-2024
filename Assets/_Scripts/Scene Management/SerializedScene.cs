using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct SerializedScene
{
    public Object _scene;
    public string SceneName => _scene.name;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SerializedScene))]
public class SerializedSceneDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PrefixLabel(position, label);

        var sceneProp = property.FindPropertyRelative("_scene");
        EditorGUI.ObjectField(position, sceneProp, typeof(SceneAsset));
    }
}
#endif
