using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using PuzzleBox;

namespace PuzzleBox
{
    [ExecuteAlways]
    public class RhythmGameSong : MonoBehaviour
    {
        [Header("Song")]
        public AudioClip audioClip;
        public float bpm = 120;
        public int beatsPerBar = 4;
        public int quantizeSteps = 4;
        public bool autoPlay = true;
        public Transform[] parts;

        [Header("Judgement")]
        public Transform judgePoint;
        public float missTime = 0.3f;

        public Action OnSongStart;
        public Action OnSongEnd;
        public Action<Vector3, float> OnNoteHit;
        public Action<Vector3> OnNoteMiss;

        [HideInInspector]
        public RhythmGameNote[] notes;

        [HideInInspector]
        public float time;

        [HideInInspector]
        public float beats;

        AudioSource audioSource;
        bool playing;

        int currentIndex = 0;

        public void Play()
        {
            audioSource = GetComponent<AudioSource>();
            notes = GetComponentsInChildren<RhythmGameNote>();

            float judgeBeat = positionToBeat(transform.InverseTransformPoint(judgePoint.position));

            foreach (RhythmGameNote note in notes)
            {
                note.beat = positionToBeat(note.transform.localPosition) - judgeBeat;
                note.time = beatToTime(note.beat);
            }

            Array.Sort(notes);

            audioSource.Play();
            OnSongStart?.Invoke();
            currentIndex = 0;
        }

        public void Hit()
        {
            time = audioSource.timeSamples / (float)audioSource.clip.frequency;

            bool hitNote = false;
            for (; currentIndex < notes.Length; currentIndex++)
            {
                RhythmGameNote note = notes[currentIndex];
                float timeDiff = note.time - time;
                if (timeDiff < -missTime)
                {
                    note.Clear();
                    OnNoteMiss?.Invoke(note.transform.position);
                }
                else if (timeDiff >= missTime)
                {
                    break;
                }
                else
                {
                    note.Clear();
                    OnNoteHit?.Invoke(note.transform.position, timeDiff);
                    hitNote = true;
                }
            }

            if (!hitNote)
            {
                OnNoteMiss?.Invoke(judgePoint.position);
            }
        }

        float positionToBeat(Vector3 localPosition)
        {
            return Vector3.Dot(localPosition, Vector3.right);
        }

        float beatToTime(float beat)
        {
            return beat * 60f / bpm;
        }

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClip;

            if (autoPlay)
            {
                Play();
            }    
        }

        // Update is called once per frame
        void Update()
        {
            if (Application.isPlaying)
            {
                if (audioSource.isPlaying)
                {
                    playing = true;
                    time = audioSource.timeSamples / (float)audioSource.clip.frequency;
                    beats = time * bpm / 60f;
                    foreach (Transform t in parts)
                    {
                        t.localPosition = t.right * -1f * beats;
                    }

                    for (; currentIndex < notes.Length; currentIndex++)
                    {
                        RhythmGameNote note = notes[currentIndex];
                        float timeDiff = note.time - time;
                        if(timeDiff < -missTime)
                        {
                            note.Clear();
                            OnNoteMiss?.Invoke(note.transform.position);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (playing && !audioSource.isPlaying)
                {
                    playing = false;
                    OnSongEnd?.Invoke();
                }
        }
            

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                quantizeSteps = Math.Max(quantizeSteps, 1);
                if (parts != null)
                {
                    foreach (Transform t in parts)
                    {
                        foreach(Transform child in t)
                        {
                            RhythmGameNote note = child.GetComponent<RhythmGameNote>();
                            if (note != null)
                            {
                                float dot = Vector3.Dot(Vector3.right, child.localPosition);
                                float beats = Mathf.Round(dot * quantizeSteps) / quantizeSteps;
                                child.localPosition = Vector3.right * beats;
                            }
                        }
                    }
                }
            }
#endif
        }

        private void OnDrawGizmos()
        {
            if (isActiveAndEnabled && parts != null)
            {
                AudioSource source = GetComponent<AudioSource>();
                if (source == null || source.clip == null)
                {
                    return;
                }
                int barsInSong = (int)Mathf.Ceil(source.clip.length * bpm / (60f * beatsPerBar));
                Gizmos.color = Color.green;

                foreach(Transform t in parts)
                {
                    if (t != null)
                    {
                        Vector3 linePosition = t.position;

                        for (int i = 0; i < barsInSong; i++)
                        {
                            for (int j = 0; j < beatsPerBar; j++)
                            {
                                if (j == 0)
                                {
                                    Gizmos.DrawLine(linePosition + t.up * -1f, linePosition + t.up);
                                }
                                else
                                {
                                    Gizmos.DrawLine(linePosition + t.up * -0.5f, linePosition + t.up * 0.5f);
                                }

                                linePosition += t.right * 1f;
                            }
                        }
                    }
                    
                }

                
            }
        }
    }
} // namespace

