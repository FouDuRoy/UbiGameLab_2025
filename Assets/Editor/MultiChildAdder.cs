using UnityEditor;
using UnityEngine;

public class MultiChildAdder : EditorWindow
{
    private GameObject childPrefab;
    
    [MenuItem("Tools/Add Specific Child to Selected")]
    public static void ShowWindow()
    {
        GetWindow<MultiChildAdder>("Add Child to Selected");
    }

    void OnGUI()
    {
        GUILayout.Label("Add Child to Multiple Parents", EditorStyles.boldLabel);
        
        childPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Child Prefab", 
            childPrefab, 
            typeof(GameObject), 
            false);
        
        if (GUILayout.Button("Add Child to Selected Parents") && childPrefab != null)
        {
            AddChildToSelected();
        }
    }

    private void AddChildToSelected()
    {
        foreach (GameObject parent in Selection.gameObjects)
        {
            GameObject newChild;
            
            if (PrefabUtility.IsPartOfAnyPrefab(childPrefab))
            {
                // Instantiate prefab
                newChild = (GameObject)PrefabUtility.InstantiatePrefab(childPrefab);
            }
            else
            {
                // Create regular GameObject
                newChild = Instantiate(childPrefab);
            }
            
            newChild.transform.SetParent(parent.transform);
            newChild.transform.localPosition = Vector3.zero;
            newChild.transform.localRotation = Quaternion.identity;
            newChild.transform.localScale = Vector3.one;
            
            Undo.RegisterCreatedObjectUndo(newChild, "Add Child to Selected");
        }
    }
}