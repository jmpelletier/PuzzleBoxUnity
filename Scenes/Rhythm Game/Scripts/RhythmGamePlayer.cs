using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace PuzzleBox
{
    public class RhythmGamePlayer : MonoBehaviour
    {
        public GameObject noteResultPrefab;

        [Header("ã»èÓïÒ")]
        public int beatsPerBar = 4;
        public int barsInSong = 36;

        [Header("îªíËéûä‘")]
        public float greatTime = 0.025f;
        public float goodTime = 0.05f;
        public float okTime = 0.1f;

        [Header("îªíËì_êî")]
        public int greatPoints = 100;
        public int goodPoints = 50;
        public int okPoints = 10;
        public int missPoints = -10;

        [HideInInspector]
        public int points = 0;

        public Action<int> OnPointsChanged;

        RhythmGameSong song;
        int currentNote = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void ShowNoteResult(string message, Vector3 position)
        {
            GameObject result = Instantiate(noteResultPrefab);
            NoteResult noteResult = result.GetComponent<NoteResult>();
            noteResult.SetText(message);
            result.transform.position = position;
        }

        void RemoveNote(RhythmGameNote note, string message)
        {
            note.gameObject.SetActive(false);
            Destroy(note.gameObject);
            ShowNoteResult(message, note.transform.position);
        }

        public void SetSong(RhythmGameSong newSong)
        {
            song = newSong;
            song.OnUpdate += UpdateSongTime;
            currentNote = 0;
        }

        void UpdateSongTime(float time, float beats)
        {
            for (; currentNote < song.notes.Length; currentNote++)
            {
                RhythmGameNote note = song.notes[currentNote];
                float timeDifference = note.time - time;

                if (timeDifference < -okTime)
                {
                    RemoveNote(note, "MISS");
                    UpdatePoints(missPoints);
                }
                else
                {
                    break;
                }
            }
        }

        void UpdatePoints(int newPoints)
        {
            points += newPoints;

            OnPointsChanged?.Invoke(points);
        }

        void OnRhythmTap(InputValue val)
        {
            if (song == null)
            {
                return;
            }

            bool hitNote = false;

            for (; currentNote < song.notes.Length; currentNote++)
            {
                RhythmGameNote note = song.notes[currentNote];
                float timeDifference = note.time - song.time;

                if (timeDifference > okTime)
                {
                    break;
                }
                else
                {
                    timeDifference = Mathf.Abs(timeDifference);

                    if (timeDifference <= greatTime)
                    {
                        RemoveNote(note, "GREAT");
                        UpdatePoints(greatPoints);
                        hitNote = true;
                    }

                    else if (timeDifference <= goodTime)
                    {
                        RemoveNote(note, "GOOD");
                        UpdatePoints(goodPoints);
                        hitNote = true;
                    }

                    else if (timeDifference <= okTime)
                    {
                        RemoveNote(note, "OK");
                        UpdatePoints(okPoints);
                        hitNote = true;
                    }
                }
            }

            if (!hitNote)
            {
                UpdatePoints(missPoints);
                ShowNoteResult("MISS", transform.position);
            }
        }
    }
} // namespace

