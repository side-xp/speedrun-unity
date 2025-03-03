/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Represents the state of a single step in a <see cref="SideXP.Speedrun.Segment"/> instance.
    /// </summary>
    public class Step
    {

        #region Fields

        /// <summary>
        /// The <see cref="Segment"/> instance to which this step belongs.
        /// </summary>
        private Segment _segment = null;

        /// <summary>
        /// The asset from which this instance has been created.
        /// </summary>
        private StepAsset _stepAsset = null;

        /// <summary>
        /// Is this step completed?
        /// </summary>
        private bool _isCompleted = false;

        /// <summary>
        /// The function to call when the state of this step changes.
        /// </summary>
        private Speedrun.StepChangeDelegate _onChange = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="Step"/>
        /// <param name="stepAsset">The asset from which this instance is created.</param>
        /// <param name="segment"><inheritdoc cref="_segment" path="/summary"/></param>
        /// <param name="onChange"><inheritdoc cref="_onChange" path="/summary"/></param>
        internal Step(StepAsset stepAsset, Segment segment, Speedrun.StepChangeDelegate onChange)
        {
            _stepAsset = stepAsset;
            _segment = segment;
            _onChange = onChange;
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_stepAsset"/>
        public StepAsset StepAsset => _stepAsset;

        /// <inheritdoc cref="StepAsset.DisplayName"/>
        public string DisplayName => _stepAsset != null ? _stepAsset.DisplayName : GetType().Name;

        /// <inheritdoc cref="_segment"/>
        public Segment Segment => _segment;

        /// <inheritdoc cref="_isCompleted"/>
        public bool IsCompleted => _isCompleted;

        /// <summary>
        /// Is this step a checkpoint?
        /// </summary>
        public bool IsCheckpoint => _stepAsset != null && _stepAsset.IsCheckpoint;

        /// <summary>
        /// Marks this step as completed.
        /// </summary>
        /// <returns>Returns true if this step has been marked as completed successfully.</returns>
        public bool Complete()
        {
            if (_isCompleted)
            {
                Debug.LogWarning($"Failed to mark the {nameof(Step)} \"{DisplayName}\" as completed: That instance is already marked as completed.", _stepAsset);
                return false;
            }

            _isCompleted = true;
            _onChange?.Invoke(this);
            return true;
        }

        #endregion

    }

}