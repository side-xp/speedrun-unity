/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;
using UnityEditor;

namespace SideXP.Speedrun.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions and extensions from our core library used in this package.
    /// </summary>
    public static class Standalone
    {

        #region GUI

        /// <summary>
        /// Size of an horizontal margin.
        /// </summary>
        public const float HMargin = 2f;

        /// <summary>
        /// Size of a vertical margin.
        /// </summary>
        public const float VMargin = 2f;

        //public const float HeightXL = 48f;
        //public const float HeightL = 36f;
        //public const float HeightM = 28f;
        //public const float HeightS = 20f;
        //public const float HeightXS = 16f;

        //public const float WidthXL = 200f;
        //public const float WidthL = 148f;
        //public const float WidthM = 112f;
        public const float WidthS = 80f;
        //public const float WidthXS = 40f;

        #endregion


        #region Extensions

        /// <summary>
        /// Gets the field label of a serialized property.
        /// </summary>
        /// <param name="property">The property you want to get the label.</param>
        /// <returns>Returns the label of the given property.</returns>
        public static GUIContent GetLabel(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        /// <summary>
        /// Reimports the given asset. You should call this function after adding, updating or removing a subasset.
        /// </summary>
        /// <param name="asset">The asset you want to save and reimport.</param>
        /// <returns>Returns true if the given object is truly an asset in the project.</returns>
        public static bool SaveAndReimport(this Object asset)
        {
            AssetDatabase.SaveAssets();
            string path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.ImportAsset(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the input style, and sets the font style to <see cref="FontStyle.Italic"/>.
        /// </summary>
        /// <inheritdoc cref="BoldItalic(GUIStyle)"/>
        public static GUIStyle Italic(this GUIStyle style)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.fontStyle = FontStyle.Italic;
            return newStyle;
        }

        /// <summary>
        /// Copies the input style, and sets the text alignment.
        /// </summary>
        /// <param name="alignment">The text alignment to set.</param>
        /// <inheritdoc cref="Margin(GUIStyle, int, int, int, int)"/>
        public static GUIStyle TextAlignment(this GUIStyle style, TextAnchor alignment)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.alignment = alignment;
            return newStyle;
        }

        ///// <summary>
        ///// Copies the input style, and sets the text alignment to <see cref="TextAnchor.MiddleLeft"/>.
        ///// </summary>
        ///// <inheritdoc cref="TextAlignment(GUIStyle, TextAnchor)"/>
        //public static GUIStyle AlignLeft(this GUIStyle style)
        //{
        //    return TextAlignment(style, TextAnchor.MiddleLeft);
        //}

        ///// <summary>
        ///// Copies the input style, and sets the text alignment to <see cref="TextAnchor.MiddleCenter"/>.
        ///// </summary>
        ///// <inheritdoc cref="TextAlignment(GUIStyle, TextAnchor)"/>
        //public static GUIStyle AlignCenter(this GUIStyle style)
        //{
        //    return TextAlignment(style, TextAnchor.MiddleCenter);
        //}

        /// <summary>
        /// Copies the input style, and sets the text alignment to <see cref="TextAnchor.MiddleRight"/>.
        /// </summary>
        /// <inheritdoc cref="TextAlignment(GUIStyle, TextAnchor)"/>
        public static GUIStyle AlignRight(this GUIStyle style)
        {
            return TextAlignment(style, TextAnchor.MiddleRight);
        }

        #endregion

    }

}