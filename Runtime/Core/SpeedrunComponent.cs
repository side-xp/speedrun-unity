/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using System.Collections.Generic;

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Auto-updates <see cref="Speedrun"/> instances.
    /// </summary>
    /// <remarks>This component behaves as a singleton, and shouldn't be instanced/destroyed outside manually.</remarks>
    [HelpURL(Constants.BaseHelpUrl)]
    [DefaultExecutionOrder(1000)]
    internal class SpeedrunComponent : MonoBehaviour
    {

        #region Fields

        private const string InstanceGameObjectName = "Speedrun Updater";
        private static readonly HideFlags HideFlags = HideFlags.NotEditable | HideFlags.DontSave;

        /// <summary>
        /// The singleton instance of this component.
        /// </summary>
        private static SpeedrunComponent s_instance = null;

        /// <summary>
        /// The active speedrun instances.
        /// </summary>
        private List<Speedrun> _speedrunInstances = new List<Speedrun>();

        /// <summary>
        /// Stores the time at which this component has been updated the previous frame, using double for accuracy.
        /// </summary>
        private double _previousTimeSinceStartup = 0;

        #endregion


        #region Lifecycle

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Debug.LogError($"Failed to setup the singleton instance of {nameof(SpeedrunComponent)}: a singleton instance already exists, which is not allowed. This other instance will be destroyed.", this);
                Destroy(this);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(this);
            this.hideFlags = HideFlags;
        }

        private void OnValidate()
        {
            this.hideFlags = HideFlags;
        }

        private void OnDisable()
        {
            if (s_instance != this)
                return;

            Debug.LogError($"The singleton instance of {nameof(SpeedrunComponent)} has been disabled, so {nameof(Speedrun)} instances won't be updated anymore.", this);
        }

        private void OnDestroy()
        {
            if (s_instance == this)
                s_instance = null;
        }

        private void LateUpdate()
        {
            // Remove ended runs
            _speedrunInstances.RemoveAll(speedrun => speedrun.IsFinished || speedrun.IsCanceled);

            // Calculate delta, as double for accuracy
            double delta = Time.realtimeSinceStartupAsDouble - _previousTimeSinceStartup;
            _previousTimeSinceStartup = Time.realtimeSinceStartupAsDouble;

            // For each active speedrun instance
            foreach (Speedrun speedrun in _speedrunInstances.ToArray())
            {
                // Update the speedrun instance if applicable
                if (speedrun.IsStarted && !speedrun.IsPaused)
                    speedrun.Update(delta);
            }
        }

        #endregion


        #region Internal API

        /// <summary>
        /// Gets or creates the singleton instance of <see cref="SpeedrunComponent"/>.
        /// </summary>
        internal static SpeedrunComponent Instance
        {
            get
            {
                if (s_instance == null)
                {
                    GameObject obj = new GameObject(InstanceGameObjectName);
                    s_instance = obj.AddComponent<SpeedrunComponent>();
                }
                return s_instance;
            }
        }

        /// <summary>
        /// Registers a <see cref="Speedrun"/> instance to this component.
        /// </summary>
        /// <param name="speedrun">The instance to register.</param>
        /// <returns>Returns true if the instance has been registered successfully.</returns>
        internal static bool Register(Speedrun speedrun)
        {
            if (Instance._speedrunInstances.Contains(speedrun))
            {
                Debug.LogWarning($"Failed to register a {nameof(Speedrun)} instance to this {nameof(SpeedrunComponent)}: That instance is already registered.", Instance);
                return false;
            }

            Instance._speedrunInstances.Add(speedrun);
            return true;
        }

        #endregion

    }

}