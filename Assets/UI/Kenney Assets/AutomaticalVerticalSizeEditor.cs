using UnityEditor;

[CustomEditor(typeof(AutomaticVerticalSize))]
public class AutomaticalVerticalSizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
