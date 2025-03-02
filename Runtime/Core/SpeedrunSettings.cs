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
            AutoFinish = true
        };

        [Tooltip("By default, a segment is marked as finished when all its steps are completed or by calling its Finish() function manually." +
            "\nIf enabled, a Segment will be automatically marked as finished if its last Checkpoint step is completed, without taking account of other steps.")]
        public bool AutoFinish;

    }

}