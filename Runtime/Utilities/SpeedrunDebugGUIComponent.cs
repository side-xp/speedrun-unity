/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using UnityEngine;

namespace SideXP.Speedrun
{

    /// <summary>
    /// 
    /// </summary>
    [HelpURL(Constants.BaseHelpUrl)]
    [AddComponentMenu(Constants.AddComponentMenu + "/Speedrun Debug GUI")]
    public class SpeedrunDebugGUIComponent : MonoBehaviour
    {

        private const float LabelWidthRatio = 0.7f;
        private const float VSpace = 16f;

        [Tooltip("The speedrun asset for which to display this debug GUI.")]
        public SpeedrunAsset SpeedrunAsset = null;

        [Tooltip("Defines the position and size of this debug GUI on screen." +
            "\nIf width is 0 or negative, " + nameof(Screen) + "." + nameof(Screen.width) + " is used instead." +
            "\nIf height is 0 or negative, " + nameof(Screen) + "." + nameof(Screen.height) + " is used instead.")]
        public Rect GUIRect = new Rect(0, 0, 400f, 0f);

        [Tooltip("The scale of the debug GUI on screen.")]
        public float GUIScale = 1f;

        private Vector2 _scrollPosition = Vector2.zero;

        private void OnGUI()
        {
            GUI.matrix = Matrix4x4.Scale(Vector3.one * GUIScale);

            Rect rect = GUIRect;
            if (rect.width <= 0)
                rect.width = Screen.width - GUI.skin.box.padding.right;
            if (rect.height <= 0)
                rect.height = Screen.height - GUI.skin.box.padding.bottom;

            rect.width /= GUIScale;
            rect.height /= GUIScale;

            using (new GUILayout.AreaScope(rect, GUIContent.none, GUI.skin.box))
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                {
                    GUILayout.Box("Speedrun Debug GUI", GUI.skin.box.Bold().FontSize(14).WordWrap(true).AlignCenter(), GUILayout.Height(Standalone.HeightM));

                    if (SpeedrunAsset.Speedrun == null)
                        DrawNoSpeedrunInstanceGUI();
                    else
                        DrawSpeedrunInstanceGUI(rect.width);
                }
                GUILayout.EndScrollView();
            }
        }

        private void DrawNoSpeedrunInstanceGUI()
        {
            using (new Standalone.GUIColorScope(Color.red))
                GUILayout.Label("No Speedrun instance created");

            if (GUILayout.Button("Create Speedrun instance"))
                SpeedrunAsset.CreateSpeedrun();
        }

        private void DrawSpeedrunInstanceGUI(float width)
        {
            GUILayout.Box("Infos", GUI.skin.box.Bold());

            ToggleField(width, "Is Started",    SpeedrunAsset.Speedrun.IsStarted);
            ToggleField(width, "Is Active",     SpeedrunAsset.Speedrun.IsActive);
            ToggleField(width, "Is Paused",     SpeedrunAsset.Speedrun.IsPaused);
            ToggleField(width, "Is Finished",   SpeedrunAsset.Speedrun.IsFinished);
            ToggleField(width, "Is Completed",  SpeedrunAsset.Speedrun.IsCompleted);
            ToggleField(width, "Is Canceled",   SpeedrunAsset.Speedrun.IsCanceled);
            ToggleField(width, "Is Ended",      SpeedrunAsset.Speedrun.IsEnded);

            GUILayout.Space(VSpace);
            GUILayout.Box("Run Controls", GUI.skin.box.Bold());

            // Start button
            using (new Standalone.GUIColorScope(SpeedrunAsset.Speedrun.IsStarted ? Color.red : Color.green))
            {
                if (GUILayout.Button("Start"))
                    SpeedrunAsset.StartSpeedrun();
            }

            // Pause/resume button
            using (new Standalone.GUIColorScope(SpeedrunAsset.Speedrun.IsStarted ? Color.red : Color.yellow))
            {
                if (SpeedrunAsset.Speedrun.IsActive && SpeedrunAsset.Speedrun.IsPaused)
                {
                    if (GUILayout.Button("Resume"))
                        SpeedrunAsset.ResumeSpeedrun();
                }
                else
                {
                    if (GUILayout.Button("Pause"))
                        SpeedrunAsset.PauseSpeedrun();
                }
            }

            // Cancel button
            using (new Standalone.GUIColorScope(SpeedrunAsset.Speedrun.IsActive ? Color.green : Color.red))
            {
                if (GUILayout.Button("Cancel"))
                    SpeedrunAsset.CancelSpeedrun();
            }

            // Restart button
            using (new Standalone.GUIColorScope(SpeedrunAsset.Speedrun.IsActive ? Color.yellow : Color.green))
            {
                if (GUILayout.Button("Restart"))
                {
                    SpeedrunAsset.CancelSpeedrun();
                    SpeedrunAsset.StartSpeedrun();
                }
            }

            GUILayout.Space(VSpace);
            GUILayout.Box("Segments", GUI.skin.box.Bold());

            foreach (Segment segment in SpeedrunAsset.Speedrun.Segments)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(segment.DisplayName, GUI.skin.label.Bold(), GUILayout.ExpandWidth(true));

                        // Finish button
                        using (new Standalone.GUIColorScope(segment.IsEnded || segment.IsFinished ? Color.red : Color.green))
                        {
                            if (GUILayout.Button("Finish", GUILayout.Width(Standalone.WidthS)))
                                segment.Finish();
                        }

                        // Finish button
                        using (new Standalone.GUIColorScope(segment.IsEnded ? Color.red : Color.green))
                        {
                            if (GUILayout.Button("Cancel", GUILayout.Width(Standalone.WidthS)))
                                segment.Cancel();
                        }
                    }
                    ToggleField(width, "\tIs Finished", segment.IsFinished);
                    ToggleField(width, "\tIs IsCompleted", segment.IsCompleted);
                    ToggleField(width, "\tIs Canceled", segment.IsCanceled);
                    ToggleField(width, "\tIs Ended", segment.IsEnded);

                    foreach (Step step in segment.Steps)
                        ToggleField(width, $"- {step.DisplayName}{(step.IsCheckpoint ? " (CP)" : "")}:", step.IsCompleted);
                }
            }

            bool ToggleField(float width, string label, bool value)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(label, GUILayout.Width(width * LabelWidthRatio));
                    return GUILayout.Toggle(value, GUIContent.none);
                }
            }
        }

    }

}