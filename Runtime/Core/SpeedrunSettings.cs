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
            EndSegmentOnFinish = false,
            EndSpeedrunOnFinish = false,
        };

        [Tooltip("By default, a " + nameof(Segment) + " is considered ended only when it's completed." +
            "\nif enabled, a " + nameof(Segment) + " is ended as soon as it's marked finished, whether it has been completed or not." +
            "\nTip: you should enable this option only if you don't allow the player to return to a level once it has been finished.")]
        public bool EndSegmentOnFinish;

        [Tooltip("By default, a " + nameof(Run) + " is considered ended only when all its " + nameof(Segment) + "s are also ended." +
            "\nIf enabled, a " + nameof(Run) + " is ended as soon as all its " + nameof(Segment) + "s are at least marked finished, whether they have bene completed or not." +
            "\nTip: you should enable this option only if the player is allowed to navigate back to levels after they have been finished to complete them, but you want the game to end as soon as all the levels have been at least finished, whether they have been completed or not.")]
        public bool EndSpeedrunOnFinish;

    }

}