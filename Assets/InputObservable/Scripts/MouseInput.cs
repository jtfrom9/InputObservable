using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

namespace InputObservable
{
    public class MouseInputContext : InputObservableContext, IMouseWheelObservable
    {
        MouseInputObservable[] observables = new MouseInputObservable[3];

        public IObservable<MouseWheelEvent> Wheel { get => wheelSubject; }
        Subject<MouseWheelEvent> wheelSubject = new Subject<MouseWheelEvent>();

        protected override void Update()
        {
            foreach (var o in observables)
            {
                if (o != null)
                {
                    o.Update(eventSystem);
                }
            }

            var wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel < 0 || 0 < wheel)
            {
                wheelSubject.OnNext(new MouseWheelEvent()
                {
                    position = Input.mousePosition,
                    wheel = wheel
                });
            }
        }

        public override IInputObservable GetObservable(int id)
        {
            if (observables[id] == null)
            {
                observables[id] = new MouseInputObservable(this, id);
            }
            return observables[id];
        }

        public override void Dispose()
        {
            for (int i = 0; i < observables.Length; i++)
            {
                if (observables[i] != null)
                {
                    observables[i].Dispose();
                    observables[i] = null;
                }
            }
        }

        public MouseInputContext(MonoBehaviour behaviour, EventSystem eventSystem) :
            base(behaviour, eventSystem)
        { }
    }

    class MouseInputObservable : InputObservableBase
    {
        int buttonId;

        internal void Update(EventSystem eventSystem)
        {
            if (Input.GetMouseButtonDown(buttonId))
            {
                if (eventSystem!=null && eventSystem.IsPointerOverGameObject())
                {
                    return;
                }
                beginPos = Input.mousePosition;
                if (beginPos.x < 0 || beginPos.y < 0 || beginPos.x >= Screen.width || beginPos.y >= Screen.height)
                {
                    return;
                }
                begin = true;
                var e = new InputEvent()
                {
                    sequenceId = this.sequenceId,
                    id = this.id,
                    type = InputEventType.Begin,
                    position = Input.mousePosition,
                    sender = this
                };
                this.id++;
                beginStream.OnNext(e);
            }
            else if (Input.GetMouseButtonUp(buttonId))
            {
                if (!begin)
                {
                    return;
                }
                begin = false;
                var e = new InputEvent()
                {
                    sequenceId = this.sequenceId,
                    id = this.id,
                    type = InputEventType.End,
                    position = Input.mousePosition,
                    sender = this
                };
                this.id = 0;
                endStream.OnNext(e);
                this.sequenceId++;
            }
            else
            {
                if (begin && beginPos != (Vector2)Input.mousePosition)
                {
                    beginPos = Input.mousePosition;
                    var e = new InputEvent()
                    {
                        sequenceId = this.sequenceId,
                        id = this.id,
                        type = InputEventType.Move,
                        position = Input.mousePosition,
                        sender = this
                    };
                    this.id++;
                    moveStream.OnNext(e);
                }
            }
        }

        public override string ToString()
        {
            return $"MouseInput(buttonId={this.buttonId})";
        }

        public MouseInputObservable(InputObservableContext context, int buttonId) : base(context)
        {
            this.buttonId = buttonId;
        }
    }
}
