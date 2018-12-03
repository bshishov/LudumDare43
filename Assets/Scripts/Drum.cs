using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Drum : MonoBehaviour
    {
        public const float MaxBpm = 1000f;
        public const float MinBpm = 100f;
        public enum NoteType { SequenceEnd, A, B, C }
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
                if (this.Type == NoteType.SequenceEnd)
                    defaultRepr = "!";

                /*
                if (this.Len == NoteLen.Unknown)
                    return defaultRepr + "?";*/

                if (this.Len == NoteLen.Eight)
                    return defaultRepr.ToLower();

                return defaultRepr;
            }
        }

        [Serializable]
        public class CommandSequence
        {
            public string Name;
            public NoteType[] Notes;
        }

        public const int MaxNotes = 8;
        public const int NotesPerCheck = 1;

        public event Action<CommandSequence> OnCommandSequence;
        public event Action<Note> OnNotePlayed;

        public float Bpm
        {
            get { return _bpm; }
        }

        public bool IsPlaying
        {
            get { return _isSeqRunning; }
        }

        public float Average4NoteDelay
        {
            get { return _avg4Delay; }
        }

        [Header("General")]
        [SerializeField]
        public CommandSequence[] Sequences;
        public bool ShowDebugInfo = true;

        // Drum stuff
        private readonly Note[] _notes = new Note[MaxNotes];
        private float _bpm;
        private bool _isSeqRunning;
        private float _avg4Delay;
        private string _lastCmd;
        private int _step = 0;


        void Start()
        {
            for (var i = 0; i < MaxNotes; i++)
            {
                _notes[i] = new Note { Len = NoteLen.Fourth, Type = NoteType.SequenceEnd };
            }
        }

        void Update()
        {
            var lastNote = _notes[_notes.Length - 1];
            
            if (_avg4Delay > 0 && Time.time >= lastNote.Time + _avg4Delay * 2)
            {
                for (var i = 0; i < NotesPerCheck; i++)
                {
                    PlayNote(NoteType.SequenceEnd);
                }

                _step = 0;
            }
            
        }

        void OnGUI()
        {
            if (ShowDebugInfo)
            {
                GUI.Box(new Rect(5, 15, 250, 400), GUIContent.none);
                GUI.Label(new Rect(10, 20, 200, 20), string.Format("BPM: {0:F2}", _bpm));
                GUI.Label(new Rect(10, 40, 200, 20), string.Format("AVG4DEL: {0}", _avg4Delay));
                GUI.Label(new Rect(10, 60, 200, 20), string.Format("SEQ: {0}", SequenceStartFrom()));
                GUI.Label(new Rect(10, 80, 200, 20), string.Format("BUF: {0}", MaxNotes));
                GUI.Label(new Rect(10, 100, 200, 20), string.Format("ISSEQ: {0}", _isSeqRunning));
                GUI.Label(new Rect(10, 120, 200, 20), string.Format("CMD: {0}", _lastCmd));


                for (var i = 0; i < _notes.Length; i++)
                {
                    var note = _notes[MaxNotes - 1 - i];
                    var ypos = 140 + 20 * i;
                    GUI.Label(new Rect(10, ypos, 30, 20), note.KeyRepresentation());
                    GUI.Label(new Rect(30, ypos, 50, 20), note.Len.ToString());
                    GUI.Label(new Rect(100, ypos, 50, 20), string.Format("{0:F3}", note.TimeSinceLast));
                    GUI.Label(new Rect(170, ypos, 50, 20), string.Format("{0:F3}", note.Time));
                }
            }
        }

        
        public void PlayNote(NoteType t)
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
            if(ShowDebugInfo)
                Debug.Log(note);
            
            // Shift all notes in buffer and append new note to the end
            for (var i = 0; i < MaxNotes - 1; i++)
                _notes[i] = _notes[i + 1];

            _notes[MaxNotes - 1] = note;
            CalcBpm();

            _step = (_step + 1) % NotesPerCheck;

            if (_step == 0)
            {
                var seq = Sequences.FirstOrDefault(CommandSequenceAtTheEnd);
                if (seq != null)
                {
                    _lastCmd = seq.Name;
                    if(OnCommandSequence != null)
                        OnCommandSequence.Invoke(seq);
                }
                else
                {
                    _lastCmd = "NO CMD";
                }
            }

            if(OnNotePlayed != null)
                OnNotePlayed.Invoke(note);
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
                timeSum += _notes[i].TimeSinceLast;
                //fullFours += _notes[i].TimeSinceLast / quaterLength;
                fullFours += 1;
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
            if (_notes[_notes.Length - 1].Type == NoteType.SequenceEnd)
                return -1;

            for (var i = MaxNotes - 2; i >= 0; i--)
            {
                if (_notes[i].Type == NoteType.SequenceEnd)
                {
                    return i + 1;
                }
            }

            return 0;
        }

        bool IsClose(float a, float b, float relTol = 0.1f)
        {
            return Mathf.Abs(a - b) <= Mathf.Max(relTol * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)));
        }

        public bool CommandSequenceAtTheEnd(CommandSequence sequence)
        {
            if (sequence == null)
                return false;

            if (sequence.Notes == null)
                return false;

            if (_notes[_notes.Length - 1].Type == NoteType.SequenceEnd)
                return false;

            for (var i = 0; i < sequence.Notes.Length; i++)
            {
                if (sequence.Notes[i] != _notes[_notes.Length - sequence.Notes.Length + i].Type)
                    return false;
            }

            return true;
        }
    }
}