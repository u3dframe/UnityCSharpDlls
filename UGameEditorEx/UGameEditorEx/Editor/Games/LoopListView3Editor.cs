using UnityEditor;


[CustomEditor(typeof(LoopListView3), true)]
public class LoopListView3Editor : Editor
{
    SerializedProperty interactable;
    SerializedProperty content;
    SerializedProperty viewport;
    SerializedProperty movementType;
    SerializedProperty elasticity;
    SerializedProperty layoutType;
    SerializedProperty center;
    SerializedProperty loop;
    SerializedProperty allowSameItem;
    SerializedProperty loopInterval;
    SerializedProperty distanceToTurnPage;
    SerializedProperty distanceMinDelta;
    SerializedProperty inertiaDecelerationRate;
    SerializedProperty autoSpeedChangeRate;
    SerializedProperty autoSpeedMinValue;
    SerializedProperty gridLayout;
    SerializedProperty contentPadding;
    SerializedProperty contentChildAlignment;
    SerializedProperty gridCellSize;
    SerializedProperty gridCellScale;
    SerializedProperty gridSpacing;
    SerializedProperty contentSpacing;
    SerializedProperty gridConstraint;
    SerializedProperty constraintCount;
    SerializedProperty scrollbar;
    SerializedProperty scrollbarVisibility;
    SerializedProperty itemPrefabs;

    protected virtual void OnEnable()
    {
        interactable = serializedObject.FindProperty("interactable");
        content = serializedObject.FindProperty("content");
        viewport = serializedObject.FindProperty("viewport");
        movementType = serializedObject.FindProperty("movementType");
        elasticity = serializedObject.FindProperty("elasticity");
        layoutType = serializedObject.FindProperty("layoutType");
        center = serializedObject.FindProperty("center");
        loop = serializedObject.FindProperty("loop");
        allowSameItem = serializedObject.FindProperty("allowSameItem");
        loopInterval = serializedObject.FindProperty("loopInterval");
        distanceToTurnPage = serializedObject.FindProperty("distanceToTurnPage");
        distanceMinDelta = serializedObject.FindProperty("distanceMinDelta");
        inertiaDecelerationRate = serializedObject.FindProperty("inertiaDecelerationRate");
        autoSpeedChangeRate = serializedObject.FindProperty("autoSpeedChangeRate");
        autoSpeedMinValue = serializedObject.FindProperty("autoSpeedMinValue");
        gridLayout = serializedObject.FindProperty("gridLayout");
        contentPadding = serializedObject.FindProperty("contentPadding");
        contentChildAlignment = serializedObject.FindProperty("contentChildAlignment");
        gridCellSize = serializedObject.FindProperty("gridCellSize");
        gridCellScale = serializedObject.FindProperty("gridCellScale");
        gridSpacing = serializedObject.FindProperty("gridSpacing");
        contentSpacing = serializedObject.FindProperty("contentSpacing");
        gridConstraint = serializedObject.FindProperty("gridConstraint");
        constraintCount = serializedObject.FindProperty("constraintCount");
        scrollbar = serializedObject.FindProperty("scrollbar");
        scrollbarVisibility = serializedObject.FindProperty("scrollbarVisibility");
        itemPrefabs = serializedObject.FindProperty("itemPrefabs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(interactable, true);
        EditorGUILayout.PropertyField(content, true);
        EditorGUILayout.PropertyField(viewport, true);
        EditorGUILayout.PropertyField(movementType, true);
        EditorGUILayout.PropertyField(elasticity, true);
        EditorGUILayout.PropertyField(layoutType, true);
        EditorGUILayout.PropertyField(center, true);
        EditorGUILayout.PropertyField(loop, true);
        EditorGUILayout.PropertyField(allowSameItem, true);
        EditorGUILayout.PropertyField(loopInterval, true);
        EditorGUILayout.PropertyField(distanceToTurnPage, true);
        EditorGUILayout.PropertyField(distanceMinDelta, true);
        EditorGUILayout.PropertyField(inertiaDecelerationRate, true);
        EditorGUILayout.PropertyField(autoSpeedChangeRate, true);
        EditorGUILayout.PropertyField(autoSpeedMinValue, true);
        EditorGUILayout.PropertyField(gridLayout, true);
        EditorGUILayout.PropertyField(contentPadding, true);
        EditorGUILayout.PropertyField(contentChildAlignment, true);
        if (gridLayout.boolValue)
        {
            EditorGUILayout.PropertyField(gridCellSize, true);
            EditorGUILayout.PropertyField(gridCellScale, true);
            EditorGUILayout.PropertyField(gridSpacing, true);
            EditorGUILayout.PropertyField(gridConstraint, true);
            if (gridConstraint.enumValueIndex > 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(constraintCount, true);
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            EditorGUILayout.PropertyField(contentSpacing, true);
        }
        EditorGUILayout.PropertyField(scrollbar, true);
        EditorGUILayout.PropertyField(scrollbarVisibility, true);
        EditorGUILayout.PropertyField(itemPrefabs, true);
        serializedObject.ApplyModifiedProperties();
    }
}