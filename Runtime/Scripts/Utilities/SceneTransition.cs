/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleBox
{
    [RequireComponent(typeof(Animator))]
    public class SceneTransition : PuzzleBoxBehaviour
    {
        public Camera screenshotCamera;
        public Image image;

        Animator animationController;
        
        RenderTexture renderTexture;
        Texture2D texture;

        static SceneTransition instance;


        bool _loading = false;
        bool loading
        {
            set
            {
                _loading = value;
                if (animationController == null)
                {
                    animationController = GetComponent<Animator>();
                }
                animationController.SetBool("Loading", _loading);
            }
            get { return _loading; }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }

            else
            {
                instance = this;
                transform.parent = null;
                GameObject.DontDestroyOnLoad(gameObject);

                SceneManager.sceneLoaded += (scene, mode) => {
                    loading = false;
                };
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            animationController = GetComponent<Animator>();
        }

        public static void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static void CreateInstance()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("Level Manager");
                instance = go.AddComponent<SceneTransition>();
            }
        }

        public static void UnloadScene(string name)
        {
            CreateInstance();

            if (instance != null)
            {
                instance.loading = true;
                instance.StartTransition(() => instance.StartCoroutine(instance.UnloadSceneAsync(name)));
            }
        }

        private IEnumerator UnloadSceneAsync(string name)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(name);

            while(operation != null && !operation.isDone)
            {
                yield return null;
            }
        }

        public static void LoadScene(string name, bool additive = false)
        {
            SceneManager.LoadScene(name, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            if (instance != null)
            {
                instance.loading = true;
                instance.StartTransition();
            }
        }

        public static void LoadScene(int buildIndex, bool additive = false)
        {
            SceneManager.LoadScene(buildIndex, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            if (instance != null)
            {
                instance.loading = true;
                instance.StartTransition();
            }
        }

        public void StartTransition(Action ready = null)
        {
            Screenshot(done: () => {
                if (animationController == null)
                {
                    animationController = GetComponent<Animator>();
                }
                animationController.SetTrigger("Show");

                ready?.Invoke();
            });
        }

        private void CreateRenderTexture(Camera cam, int w = 512, int h = 512)
        {
            if (cam != null)
            {
                renderTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 32);
            }
            else
            {
                renderTexture = new RenderTexture(w, h, 32);
            }
            renderTexture.Create();

            texture = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.ARGB32, false);
        }

        private void ReleaseRenderTexture()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }

            if (texture != null)
            {
                Destroy(texture);
                texture = null;
            }
        }

        public void Screenshot(Action done = null)
        {
            StartCoroutine(TakeScreenshot(done));
        }

        void ClearRenderTexture(RenderTexture renderTexture)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        IEnumerator TakeScreenshot(Action done)
        {
            yield return new WaitForEndOfFrame();

            Camera cam = screenshotCamera != null ? screenshotCamera : Camera.main;

            if (image == null)
            {
                yield break;
            }


            if (renderTexture == null)
            {
                CreateRenderTexture(cam);
            }

            if (cam == null)
            {
                ClearRenderTexture(renderTexture);


                RenderTexture oldActiveTexture = RenderTexture.active;
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();

                RenderTexture.active = oldActiveTexture;
            }
            else
            {
                if (renderTexture.width != cam.pixelWidth || renderTexture.height != cam.pixelHeight)
                {
                    ReleaseRenderTexture();
                    CreateRenderTexture(cam);
                }

                RenderTexture oldTargetTexture = cam.targetTexture;
                cam.targetTexture = renderTexture;
                cam.Render();

                RenderTexture oldActiveTexture = RenderTexture.active;
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();

                RenderTexture.active = oldActiveTexture;
                cam.targetTexture = oldTargetTexture;
            }
            

            float pixelsPerUnit = 100;

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            image.sprite = sprite;

            done?.Invoke();
        }

        private void OnDestroy()
        {
            ReleaseRenderTexture();
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}

