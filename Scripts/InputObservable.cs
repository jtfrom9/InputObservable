using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace InputObservable
{
    public enum InputEventType
    {
        Begin = 1,
        Move = 2,
        End = 4,
    }

    public struct InputEvent
    {
        public long sequenceId;
        public long id;
        public InputEventType type;
        public Vector2 position;
        public IInputObservable sender;
        public override string ToString() { return $"[{sender}]({sequenceId}.{id},{type},{position})"; }
    }

    public struct VerocityInfo
    {
        public InputEvent @event;
        public Vector2 vector;
        public override string ToString() { return $"<{@event},{vector}>"; }
    }

    public interface IInputObservable
    {
        GameObject gameObject { get; }
        IObservable<InputEvent> Begin { get; }
        IObservable<InputEvent> End { get; }
        IObservable<InputEvent> Move { get; }
    }

    public abstract class InputObservableBase : IInputObservable, IDisposable
    {
        MonoBehaviour behaviour;
        protected long sequenceId = 0;
        protected long id = 0;
        protected bool begin = false;
        protected Vector2 beginPos = Vector2.zero;

        protected Subject<InputEvent> beginStream = new Subject<InputEvent>();
        protected Subject<InputEvent> endStream = new Subject<InputEvent>();
        protected Subject<InputEvent> moveStream = new Subject<InputEvent>();

        public GameObject gameObject { get => behaviour.gameObject; }
        public IObservable<InputEvent> Begin { get => beginStream; }
        public IObservable<InputEvent> End { get => endStream; }
        public IObservable<InputEvent> Move { get => moveStream; }

        protected abstract void Update();

        public InputObservableBase(MonoBehaviour behaviour)
        {
            this.behaviour = behaviour;
            behaviour.UpdateAsObservable()
                .Subscribe(_ => Update())
                .AddTo(behaviour);
            behaviour.OnDestroyAsObservable()
                .Subscribe(_ => Dispose())
                .AddTo(behaviour);
        }

        public void Dispose()
        {
            beginStream.Dispose();
            endStream.Dispose();
            moveStream.Dispose();
        }
    }
}
