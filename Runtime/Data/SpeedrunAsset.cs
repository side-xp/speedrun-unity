/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Defines the structure of the game and the informations that can be used to track and save the progression of the player.
    /// </summary>
    [HelpURL(Constants.BaseHelpUrl)]
    [CreateAssetMenu(fileName = "NewSpeedrunAsset", menuName = Constants.CreateAssetMenu + "/Speedrun Asset")]
    public class SpeedrunAsset : ScriptableObject
    {

        #region Fields

        /// <summary>
        /// Called when a run is started from this asset.
        /// </summary>
        public event Run.StartDelegate OnSpeedrunStart;

        /// <summary>
        /// Called when the run started from this asset is paused or resumed.
        /// </summary>
        public event Run.PauseStateChangeDelegate OnSpeedrunPauseStateChange;

        /// <summary>
        /// Called when the run started from this asset is finished.
        /// </summary>
        /// <inheritdoc cref="Run.FinishDelegate" path="/remarks"/>
        public event Run.FinishDelegate OnSpeedrunFinish;

        /// <summary>
        /// Called when the run started from this asset is completed.
        /// </summary>
        /// <inheritdoc cref="Run.CompleteDelegate" path="/remarks"/>
        public event Run.CompleteDelegate OnSpeedrunComplete;

        /// <summary>
        /// Called when the run started from this asset is canceled.
        /// </summary>
        /// <inheritdoc cref="Run.CancelDelegate" path="/remarks"/>
        public event Run.CancelDelegate OnSpeedrunCancel;

        /// <summary>
        /// Called when the run started from this asset is ended.
        /// </summary>
        /// <remarks>This is called just after <see cref="OnSpeedrunComplete"/> or <see cref="OnSpeedrunCancel"/>.</remarks>
        public event Run.EndDelegate OnSpeedrunEnd;

        [SerializeField]
        [Tooltip("Defines the settings to use for a run.")]
        private SpeedrunSettings _settings = SpeedrunSettings.Default;

        [SerializeField]
        [Tooltip("The segments that can be played during a run." +
            "\nA Segment is generally a single level or a single region of the game, with its own checkpoints and elements to unlock or discover." +
            "\nIf your game is built as a single big level, you could have only one Segment defined, and use only checkpoints to track the player's progression." +
            "\nIf your game is more organic, it's also fine to declare only one Segment with only unlockable steps and no checkpoint.")]
        private SegmentAsset[] _segments = null;

        /// <summary>
        /// The state of a speedrun started from this asset.
        /// </summary>
        /// <remarks>This value is only available after calling <see cref="StartSpeedrun()"/> once.</remarks>
        private Run _speedrunInstance = null;

        #endregion


        #region Lifecycle

        private void OnDisable()
        {
            _speedrunInstance = null;
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_speedrunInstance"/>
        public Run Speedrun => _speedrunInstance;

        /// <inheritdoc cref="_segments"/>
        public SegmentAsset[] Segments => _segments;

        /// <summary>
        /// Checks if a <see cref="Run"/> instance has been started from this asset, and is still being played (neither canceled or
        /// finished).
        /// </summary>
        public bool HasActiveSpeedrunInstance => _speedrunInstance != null && !_speedrunInstance.IsCanceled && !_speedrunInstance.IsFinished;

        /// <summary>
        /// Creates a new <see cref="Run"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You may want to call <see cref="StartSpeedrun()"/> soon after to actually start the run.
        /// </para>
        /// <para>
        /// This function overload is designed to be used from the inspector, especially from Unity Events. if you want to start a
        /// <see cref="Run"/> instance through scripting, prefer using the <see cref="CreateSpeedrun(out Run)"/> overload, which
        /// outputs the started instance directly.
        /// </para>
        /// <para>
        /// In any case, you can get the started instance with <see cref="Speedrun"/>.
        /// </para>
        /// </remarks>
        public void CreateSpeedrun()
        {
            CreateSpeedrun(out _);
        }

        /// <remarks></remarks>
        /// <inheritdoc cref="CreateSpeedrun()"/>
        /// <param name="speedrun">Outputs the created <see cref="Run"/> instance, or the existing one if it's not yet finished or
        /// canceled.</param>
        /// <returns>Returns true if a <see cref="Run"/> instance has been created successfully.</returns>
        public bool CreateSpeedrun(out Run speedrun)
        {
            speedrun = _speedrunInstance;

            if (speedrun != null)
            {
                // Stop if the existing instance has not even been started yet (so it will be reused)
                if (!speedrun.IsStarted && !speedrun.IsEnded)
                    return true;

                if (speedrun.IsEnded)
                {
                    Debug.Log($"A {nameof(Run)} instance was already existing from this asset, but has been ended. The new instance will replace it, and start.", this);
                }
                else
                {
                    Debug.LogWarning($"Failed to start a {nameof(Run)} instance from this asset: Another instance is currently active. You must wait for it to finish or call {nameof(CancelSpeedrun)}() to cancel it before starting a new one.", this);
                    return false;
                }
            }

            speedrun = new Run(this, _settings);
            speedrun.OnStart += run => OnSpeedrunStart?.Invoke(run);
            speedrun.OnPauseStateChange += (run, isPaused) => OnSpeedrunPauseStateChange?.Invoke(run, isPaused);
            speedrun.OnFinish += run => OnSpeedrunFinish?.Invoke(run);
            speedrun.OnComplete += run => OnSpeedrunComplete?.Invoke(run);
            speedrun.OnCancel += run => OnSpeedrunCancel?.Invoke(run);
            speedrun.OnEnd += run => OnSpeedrunEnd?.Invoke(run);

            _speedrunInstance = speedrun;

            return speedrun != null;
        }

        /// <summary>
        /// Starts a new <see cref="Run"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function overload is designed to be used from the inspector, especially from Unity Events. if you want to start a
        /// <see cref="Run"/> instance through scripting, prefer using the <see cref="StartSpeedrun(out Run)"/> overload, which
        /// outputs the started instance directly.
        /// </para>
        /// <para>
        /// In any case, you can get the started instance with <see cref="Speedrun"/>.
        /// </para>
        /// </remarks>
        public void StartSpeedrun()
        {
            StartSpeedrun(out _);
        }

        /// <remarks></remarks>
        /// <inheritdoc cref="StartSpeedrun()"/>
        /// <param name="speedrun">Outputs the created <see cref="Run"/> instance, or the existing one if it's not yet finished or
        /// canceled.</param>
        /// <returns>Returns true if a <see cref="Run"/> instance has been started successfully.</returns>
        public bool StartSpeedrun(out Run speedrun)
        {
            if (!CreateSpeedrun(out speedrun))
                return false;

            if (!speedrun.IsStarted)
                speedrun.Start();
            
            return true;
        }

        /// <summary>
        /// Pauses the <see cref="Run"/> instance started from this asset.
        /// </summary>
        public void PauseSpeedrun()
        {
            if (_speedrunInstance == null)
                Debug.LogWarning($"Failed to pause the {nameof(Run)} instance from this asset: No instance has been started yet.", this);
            else if (_speedrunInstance.IsCanceled)
                Debug.LogWarning($"Failed to pause the {nameof(Run)} instance from this asset: The existing instance has already been canceled.", this);
            else if (_speedrunInstance.IsFinished)
                Debug.LogWarning($"Failed to pause the {nameof(Run)} instance from this asset: The existing instance has already been finished.", this);
            else
                _speedrunInstance.Pause();
        }

        /// <summary>
        /// Resumes the <see cref="Run"/> instance started from this asset.
        /// </summary>
        public void ResumeSpeedrun()
        {
            if (_speedrunInstance == null)
                Debug.LogWarning($"Failed to resume the {nameof(Run)} instance from this asset: No instance has been started yet.", this);
            else if (_speedrunInstance.IsCanceled)
                Debug.LogWarning($"Failed to resume the {nameof(Run)} instance from this asset: The existing instance has already been canceled.", this);
            else if (_speedrunInstance.IsFinished)
                Debug.LogWarning($"Failed to resume the {nameof(Run)} instance from this asset: The existing instance has already been finished.", this);
            else
                _speedrunInstance.Resume();
        }

        /// <summary>
        /// Cancels the <see cref="Run"/> instance started from this asset.
        /// </summary>
        public void CancelSpeedrun()
        {
            if (_speedrunInstance == null)
                Debug.LogWarning($"Failed to cancel the {nameof(Run)} instance from this asset: No instance has been started yet.", this);
            else if (_speedrunInstance.IsCanceled)
                Debug.LogWarning($"Failed to cancel the {nameof(Run)} instance from this asset: The existing instance has already been canceled.", this);
            else if (_speedrunInstance.IsFinished)
                Debug.LogWarning($"Failed to cancel the {nameof(Run)} instance from this asset: The existing instance has already been finished.", this);
            else
                _speedrunInstance.Cancel();
        }

        /// <inheritdoc cref="FindSegment(SegmentAsset, out Segment)"/>
        /// <inheritdoc cref="Run.FindSegment(SegmentAsset)"/>
        public Segment FindSegment(SegmentAsset segmentAsset)
        {
            return FindSegment(segmentAsset, out Segment segment) ? segment : null;
        }

        /// <summary>
        /// Gets a <see cref="Segment"/> instance from the <see cref="Run"/> instance started from this asset.
        /// </summary>
        /// <inheritdoc cref="Run.FindSegment(SegmentAsset, out Segment)"/>
        public bool FindSegment(SegmentAsset segmentAsset, out Segment segment)
        {
            if (_speedrunInstance == null)
            {
                Debug.LogWarning($"Failed to get a {nameof(Segment)} instance from this asset: No {nameof(Run)} instance has been started yet.", this);
                segment = null;
                return false;
            }

            return _speedrunInstance.FindSegment(segmentAsset, out segment);
        }

        /// <inheritdoc cref="FindStep(StepAsset, out Step)"/>
        /// <inheritdoc cref="Run.FindStep(StepAsset)"/>
        public Step FindStep(StepAsset stepAsset)
        {
            return FindStep(stepAsset, out Step step) ? step : null;
        }

        /// <summary>
        /// Gets a <see cref="Step"/> instance from the <see cref="Run"/> instance started from this asset.
        /// </summary>
        /// <inheritdoc cref="Run.FindStep(StepAsset, out Step)"/>
        public bool FindStep(StepAsset stepAsset, out Step step)
        {
            if (_speedrunInstance == null)
            {
                Debug.LogWarning($"Failed to get a {nameof(Step)} instance from this asset: No {nameof(Run)} instance has been started yet.", this);
                step = null;
                return false;
            }

            return _speedrunInstance.FindStep(stepAsset, out step);
        }

        #endregion

    }

}