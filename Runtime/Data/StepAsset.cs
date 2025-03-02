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
    [CreateAssetMenu(fileName = "NewStepAsset", menuName = Constants.CreateAssetMenu + "/StepAsset")]
    public abstract class StepAsset : ScriptableObject
    {

        #region Fields

        [Header("References")]

        [SerializeField]
        [Tooltip("The " + nameof(SegmentAsset) + " that contains this step.")]
        private SegmentAsset _segmentAsset = null;

        [Header("Label")]

        [SerializeField]
        [Tooltip("The description of this segment, as displayed on UI.")]
        private string _description = null;

        #endregion


        #region Public API

        /// <inheritdoc cref="_displayName"/>
        public string DisplayName => name;

        /// <inheritdoc cref="_description"/>
        public string Description => _description;

        /// <inheritdoc cref="_segmentAsset"/>
        public SegmentAsset SegmentAsset => _segmentAsset;

        /// <summary>
        /// The top-level <see cref="SideXP.Speedrun.SpeedrunAsset"/> that contains this step.
        /// </summary>
        public SpeedrunAsset SpeedrunAsset => _segmentAsset != null ? _segmentAsset.SpeedrunAsset : null;

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