/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <inheritdoc cref="Step"/>
    [HelpURL(Constants.BaseHelpUrl)]
    [CreateAssetMenu(fileName = "NewStepAsset", menuName = Constants.CreateAssetMenu + "/StepAsset")]
    public class StepAsset : ScriptableObject
    {

        #region Fields

        [Header("Label")]

        [SerializeField]
        [Tooltip("The name of this segment, as displayed on UI." +
            "\nIf not defined, uses the asset name instead.")]
        private string _displayName = null;

        [SerializeField]
        [Tooltip("The description of this segment, as displayed on UI.")]
        private string _description = null;

        #endregion


        #region Public API

        /// <inheritdoc cref="_displayName"/>
        public string DisplayName => !string.IsNullOrWhiteSpace(_displayName) ? _displayName : name;

        /// <inheritdoc cref="_description"/>
        public string Description => _description;

        #endregion

    }

}