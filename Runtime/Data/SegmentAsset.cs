/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <inheritdoc cref="Segment"/>
    [HelpURL(Constants.BaseHelpUrl)]
    public class SegmentAsset : ScriptableObject
    {

        #region Fields

        [Header("Label")]

        [SerializeField]
        [Tooltip("The name of this segment, as displayed on UI." +
            "\nIf not defined, uses the asset name instead.")]
        private string _displayName = null;

        [SerializeField]
        [Tooltip("The description of this segment, as displayed on UI.")]
        private string _description = null;

        [Header("Progression")]

        [SerializeField]
        [Tooltip("The steps of this segment." +
            "\nEach step represents a part of the progression, a checkpoint or something to unlock or discover.")]
        private StepAsset[] _steps = { };

        #endregion


        #region Public API

        /// <inheritdoc cref="_displayName"/>
        public string DisplayName => !string.IsNullOrWhiteSpace(_displayName) ? _displayName : name;

        /// <inheritdoc cref="_description"/>
        public string Description => _description;

        /// <inheritdoc cref="_steps"/>
        public StepAsset[] Steps => _steps;

        #endregion

    }

}