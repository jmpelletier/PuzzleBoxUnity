using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PuzzleBox
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Label : MonoBehaviour
    {
        public string valueName = "";
        public string format = "{0}";

        TextMeshProUGUI _label;
        TextMeshProUGUI label
        {
            get
            {
                if (_label == null)
                {
                    _label = GetComponent<TextMeshProUGUI>();
                }
                return _label;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //label = GetComponent<TextMeshProUGUI>();
        }

        public void SetFloat(float value)
        {
            label.text = string.Format(format, value);
        }

        public void SetNamedFloat(NamedValue<float> val)
        {
            if (val.name == valueName)
            {
                SetFloat(val.value);
            }
        }

        public void SetInt(int value)
        {
            label.text = string.Format(format, value);
        }

        public void SetNamedInt(NamedValue<int> val)
        {
            if (val.name == valueName)
            {
                SetInt(val.value);
            }
        }

        public void SetString(string value)
        {
            label.text = string.Format(format, value);
        }

        public void SetNamedString(NamedValue<string> val)
        {
            if (val.name == valueName)
            {
                SetString(val.value);
            }
        }

        public void SetNamedValue(string n, float val)
        {
            if (n == valueName)
            {
                SetFloat(val);
            }
        }

        public void SetNamedValue(string n, int val)
        {
            if (n == valueName)
            {
                SetInt(val);
            }
        }

        public void SetNamedValue(string n, string val)
        {
            if (n == valueName)
            {
                SetString(val);
            }
        }

    }
}

