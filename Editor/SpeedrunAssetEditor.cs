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
        private const float TitleLabelHeight = Standalone.HeightS;

        private const string SettingsProp = "_settings";
        private const string SegmentsArrayProp = "_segments";

        private static GUIStyle s_italicLabelStyle = null;
        private static GUIStyle s_titleLabelStyle = null;

        private SerializedProperty _settingsProp = null;
        private SerializedProperty _segmentsArrayProp = null;

        private ReorderableList _segmentsReorderableList = null;
        private Editor _selectedSegmentEditor = null;

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            _settingsProp = serializedObject.FindProperty(SettingsProp);
            _segmentsArrayProp = serializedObject.FindProperty(SegmentsArrayProp);

            _segmentsReorderableList = BuildSegmentsReorderableList(_segmentsArrayProp);
        }

        private void OnDisable()
        {
            if (_selectedSegmentEditor != null)
            {
                DestroyImmediate(_selectedSegmentEditor);
                _selectedSegmentEditor = null;
            }
        }

        #endregion


        #region UI

        /// <inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            // Draw settings property
            EditorGUILayout.PropertyField(_settingsProp);
            
            // Draw segments list
            EditorGUILayout.Space();
            _segmentsReorderableList.DoLayoutList();

            // Draw selected segment editor if applicable
            if (SelectedSegmentEditor != null)
            {
                EditorGUILayout.Space();
                Standalone.HorizontalSeparator();
                EditorGUILayout.LabelField(SelectedSegmentEditor.target.name, TitleLabelStyle, GUILayout.Height(TitleLabelHeight));

                EditorGUILayout.Space();
                SelectedSegmentEditor.OnInspectorGUI();
            }
        }

        /// <summary>
        /// Draws the custom GUI for editing a <see cref="SegmentAsset"/>.
        /// </summary>
        /// <param name="segmentProp">The serialized representation of property taht references the <see cref="SegmentAsset"/> to
        /// edit.</param>
        private void DrawSegmentGUI(SerializedProperty segmentProp)
        {
            SegmentAsset segmentAsset = segmentProp.objectReferenceValue as SegmentAsset;
            SerializedObject segmentAssetObj = new SerializedObject(segmentAsset);

        }

        #endregion


        #region Private API

        /// <inheritdoc cref="s_italicLabelStyle"/>
        private static GUIStyle ItalicLabelStyle
        {
            get
            {
                if (s_italicLabelStyle == null)
                {
                    s_italicLabelStyle = new GUIStyle(EditorStyles.label);
                    s_italicLabelStyle.fontStyle = FontStyle.Italic;
                }
                return s_italicLabelStyle;
            }
        }

        /// <inheritdoc cref="s_titleLabelStyle"/>
        private static GUIStyle TitleLabelStyle
        {
            get
            {
                if (s_titleLabelStyle == null)
                {
                    s_titleLabelStyle = new GUIStyle(EditorStyles.largeLabel);
                    s_titleLabelStyle.fontStyle = FontStyle.Bold;
                }
                return s_titleLabelStyle;
            }
        }

        /// <inheritdoc cref="_selectedSegmentEditor"/>
        private Editor SelectedSegmentEditor
        {
            get
            {
                if (_selectedSegmentEditor == null)
                {
                    // Cancel if there's no valid segment asset in the list
                    if (_segmentsReorderableList == null || _segmentsReorderableList.index < 0 || _segmentsReorderableList.serializedProperty.arraySize <= 0)
                        return null;

                    SerializedProperty selectedSegmentProp = _segmentsReorderableList.serializedProperty.GetArrayElementAtIndex(_segmentsReorderableList.index);
                    // Cancel if the selected segment asset is not valid
                    if (selectedSegmentProp.objectReferenceValue == null)
                        return null;

                    _selectedSegmentEditor = CreateEditor(selectedSegmentProp.objectReferenceValue);
                }
                return _selectedSegmentEditor;
            }
        }

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
            list.onAddCallback = list =>
            {
                Standalone.CreateAndAttachAsset<SegmentAsset>(list.serializedProperty, assetObj =>
                {
                    // Assign the owning Speedrun asset to the created Segment asset
                    SerializedProperty speedrunProp = assetObj.FindProperty(SegmentAssetEditor.SpeedrunProp);
                    speedrunProp.objectReferenceValue = list.serializedProperty.serializedObject.targetObject;
                    assetObj.ApplyModifiedPropertiesWithoutUndo();
                });
            };

            // "Remove" button behavior
            list.onRemoveCallback = list =>
            {
                int index = list.index;
                SerializedProperty segmentItemProp = list.serializedProperty.GetArrayElementAtIndex(index);
                SegmentAsset segmentAsset = segmentItemProp.objectReferenceValue as SegmentAsset;

                if (segmentAsset != null)
                {
                    // Cancel if the user cancels the operation
                    if (!EditorUtility.DisplayDialog(
                    "Delete Segment",
                    $"The selected {nameof(Segment)} asset \"{(segmentAsset != null ? segmentAsset.DisplayName : "null")}\" and all the Steps attached to it will be deleted." +
                    "\nThis operation can't be undone. Proceed?",
                    "Yes, delete Segment", "No"))
                    {
                        return;
                    }

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

                if (segmentAsset == null)
                {
                    EditorGUI.HelpBox(rect, $"Invalid {nameof(Segment)} asset", MessageType.Warning);
                    return;
                }

                rect.width -= SegmentStepsCountFieldWidth + Standalone.HMargin;
                EditorGUI.BeginChangeCheck();
                string name = EditorGUI.DelayedTextField(rect, segmentAsset.name);
                if (EditorGUI.EndChangeCheck())
                {
                    segmentAsset.name = name;
                    AssetDatabase.SaveAssets();
                }

                rect.x += rect.width + Standalone.HMargin;
                rect.width = SegmentStepsCountFieldWidth;
                EditorGUI.LabelField(rect, segmentAsset.Steps.Length + " Steps", EditorStyles.label.Italic().AlignRight());
            };

            // Reorder behavior
            list.onReorderCallback = list => list.serializedProperty.serializedObject.ApplyModifiedProperties();

            // Selection behavior
            list.onSelectCallback = list =>
            {
                if (_selectedSegmentEditor != null)
                {
                    DestroyImmediate(_selectedSegmentEditor);
                    _selectedSegmentEditor = null;
                }
            };

            // Forces the first item to be selected after creating the list
            if (list.serializedProperty.arraySize > 0)
                list.Select(0);

            return list;
        }

        #endregion

    }

}