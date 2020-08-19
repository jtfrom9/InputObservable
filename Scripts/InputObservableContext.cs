using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;

namespace InputObservable
{
    public abstract class InputObservableContext : IDisposable
    {
        MonoBehaviour behaviour;
        protected EventSystem eventSystem;

        public GameObject gameObject { get => behaviour.gameObject; }
        protected abstract void Update();

        public abstract void Dispose();
        public abstract IInputObservable GetObservable(int id);

        public InputObservableContext(MonoBehaviour behaviour, EventSystem eventSystem)
        {
            this.behaviour = behaviour;
            this.eventSystem = eventSystem;

            behaviour.UpdateAsObservable()
                .Subscribe(_ => Update())
                .AddTo(behaviour);
            behaviour.OnDestroyAsObservable()
                .Subscribe(_ => Dispose())
                .AddTo(behaviour);
        }
    }
}
