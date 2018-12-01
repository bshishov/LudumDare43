using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Drum : MonoBehaviour
    {
        public enum NoteType { Pause, A, B, C }
        public enum NoteLen { Fourth, Eight, Unknown }

        public struct Note
        {
            public float Time;
            public float TimeSinceLast;
            public NoteType Type;
            public NoteLen Len;

            public override string ToString()
            {
                return string.Format("Note<{0} dt={1:F2} t={2:F2}>", this.KeyRepresentation(), this.TimeSinceLast, this.Time);
            }

            /// <summary>
            /// String representation of a note played
            /// </summary>
            /// <returns></returns>
            public string KeyRepresentation()
            {
                var defaultRepr = this.Type.ToString();
                if (this.Type == NoteType.Pause)
                    defaultRepr = "~";

                if (this.Len == NoteLen.Unknown)
                    return defaultRepr + "?";

                if (this.Len == NoteLen.Eight)
                    return defaultRepr.ToLower();

                return defaultRepr;
            }
        }

        public const int MaxNotes = 16;

        public AudioClipWithVolume SoundA;
        public AudioClipWithVolume SoundB;

        private Note[] _notes = new Note[MaxNotes];
        private float[] _pitchesA = new float[3] { 0.99f, 1f, 1.01f };
        private float[] _pitchesB = new float[4] { 0.99f, 1f, 1.01f, 0.99f };

        private float _bpm;
        private float _avg4Delay;
        private bool _isSeqRunning;

        private int _aHits = 0;
        private int _bHits = 0;

        void Start()
        {
            for (var i = 0; i < MaxNotes; i++)
            {
                _notes[i] = new Note { Len = NoteLen.Fourth, Type = NoteType.Pause};
            }
        }

        void Update()
        {
            var lastNote = _notes[_notes.Length - 1];
            
            if (_avg4Delay > 0 && Time.time >= lastNote.Time + _avg4Delay)
            {
                TapNote(NoteType.Pause);
            }

            if (Input.GetMouseButtonDown(0))
            {
                _aHits = (_aHits + 1) % _pitchesA.Length;
                SoundManager.Instance.Play(SoundA, pitch: _pitchesA[_aHits]);
                TapNote(NoteType.A);
            }

            if (Input.GetMouseButtonDown(1))
            {
                _bHits = (_bHits + 1) % _pitchesB.Length;
                SoundManager.Instance.Play(SoundB, pitch: _pitchesB[_bHits]);
                TapNote(NoteType.B);
            }

            if (Input.GetMouseButtonDown(2))
                TapNote(NoteType.Pause);
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 200), GUIContent.none))
                TapNote(NoteType.A);

            if (GUI.Button(new Rect(110, 10, 100, 200), GUIContent.none))
                TapNote(NoteType.B);

            GUI.Label(new Rect(10, 220, 200, 20), string.Format("BPM: {0:F2}", _bpm));
            GUI.Label(new Rect(10, 240, 200, 20), string.Format("DEL: {0}", _avg4Delay));
            GUI.Label(new Rect(10, 260, 200, 20), string.Format("SEQ: {0}", SequenceStartFrom()));
            GUI.Label(new Rect(10, 280, 200, 20), string.Format("BUF: {0}", MaxNotes));
            GUI.Label(new Rect(10, 300, 200, 20), string.Format("ISSEQ: {0}", _isSeqRunning));


            for (var i = 0; i < _notes.Length; i++)
            {
                var note = _notes[MaxNotes - 1 - i];
                var ypos = 320 + 20 * i;
                GUI.Label(new Rect(10, ypos, 30, 20), note.KeyRepresentation());
                GUI.Label(new Rect(30, ypos, 50, 20), note.Len.ToString());
                GUI.Label(new Rect(100, ypos, 50, 20), string.Format("{0:F3}", note.TimeSinceLast));
                GUI.Label(new Rect(170, ypos, 50, 20), string.Format("{0:F3}", note.Time));
            }

            //var buffer = string.Join(null, this._notes.Select(n => n.KeyRepresentation()).ToArray());
            //GUI.Label(new Rect(10, 320, 400, 20), string.Format("BUF: {0}", buffer));
        }

        
        void TapNote(NoteType t)
        {
            var currentTime = Time.time;
            var lastNote = _notes[_notes.Length - 1];
            var lastNoteTime = lastNote.Time;
            var delta = currentTime - lastNoteTime;

            var note = new Note
            {
                Type = t,
                Time = currentTime,
                TimeSinceLast = delta,
                Len = NoteLen.Unknown
            };
            Debug.Log(note);


            // Quantize and replace nearest pause
            if (lastNote.Type == NoteType.Pause && Mathf.Abs(lastNoteTime - currentTime) < 0.05f)
            {
                Debug.Log("CLOSE!");
                // Replace auto-placed pause
                _notes[_notes.Length - 1] = note;
            }
            else
            {
                // Shift all notes in buffer and append new note to the end
                for (var i = 0; i < MaxNotes - 1; i++)
                    _notes[i] = _notes[i + 1];

                _notes[MaxNotes - 1] = note;
            }

            CalcBpm();
        }
        
        /// <summary>
        /// Updates bpm using the buffer of notes
        /// </summary>
        void CalcBpm()
        {
            var timeSum = 0f;
            var startsFrom = SequenceStartFrom();
            if (startsFrom == -1)
            {
                _bpm = 0;
                _avg4Delay = 0;
                _isSeqRunning = false;
                return;
            }

            _isSeqRunning = true;

            /*
            // Notes len resolution (last note cant be resolved)
            for (var i = startsFrom; i < MaxNotes - 2; i++)
            {
                // If note is resolved already
                if(_notes[i].Len != NoteLen.Unknown)
                    continue;

                var curLenGuess = NoteLen.Unknown;

                // Length of the note is the delay of the second note
                var nextLen = _notes[i+2].TimeSinceLast;
                var curLen = _notes[i+1].TimeSinceLast;
                
                if (IsClose(nextLen, curLen * 0.5f, relTol: 0.5f))
                {
                    Debug.Log("SAME");
                    // Nearby notes have same length
                    curLenGuess = NoteLen.Unknown;
                }
                else if (IsClose(nextLen, curLen * 0.5f, relTol: 0.5f))
                {
                    // Current note is twice as big
                    curLenGuess = NoteLen.Fourth;
                }
                else if (IsClose(nextLen * 0.5f, curLen, relTol: 0.5f))
                {
                    // Next note is twice as big
                    curLenGuess = NoteLen.Eight;
                }
                else
                {
                    Debug.Log("MEH");
                    curLenGuess = NoteLen.Unknown;
                }

                // Resolve all unknown
                for (var j = i; j >= startsFrom; j--)
                {
                    if (_notes[j].Len != NoteLen.Unknown)
                        break;

                    _notes[j].Len = curLenGuess;
                }
            }*/


            var fullFours = 0f;

            // Find longest note as set it as quarter length
            var quaterLength = 0f;
            for (var i = startsFrom + 1; i < MaxNotes; i++)
            {
                quaterLength = Mathf.Max(quaterLength, _notes[i].TimeSinceLast);
            }

            for (var i = startsFrom + 1; i < MaxNotes; i++)
            {
                timeSum += quaterLength;
                fullFours += _notes[i].TimeSinceLast / quaterLength;
            }

            // Average time between notes = Sum of times / sequence length (number of fourth notes)
            _avg4Delay = timeSum / fullFours;
            if (_avg4Delay > 0)
                _bpm = 60f / _avg4Delay;
            else
                _bpm = 0;
        }

        /// <summary>
        /// Returns index of sequence start note. Two pauses in a row means a sequence end
        /// If buffer ends with 2 pauses -> no sequence. Return is -1
        /// Return 0 means that the whole buffer is a sequence
        /// </summary>
        /// <returns></returns>
        int SequenceStartFrom()
        {
            var pause1Found = false;
            for (var i = MaxNotes - 1; i >= 0; i--)
            {
                if (_notes[i].Type == NoteType.Pause)
                {
                    if (pause1Found)
                    {
                        if (i + 2 > MaxNotes - 1)
                            return -1; // No sequence (last 2 notes are pauses)
                        return i + 2;
                    }

                    pause1Found = true;
                }
                else
                {
                    pause1Found = false;
                }
            }

            return 0;
        }

        bool IsClose(float a, float b, float relTol = 0.1f)
        {
            return Mathf.Abs(a - b) <= Mathf.Max(relTol * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)));
        }
    }
}