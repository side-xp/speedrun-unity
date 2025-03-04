/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Represents a single segment in a <see cref="SideXP.Speedrun.Run"/>, with its own steps to complete.
    /// </summary>
    public class Segment
    {

        #region Delegates

        /// <summary>
        /// Called when a segment is finiished.
        /// </summary>
        /// <remarks><inheritdoc cref="_finishedAt" path="/remarks"/></remarks>
        /// <param name="segment">The finished segment.</param>
        public delegate void FinishDelegate(Segment segment);

        /// <summary>
        /// Called when a segment is completed.
        /// </summary>
        /// <remarks><inheritdoc cref="_completedAt" path="/remarks"/></remarks>
        /// <param name="segment">The completed segment.</param>
        public delegate void CompleteDelegate(Segment segment);

        /// <summary>
        /// Called when a segment is canceled.
        /// </summary>
        /// <remarks><inheritdoc cref="_canceledAt" path="/remarks"/></remarks>
        /// <param name="segment">The canceled segment.</param>
        public delegate void CancelDelegate(Segment segment);

        /// <summary>
        /// Called when a segment is ended (completed or canceled).
        /// </summary>
        /// <remarks>This is called just after <see cref="OnComplete"/> or <see cref="OnCancel"/>.</remarks>
        /// <param name="segment">The ended segment.</param>
        public delegate void EndDelegate(Segment segment);

        #endregion


        #region Fields

        /// <inheritdoc cref="FinishDelegate"/>
        public event FinishDelegate OnFinish;

        /// <inheritdoc cref="CompleteDelegate"/>
        public event CompleteDelegate OnComplete;

        /// <inheritdoc cref="CancelDelegate"/>
        public event CancelDelegate OnCancel;

        /// <inheritdoc cref="EndDelegate"/>
        public event EndDelegate OnEnd;

        /// <summary>
        /// The <see cref="Speedrun.Run"/> instance to which this instance belongs.
        /// </summary>
        private Run _speedrun = null;

        /// <summary>
        /// The asset from which this instance has been created.
        /// </summary>
        private SegmentAsset _segmentAsset = null;

        /// <summary>
        /// The state of the steps defined for this segment.
        /// </summary>
        private Step[] _steps = null;

        /// <summary>
        /// The date and time when this Segment has been finished. Always null if this segment has defined steps but none is marked as a
        /// checkpoint.
        /// </summary>
        /// <remarks>
        /// A segment is considered as finished when all its steps marked as checkpoints are completed, or if it has defined steps but none
        /// is marked as a checkpoint.</remarks>
        private DateTime? _finishedAt = null;

        /// <summary>
        /// The date and time when this segment has been completed. Always null if this segment has been finished with incomplete steps but
        /// <see cref="SpeedrunSettings.EndSegmentOnFinish"/> option is enabled.
        /// </summary>
        /// <remarks>A segment is considered as completed when all its steps are completed.</remarks>
        private DateTime? _completedAt = null;

        /// <summary>
        /// The date and time when this segment has been canceled. Always null if this segment doesn't have any step defined.
        /// </summary>
        /// <remarks>
        /// A segment is canceled after calling <see cref="Cancel()"/> once, or if it doesn't have any step defined.<br/>
        /// Also, a canceled segment still counts for the run completion.
        /// </remarks>
        private DateTime? _canceledAt = null;

        /// <summary>
        /// The function to call when this segment changes its state (finished, step completed, ...).
        /// </summary>
        private Run.SegmentChangeDelegate _onChange = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="Segment"/>
        /// <param name="segmentAsset">The asset from which this instance is created.</param>
        /// <param name="speedrun"><inheritdoc cref="_speedrun" path="/summary"/></param>
        /// <param name="onChange"><inheritdoc cref="_onChange" path="/summary"/></param>
        internal Segment(SegmentAsset segmentAsset, Run speedrun, Run.SegmentChangeDelegate onChange)
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
            {
                Debug.LogError($"Invalid {nameof(Segment)} instance: No valid {nameof(StepAsset)} found on the original {nameof(SegmentAsset)}. The segment will be considered as canceled.", segmentAsset);
                return;
            }
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_speedrun"/>
        public Run Run => _speedrun;

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
        /// Checks if this segment is finished.
        /// </summary>
        /// <inheritdoc cref="_finishedAt" path="/remarks"/>
        public bool IsFinished => _finishedAt != null || !HasCheckpoint;

        /// <summary>
        /// Checks if this segment is completed.
        /// </summary>
        /// <inheritdoc cref="_completedAt" path="/remarks"/>
        public bool IsCompleted => _completedAt != null;

        /// <summary>
        /// Checks if this segment is canceled.
        /// </summary>
        /// <inheritdoc cref="_canceledAt" path="/remarks"/>
        public bool IsCanceled => _canceledAt != null || _steps == null || _steps.Length <= 0;

        /// <summary>
        /// Checks if this segment is ended (completed or canceled).
        /// </summary>
        public bool IsEnded => IsCompleted || IsCanceled || (IsFinished && _speedrun.Settings.EndSegmentOnFinish);

        /// <summary>
        /// Checks if this segment has a checkpoint step defined.
        /// </summary>
        public bool HasCheckpoint
        {
            get
            {
                foreach (Step step in _steps)
                {
                    if (step.IsCheckpoint)
                        return true;
                }
                return false;
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
            return Finish(true);
        }

        /// <summary>
        /// Marks this segment as canceled.
        /// </summary>
        /// <returns></returns>
        public bool Cancel()
        {
            if (!_speedrun.IsActive)
            {
                Debug.LogWarning($"Failed to cancel the {nameof(Segment)} \"{DisplayName}\": Its owning {nameof(Run)} instance is not active.", _segmentAsset);
                return false;
            }
            else if (IsEnded)
            {
                Debug.LogWarning($"Failed to cancel the {nameof(Segment)} \"{DisplayName}\": That segment is already ended.", _segmentAsset);
                return false;
            }

            _canceledAt = _speedrun.TimeMilliseconds.ToDateTime();
            try
            {
                OnCancel?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            _onChange(this);
            return true;
        }

        #endregion


        #region Private API

        /// <inheritdoc cref="Finish()"/>
        /// <param name="invokeChange">Should the <see cref="_onChange"/> callback be invoked after changing this segment state? This is
        /// used to prevent invoking this callback multiple times when handling a step change in
        /// <see cref="HandleStepChange(Step)"/>.</param>
        private bool Finish(bool invokeChange)
        {
            if (!_speedrun.IsActive)
            {
                Debug.LogWarning($"Failed to finish the {nameof(Segment)} \"{DisplayName}\": Its owning {nameof(Run)} instance is not active.", _segmentAsset);
                return false;
            }
            else if (IsFinished)
            {
                Debug.LogWarning($"Failed to finish the {nameof(Segment)} \"{DisplayName}\": That segment is already finished.", _segmentAsset);
                return false;
            }
            else if (IsEnded)
            {
                Debug.LogWarning($"Failed to finish the {nameof(Segment)} \"{DisplayName}\": That segment is already ended.", _segmentAsset);
                return false;
            }

            _finishedAt = _speedrun.TimeMilliseconds.ToDateTime();
            try
            {
                OnFinish?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (invokeChange)
                _onChange(this);

            return true;
        }

        /// <inheritdoc cref="Run.StepChangeDelegate"/>
        private void HandleStepChange(Step step)
        {
            // Cancel if this segment has already been ended
            if (IsEnded)
                return;

            // Check if this segment has just been finished (by completing all its checkpoints)
            bool hasJustBeenFinished = false;
            if (!IsFinished)
            {
                bool areAllCheckpointsFinished = true;
                foreach (Step s in _steps)
                {
                    if (s.IsCheckpoint && !s.IsCompleted)
                    {
                        areAllCheckpointsFinished = false;
                        break;
                    }
                }

                if (areAllCheckpointsFinished)
                {
                    Finish(false);
                    hasJustBeenFinished = true;
                }
            }

            // Check if this segment has just been finished (by completing all its steps)
            if (!IsCompleted)
            {
                bool areAllStepsCompleted = true;
                foreach (Step s in _steps)
                {
                    if (s.IsCheckpoint && !s.IsCompleted)
                    {
                        areAllStepsCompleted = false;
                        break;
                    }
                }

                if (areAllStepsCompleted)
                {
                    _completedAt = hasJustBeenFinished
                        ? _finishedAt
                        : _completedAt = _speedrun.TimeMilliseconds.ToDateTime();

                    try
                    {
                        OnComplete?.Invoke(this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            // If the step change caused the segment to end, emit the related event
            if (IsEnded)
            {
                try
                {
                    OnEnd?.Invoke(this);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _onChange(this);
        }

        #endregion

    }

}