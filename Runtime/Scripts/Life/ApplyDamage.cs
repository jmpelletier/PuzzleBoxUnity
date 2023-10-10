using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Life/Apply Damage")]
    public class ApplyDamage : MonoBehaviour
    {
        public enum DisplayPosition
        {
            Self,
            Target
        }

        public float baseDamage = 1;
        public GameObject displayPrefab;
        public DisplayPosition displayPosition = DisplayPosition.Self;

        public UnityEvent<float> OnDamage;

        protected virtual float CalculateDamage(GameObject target)
        {
            return baseDamage;
        }

        public void ApplyDamageToTarget(GameObject target)
        {
            Life targetLife = target.GetComponent<Life>();
            if (targetLife != null)
            {
                float damage = CalculateDamage(target);
                targetLife.ChangeLife(-damage);

                OnDamage?.Invoke(damage);

                if (displayPrefab != null)
                {
                    GameObject display = Instantiate(displayPrefab);
                    Label label = display.GetComponentInChildren<Label>();
                    if (label != null)
                    {
                        label.SetFloat(damage);
                    }
                    if (displayPosition == DisplayPosition.Self)
                    {
                        display.transform.position = transform.position;
                    }
                    else if (displayPosition == DisplayPosition.Target)
                    {
                        display.transform.position = target.transform.position;
                    }
                    
                }
            }
        }
    }
}

