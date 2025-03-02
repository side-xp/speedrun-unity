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
    /// Custom editor for <see cref="SpeedrunAsset"/>
    /// </summary>
    [CustomEditor(typeof(SpeedrunAsset))]
    public class SpeedrunAssetEditor : Editor
    {

        #region Fields

        private const float SegmentStepsCountFieldWidth = Standalone.WidthS;

        private const string Speedtun_SettingsProp = "_settings";
        private const string Speedrun_SegmentsArrayProp = "_segments";
        private const string SegmentAsset_SpeedrunProp = "_speedrunAsset";
        private const string SegmentAsset_StepsArrayProp = "_steps";

        private SerializedProperty _settingsProp = null;
        private SerializedProperty _segmentsArrayProp = null;

        private ReorderableList _segmentsReorderableList = null;

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            _settingsProp = serializedObject.FindProperty(Speedtun_SettingsProp);
            _segmentsArrayProp = serializedObject.FindProperty(Speedrun_SegmentsArrayProp);

            _segmentsReorderableList = BuildSegmentsReorderableList(_segmentsArrayProp);
        }

        #endregion


        #region UI

        /// <inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_settingsProp);
            
            EditorGUILayout.Space();
            _segmentsReorderableList.DoLayoutList();
        }

        #endregion


        #region Private API

        /// <summary>
        /// Creates a <see cref="ReorderableList"/> for the Segment assets attached to a <see cref="SpeedrunAsset"/>.
        /// </summary>
        /// <param name="segmentsArrayProp">The serialized representation of the property to display as a <see cref="ReorderableList"/>.
        /// Assumes that this property is an array of <see cref="SegmentAsset"/>.</param>
        /// <returns>Returns the created <see cref="ReorderableList"/>.</returns>
        private ReorderableList BuildSegmentsReorderableList(SerializedProperty segmentsArrayProp)
        {
            ReorderableList list = new ReorderableList(segmentsArrayProp.serializedObject, segmentsArrayProp);

            // Draw list header
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, segmentsArrayProp.GetLabel(), EditorStyles.boldLabel);

            // "Add" button behavior
            list.onAddCallback = list => CreateAndAttachSegmentAsset(segmentsArrayProp);

            // "Remove" button behavior
            list.onRemoveCallback = list =>
            {
                int index = list.index;
                SerializedProperty segmentItemProp = list.serializedProperty.GetArrayElementAtIndex(index);
                SegmentAsset segmentAsset = segmentItemProp.objectReferenceValue as SegmentAsset;

                // Cancel if the user cancels the operation
                if (!EditorUtility.DisplayDialog(
                    "Delete Segment",
                    $"The selected Segment \"{(segmentAsset != null ? segmentAsset.DisplayName : "null")}\" and all the Steps attached to it will be deleted." +
                    $"\nThis operation can't be undone. Proceed?",
                    "Yes, delete Segment", "No"))
                {
                    return;
                }

                if (segmentAsset != null)
                {
                    // Destroy all step assets from the segment to delete
                    foreach (StepAsset stepAsset in segmentAsset.Steps)
                        DestroyImmediate(stepAsset, true);

                    // Destroy the segment asset itself
                    DestroyImmediate(segmentAsset, true);
                }

                // Remove the entry from the list
                segmentItemProp.objectReferenceValue = null;
                list.serializedProperty.DeleteArrayElementAtIndex(index);

                // Save changes
                list.serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                list.serializedProperty.serializedObject.targetObject.SaveAndReimport();
            };

            // Calculate the height of a single element in the list
            list.elementHeightCallback = index =>
            {
                return EditorGUIUtility.singleLineHeight + Standalone.VMargin * 2;
            };

            // Draw element GUI
            list.drawElementCallback = (Rect position, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty segmentItemProp = list.serializedProperty.GetArrayElementAtIndex(index);
                SegmentAsset segmentAsset = segmentItemProp.objectReferenceValue as SegmentAsset;

                Rect rect = position;
                rect.y += Standalone.VMargin;
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.width -= SegmentStepsCountFieldWidth + Standalone.HMargin;

                if (segmentAsset == null)
                {
                    EditorGUI.HelpBox(rect, "Invalid Segment asset", MessageType.Warning);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    string name = EditorGUI.DelayedTextField(rect, segmentAsset.name);
                    if (EditorGUI.EndChangeCheck())
                    {
                        segmentAsset.name = name;
                        AssetDatabase.SaveAssets();
                    }
                }

                if (segmentAsset != null)
                {
                    rect.x += rect.width + Standalone.HMargin;
                    rect.width = SegmentStepsCountFieldWidth;
                    EditorGUI.LabelField(rect, segmentAsset.Steps.Length + " Steps", EditorStyles.label.Italic().AlignRight());
                }
            };

            return list;
        }

        /// <summary>
        /// Creates a new <see cref="SegmentAsset"/>, attach it to the inspected asset, and add it to the given array property.
        /// </summary>
        /// <param name="segmentsArrayProp"><inheritdoc cref="BuildSegmentsReorderableList" path="/param[@name='segmentsArrayProp']"/></param>
        /// <returns>Returns the created <see cref="SegmentAsset"/>.</returns>
        private SegmentAsset CreateAndAttachSegmentAsset(SerializedProperty segmentsArrayProp)
        {
            // Create the new Segment asset
            SegmentAsset segmentAsset = CreateInstance<SegmentAsset>();
            segmentAsset.name = $"New{nameof(SegmentAsset)}";

            // Assign the owning Speedrun asset to the created Segment asset
            SerializedObject segmentAssetObj = new SerializedObject(segmentAsset);
            SerializedProperty speedrunProp = segmentAssetObj.FindProperty(SegmentAsset_SpeedrunProp);
            speedrunProp.objectReferenceValue = segmentsArrayProp.serializedObject.targetObject;
            segmentAssetObj.ApplyModifiedPropertiesWithoutUndo();

            // Attach the created Segment asset to the inspected Speedrun asset
            AssetDatabase.AddObjectToAsset(segmentAsset, segmentsArrayProp.serializedObject.targetObject);

            // Insert the created Segment asset in the list
            int index = segmentsArrayProp.arraySize;
            segmentsArrayProp.InsertArrayElementAtIndex(index);
            segmentsArrayProp.GetArrayElementAtIndex(index).objectReferenceValue = segmentAsset;
            segmentsArrayProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Save and reimport the Speedrun asset to refresh the Project view
            segmentsArrayProp.serializedObject.targetObject.SaveAndReimport();
            return segmentAsset;
        }

        #endregion

    }

}