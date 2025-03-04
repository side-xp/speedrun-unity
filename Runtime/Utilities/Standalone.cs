/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using System;

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Miscellaneous functions and extensions from our core library used in this package.
    /// </summary>
    public static class Standalone
    {

        #region GUI

        public class GUIColorScope : IDisposable
        {
            private Color _previousColor = Color.clear;

            public GUIColorScope(Color color)
            {
                _previousColor = GUI.color;
                GUI.color = color;
            }

            public void Dispose()
            {
                GUI.color = _previousColor;
            }
        }

        // Fonts
        public const int MinFontSize = 1;
        public const int MaxFontSize = 255;

        // Sizing
        public const float HeightXL = 48f;
        public const float HeightL = 36f;
        public const float HeightM = 28f;
        public const float HeightS = 20f;
        public const float HeightXS = 16f;

        public const float WidthXL = 200f;
        public const float WidthL = 148f;
        public const float WidthM = 112f;
        public const float WidthS = 80f;
        public const float WidthXS = 40f;

        /// <summary>
        /// Copies the input style, and sets the margins.
        /// </summary>
        /// <param name="style">The style you want to update.</param>
        /// <param name="left">The left margin.</param>
        /// <param name="right">The right margin.</param>
        /// <param name="top">The top margin.</param>
        /// <param name="bottom">The bottom margin.</param>
        public static GUIStyle Margin(this GUIStyle style, int left, int right, int top, int bottom)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.margin = new RectOffset(left, right, top, bottom);
            return newStyle;
        }

        /// <inheritdoc cref="Margin(GUIStyle, int, int, int, int)"/>
        /// <param name="horizontal">The left and right margin.</param>
        /// <param name="vertical">The top and bottom margin.</param>
        public static GUIStyle Margin(this GUIStyle style, int horizontal, int vertical)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.margin = new RectOffset(horizontal, horizontal, vertical, vertical);
            return newStyle;
        }

        /// <inheritdoc cref="Margin(GUIStyle, int, int, int, int)"/>
        /// <param name="margin">The margin to set.</param>
        public static GUIStyle Margin(this GUIStyle style, RectOffset margin)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.margin = margin;
            return newStyle;
        }

        /// <summary>
        /// Copies the input style, and sets the padding.
        /// </summary>
        /// <param name="left">The left padding.</param>
        /// <param name="right">The right padding.</param>
        /// <param name="top">The top padding.</param>
        /// <param name="bottom">The bottom padding.</param>
        /// <inheritdoc cref="Margin(GUIStyle, int, int, int, int)"/>
        public static GUIStyle Padding(this GUIStyle style, int left, int right, int top, int bottom)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.padding = new RectOffset(left, right, top, bottom);
            return newStyle;
        }

        /// <inheritdoc cref="Padding(GUIStyle, int, int, int, int)"/>
        /// <param name="horizontal">The left and right padding.</param>
        /// <param name="vertical">The top and bottom padding.</param>
        public static GUIStyle Padding(this GUIStyle style, int horizontal, int vertical)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.padding = new RectOffset(horizontal, horizontal, vertical, vertical);
            return newStyle;
        }

        /// <inheritdoc cref="Padding(GUIStyle, int, int, int, int)"/>
        /// <param name="padding">The padding to set.</param>
        public static GUIStyle Padding(this GUIStyle style, RectOffset padding)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.padding = padding;
            return newStyle;
        }

        /// <summary>
        /// Copies the input style, and enables/disables word wrapping.
        /// </summary>
        /// <inheritdoc cref="Margin(GUIStyle, int, int, int, int)"/>
        /// <param name="enable">Is word wrapping enabled?</param>
        public static GUIStyle WordWrap(this GUIStyle style, bool enable)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.wordWrap = enable;
            return newStyle;
        }

        /// <summary>
        /// Copies the input style, and sets the font style to <see cref="FontStyle.BoldAndItalic"/>.
        /// </summary>
        /// <inheritdoc cref="Margin(GUIStyle, int, int, int, int)"/>
        public static GUIStyle BoldItalic(this GUIStyle style)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.fontStyle = FontStyle.BoldAndItalic;
            return newStyle;
        }

        /// <summary>
        /// Copies the input style, and sets the font style to <see cref="FontStyle.Bold"/>.
        /// </summary>
        /// <inheritdoc cref="BoldItalic(GUIStyle)"/>
        public static GUIStyle Bold(this GUIStyle style)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.fontStyle = FontStyle.Bold;
            return newStyle;
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

        /// <summary>
        /// Copies the input style, and sets the text alignment to <see cref="TextAnchor.MiddleLeft"/>.
        /// </summary>
        /// <inheritdoc cref="TextAlignment(GUIStyle, TextAnchor)"/>
        public static GUIStyle AlignLeft(this GUIStyle style)
        {
            return TextAlignment(style, TextAnchor.MiddleLeft);
        }

        /// <summary>
        /// Copies the input style, and sets the text alignment to <see cref="TextAnchor.MiddleCenter"/>.
        /// </summary>
        /// <inheritdoc cref="TextAlignment(GUIStyle, TextAnchor)"/>
        public static GUIStyle AlignCenter(this GUIStyle style)
        {
            return TextAlignment(style, TextAnchor.MiddleCenter);
        }

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

    }

}