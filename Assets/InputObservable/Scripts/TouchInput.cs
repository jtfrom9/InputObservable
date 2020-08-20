using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

namespace InputObservable
{
    public class TouchInputContext : InputObservableContext
    {
        Dictionary<int, TouchInputObservable> observables = new Dictionary<int, TouchInputObservable>();

        protected override void Update()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                TouchInputObservable o;
                if (observables.TryGetValue(touch.fingerId, out o))
                {
                    o.Update(touch, eventSystem);
                }
            }
        }

        public override IInputObservable GetObservable(int id)
        {
            if (!observables.ContainsKey(id))
            {
                observables[id] = new TouchInputObservable(this, id);
            }
            return observables[id];
        }

        public override void Dispose()
        {
            foreach (var o in observables.Values)
            {
                o.Dispose();
            }
            observables.Clear();
        }

        public TouchInputContext(MonoBehaviour behaviour, EventSystem eventSystem) :
            base(behaviour, eventSystem)
        { }
    }

    class TouchInputObservable : InputObservableBase
    {
        int index;

        internal void Update(Touch touch, EventSystem eventSystem)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
                        if(begin) {
                            Debug.LogError("invalid Begin");
                            return;
                        }
                        if (eventSystem!=null && eventSystem.IsPointerOverGameObject(touch.fingerId))
                        {
                            return;
                        }
                        begin = true;
                        beginPos = touch.position;
                        var e = new InputEvent()
                        {
                            sequenceId = this.sequenceId,
                            id = this.id,
                            type = InputEventType.Begin,
                            position = touch.position,
                            sender = this
                        };
                        this.id++;
                        beginStream.OnNext(e);
                    }
                    break;
                case TouchPhase.Moved:
                    {
                        if (!begin)
                            return;
                        var e = new InputEvent()
                        {
                            sequenceId = this.sequenceId,
                            id = this.id,
                            type = InputEventType.Move,
                            position = touch.position,
                            sender = this
                        };
                        this.id++;
                        moveStream.OnNext(e);
                    }
                    break;
                case TouchPhase.Ended:
                    {
                        if (!begin)
                            return;
                        begin = false;
                        var e = new InputEvent()
                        {
                            sequenceId = this.sequenceId,
                            id = this.id,
                            type = InputEventType.End,
                            position = touch.position,
                            sender = this
                        };
                        this.id = 0;
                        endStream.OnNext(e);
                        this.sequenceId++;
                    }
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return $"TouchInput(index={this.index})";
        }

        public TouchInputObservable(InputObservableContext context, int index) : base(context)
        {
            this.index = index;
        }
    }
}
