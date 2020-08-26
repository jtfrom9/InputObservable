using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace InputObservable
{
    public abstract class InputObservableBase : IInputObservable, IDisposable
    {
        InputObservableContext context;
        protected long sequenceId = 0;
        protected long id = 0;
        protected bool begin = false;
        protected Vector2 beginPos = Vector2.zero;

        protected Subject<InputEvent> beginStream = new Subject<InputEvent>();
        protected Subject<InputEvent> endStream = new Subject<InputEvent>();
        protected Subject<InputEvent> moveStream = new Subject<InputEvent>();

        public IObservable<InputEvent> OnBegin { get => beginStream; }
        public IObservable<InputEvent> OnEnd { get => endStream; }
        public IObservable<InputEvent> OnMove { get => moveStream; }
        public bool IsBegin { get => begin; }

        public void Dispose()
        {
            beginStream.Dispose();
            endStream.Dispose();
            moveStream.Dispose();
        }

        public InputObservableBase(InputObservableContext context)
        {
            this.context = context;
        }
    }
}
