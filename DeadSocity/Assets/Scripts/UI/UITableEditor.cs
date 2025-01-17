using UnityEditor;
using UnityEngine;

namespace UI
{
    [CustomEditor(typeof(UITable), true)]
    public class UITableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            UITable myTarget = (UITable)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Update"))
            {
                myTarget.UpdateTable(null);
            }
        }
    }
}