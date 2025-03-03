/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <inheritdoc cref="Step"/>
    [HelpURL(Constants.BaseHelpUrl)]
    public class StepAsset : ScriptableObject
    {

        #region Fields

        [SerializeField]
        [Tooltip("The " + nameof(SegmentAsset) + " that contains this " + nameof(Step) + ".")]
        private SegmentAsset _segmentAsset = null;

        [SerializeField, TextArea(3, 6)]
        [Tooltip("The description of this " + nameof(Step) + ", as displayed on UI.")]
        private string _description = null;

        [SerializeField]
        [Tooltip("Does this step counts as milestone that can cause the " + nameof(Segment) + " to finish?" +
            "\nBy default, a " + nameof(Segment) + " is considered finished when all its checkpoints are completed, or by calling its " + nameof(Segment.Finish) + "() function manually." +
            "\nIf the " + nameof(SpeedrunSettings.FinishOnCompleteLastCheckpoint) + " option is enabled on the " + nameof(Speedrun) + " instance, a " + nameof(Segment) + " will be considered finished as soon as its last checkpoint is completed, no matter the other ones.")]
        private bool _isCheckpoint = false;

        #endregion


        #region Public API

        /// <inheritdoc cref="_displayName"/>
        public string DisplayName => name;

        /// <inheritdoc cref="_description"/>
        public string Description => _description;

        /// <inheritdoc cref="_isCheckpoint"/>
        public bool IsCheckpoint => _isCheckpoint;

        /// <inheritdoc cref="_segmentAsset"/>
        public SegmentAsset SegmentAsset => _segmentAsset;

        /// <summary>
        /// The top-level <see cref="SideXP.Speedrun.SpeedrunAsset"/> that contains this step.
        /// </summary>
        public SpeedrunAsset SpeedrunAsset => _segmentAsset != null ? _segmentAsset.SpeedrunAsset : null;

        /// <inheritdoc cref="Step.IsCompleted"/>
        public bool IsCompleted => SpeedrunAsset.FindStep(this, out Step step) && step.IsCompleted;

        /// <summary>
        /// Marks this step as completed in the <see cref="Speedrun"/> instance started from the owning <see cref="SpeedrunAsset"/>.
        /// </summary>
        public void Complete()
        {
            if (!SpeedrunAsset.HasActiveSpeedrunInstance)
            {
                Debug.LogWarning($"Failed to complete a segment step from this asset: No active {nameof(Speedrun)} instance found in the owining {nameof(SpeedrunAsset)}. You can start a {nameof(Speedrun)} instance by calling {nameof(SpeedrunAsset)}.{nameof(SpeedrunAsset.StartSpeedrun)}().", this);
                return;
            }

            if (!SpeedrunAsset.SpeedrunInstance.FindStep(this, out Step step))
            {
                Debug.LogError($"Failed to complete a segment step from this asset: The active {nameof(Speedrun)} instance doesn't contain any {nameof(Step)} instance created from this asset. This may happen if you have loaded an unfinished {nameof(Speedrun)} from a file that has been started with a previous version of the game (that didn't include this step).", this);
                return;
            }

            step.Complete();
        }

        #endregion

    }

}