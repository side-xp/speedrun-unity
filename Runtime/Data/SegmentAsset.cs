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

        [SerializeField]
        [Tooltip("The " + nameof(SpeedrunAsset) + " that contains this segment.")]
        private SpeedrunAsset _speedrunAsset = null;

        [SerializeField, TextArea(3, 6)]
        [Tooltip("The description of this segment, as displayed on UI.")]
        private string _description = null;

        [SerializeField]
        [Tooltip("The steps of this segment." +
            "\nEach step represents a part of the progression, a checkpoint or something to unlock or discover.")]
        private StepAsset[] _steps = { };

        #endregion


        #region Public API

        /// <inheritdoc cref="_displayName"/>
        public string DisplayName => name;

        /// <inheritdoc cref="_description"/>
        public string Description => _description;

        /// <inheritdoc cref="_speedrunAsset"/>
        public SpeedrunAsset SpeedrunAsset => _speedrunAsset;

        /// <inheritdoc cref="_steps"/>
        public StepAsset[] Steps => _steps;

        /// <inheritdoc cref="Segment.IsCompleted"/>
        public bool IsCompleted => SpeedrunAsset.FindSegment(this, out Segment segment) && segment.IsCompleted;

        /// <inheritdoc cref="Segment.IsFinished"/>
        public bool IsFinished => SpeedrunAsset.FindSegment(this, out Segment segment) && segment.IsFinished;

        #endregion

    }

}