/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace SideXP.Speedrun.EditorOnly
{

    /// <summary>
    /// Custom editor for <see cref="SegmentAsset"/>.
    /// </summary>
    [CustomEditor(typeof(SegmentAsset))]
    public class SegmentAssetEditor : Editor
    {

        #region Fields

        private const float IsCheckpointLabelWidth = 88f;
        private const float IsCheckpointFieldWidth = Standalone.WidthM;
        private const float StepFoldoutWidth = Standalone.WidthXS;
        private const float FoldoutOffset = 12f;
        private const float StepDescriptionFieldHeight = 40f;

        internal const string SpeedrunProp = "_speedrunAsset";
        private const string DescriptionProp = "_description";
        private const string StepsArrayProp = "_steps";

        private SerializedProperty _speedrunProp = null;
        private SerializedProperty _descriptionProp = null;
        private SerializedProperty _stepsArrayProp = null;

        private ReorderableList _stepsReorderableList = null;

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            _speedrunProp = serializedObject.FindProperty(SpeedrunProp);
            _descriptionProp = serializedObject.FindProperty(DescriptionProp);
            _stepsArrayProp = serializedObject.FindProperty(StepsArrayProp);

            _stepsReorderableList = BuildStepsReorderableList(_stepsArrayProp);
        }

        #endregion


        #region UI

        /// <inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_speedrunProp);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_descriptionProp);

            serializedObject.ApplyModifiedProperties();

            // Draw segments list
            EditorGUILayout.Space();
            _stepsReorderableList.DoLayoutList();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Creates a <see cref="ReorderableList"/> for the Step assets attached to a <see cref="SegmentAsset"/>.
        /// </summary>
        /// <param name="stepsArrayProp">The serialized representation of the property to display as a <see cref="ReorderableList"/>.
        /// Assumes that this property is an array of <see cref="StepAsset"/>.</param>
        /// <returns>Returns the created <see cref="ReorderableList"/>.</returns>
        private ReorderableList BuildStepsReorderableList(SerializedProperty stepsArrayProp)
        {
            ReorderableList list = new ReorderableList(stepsArrayProp.serializedObject, stepsArrayProp);

            // Draw list header
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, stepsArrayProp.GetLabel(), EditorStyles.boldLabel);

            // "Add" button behavior
            list.onAddCallback = list =>
            {
                Standalone.CreateAndAttachAsset<StepAsset>(list.serializedProperty, assetObj =>
                {
                    // Assign the owning Speedrun asset to the created Segment asset
                    SerializedProperty segmentprop = assetObj.FindProperty(StepAssetEditor.SegmentProp);
                    segmentprop.objectReferenceValue = list.serializedProperty.serializedObject.targetObject;
                    assetObj.ApplyModifiedPropertiesWithoutUndo();
                });
            };

            // "Remove" button behavior
            list.onRemoveCallback = list =>
            {
                int index = list.index;
                SerializedProperty stepItemProp = list.serializedProperty.GetArrayElementAtIndex(index);
                StepAsset stepAsset = stepItemProp.objectReferenceValue as StepAsset;

                if (stepAsset != null)
                {
                    if (!EditorUtility.DisplayDialog(
                        "Delete Segment",
                        $"The selected {nameof(Step)} asset \"{(stepAsset != null ? stepAsset.DisplayName : "null")}\" will be deleted from the {nameof(Segment)} \"{(stepAsset != null && stepAsset.SegmentAsset != null ? stepAsset.SegmentAsset.DisplayName : "null")}\"." +
                        "\nThis operation can't be undone. Proceed?",
                        "Yes, delete Segment", "No"))
                    {
                        return;
                    }

                    // Destroy the step asset itself
                    DestroyImmediate(stepAsset, true);
                }

                // Remove the entry from the list
                stepItemProp.objectReferenceValue = null;
                list.serializedProperty.DeleteArrayElementAtIndex(index);

                // Save changes
                list.serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                list.serializedProperty.serializedObject.targetObject.SaveAndReimport();
            };

            // Calculate the height of a single element in the list
            list.elementHeightCallback = index =>
            {
                SerializedProperty stepItemProp = list.serializedProperty.GetArrayElementAtIndex(index);
                float height = EditorGUIUtility.singleLineHeight + Standalone.VMargin * 2;
                if (!stepItemProp.isExpanded)
                    return height;

                // Add space for addition Step fields
                height += EditorGUIUtility.singleLineHeight + StepDescriptionFieldHeight + Standalone.VMargin * 2;
                return height;
            };

            // Draw element GUI
            list.drawElementCallback = (Rect position, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty stepItemProp = list.serializedProperty.GetArrayElementAtIndex(index);
                StepAsset stepAsset = stepItemProp.objectReferenceValue as StepAsset;

                Rect rect = position;
                rect.y += Standalone.VMargin;
                rect.height = EditorGUIUtility.singleLineHeight;
                
                // Cancel if the step asset reference is not valid
                if (stepAsset == null)
                {
                    EditorGUI.HelpBox(rect, $"Invalid {nameof(Step)} asset", MessageType.Warning);
                    return;
                }

                SerializedObject stepAssetObj = new SerializedObject(stepAsset);
                SerializedProperty isCheckpointProp = stepAssetObj.FindProperty(StepAssetEditor.IsCheckpointProp);

                // Draw foldout
                rect.x += FoldoutOffset;
                rect.width = StepFoldoutWidth - FoldoutOffset;
                stepItemProp.isExpanded = EditorGUI.Foldout(rect, stepItemProp.isExpanded, index.ToString(), true);

                // Draw step name field
                rect.x += rect.width;
                rect.width = position.width - rect.width - IsCheckpointFieldWidth - Standalone.HMargin;
                EditorGUI.BeginChangeCheck();
                string name = EditorGUI.DelayedTextField(rect, stepAsset.name);
                if (EditorGUI.EndChangeCheck())
                {
                    stepAsset.name = name;
                    AssetDatabase.SaveAssets();
                }

                // Draw "is checkpoint" field
                rect.x += rect.width + Standalone.HMargin;
                rect.width = IsCheckpointFieldWidth;
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = IsCheckpointLabelWidth;
                isCheckpointProp.boolValue = EditorGUI.Toggle(rect, isCheckpointProp.GetLabel(), isCheckpointProp.boolValue);
                EditorGUIUtility.labelWidth = previousLabelWidth;

                if (stepItemProp.isExpanded)
                {
                    SerializedProperty descriptionProp = stepAssetObj.FindProperty(StepAssetEditor.DescriptionProp);

                    rect.x = position.x + StepFoldoutWidth + Standalone.HMargin;
                    rect.width = position.width - StepFoldoutWidth;
                    rect.y += rect.height + Standalone.VMargin;
                    EditorGUI.LabelField(rect, descriptionProp.GetLabel());

                    rect.y += rect.height + Standalone.VMargin;
                    rect.height = StepDescriptionFieldHeight;
                    descriptionProp.stringValue = EditorGUI.TextArea(rect, descriptionProp.stringValue);
                }

                stepAssetObj.ApplyModifiedProperties();
            };

            // Reorder behavior
            list.onReorderCallback = list => list.serializedProperty.serializedObject.ApplyModifiedProperties();

            return list;
        }

        #endregion

    }

}