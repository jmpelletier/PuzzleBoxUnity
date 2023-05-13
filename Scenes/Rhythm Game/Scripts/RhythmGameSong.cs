using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PuzzleBox;

namespace PuzzleBox
{
    public class RhythmGameSong : MonoBehaviour
    {
        public float bpm = 120;
        public int beatsPerBar = 4;

        public Action OnSongEnd;
        public Action<float, float> OnUpdate;

        [HideInInspector]
        public RhythmGameNote[] notes;

        [HideInInspector]
        public float time;
        public float beats;

        AudioSource audioSource;
        bool playing;
        Vector3 startPosition;

        public void Play()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
            playing = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            startPosition = transform.position;

            notes = GetComponentsInChildren<RhythmGameNote>();
            foreach(RhythmGameNote note in notes)
            {
                note.beat = note.transform.localPosition.x;
                note.time = note.beat * 60f / bpm;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (audioSource.isPlaying)
            {
                time = audioSource.timeSamples / (float)audioSource.clip.frequency;
                beats = time * bpm / 60f;
                transform.position = startPosition + Vector3.left * beats;
                OnUpdate?.Invoke(time, beats);
            }

            if (playing && !audioSource.isPlaying)
            {
                playing = false;
                OnSongEnd?.Invoke();
            }
        }

        private void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                AudioSource source = GetComponent<AudioSource>();
                if (source == null || source.clip == null)
                {
                    return;
                }
                int barsInSong = (int)Mathf.Ceil(source.clip.length * bpm / (60f * beatsPerBar));
                Gizmos.color = Color.green;
                Vector3 linePosition = transform.position;
                for (int i = 0; i < barsInSong; i++)
                {
                    for (int j = 0; j < beatsPerBar; j++)
                    {
                        if (j == 0)
                        {
                            Gizmos.DrawLine(linePosition + Vector3.down, linePosition + Vector3.up);
                        }
                        else
                        {
                            Gizmos.DrawLine(linePosition + Vector3.down * 0.5f, linePosition + Vector3.up * 0.5f);
                        }

                        linePosition += Vector3.right * 1f;
                    }
                }
            }
        }
    }
} // namespace

