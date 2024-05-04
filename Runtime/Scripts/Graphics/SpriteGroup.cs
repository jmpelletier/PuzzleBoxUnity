/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace PuzzleBox
{
    [ExecuteAlways]
    public class SpriteGroup : PuzzleBoxBehaviour
    {
        private struct Target
        {
            public SpriteRenderer renderer;
            public float defaultAlpha;

            public Target(SpriteRenderer renderer)
            {
                this.renderer = renderer;
                this.defaultAlpha = renderer != null ? renderer.color.a : 1;
            }

            public float SaveAlpha()
            {
                if (renderer != null)
                {
                    defaultAlpha = renderer.color.a;
                }

                return defaultAlpha;
            }

            public void SetAlpha(float value)
            {
                if (renderer != null)
                {
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, value);
                }
            }

            public void RestoreAlpha()
            {
                SetAlpha(defaultAlpha);
            }
        }

        private Target[] targets = new Target[0];

        [Range(0, 1)]
        public float alpha = 1f;

        private void OnEnable()
        {
            OnTransformChildrenChanged();

            RenderPipelineManager.beginCameraRendering += OnPreRenderCallback;
            RenderPipelineManager.endCameraRendering += OnPostRenderCallback;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnPreRenderCallback;
            RenderPipelineManager.endCameraRendering -= OnPostRenderCallback;
        }

        private void OnTransformChildrenChanged()
        {
            targets = GetComponentsInChildren<SpriteRenderer>().Select(c => new Target(c)).ToArray();
        }

        private void OnPreRenderCallback(ScriptableRenderContext context, Camera camera)
        {
            foreach (Target target in targets)
            {
                float a = target.SaveAlpha();
                target.SetAlpha(alpha * a);
            }
        }

        private void OnPostRenderCallback(ScriptableRenderContext context, Camera camera)
        {
            foreach (Target target in targets)
            {
                target.RestoreAlpha();
            }
        }
    }
}

