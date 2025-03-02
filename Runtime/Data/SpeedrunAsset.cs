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

        [Header("Run Settings")]

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
        private Speedrun _speedrunInstance = null;

        #endregion


        #region Public API

        /// <inheritdoc cref="_speedrunInstance"/>
        public Speedrun SpeedrunInstance => _speedrunInstance;

        /// <inheritdoc cref="_segments"/>
        public SegmentAsset[] Segments => _segments;

        /// <summary>
        /// 
        /// </summary>
        public bool HasActiveSpeedrunInstance => _speedrunInstance != null && !_speedrunInstance.IsCanceled && !_speedrunInstance.IsFinished;

        /// <summary>
        /// Starts a new <see cref="Speedrun"/> instance.
        /// </summary>
        /// <remarks>
        /// This function overload is designed to be used from the inspector, especially from Unity Events. if you want to start a
        /// <see cref="Speedrun"/> instance through scripting, prefer usinn the <see cref="StartSpeedrun(out Speedrun)"/> overload, which
        /// outputs the started instance directly.<br/>
        /// In any case, you can get the started instance with <see cref="SpeedrunInstance"/>.
        /// </remarks>
        public void StartSpeedrun()
        {
            StartSpeedrun(out _);
        }

        /// <remarks></remarks>
        /// <inheritdoc cref="StartSpeedrun()"/>
        /// <param name="speedrun">Outputs the created <see cref="Speedrun"/> instance, or the existing one if it's not yet finished or
        /// canceled.</param>
        /// <returns>Returns true if a <see cref="Speedrun"/> instance has been started successfully.</returns>
        public bool StartSpeedrun(out Speedrun speedrun)
        {
            speedrun = _speedrunInstance;

            if (speedrun != null)
            {
                if (!speedrun.IsStarted)
                {
                    Debug.LogWarning($"A {nameof(Speedrun)} instance was already existing from this asset, but has never been started (which shouldn't happen). That instance will be started instead of creating a new one.", this);
                }
                if (speedrun.IsCanceled)
                {
                    Debug.Log($"A {nameof(Speedrun)} instance was already existing from this asset, but has been canceled. The new instance will replace it, and start.", this);
                }
                else if (speedrun.IsFinished)
                {
                    Debug.Log($"A {nameof(Speedrun)} instance was already existing from this asset, but has been finished. The new instance will replace it, and start.", this);
                }
                else
                {
                    Debug.LogWarning($"Failed to start a {nameof(Speedrun)} instance from this asset: Another instance is currently active. You must wait for it to finish or call {nameof(CancelSpeedrun)}() to cancel before starting a new one.", this);
                    return false;
                }
            }
            else
            {
                speedrun = new Speedrun(this, _settings);
                SpeedrunComponent.Register(speedrun);
                _speedrunInstance = speedrun;
            }

            speedrun.Start();
            return true;
        }

        /// <summary>
        /// Pauses the <see cref="Speedrun"/> instance started from this asset.
        /// </summary>
        public void PauseSpeedrun()
        {
            if (_speedrunInstance == null)
                Debug.LogWarning($"Failed to pause the {nameof(Speedrun)} instance from this asset: No instance has been started yet.", this);
            else if (_speedrunInstance.IsCanceled)
                Debug.LogWarning($"Failed to pause the {nameof(Speedrun)} instance from this asset: The existing instance has already been canceled.", this);
            else if (_speedrunInstance.IsFinished)
                Debug.LogWarning($"Failed to pause the {nameof(Speedrun)} instance from this asset: The existing instance has already been finished.", this);
            else
                _speedrunInstance.Pause();
        }

        /// <summary>
        /// Resumes the <see cref="Speedrun"/> instance started from this asset.
        /// </summary>
        public void ResumeSpeedrun()
        {
            if (_speedrunInstance == null)
                Debug.LogWarning($"Failed to resume the {nameof(Speedrun)} instance from this asset: No instance has been started yet.", this);
            else if (_speedrunInstance.IsCanceled)
                Debug.LogWarning($"Failed to resume the {nameof(Speedrun)} instance from this asset: The existing instance has already been canceled.", this);
            else if (_speedrunInstance.IsFinished)
                Debug.LogWarning($"Failed to resume the {nameof(Speedrun)} instance from this asset: The existing instance has already been finished.", this);
            else
                _speedrunInstance.Resume();
        }

        /// <summary>
        /// Cancels the <see cref="Speedrun"/> instance started from this asset.
        /// </summary>
        public void CancelSpeedrun()
        {
            if (_speedrunInstance == null)
                Debug.LogWarning($"Failed to cancel the {nameof(Speedrun)} instance from this asset: No instance has been started yet.", this);
            else if (_speedrunInstance.IsCanceled)
                Debug.LogWarning($"Failed to cancel the {nameof(Speedrun)} instance from this asset: The existing instance has already been canceled.", this);
            else if (_speedrunInstance.IsFinished)
                Debug.LogWarning($"Failed to cancel the {nameof(Speedrun)} instance from this asset: The existing instance has already been finished.", this);
            else
                _speedrunInstance.Cancel();
        }

        #endregion

    }

}