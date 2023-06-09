using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using PuzzleBox;

namespace PuzzleBox
{
    [System.Serializable]
    public struct NamedValue<T>
    {
        public string name;
        public T value;

        public NamedValue(string n, T v)
        {
            name = n;
            value = v;
        }
    }

    public class BaseUI : MonoBehaviour
    {
        public CanvasTransition startScreen;
        public CanvasTransition resultScreen;
        public CanvasTransition hud;

        public UnityEvent OnGameStart;

        private Label[] labels;

        private void OnEnable()
        {
            labels = GetComponentsInChildren<Label>(true);
        }

        public void StartGame()
        {
            ShowPlayScreen();
            OnGameStart?.Invoke();
        }

    
        public void BroadcastValue(string valueName, float val)
        {
            foreach (Label l in labels)
            {
                l.SetNamedValue(valueName, val);
            }
        }

        public void BroadcastValue(string valueName, int val)
        {
            foreach (Label l in labels)
            {
                l.SetNamedValue(valueName, val);
            }
        }

        public void BroadcastValue(string valueName, string val)
        {
            foreach (Label l in labels)
            {
                l.SetNamedValue(valueName, val);
            }
        }

        public void ShowStartScreen()
        {
            startScreen.Show();
            resultScreen.Hide();
            hud.Hide();
        }

        public void ShowPlayScreen()
        {
            startScreen.Hide();
            resultScreen.Hide();
            hud.Show();
        }

        public void ShowResultScreen()
        {
            startScreen.Hide();
            resultScreen.Show();
            hud.Hide();
        }
    }
}
