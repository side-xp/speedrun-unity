/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using System;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Speedrun.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions and extensions from our core library used in this package.
    /// </summary>
    public static class Standalone
    {

        #region GUI

        // Separators
        public static readonly Color SeparatorColor = new Color(1, 1, 1, .4f);
        public static readonly Color DarkSeparatorColor = new Color(0, 0, 0, .53f);
        public const float DefaultSeparatorSize = 2f;

        // Fonts
        public const int MinFontSize = 1;
        public const int MaxFontSize = 255;

        // Margins
        public const float HMargin = 2f;
        public const float VMargin = 2f;

        // Sizing
        //public const float HeightXL = 48f;
        //public const float HeightL = 36f;
        //public const float HeightM = 28f;
        public const float HeightS = 20f;
        //public const float HeightXS = 16f;

        //public const float WidthXL = 200f;
        //public const float WidthL = 148f;
        public const float WidthM = 112f;
        public const float WidthS = 80f;
        public const float WidthXS = 40f;

        /// <inheritdoc cref="HorizontalSeparator(float, bool, bool)"/>
        public static void HorizontalSeparator(bool wide = false, bool dark = false)
        {
            HorizontalSeparator(DefaultSeparatorSize, wide, dark);
        }

        /// <param name="dark">If enabled, uses the dark separator color.</param>
        /// <inheritdoc cref="HorizontalSeparator(float, Color, bool)"/>
        public static void HorizontalSeparator(float size, bool wide = false, bool dark = false)
        {
            HorizontalSeparator(size, dark ? DarkSeparatorColor : SeparatorColor, wide);
        }

        /// <summary>
        /// Draws an horizontal line.
        /// </summary>
        /// <param name="size">The height of the separator.</param>
        /// <param name="color">The color of the separator.</param>
        /// <param name="wide">If enabled, the separator will use the full view width. This is designed to draw a separator that doesn't
        /// use the margins in the inspector window.</param>
        public static void HorizontalSeparator(float size, Color color, bool wide = false)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, size);

            if (wide && rect.width < EditorGUIUtility.currentViewWidth)
            {
                rect.x = 0;
                rect.width = EditorGUIUtility.currentViewWidth;
            }

            EditorGUI.DrawRect(rect, color);
        }

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

        /// <summary>
        /// Copies the input style, and sets the given font size.
        /// </summary>
        /// <param name="fontSize">The font size to set.</param>
        /// <inheritdoc cref="Margin(GUIStyle, int, int, int, int)"/>
        public static GUIStyle FontSize(this GUIStyle style, int fontSize)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.fontSize = Mathf.Clamp(fontSize, MinFontSize, MaxFontSize);
            return newStyle;
        }

        /// <summary>
        /// Copies the input style, and set the font size by adding the given difference to the original size.
        /// </summary>
        /// <param name="diff">The difference to add to the original font size.</param>
        /// <inheritdoc cref="FontSize(GUIStyle, int)"/>
        public static GUIStyle FontSizeDiff(this GUIStyle style, int diff)
        {
            return FontSize(style, style.fontSize + diff);
        }

        #endregion


        #region Objects & Assets

        /// <summary>
        /// Creates a new asset of a given type, add it to a given array property and add it in that array.
        /// </summary>
        /// <typeparam name="T">The type of the asset to create and add to the given array property.</typeparam>
        /// <param name="assetsArrayProp">The serialized representation of the array property to which the created asset will be added.
        /// Assumes that this property's type is an array of asset references with the given type.</param>
        /// <param name="onInit">The function to call after the asset has been created. The serialized representation of that created asset
        /// is passed as parameter.</param>
        /// <returns>Returns the created asset instance.</returns>
        public static T CreateAndAttachAsset<T>(SerializedProperty assetsArrayProp, Action<SerializedObject> onInit = null)
            where T : ScriptableObject
        {
            // Create the new Segment asset
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = $"New{typeof(T).Name}";

            // Create the serialized representation of the created asset and initialize it if applicable
            if (onInit != null)
            {
                SerializedObject assetObj = new SerializedObject(asset);
                onInit.Invoke(assetObj);
            }

            // Attach the createt asset to the owner of the given property
            AssetDatabase.AddObjectToAsset(asset, assetsArrayProp.serializedObject.targetObject);

            // Insert the created Segment asset in the list
            int index = assetsArrayProp.arraySize;
            assetsArrayProp.InsertArrayElementAtIndex(index);
            assetsArrayProp.GetArrayElementAtIndex(index).objectReferenceValue = asset;
            assetsArrayProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Save and reimport the containing asset to update the Project view
            SaveAndReimport(assetsArrayProp.serializedObject.targetObject);
            return asset;
        }

        #endregion

    }

}