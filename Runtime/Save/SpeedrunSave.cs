/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Utilitty functions to save and load the entries of speedruns.
    /// </summary>
    public static class SpeedrunSave
    {

        /// <summary>
        /// Saves the state of the <see cref="Run"/> instance started from a given asset.
        /// </summary>
        /// <param name="speedrunAsset">The asset from which the <see cref="Run"/> instance to save has been started.</param>
        /// <param name="username">The name of the user that played the run.</param>
        /// <param name="replaceRule"><inheritdoc cref="ESaveReplacementRule" path="/summary"/></param>
        /// <returns>Returns true if the entry has been saved successfully.</returns>
        public static bool Save(this SpeedrunAsset speedrunAsset, string username, ESaveReplacementRule replaceRule)
        {
            Debug.LogWarning("@todo");
            return false;
        }

    }

}