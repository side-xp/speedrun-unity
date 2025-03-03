/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

namespace SideXP.Speedrun
{

    /// <summary>
    /// Miscellaneous constant values used in this package.
    /// </summary>
    public static class Constants
    {

        /// <summary>
        /// Name of the company.
        /// </summary>
        public const string CompanyName = "Sideways Experiments";

        /// <summary>
        /// Name of this package.
        /// </summary>
        public const string PackageName = "Speedrun";

        /// <summary>
        /// Base URL for online documentation of the framework. You must concatenate your own path starting with "/".
        /// </summary>
        public const string BaseHelpUrl = "https://github.com/side-xp/speedrun-unity";

        /// <summary>
        /// Base path used for <see cref="UnityEngine.AddComponentMenu"/>. You must concatenate your own path starting with "/".
        /// </summary>
        public const string AddComponentMenu = CompanyName + "/" + PackageName;

        /// <summary>
        /// Base path used for <see cref="UnityEngine.CreateAssetMenuAttribute"/>. You must concatenate your own path starting with "/".
        /// </summary>
        public const string CreateAssetMenu = CompanyName + "/" + PackageName;

    }

}