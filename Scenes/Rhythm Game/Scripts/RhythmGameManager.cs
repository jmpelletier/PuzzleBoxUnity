using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class RhythmGameManager : MonoBehaviour
    {
        public GameObject songPrefab;
        public RhythmGamePlayer player;
        public RhythmGameUI ui;

        GameObject song;

        // Start is called before the first frame update
        void Start()
        {
            player.OnPointsChanged += ui.SetScore;

            ui.ShowStartScreen();
        }


        public void EndGame()
        {
            ui.ShowResultScreen();
        }

        public void StartGame()
        {
            ui.ShowPlayScreen();
            ui.SetScore(0);

            if (song != null)
            {
                Destroy(song);
            }

            song = Instantiate(songPrefab);
            song.transform.position = player.transform.position;
            RhythmGameSong rhythmGameSong = song.GetComponent<RhythmGameSong>();
            rhythmGameSong.OnSongEnd += EndGame;

            player.SetSong(rhythmGameSong);

            player.points = 0;

            rhythmGameSong.Play();
        }
    }
} // namespace

