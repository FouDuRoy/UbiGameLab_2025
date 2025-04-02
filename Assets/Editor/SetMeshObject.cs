using System.Linq;
using UnityEditor;
using UnityEngine;

public class SetMeshObjects : EditorWindow
{
    private GameObject childPrefab;
    
    [MenuItem("Tools/Add Specific Object")]
    public static void ShowWindow()
    {
        GetWindow<SetMeshObjects>("Add Object");
    }

    void OnGUI()
    {
        
        if (GUILayout.Button("Add Child to Selected "))
        {
            AddChildToSelected();
        }
    }

    private void AddChildToSelected()
    {
        foreach (GameObject parent in Selection.gameObjects)
        {
           parent.GetComponent<Bloc>().objectToChangeMesh = parent.transform.Find("BasicCube").Find("SM_BasicCube_02").gameObject;
        }
    }
}