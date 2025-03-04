/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using System.Security.Cryptography;

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <inheritdoc cref="Step"/>
    [HelpURL(Constants.BaseHelpUrl)]
    public class StepAsset : ScriptableObject
    {

        #region Fields

        /// <summary>
        /// Called when a <see cref="Run"/> is started from the <see cref="SideXP.Speedrun.SpeedrunAsset"/> that owns this
        /// <see cref="StepAsset"/>.
        /// </summary>
        public event Run.StartDelegate OnSpeedrunStart;

        /// <summary>
        /// Called when the <see cref="Run"/> started from the <see cref="SideXP.Speedrun.SpeedrunAsset"/> that owns this
        /// <see cref="StepAsset"/> is completed.
        /// </summary>
        public event Step.CompleteDelegate OnComplete;

        [SerializeField]
        [Tooltip("The " + nameof(SegmentAsset) + " that contains this " + nameof(Step) + ".")]
        private SegmentAsset _segmentAsset = null;

        [SerializeField, TextArea(3, 6)]
        [Tooltip("The description of this " + nameof(Step) + ", as displayed on UI.")]
        private string _description = null;

        [SerializeField]
        [Tooltip("Does this step counts as milestone that can cause the " + nameof(Segment) + " to finish?" +
            "\nA " + nameof(Segment) + " is considered finished when all its checkpoints are completed, or by calling its " + nameof(Segment.Finish) + "() function manually.")]
        private bool _isCheckpoint = false;

        /// <summary>
        /// 
        /// </summary>
        private Step _step = null;

        #endregion


        #region Lifeycle

        private void OnDisable()
        {
            _step = null;
        }

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
        /// Marks this step as completed in the <see cref="Run"/> instance started from the owning <see cref="SpeedrunAsset"/>.
        /// </summary>
        public void Complete()
        {
            if (!SpeedrunAsset.HasActiveSpeedrunInstance)
            {
                Debug.LogWarning($"Failed to complete a segment step from this asset: No active {nameof(Run)} instance found in the owining {nameof(SpeedrunAsset)}. You can start a {nameof(Run)} instance by calling {nameof(SpeedrunAsset)}.{nameof(SpeedrunAsset.StartSpeedrun)}().", this);
                return;
            }

            if (!SpeedrunAsset.Speedrun.FindStep(this, out Step step))
            {
                Debug.LogError($"Failed to complete a segment step from this asset: The active {nameof(Run)} instance doesn't contain any {nameof(Step)} instance created from this asset. This may happen if you have loaded an unfinished {nameof(Run)} from a file that has been started with a previous version of the game (that didn't include this step).", this);
                return;
            }

            step.Complete();
        }

        #endregion


        #region Internal API

        /// <summary>
        /// Called at runtime when a <see cref="Run"/> instance is created from the <see cref="SideXP.Speedrun.SpeedrunAsset"/> that owns
        /// this <see cref="StepAsset"/>.
        /// </summary>
        /// <param name="step">The <see cref="Step"/> instance cerated from this asset in the <see cref="Run"/> instance created from the
        /// <see cref="SideXP.Speedrun.SpeedrunAsset"/> that owns this <see cref="StepAsset"/>.</param>
        internal void AssignStep(Step step)
        {
            if (_step != null)
            {
                _step.Segment.Run.OnStart -= HandleRunStarted;
                _step.OnComplete -= HandleStepCompleted;
            }

            _step = step;

        }

        #endregion


        #region Private API

        /// <inheritdoc cref="Run.OnStart"/>
        private void HandleRunStarted(Run run)
        {
            OnSpeedrunStart(run);
        }

        /// <inheritdoc cref="Step.OnComplete"/>
        private void HandleStepCompleted(Step step)
        {
            OnComplete?.Invoke(step);
        }

        #endregion

    }

}