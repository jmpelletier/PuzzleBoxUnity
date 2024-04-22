using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace PuzzleBox
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(RawImage))]
    public class EditorVideoPlayer : MonoBehaviour
    {
        public Vector2 resolution = new Vector2(720, 405);

        VideoPlayer videoPlayer;
        RawImage image;
        RenderTexture renderTexture;


        // Start is called before the first frame update
        void Start()
        {
            renderTexture = new RenderTexture((int)Mathf.Max(1, resolution.x), (int)Mathf.Max(1, resolution.y), 32);

            videoPlayer = GetComponent<VideoPlayer>();
            image = GetComponent<RawImage>();

            image.texture = renderTexture;
            videoPlayer.targetTexture = renderTexture;

            if(!Application.isPlaying)
            {
                videoPlayer.Play();
            }
            
        }


        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying && videoPlayer != null && !videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }
    }
}

