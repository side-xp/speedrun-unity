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
    /// Represents the state of a single step in a <see cref="Speedrun.Segment"/> instance.
    /// </summary>
    public class Step
    {

        #region Delegates

        /// <summary>
        /// Called when this step is completed.
        /// </summary>
        /// <param name="step">The completed step.</param>
        public delegate void CompleteDelegate(Step step);

        #endregion


        #region Fields

        /// <inheritdoc cref="CompleteDelegate"/>
        public event CompleteDelegate OnComplete;

        /// <summary>
        /// The <see cref="Segment"/> instance to which this step belongs.
        /// </summary>
        private Segment _segment = null;

        /// <summary>
        /// The asset from which this instance has been created.
        /// </summary>
        private StepAsset _stepAsset = null;

        /// <summary>
        /// The date and time when this Step has been completed.
        /// </summary>
        private DateTime? _completedAt = null;

        /// <summary>
        /// The function to call when the state of this step changes.
        /// </summary>
        private Run.StepChangeDelegate _onChange = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="Step"/>
        /// <param name="stepAsset">The asset from which this instance is created.</param>
        /// <param name="segment"><inheritdoc cref="_segment" path="/summary"/></param>
        /// <param name="onChange"><inheritdoc cref="_onChange" path="/summary"/></param>
        internal Step(StepAsset stepAsset, Segment segment, Run.StepChangeDelegate onChange)
        {
            _stepAsset = stepAsset;
            _segment = segment;
            _onChange = onChange;
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_stepAsset"/>
        public StepAsset StepAsset => _stepAsset;

        /// <inheritdoc cref="StepAsset.DisplayName"/>
        public string DisplayName => _stepAsset != null ? _stepAsset.DisplayName : GetType().Name;

        /// <inheritdoc cref="_segment"/>
        public Segment Segment => _segment;
        
        /// <inheritdoc cref="Segment.Run"/>
        public Run Run => _segment.Run;

        /// <inheritdoc cref="_completedAt"/>
        public bool IsCompleted => _completedAt != null;

        /// <summary>
        /// Is this step a checkpoint?
        /// </summary>
        public bool IsCheckpoint => _stepAsset != null && _stepAsset.IsCheckpoint;

        /// <summary>
        /// Marks this step as completed.
        /// </summary>
        /// <returns>Returns true if this step has been marked as completed successfully.</returns>
        public bool Complete()
        {
            if (_segment.IsEnded)
            {
                Debug.LogWarning($"Failed to complete the {nameof(Step)} \"{DisplayName}\": Its owning {nameof(Segment)} instance is already ended.", _stepAsset);
                return false;
            }
            else if (IsCompleted)
            {
                Debug.LogWarning($"Failed to complete the {nameof(Step)} \"{DisplayName}\": That instance is already marked as completed.", _stepAsset);
                return false;
            }

            _completedAt = Run.TimeMilliseconds.ToDateTime();

            try
            {
                OnComplete?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            _onChange?.Invoke(this);
            return true;
        }

        #endregion

    }

}