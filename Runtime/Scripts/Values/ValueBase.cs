using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace PuzzleBox
{
    public abstract class ValueBase : PersistentBehaviour, IValueObserver
    {
        [Space]
        public ObservableProperty source;

        protected Action OnValueChanged;

        private PuzzleBox.IObservable _target = null;

        private static Dictionary<string, List<ValueBase>> _instances = new Dictionary<string, List<ValueBase>>();

        protected abstract void SilentlyUpdateValue(object value);

        public void Subscribe(Action callback)
        {
            OnValueChanged += callback;
            callback();
        }

        public void Unsubscribe(Action callback)
        {
            OnValueChanged -= callback;
        }

        protected void Initialize<T>() where T : struct
        {
            if (_target != null)
            {
                T val = LevelManager.GetSavedValue<T>(persistenceKey);
                _target.Initialize(val);
            }
        }

        protected void Save<T>(T val)
        {
            foreach(ValueBase v in _instances[persistenceKey])
            {
                if (v != this && v != null)
                {
                    v.SilentlyUpdateValue(val);
                    v.OnValueChanged?.Invoke();
                }
            }

            switch(persistence)
            {
                case Persistence.None:
                    LevelManager.temporarySaveState.Set(persistenceKey, val);
                    break;
                case Persistence.Level:
                    LevelManager.saveState.Set(persistenceKey, val);
                    break;
                case Persistence.Session:
                    Manager.saveState.Set(persistenceKey, val);
                    break;
                case Persistence.Save:
                    Manager.WriteToSaveGame(persistenceKey, val);
                    break;
            }
        }

        protected void InitializeString()
        {
            if (_target != null)
            {
                string val = LevelManager.GetSavedString(persistenceKey);
                _target.Initialize(val);
            }
        }

        protected virtual void OnEnable()
        {
            if (!_instances.ContainsKey(persistenceKey))
            {
                _instances.Add(persistenceKey, new List<ValueBase>(){ this });
            }
            else
            {
                _instances[persistenceKey].Add(this);
            }

            if (_target == null)
            {
                if (source.behaviour != null && !string.IsNullOrEmpty(source.propertyName))
                {
                    var fields = source.behaviour.GetType().GetFields().Where(f => typeof(PuzzleBox.IObservable).IsAssignableFrom(f.FieldType));
                    foreach (FieldInfo field in fields)
                    {
                        if (source.propertyName.Equals(field.Name))
                        {
                            _target = field.GetValue(source.behaviour) as PuzzleBox.IObservable;
                            InitializeTarget();
                            break;
                        }
                    }
                }
            }

            if (_target != null)
            {
                _target.Subscribe(this);
            }
        }

        protected virtual void OnDisable()
        {
            _instances[persistenceKey].Remove(this);

            if (_target != null)
            {
                _target.Unsubscribe(this);
            }
        }

        protected abstract void InitializeTarget();

        public virtual void ValueChanged(int value) { }
        public virtual void ValueChanged(float value) { }
        public virtual void ValueChanged(bool value) { }
        public virtual void ValueChanged(string value) { }
        public virtual void ValueChanged(Vector2 value) { }
        public virtual void ValueChanged(Vector3 value) { }
        public virtual void ValueChanged(object value) { }
    }
}

