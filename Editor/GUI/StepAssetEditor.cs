/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;
using UnityEditor;

namespace SideXP.Speedrun.EditorOnly
{

    /// <summary>
    /// Custom editor for <see cref="StepAsset"/>.
    /// </summary>
    [CustomEditor(typeof(StepAsset))]
    public class StepAssetEditor : Editor
    {

        #region Fields

        internal const string SegmentProp = "_segmentAsset";
        internal const string DescriptionProp = "_description";
        internal const string IsCheckpointProp = "_isCheckpoint";

        private SerializedProperty _segmentProp = null;
        private SerializedProperty _descriptionProp = null;
        private SerializedProperty _isCheckpointProp = null;

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            _segmentProp = serializedObject.FindProperty(SegmentProp);
            _descriptionProp = serializedObject.FindProperty(DescriptionProp);
            _isCheckpointProp = serializedObject.FindProperty(IsCheckpointProp);
        }

        #endregion


        #region UI

        /// <inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_segmentProp);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_descriptionProp);
            EditorGUILayout.PropertyField(_isCheckpointProp);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

    }

}