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
    /// Represents a speedrun, with its own timer and progression.
    /// </summary>
    public class Speedrun
    {

        #region Delegates

        /// <summary>
        /// Called when a run is paused or resumed.
        /// </summary>
        /// <param name="run">The <see cref="Speedrun"/> instance of which the state changed.</param>
        /// <param name="isPaused">Is the run now paused?</param>
        public delegate void PauseStateChangeDelegate(Speedrun run, bool isPaused);

        /// <summary>
        /// Called when a run is finished.
        /// </summary>
        /// <remarks>A run is considered finished if all its segments are finished.</remarks>
        /// <param name="run">The run that has been finished.</param>
        public delegate void FinishDelegate(Speedrun run);

        /// <summary>
        /// Called when a run is canceled.
        /// </summary>
        /// <param name="run">The run that has been canceled.</param>
        public delegate void CancelDelegate(Speedrun run);

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

        /// <inheritdoc cref="PauseStateChangeDelegate"/>
        public event PauseStateChangeDelegate OnPauseStateChange;

        /// <inheritdoc cref="FinishDelegate"/>
        public event FinishDelegate OnFinish;

        /// <inheritdoc cref="CancelDelegate"/>
        public event CancelDelegate OnCancel;

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
        /// The time (in seconds) elapsed since this run has been started.
        /// </summary>
        /// <remarks>The timer is updated without taking account of Unity's time scale, and use double for accuracy.</remarks>
        private double _timer = 0;

        /// <summary>
        /// Was this run started once?
        /// </summary>
        /// <remarks>This flag is enabled after calling <see cref="Start()"/> once.</remarks>
        private bool _isStarted = false;

        /// <summary>
        /// Is this run paused?
        /// </summary>
        private bool _isPaused = false;

        /// <summary>
        /// Is this run finished?
        /// </summary>
        /// <remarks>A run is considered finished if all of its segments are finished.</remarks>
        private bool _isFinished = false;

        /// <summary>
        /// Is this run canceled?
        /// </summary>
        /// <remarks>This flag is enabled after calling <see cref="Cancel()"/> once.</remarks>
        private bool _isCanceled = false;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="Speedrun"/>
        /// <param name="speedrunAsset">The asset from which this instance is created.</param>
        /// <param name="settings"><inheritdoc cref="_settings" path="/summary"/></param>
        internal Speedrun(SpeedrunAsset speedrunAsset, SpeedrunSettings settings)
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
                Debug.LogError($"Invalid {nameof(Speedrun)} instance: No valid {nameof(SegmentAsset)} found on the original {nameof(SpeedrunAsset)}. The run will be considered as completed as soon as it's started.", speedrunAsset);
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_isStarted"/>
        public bool IsStarted => _isStarted;

        /// <inheritdoc cref="_isCanceled"/>
        public bool IsCanceled => _isCanceled;

        /// <summary>
        /// Is this run finished?
        /// </summary>
        /// <remarks>A run is considered finished if all of its segments are finished.</remarks>
        public bool IsFinished
        {
            get
            {
                foreach (Segment segment in _segments)
                {
                    if (!segment.IsFinished)
                        return false;
                }
                return true;
            }
        }

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

        /// <summary>
        /// Are all the steps from all the segments of this speedrun completed?
        /// </summary>
        public bool IsCompleted => StepsCompletedCount == StepsCount;

        /// <inheritdoc cref="_isPaused"/>
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (value)
                    Pause();
                else
                    Resume();
            }
        }

        /// <inheritdoc cref="_timer"/>
        public double Timer => _timer;

        /// <inheritdoc cref="_settings"/>
        public SpeedrunSettings Settings => _settings;

        /// <summary>
        /// Starts this run.
        /// </summary>
        /// <remarks>If this run has already been started, you can use this function as an alias of <see cref="Resume()"/> to unpause
        /// it.</remarks>
        /// <returns>Returns true if this run has been started successfully.</returns>
        public bool Start()
        {
            if (!_isStarted)
            {
                _isStarted = true;
                return true;
            }
            else
            {
                if (IsCanceled || IsFinished)
                {
                    Debug.LogWarning($"Failed to start a {nameof(Speedrun)} instance: This run has already been canceled or finished.");
                    return false;
                }
                else if (_isPaused)
                {
                    return Resume();
                }

                Debug.LogWarning($"Failed to start a {nameof(Speedrun)} instance: This run has already been started.");
                return false;
            }
        }

        /// <summary>
        /// Pauses the timer of this run.
        /// </summary>
        /// <returns>Returns true if this run has been paused successfully.</returns>
        public bool Pause()
        {
            if (!_isStarted)
            {
                Debug.LogWarning($"Failed to pause a {nameof(Speedrun)} instance: This run has never been started.");
                return false;
            }
            else if (IsCanceled || IsFinished)
            {
                Debug.LogWarning($"Failed to pause a {nameof(Speedrun)} instance: This run has already been canceled or finished.");
                return false;
            }
            else if (_isPaused)
            {
                return true;
            }

            _isPaused = true;
            OnPauseStateChange?.Invoke(this, _isPaused);
            return true;
        }

        /// <summary>
        /// Resumes the timer of this run.
        /// </summary>
        /// <returns>Returns true if this run has been resumed sucessfully.</returns>
        public bool Resume()
        {
            if (!_isStarted)
            {
                Debug.LogWarning($"Failed to resume a {nameof(Speedrun)} instance: This run has never been started.");
                return false;
            }
            else if (IsCanceled || IsFinished)
            {
                Debug.LogWarning($"Failed to resume a {nameof(Speedrun)} instance: This run has already been canceled or finished.");
                return false;
            }
            else if (!_isPaused)
            {
                return true;
            }

            _isPaused = false;
            OnPauseStateChange?.Invoke(this, _isPaused);
            return true;
        }

        /// <summary>
        /// Stops the timer of this run and cancel it.
        /// </summary>
        /// <returns>Returns true if this run has bene canceled successfully.</returns>
        public bool Cancel()
        {
            if (!_isStarted)
            {
                Debug.LogWarning($"Failed to cancel a {nameof(Speedrun)} instance: This run has never been started.");
                return false;
            }
            else if (IsCanceled || IsFinished)
            {
                Debug.LogWarning($"Failed to cancel a {nameof(Speedrun)} instance: This run has already been canceled or finished.");
                return false;
            }

            _isCanceled = true;
            OnCancel?.Invoke(this);
            return true;
        }

        #endregion


        #region Internal API

        /// <summary>
        /// Updates the timer of this run, if applicable.
        /// </summary>
        /// <param name="delta">The time (in seconds) elapsed since the previous update.</param>
        /// <returns>Returns true if this instance has been updated successfully.</returns>
        internal bool Update(double delta)
        {
            if (!_isStarted)
            {
                Debug.LogWarning($"Failed to update a {nameof(Speedrun)} instance: This run has never been started.");
                return false;
            }
            else if (IsCanceled || IsFinished)
            {
                Debug.LogWarning($"Failed to update a {nameof(Speedrun)} instance: This run has already been canceled or finished.");
                return false;
            }
            else if (_isPaused)
            {
                Debug.LogWarning($"Failed to update a {nameof(Speedrun)} instance: This run is paused.");
                return false;
            }

            _timer += delta;
            return true;
        }

        #endregion


        #region Private API

        /// <inheritdoc cref="SegmentChangeDelegate"/>
        private void HandleSegmentChange(Segment segment)
        {
            // Cancel is this run has already been finished or canceled
            if (_isFinished || _isCanceled)
                return;

            // Cancel if a segment is not yet finished
            foreach (Segment s in _segments)
            {
                if (!s.IsFinished)
                    return;
            }

            // Mark this run as finished if all the segments have been finished
            _isFinished = true;
            OnFinish?.Invoke(this);
        }

        #endregion

    }

}