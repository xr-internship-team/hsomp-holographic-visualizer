using UnityEngine;
using UnityEditor;

public class MissingScriptCleaner
{
    [MenuItem("Tools/Clean Missing Scripts")]
    private static void Clean()
    {
        int count = 0;
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
        {
            var components = go.GetComponents<Component>();
            var serializedObject = new SerializedObject(go);
            var prop = serializedObject.FindProperty("m_Component");

            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    prop.DeleteArrayElementAtIndex(i);
                    count++;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        Debug.Log($"Missing script temizlendi: {count} adet kaldırıldı.");
    }
}
