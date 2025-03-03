/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Stores the settings used for a speedrun instance.
    /// </summary>
    [System.Serializable]
    public struct SpeedrunSettings
    {

        /// <summary>
        /// Default settings.
        /// </summary>
        public static readonly SpeedrunSettings Default = new SpeedrunSettings
        {
            FinishOnCompleteLastCheckpoint = true
        };

        [Tooltip("By default, a " + nameof(Segment) + " is considered finished when all its checkpoints are completed, or by calling its " + nameof(Segment.Finish) + "() function manually." +
            "\nIf enabled, a " + nameof(Segment) + " will be considered finished as soon as its last checkpoint is completed, no matter the other ones.")]
        public bool FinishOnCompleteLastCheckpoint;

    }

}