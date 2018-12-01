using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cursor : Singleton<Cursor>
    {
        public MeshRenderer Renderer;
        private Drum _drum;

        private float _noteActivity = 0f;
        private Color _targetColor;

        void Start ()
        {
            _drum = FindObjectOfType<Drum>();
            _drum.OnNotePlayed += DrumOnOnNotePlayed;
        }

        void Update ()
        {
            _noteActivity *= 0.99f;

            if (Renderer != null)
                Renderer.material.color = Color.Lerp(Color.white, _targetColor, _noteActivity);
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            if (Renderer != null)
            {
                if (note.Type == Drum.NoteType.A)
                    _targetColor = Color.blue;

                if (note.Type == Drum.NoteType.B)
                    _targetColor = Color.cyan;
            }

            _noteActivity = 1f;
        }
    }
}
