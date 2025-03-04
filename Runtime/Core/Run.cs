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
    /// Represents a speedrun, with its own timer and progression.
    /// </summary>
    public class Run
    {

        #region Delegates

        /// <summary>
        /// Called when a run is started.
        /// </summary>
        /// <param name="run">The started run.</param>
        public delegate void StartDelegate(Run run);

        /// <summary>
        /// Called when a run is paused or resumed.
        /// </summary>
        /// <param name="run">The <see cref="Run"/> instance of which the state changed.</param>
        /// <param name="isPaused">Is the run now paused?</param>
        public delegate void PauseStateChangeDelegate(Run run, bool isPaused);

        /// <summary>
        /// Called when a run is finished.
        /// </summary>
        /// <remarks>A run is considered finished if all its Segments are finished.</remarks>
        /// <param name="run">The run that has been finished.</param>
        public delegate void FinishDelegate(Run run);

        /// <summary>
        /// Called when a run is completed.
        /// </summary>
        /// <remarks>A run is considered completed if all its Segments are completed.</remarks>
        /// <param name="run">The completed run.</param>
        public delegate void CompleteDelegate(Run run);

        /// <summary>
        /// Called when a run is canceled.
        /// </summary>
        /// <param name="run">The run that has been canceled.</param>
        public delegate void CancelDelegate(Run run);

        /// <summary>
        /// Called when a run is ended (completed or canceled).
        /// </summary>
        /// <remarks>This is called just after <see cref="OnComplete"/> or <see cref="OnCancel"/>.</remarks>
        /// <param name="run">The ended run.</param>
        public delegate void EndDelegate(Run run);

        /// <summary>
        /// Called when the state of a <see cref="Segment"/> changes.
        /// </summary>
        /// <param name="segment">The changed segment instance.</param>
        internal delegate void SegmentChangeDelegate(Segment segment);

        /// <summary>
        /// Called when the state of a <see cref="Step"/> changes.
        /// </summary>
        /// <param name="step">The changed step instance.</param>
        internal delegate void StepChangeDelegate(Step step);

        #endregion


        #region Fields

        /// <inheritdoc cref="StartDelegate"/>
        public event StartDelegate OnStart;

        /// <inheritdoc cref="PauseStateChangeDelegate"/>
        public event PauseStateChangeDelegate OnPauseStateChange;

        /// <inheritdoc cref="FinishDelegate"/>
        public event FinishDelegate OnFinish;

        /// <inheritdoc cref="CompleteDelegate"/>
        public event CompleteDelegate OnComplete;

        /// <inheritdoc cref="CancelDelegate"/>
        public event CancelDelegate OnCancel;

        /// <inheritdoc cref="EndDelegate"/>
        public event EndDelegate OnEnd;

        /// <summary>
        /// The asset from which this instance has been created.
        /// </summary>
        private SpeedrunAsset _speedrunAsset = null;

        /// <summary>
        /// The settings used for this run.
        /// </summary>
        private SpeedrunSettings _settings = default;

        /// <summary>
        /// The state of the segments defined for this run.
        /// </summary>
        private Segment[] _segments = null;

        /// <summary>
        /// Tracks the amount the amount of time (in milliseconds) elapsed since this run has been started to the last time it has been
        /// resumed.
        /// </summary>
        private long _timeMilliseconds = 0;

        /// <summary>
        /// The date and time at which this run has been started. Null if this run has not been started yet.
        /// </summary>
        private DateTime? _startedAt = null;

        /// <summary>
        /// The date and time at which this run has been paused. Null if this run is not paused.
        /// </summary>
        private DateTime? _pausedAt = null;

        /// <summary>
        /// The date and time at which this run has been resumed for the last time. Null if this run has never been resumed.
        /// </summary>
        private DateTime? _resumedAt = null;

        /// <summary>
        /// The date and time at which this run has been finished. Null if this run is not finished.
        /// </summary>
        /// <remarks>A run is considered as finished when all its <see cref="Segment"/> are marked as finished too.</remarks>
        private DateTime? _finishedAt = null;

        /// <summary>
        /// The date and time at which this run has been completed. Null if this run is not completed.
        /// </summary>
        /// <remarks>A run is considered as completed when all its <see cref="Segment"/> are marked as completed too.</remarks>
        private DateTime? _completedAt = null;

        /// <summary>
        /// The date and time at which this run has been canceled. Null if this run is not canceled.
        /// </summary>
        /// <remarks>This value is set by calling <see cref="Cancel()"/> once when the run is still active.</remarks>
        private DateTime? _canceledAt = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="Run"/>
        /// <param name="speedrunAsset">The asset from which this instance is created.</param>
        /// <param name="settings"><inheritdoc cref="_settings" path="/summary"/></param>
        internal Run(SpeedrunAsset speedrunAsset, SpeedrunSettings settings)
        {
            _speedrunAsset = speedrunAsset;
            _settings = settings;

            List<Segment> segmentsList = ListPool<Segment>.Get();
            foreach (SegmentAsset segmentAsset in speedrunAsset.Segments)
            {
                if (segmentAsset == null)
                    continue;

                bool hasValidStep = false;
                foreach (StepAsset stepAsset in segmentAsset.Steps)
                {
                    if (stepAsset != null)
                    {
                        hasValidStep = true;
                        break;
                    }
                }

                if (!hasValidStep)
                {
                    Debug.LogWarning($"Invalid {nameof(SegmentAsset)}: That asset doesn't declare any valid step.", segmentAsset);
                    continue;
                }

                Segment segment = new Segment(segmentAsset, this, HandleSegmentChange);
                segmentsList.Add(segment);
            }

            _segments = segmentsList.ToArray();
            ListPool<Segment>.Release(segmentsList);
            if (_segments.Length <= 0)
                Debug.LogError($"Invalid {nameof(Run)} instance: No valid {nameof(SegmentAsset)} found on the original {nameof(SpeedrunAsset)}. The run will be considered as completed as soon as it's started.", speedrunAsset);
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_speedrunAsset"/>
        public SpeedrunAsset SpeedrunAsset => _speedrunAsset;

        /// <summary>
        /// Checks if this run has been started.
        /// </summary>
        public bool IsStarted => _startedAt != null;

        /// <summary>
        /// Checks if this run has been started but is not yet ended. Returns true even if this run is paused.
        /// </summary>
        public bool IsActive => IsStarted && !IsEnded;

        /// <summary>
        /// Checks if this run is paused.
        /// </summary>
        public bool IsPaused
        {
            get => _pausedAt != null;
            set
            {
                if (value)
                    Pause();
                else
                    Resume();
            }
        }

        /// <summary>
        /// Checks if thiss run has been finished.
        /// </summary>
        public bool IsFinished => _finishedAt != null;

        /// <summary>
        /// Checks if this run has been completed.
        /// </summary>
        public bool IsCompleted => _completedAt != null;

        /// <summary>
        /// Checks if this run has been canceled.
        /// </summary>
        public bool IsCanceled => _canceledAt != null;

        /// <summary>
        /// Checks if this run is ended (and shouldn't be modified anymore.
        /// </summary>
        public bool IsEnded
        {
            get
            {
                // Consider the run as ended if it has been completed or canceled
                if (IsCompleted || IsCanceled)
                    return true;

                // Consider the run as ended if it has been finished, and settings allows it to end in this case
                return IsFinished && _settings.EndSpeedrunOnFinish;
            }
        }

        /// <inheritdoc cref="_startedAt"/>
        public DateTime StartedAt => (DateTime)_startedAt;

        /// <inheritdoc cref="_finishedAt"/>
        public DateTime FinishedAt => (DateTime)_finishedAt;

        /// <inheritdoc cref="_completedAt"/>
        public DateTime CompletedAt => (DateTime)_completedAt;

        /// <inheritdoc cref="_canceledAt"/>
        public DateTime CanceledAt => (DateTime)_canceledAt;

        /// <summary>
        /// Gets the elapsed time (in milliseconds) since this run has been started, excluding pauses.
        /// </summary>
        public long TimeMilliseconds
        {
            get
            {
                if (!IsStarted)
                    return 0;
                else if (IsEnded || IsPaused)
                    return _timeMilliseconds;
                else if (_resumedAt != null)
                    return _timeMilliseconds + (SpeedrunUtility.GetMillisecondsToNow() - ((DateTime)_resumedAt).GetMillisecondsToDate());
                else
                    return SpeedrunUtility.GetMillisecondsToNow() - ((DateTime)_startedAt).GetMillisecondsToDate();
            }
        }

        /// <summary>
        /// Gets a formatted time span from the number of milliseconds elapsed since this run has been started.
        /// </summary>
        public TimeSpan Time => TimeSpan.FromMilliseconds(TimeMilliseconds);

        /// <summary>
        /// Gets the number of steps in all the segments of this speedrun.
        /// </summary>
        public int StepsCount
        {
            get
            {
                int count = 0;
                foreach (Segment segment in _segments)
                    count += segment.StepsCount;
                return count;
            }
        }

        /// <summary>
        /// gets the nnumber of steps that have been completed in all the segments of this speedrun.
        /// </summary>
        public int StepsCompletedCount
        {
            get
            {
                int count = 0;
                foreach (Segment segment in _segments)
                    count += segment.StepsCompletedCount;
                return count;
            }
        }

        /// <summary>
        /// Gets the completion ratio, where 0 means no step of any segment has been completed, and 1 means all of them have been completed.
        /// </summary>
        public float CompletionRatio
        {
            get
            {
                float stepsCount = StepsCount;
                float stepsCompletedCount = StepsCompletedCount;
                return stepsCount > 0 ? Mathf.Clamp01(stepsCompletedCount / StepsCount) : 0;
            }
        }

        /// <inheritdoc cref="_settings"/>
        public SpeedrunSettings Settings => _settings;

        /// <inheritdoc cref="_segments"/>
        public Segment[] Segments => _segments;

        /// <summary>
        /// Starts this run.
        /// </summary>
        /// <returns>Returns true if this run has been started successfully.</returns>
        public bool Start()
        {
            if (_startedAt == null)
            {
                _startedAt = DateTime.Now;
                try
                {
                    OnStart?.Invoke(this);
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }

                return true;
            }
            else
            {
                if (IsEnded)
                {
                    Debug.LogWarning($"Failed to start a {nameof(Run)} instance: This run has already been ended.");
                    return false;
                }

                Debug.LogWarning($"Failed to start a {nameof(Run)} instance: This run has already been started.");
                return false;
            }
        }

        /// <summary>
        /// Pauses the timer of this run.
        /// </summary>
        /// <returns>Returns true if this run has been paused successfully.</returns>
        public bool Pause()
        {
            if (!IsStarted)
            {
                Debug.LogWarning($"Failed to pause a {nameof(Run)} instance: This run has never been started.");
                return false;
            }
            else if (IsEnded)
            {
                Debug.LogWarning($"Failed to pause a {nameof(Run)} instance: This run has already been ended.");
                return false;
            }
            else if (_pausedAt != null)
            {
                return true;
            }

            _pausedAt = DateTime.Now;
            _timeMilliseconds += _resumedAt != null
                ? ((DateTime)_pausedAt).GetMillisecondsToDate() - ((DateTime)_resumedAt).GetMillisecondsToDate()
                : ((DateTime)_pausedAt).GetMillisecondsToDate() - ((DateTime)_startedAt).GetMillisecondsToDate();

            try
            {
                OnPauseStateChange?.Invoke(this, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }

        /// <summary>
        /// Resumes the timer of this run.
        /// </summary>
        /// <returns>Returns true if this run has been resumed sucessfully.</returns>
        public bool Resume()
        {
            if (!IsStarted)
            {
                Debug.LogWarning($"Failed to resume a {nameof(Run)} instance: This run has never been started.");
                return false;
            }
            else if (IsEnded)
            {
                Debug.LogWarning($"Failed to resume a {nameof(Run)} instance: This run has already been ended.");
                return false;
            }
            else if (!IsPaused)
            {
                return true;
            }

            _pausedAt = null;
            _resumedAt = DateTime.Now;

            try
            {
                OnPauseStateChange?.Invoke(this, false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return true;
        }

        /// <summary>
        /// Cancels this run.
        /// </summary>
        /// <returns>Returns true if this run has been canceled successfully.</returns>
        public bool Cancel()
        {
            if (!IsStarted)
            {
                Debug.LogWarning($"Failed to cancel a {nameof(Run)} instance: This run has never been started.");
                return false;
            }
            else if (IsEnded)
            {
                Debug.LogWarning($"Failed to cancel a {nameof(Run)} instance: This run has already been canceled or finished.");
                return false;
            }

            _timeMilliseconds = TimeMilliseconds;
            _canceledAt = TimeMilliseconds.ToDateTime();
            try
            {
                OnCancel?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return true;
        }

        /// <summary>
        /// Finds a <see cref="Segment"/> instance from this speedrun that has been created from a given asset.
        /// </summary>
        /// <param name="segmentAsset">The asset from which the <see cref="Segment"/> instance to get has been created.</param>
        /// <returns>Returns the found <see cref="Segment"/> instance created from the given asset.</returns>
        public Segment FindSegment(SegmentAsset segmentAsset)
        {
            foreach (Segment segment in _segments)
            {
                if (segment.SegmentAsset == segmentAsset)
                    return segment;
            }

            return null;
        }

        /// <inheritdoc cref="FindSegment(SegmentAsset)"/>
        /// <param name="segment">Outputs the found <see cref="Segment"/> instance created from the given asset.</param>
        /// <returns>Returns true if a <see cref="Segment"/> instance has been found successfully.</returns>
        public bool FindSegment(SegmentAsset segmentAsset, out Segment segment)
        {
            segment = FindSegment(segmentAsset);
            return segment != null;
        }

        /// <summary>
        /// Finds a <see cref="Step"/> instance from this speedrun that has been created from a given asset.
        /// </summary>
        /// <param name="stepAsset">The asset from which the <see cref="Step"/> instance to get has been created.</param>
        /// <returns>Returns the found <see cref="Step"/> instance created from the given asset.</returns>
        public Step FindStep(StepAsset stepAsset)
        {
            // Cancel if the segment that contains the given step asset doesn't exist
            if (!FindSegment(stepAsset.SegmentAsset, out Segment segment))
                return null;

            foreach (Step step in segment.Steps)
            {
                if (step.StepAsset == stepAsset)
                    return step;
            }

            return null;
        }

        /// <inheritdoc cref="FindStep(StepAsset)"/>
        /// <param name="step">Outputs the found <see cref="Step"/> instance created from the given asset.</param>
        /// <returns>Returns true if a <see cref="Step"/> instance has been found successfully.</returns>
        public bool FindStep(StepAsset stepAsset, out Step step)
        {
            step = FindStep(stepAsset);
            return step != null;
        }

        #endregion


        #region Private API

        /// <inheritdoc cref="SegmentChangeDelegate"/>
        private void HandleSegmentChange(Segment segment)
        {
            // Cancel is this run has already been ended
            if (IsEnded)
                return;

            // If the run has not been finished yet
            if (!IsFinished)
            {
                // Check if all Segments have been finished
                foreach (Segment s in _segments)
                {
                    // Cancel if one of a Segment is not finished
                    if (!s.IsFinished)
                        return;
                }

                // Save time if the run is meant to end as soon as it's finished
                if (_settings.EndSpeedrunOnFinish)
                    _timeMilliseconds = TimeMilliseconds;

                // Mark this run as finished
                _finishedAt = TimeMilliseconds.ToDateTime();

                try
                {
                    OnFinish?.Invoke(this);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // If the run has not been completed yet
            if (!IsCompleted)
            {
                // Check if all Segments have been completed
                foreach (Segment s in _segments)
                {
                    // Cancel if one of a Segment is not completed
                    if (!s.IsCompleted)
                        return;
                }

                // Save time, as the run is considered ended when completed
                _timeMilliseconds = TimeMilliseconds;

                _completedAt = TimeMilliseconds.ToDateTime();
                try
                {
                    OnComplete?.Invoke(this);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // If the segment change caused the run to end, emit the related event
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
        }

        #endregion

    }

}