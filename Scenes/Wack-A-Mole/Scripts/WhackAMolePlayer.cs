using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PuzzleBox;

namespace PuzzleBox
{
    public class WhackAMolePlayer : MonoBehaviour
    {
        public GameObject resultPrefab;

        public int score = 0;
        public int hitPoints = 1;
        public int missPoints = -1;

        public Action<int> OnScoreChanged;
        
        Attack2D attack;


        // Start is called before the first frame update
        void Start()
        {
            attack = GetComponentInChildren<Attack2D>();
            attack.OnAttackObject += Attack;  
            attack.OnMiss += OnMiss;
        }

        void OnAttack()
        {
            if (isActiveAndEnabled)
            {
                attack.Attack();
            }
        }

        void Attack(GameObject target)
        {
            PuzzleBox.Life life = target.GetComponent<PuzzleBox.Life>();
            if (life != null)
            {
                life.ChangeLife(-life.life);
                UpdateScore(hitPoints);

                ResultLabel.Show(resultPrefab, $"+{hitPoints}", target.transform.position);
            }
        }

        void OnMiss()
        {
            UpdateScore(missPoints);

            ResultLabel.Show(resultPrefab, "MISS", transform.position);
        }

        void UpdateScore(int change)
        {
            score += change;
            OnScoreChanged?.Invoke(score);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
