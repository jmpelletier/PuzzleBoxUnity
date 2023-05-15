using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PuzzleBox;

namespace PuzzleBox
{
    public class BaseManager : MonoBehaviour
    {
        public GameObject playerPrefab;
        public Transform playerSpawnPoint;
        public BaseUI ui;

        [Header("Actions")]
        public UnityEvent OnStart;
        public UnityEvent OnGameStarted;
        public UnityEvent OnGameEnded;
        public UnityEvent<GameObject> OnPlayerSpawned;
        public UnityEvent<GameObject> OnPlayerDestroyed;
        
        protected bool playing = false;
        protected GameObject player;

        void SpawnPlayer()
        {
            player = Instantiate(playerPrefab);
            if (playerSpawnPoint != null)
            {
                player.transform.position = playerSpawnPoint.position;
                player.transform.rotation = playerSpawnPoint.rotation;
            }
            BroadcastMessage("OnPlayerSpawned", player, SendMessageOptions.DontRequireReceiver);
            OnPlayerSpawned?.Invoke(player);
        }

        void DestroyPlayer()
        {
            if (player != null)
            {
                BroadcastMessage("OnPlayerDestroyed", player, SendMessageOptions.DontRequireReceiver);
                OnPlayerDestroyed?.Invoke(player);
                Destroy(player);
                player = null;
            }
        }

        public void StartGame()
        {
            DestroyPlayer();
            SpawnPlayer();

            playing = true;

            BroadcastMessage("OnGameStarted", SendMessageOptions.DontRequireReceiver);
            OnGameStarted?.Invoke();
        }

        public void EndGame()
        {

            DestroyPlayer();

            playing = false;

            BroadcastMessage("OnGameEnded", SendMessageOptions.DontRequireReceiver);
            OnGameEnded?.Invoke();
        }

        public void SetScore(float score)
        {
            ui.SetScore(score);
        }

        // Start is called before the first frame update
        void Start()
        {
            OnStart?.Invoke();
        }

        // Update is called once per frame
        void Update()
        {
            if (playing)
            {
                
            }
        }
    }
}