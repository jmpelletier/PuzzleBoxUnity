/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Animations;

namespace PuzzleBox
{
    [CustomEditor(typeof(PuzzleBox.Animate))]
    public class AnimateEditor : PuzzleBoxBehaviourEditor
    {
        SerializedProperty mode;
        SerializedProperty duration;
        SerializedProperty initialState;
        SerializedProperty onTurnOn;
        SerializedProperty onTurningOff;
        SerializedProperty onTurnedOff;
        SerializedProperty onStartPerforming;
        SerializedProperty onFinishedPerforming;

        private void OnEnable()
        {
            mode = serializedObject.FindProperty("mode");
            duration = serializedObject.FindProperty("duration");
            initialState = serializedObject.FindProperty("initialState");
            onTurnOn = serializedObject.FindProperty("OnTurnOn");
            onTurningOff = serializedObject.FindProperty("OnTurningOff");
            onTurnedOff = serializedObject.FindProperty("OnTurnedOff");
            onStartPerforming = serializedObject.FindProperty("OnStartPerforming");
            onFinishedPerforming = serializedObject.FindProperty("OnFinishedPerforming");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            Animate animateComponent = (Animate)target;

            EditorGUILayout.PropertyField(duration);

            EditorGUILayout.PropertyField(mode);

            if (animateComponent.mode == Animate.Mode.Toggle)
            {
                EditorGUILayout.PropertyField(initialState);
                EditorGUILayout.PropertyField(onTurnOn);
                EditorGUILayout.PropertyField(onTurningOff);
                EditorGUILayout.PropertyField(onTurnedOff);
            }
            else if (animateComponent.mode == Animate.Mode.OneShot)
            {
                EditorGUILayout.PropertyField(onStartPerforming);
                EditorGUILayout.PropertyField(onFinishedPerforming);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space (5);
            EditorGUILayout.LabelField("新しいアニメーションクリップを作成する。");
            if (GUILayout.Button("Create Animations"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save animations as...", "MyNewAnimation.controller", "controller", "");

                // Get the filename
                string fn = Path.GetFileNameWithoutExtension(path);
                string folder = Path.GetDirectoryName(path);
                string onClipPath = Path.Combine(folder, fn + "On.anim");
                string offClipPath = Path.Combine(folder, fn + "Off.anim");
                string performClipPath = Path.Combine(folder, fn + "Perform.anim");

                AnimationClip onClip = new AnimationClip();
                AnimationClip offClip = new AnimationClip();
                AnimationClip performClip = new AnimationClip();

                // Create a new controller
                AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);

                // Add the parameters
                controller.AddParameter("State", AnimatorControllerParameterType.Bool);
                controller.AddParameter("Perform", AnimatorControllerParameterType.Trigger);

                // Add the states
                AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
                rootStateMachine.anyStatePosition = new Vector3(400, -150, 0);
                rootStateMachine.entryPosition = new Vector3(-170, -230, 0);

                AnimatorState onState = rootStateMachine.AddState("On", new Vector3(100, -60, 0));
                onState.motion = onClip;
                AnimatorState offState = rootStateMachine.AddState("Off", new Vector3(100, 40, 0));
                offState.motion = offClip;
                AnimatorState performState = rootStateMachine.AddState("Perform", new Vector3(380, -60, 0));
                performState.motion = performClip;
                AnimatorState routeState = rootStateMachine.AddState("Route", new Vector3(-190, -160, 0));

                // Connect
                rootStateMachine.defaultState = routeState;
                AnimatorStateTransition performTransition = rootStateMachine.AddAnyStateTransition(performState);
                AnimatorStateTransition onOffTransition = onState.AddTransition(offState);
                AnimatorStateTransition offOnTransition = offState.AddTransition(onState);
                AnimatorStateTransition routeOnTransition = routeState.AddTransition(onState);
                AnimatorStateTransition routeOffTransition = routeState.AddTransition(offState);

                // Set conditions
                performTransition.AddCondition(AnimatorConditionMode.If, 0, "Perform");
                onOffTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "State");
                offOnTransition.AddCondition(AnimatorConditionMode.If, 0, "State");
                routeOnTransition.AddCondition(AnimatorConditionMode.If, 0, "State");
                routeOffTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "State");

                // Add behaviours
                AnimateStateBehaviour onStateBehaviour = onState.AddStateMachineBehaviour<AnimateStateBehaviour>();
                onStateBehaviour.state = Animate.State.On;
                AnimateStateBehaviour offStateBehaviour = offState.AddStateMachineBehaviour<AnimateStateBehaviour>();
                offStateBehaviour.state = Animate.State.Off;
                AnimateStateBehaviour performStateBehaviour = performState.AddStateMachineBehaviour<AnimateStateBehaviour>();
                performStateBehaviour.state = Animate.State.Perform;

                // Adjust transitions
                performTransition.hasFixedDuration = true;
                performTransition.canTransitionToSelf = true;
                performTransition.duration = 0f;
                performTransition.exitTime = 1f;
                performTransition.hasExitTime = false;

                routeOnTransition.hasFixedDuration = true;
                routeOnTransition.canTransitionToSelf = false;
                routeOnTransition.duration = 0f;
                routeOnTransition.exitTime = 1f;
                routeOnTransition.hasExitTime = false;

                routeOffTransition.hasFixedDuration = true;
                routeOffTransition.canTransitionToSelf = false;
                routeOffTransition.duration = 0f;
                routeOffTransition.exitTime = 1f;
                routeOffTransition.hasExitTime = false;

                onOffTransition.hasFixedDuration = true;
                onOffTransition.canTransitionToSelf = false;
                onOffTransition.duration = 0.2f;
                onOffTransition.exitTime = 1f;
                onOffTransition.hasExitTime = false;

                offOnTransition.hasFixedDuration = true;
                offOnTransition.canTransitionToSelf = false;
                offOnTransition.duration = 0.2f;
                offOnTransition.exitTime = 1f;
                offOnTransition.hasExitTime = false;

                // Set the controller
                Animate animate = target as Animate;
                if (animate != null)
                {
                    Animator animator = animate.GetComponent<Animator>();
                    if (animator != null) 
                    {
                        animator.runtimeAnimatorController = controller;
                    }
                }

                // Save assets
                AssetDatabase.CreateAsset(onClip, onClipPath);
                AssetDatabase.CreateAsset(offClip, offClipPath);
                AssetDatabase.CreateAsset(performClip, performClipPath);

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }
}

