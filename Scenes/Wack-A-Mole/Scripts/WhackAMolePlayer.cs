using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
            attack.Attack();
        }

        void Attack(GameObject target)
        {
            WhackAMoleMole mole = target.GetComponent<WhackAMoleMole>();
            if (mole != null)
            {
                mole.ApplyDamage(1);
                UpdateScore(hitPoints);

                ShowResult($"+{hitPoints}", mole.transform.position);
            }
        }

        void OnMiss()
        {
            UpdateScore(missPoints);

            ShowResult("MISS", transform.position);
        }

        void UpdateScore(int change)
        {
            score += change;
            OnScoreChanged?.Invoke(score);
        }

        void ShowResult(string message, Vector3 position)
        {
            GameObject result = Instantiate(resultPrefab);
            WhackResult noteResult = result.GetComponent<WhackResult>();
            noteResult.SetText(message);
            result.transform.position = position;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
