/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEffect : ActionDelegate
    {
        public AudioClip[] clips;

        [Min(0)]
        public float minimumVolume = 1.0f;

        [Min(0)]
        public float maximumVolume = 1.0f;

        AudioSource source;

        // Start is called before the first frame update
        void Start()
        {
            source = GetComponent<AudioSource>();
        }

        public void Play()
        {
            if (clips.Length > 0)
            {
                int i = Random.Range(0,clips.Length);
                GameObject sound = Instantiate(gameObject);
                sound.transform.position = transform.position;
                sound.SendMessage("PlayClipOneShot", i);
                GameObject.DontDestroyOnLoad(sound);
            }
        }

        void PlayClipOneShot(int clipIndex)
        {
            if (clipIndex >= 0 && clipIndex < clips.Length && clips[clipIndex] != null)
            {
                if (source == null)
                {
                    source = GetComponent<AudioSource>();
                }
                float vol = Random.Range(minimumVolume, maximumVolume);
                source.PlayOneShot(clips[clipIndex], vol);

                Destroy(gameObject, clips[clipIndex].length);
            }
        }

        public override void Perform(GameObject sender)
        {
            PerformAction(Play);
        }

        public override string GetIcon()
        {
            return "SoundIcon";
        }
    }
}

