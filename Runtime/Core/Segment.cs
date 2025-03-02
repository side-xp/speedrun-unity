/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Represents a single segment in a <see cref="SideXP.Speedrun.Speedrun"/>, with its own steps to complete.
    /// </summary>
    public class Segment
    {

        #region Fields

        /// <summary>
        /// The <see cref="Speedrun"/> instance to which this instance belongs.
        /// </summary>
        private Speedrun _speedrun = null;

        /// <summary>
        /// The asset from which this instance has been created.
        /// </summary>
        private SegmentAsset _segmentAsset = null;

        /// <summary>
        /// The state of the steps defined for this segment.
        /// </summary>
        private Step[] _steps = null;

        /// <summary>
        /// The time (in seconds) since the beginning of the run at which this segment has been finished.
        /// </summary>
        private double _finishedAt = -1;

        /// <summary>
        /// The function to call when this segment changes its state (finished, step completed, ...).
        /// </summary>
        private Speedrun.SegmentChangeDelegate _onChange = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="Segment"/>
        /// <param name="segmentAsset">The asset from which this instance is created.</param>
        /// <param name="speedrun"><inheritdoc cref="_speedrun" path="/summary"/></param>
        /// <param name="onChange"><inheritdoc cref="_onChange" path="/summary"/></param>
        internal Segment(SegmentAsset segmentAsset, Speedrun speedrun, Speedrun.SegmentChangeDelegate onChange)
        {
            _segmentAsset = segmentAsset;
            _speedrun = speedrun;
            _onChange = onChange;

            List<Step> stepsList = ListPool<Step>.Get();
            foreach (StepAsset stepAsset in segmentAsset.Steps)
            {
                if (stepAsset == null)
                    continue;

                Step step = new Step(stepAsset, this, HandleStepChange);
                stepsList.Add(step);
            }

            _steps = stepsList.ToArray();
            ListPool<Step>.Release(stepsList);
            if (_steps.Length <= 0)
                Debug.LogError($"Invalid {nameof(Segment)} instance: No valid {nameof(StepAsset)} found on the original {nameof(SegmentAsset)}. The segment will be considered as completed.", segmentAsset);
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_speedrun"/>
        public Speedrun Speedrun => _speedrun;

        /// <inheritdoc cref="_segmentAsset"/>
        public SegmentAsset SegmentAsset => _segmentAsset;

        /// <inheritdoc cref="SegmentAsset.DisplayName"/>
        public string DisplayName => _segmentAsset != null ? _segmentAsset.DisplayName : GetType().Name;

        /// <inheritdoc cref="_steps"/>
        public Step[] Steps => _steps;

        /// <summary>
        /// Gets the number of steps for this segment.
        /// </summary>
        public int StepsCount => _steps.Length;

        /// <summary>
        /// Gets the number of steps for this segment that have been completed so far.
        /// </summary>
        public int StepsCompletedCount
        {
            get
            {
                int count = 0;
                foreach (Step step in _steps)
                {
                    if (step.IsCompleted)
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Is this segment finished?
        /// </summary>
        /// <remarks>
        /// A segment is considered finished if:<br/>
        /// - <see cref="Finish()"/> has been called once<br/>
        /// - The run allows auto-finish, and the last defined checkpoint step has been completed
        /// </remarks>
        public bool IsFinished => _finishedAt >= 0;

        /// <summary>
        /// Have all the steps of this segment been completed?
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                foreach (Step step in _steps)
                {
                    if (!step.IsCompleted)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the last checkpoint step of this segment.
        /// </summary>
        public Step LastCheckpoint
        {
            get
            {
                for (int i = _steps.Length - 1; i >= 0; i--)
                {
                    if (_steps[i].IsCheckpoint)
                        return _steps[i];
                }
                return null;
            }
        }

        /// <summary>
        /// Marks this segment as finished.
        /// </summary>
        /// <returns>Returns true if this segment has been marked as finished successfully.</returns>
        public bool Finish()
        {
            if (IsFinished)
            {
                Debug.LogWarning($"Failed to finish the {nameof(Segment)} \"{DisplayName}\": That segment is already marked as finished.", _segmentAsset);
                return false;
            }

            _finishedAt = _speedrun.Timer;
            _onChange(this);
            return true;
        }

        #endregion


        #region Private API

        /// <inheritdoc cref="Speedrun.StepChangeDelegate"/>
        private void HandleStepChange(Step step)
        {
            // Automaticaly mark this segment as finished if the last checkpoint has been completed and auto-finish is allowed
            if (!IsFinished && _speedrun.Settings.AutoFinish && step.IsCheckpoint && LastCheckpoint == step)
                Finish();

            _onChange(this);
        }

        #endregion

    }

}