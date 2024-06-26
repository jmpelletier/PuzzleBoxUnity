/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 namespace PuzzleBox
 {
    public interface IValueObserver
    {
        public void ValueChanged(int value);
        public void ValueChanged(float value);
        public void ValueChanged(bool value);
        public void ValueChanged(string value);
        public void ValueChanged(Vector2 value);
        public void ValueChanged(Vector3 value);
        public void ValueChanged(object value);
    }

    public abstract class ValueObserver : IValueObserver
    {
        public virtual void ValueChanged(int value){}
        public virtual void ValueChanged(float value){}
        public virtual void ValueChanged(bool value){}
        public virtual void ValueChanged(string value){}
        public virtual void ValueChanged(Vector2 value){}
        public virtual void ValueChanged(Vector3 value){}
        public virtual void ValueChanged(object value){}
    }

    public interface IObservable
    {
        public void Subscribe(IValueObserver observer);
        public void Unsubscribe(IValueObserver observer);
        public void Initialize(object initialValue);
    }

    public abstract class ObservableValue : IObservable
    {
        private List<IValueObserver> _observers = new List<IValueObserver>();

        protected abstract void Notify(IValueObserver observer);

        public abstract void Initialize(object initialValue);

        public void NotifyAll()
        {
            foreach(IValueObserver observer in _observers)
            {
                Notify(observer);
            }
        }

        public void Subscribe(IValueObserver observer)
        {
            if (observer != null)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }

                Notify(observer);
            }
        }

        public void Unsubscribe(IValueObserver observer)
        {
            if (observer != null && _observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }
    }

    [System.Serializable]
    public class ObservableInt : ObservableValue, IEquatable<int>, IComparable<int>
    {
        [SerializeField]
        private int _value;

        public static implicit operator int(ObservableInt value) => value._value;

        protected override void Notify(IValueObserver observer)
        {
            observer.ValueChanged(_value);
        }

        public void Set(int value)
        {
            _value = value;
            NotifyAll();
        }

        public int Get()
        {
            return _value;
        }

        public override void Initialize(object initialValue)
        {
            if (initialValue != null && initialValue is int)
            {
                _value = (int)initialValue;
            }
            else if (initialValue != null && initialValue is float)
            {
                _value = (int)(float)initialValue;
            }
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool Equals(int other)
        {
            return _value.Equals(other);
        }

        public int CompareTo(int other)
        {
            return _value.CompareTo(other);
        }
    }

    [System.Serializable]
    public class ObservableFloat : ObservableValue, IEquatable<float>, IComparable<float>
    {
        [SerializeField]
        private float _value;

        public static implicit operator float(ObservableFloat value) => value._value;

        protected override void Notify(IValueObserver observer)
        {
            observer.ValueChanged(_value);
        }

        public void Set(float value)
        {
            _value = value;
            NotifyAll();
        }

        public float Get()
        {
            return _value;
        }

        public override void Initialize(object initialValue)
        {
            if (initialValue != null && initialValue is float)
            {
                _value = (float)initialValue;
            }
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool Equals(float other)
        {
            return _value.Equals(other);
        }

        public int CompareTo(float other)
        {
            return _value.CompareTo(other);
        }
    }

    [System.Serializable]
    public class ObservableBool : ObservableValue, IEquatable<bool>
    {
        [SerializeField]
        private bool _value;

        public static implicit operator bool(ObservableBool value) => value._value;

        protected override void Notify(IValueObserver observer)
        {
            observer.ValueChanged(_value);
        }

        public void Set(bool value)
        {
            _value = value;
            NotifyAll();
        }

        public bool Get()
        {
            return _value;
        }

        public override void Initialize(object initialValue)
        {
            if (initialValue != null && initialValue is bool)
            {
                _value = (bool)initialValue;
            }
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool Equals(bool other)
        {
            return _value.Equals(other);
        }
    }

    [System.Serializable]
    public class ObservableString : ObservableValue, IEquatable<string>, IComparable<string>
    {
        [SerializeField]
        private string _value;

        public static implicit operator string(ObservableString value) => value._value;

        protected override void Notify(IValueObserver observer)
        {
            observer.ValueChanged(_value);
        }

        public void Set(string value)
        {
            _value = value;
            NotifyAll();
        }

        public string Get()
        {
            return _value;
        }

        public override void Initialize(object initialValue)
        {
            if (initialValue != null && initialValue is string)
            {
                _value = (string)initialValue;
            }
        }

        public override string ToString()
        {
            return _value;
        }

        public bool Equals(string other)
        {
            return _value != null ? _value.Equals(other) : other == null;
        }

        public int CompareTo(string other)
        {
            return _value.CompareTo(other);
        }
    }

    [System.Serializable]
    public class ObservableVector2 : ObservableValue, IEquatable<Vector2>
    {
        [SerializeField]
        private Vector2 _value;

        public static implicit operator Vector2(ObservableVector2 value) => value._value;

        protected override void Notify(IValueObserver observer)
        {
            observer.ValueChanged(_value);
        }

        public void Set(Vector2 value)
        {
            _value = value;
            NotifyAll();
        }

        public Vector2 Get()
        {
            return _value;
        }

        public override void Initialize(object initialValue)
        {
            if (initialValue != null && initialValue is Vector2)
            {
                _value = (Vector2)initialValue;
            }
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool Equals(Vector2 other)
        {
            return _value.Equals(other);
        }
    }

    [System.Serializable]
    public class ObservableVector3 : ObservableValue, IEquatable<Vector3>
    {
        [SerializeField]
        private Vector3 _value;

        public static implicit operator Vector3(ObservableVector3 value) => value._value;

        protected override void Notify(IValueObserver observer)
        {
            observer.ValueChanged(_value);
        }

        public void Set(Vector3 value)
        {
            _value = value;
            NotifyAll();
        }

        public Vector3 Get()
        {
            return _value;
        }

        public override void Initialize(object initialValue)
        {
            if (initialValue != null && initialValue is Vector3)
            {
                _value = (Vector3)initialValue;
            }
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool Equals(Vector3 other)
        {
            return _value.Equals(other);
        }
    }

    [System.Serializable]
    public struct ObservableProperty
    {
        public GameObject gameObject;
        public MonoBehaviour behaviour;
        public string propertyName;
    }
}
