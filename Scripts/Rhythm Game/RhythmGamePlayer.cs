using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using PuzzleBox;

namespace PuzzleBox
{
    [RequireComponent(typeof(Spawner))]
    public class RhythmGamePlayer : MonoBehaviour
    {
        public GameObject noteResultPrefab;

        [Header("Events")]
        public UnityEvent OnSongStart;
        public UnityEvent OnSongEnd;
        public UnityEvent OnNoteMiss;
        public UnityEvent<float> OnNoteHit;

        RhythmGameSong song;

        Spawner spawner;

        public void Play()
        {
            if (song != null)
            {
                Destroy(song.gameObject);
                song = null;
            }

            GameObject newObject = spawner.Spawn();
            song = newObject.GetComponent<RhythmGameSong>();
            if (song == null)
            {
                Debug.LogError("RhythmGameSongコンポーネントが見つかりません。");
                Destroy(newObject);
            }
            else
            {
                song.OnNoteHit += HitNote;
                song.OnNoteMiss += MissedNote;
                song.OnSongEnd += SongEnded;
                song.Play();
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            spawner = GetComponent<Spawner>();
        }

        void OnHit()
        {
            if (song != null)
            {
                song.Hit();
            }
        }

        protected void ShowNoteResult(string message, Vector3 position)
        {
            if (noteResultPrefab != null)
            {
                GameObject result = Instantiate(noteResultPrefab);
                ResultLabel noteResult = result.GetComponent<ResultLabel>();
                if (noteResult != null)
                {
                    noteResult.SetText(message);
                }
                result.transform.position = position;
            }
        }

        virtual protected void MissedNote(Vector3 position)
        {
            ShowNoteResult("MISS!", position);
            OnNoteMiss?.Invoke();
            
        }

        virtual protected void HitNote(Vector3 position, float timeDifference)
        {
            ShowNoteResult("HIT!", position);
            OnNoteHit?.Invoke(timeDifference);
        }

        virtual protected void SongEnded()
        {
            OnSongEnd?.Invoke();
        }
    }
} // namespace

