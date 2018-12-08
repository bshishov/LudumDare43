using System;
using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    /// <summary>
    /// Base component for drum-cursor interaction.
    /// It listens for messages, sent by cursor
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DrumArea : MonoBehaviour
    {
        [Header("Cursor interaction settings")]
        public bool HideCursor = false;
        public bool SnapCursor = false;

        [Header("Activations")]
        public ActivatorProxy[] Targets;

        public event Action CursorEnter;
        public event Action CursorExit;

        public bool IsCursorInArea
        {
            get { return _cursor != null; }
        }

        public Cursor ActiveCursor
        {
            get { return _cursor; }
        }
        
        private Cursor _cursor;

        void Start()
        {
            if(!gameObject.CompareTag(Common.Tags.DrumArea))
                Debug.LogWarningFormat("[{0}] Tag of object should be set to {1}", gameObject.name, Common.Tags.DrumArea);
        }

        /// <summary>
        /// Called when cursor hits DrumArea collider
        /// </summary>
        public void OnCursorEnter(Cursor cursor)
        {
            _cursor = cursor;

            if(CursorEnter != null)
                CursorEnter.Invoke();

            // Fire cursor entered activation events
            if (Targets != null)
                foreach (var target in Targets)
                    target.Activate();
        }

        /// <summary>
        /// Called when cursor leaves DrumArea collider
        /// </summary>
        public void OnCursorExit(Cursor cursor)
        {
            _cursor = null;

            if (CursorExit != null)
                CursorExit.Invoke();

            // Fire cursor exit deactivation events
            if (Targets != null)
                foreach (var target in Targets)
                    target.Deactivate();
        }
    }
}
