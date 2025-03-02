/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

namespace SideXP.Speedrun
{

    /// <summary>
    /// Defines how a speedrun entry is saved if another one already exists for the same username.
    /// </summary>
    public enum ESaveReplacementRule
    {

        /// <summary>
        /// Entries are just added to the save file. Used by default.
        /// </summary>
        Incremental,

        /// <summary>
        /// Only the entry with the best time is saved in the file.
        /// </summary>
        SaveBestTime,

        /// <summary>
        /// Only the entry with the best completion ratio is saved in the file.
        /// </summary>
        SaveBestCompletion,

        /// <summary>
        /// Only the latest entry is saved in the file. Note that this mode will replace an existing entry even if the previous one has a
        /// better score.
        /// </summary>
        SaveLatest,

    }

}