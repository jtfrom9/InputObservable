using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

namespace InputObservable
{
    public class MouseInputObservable : InputObservableBase
    {
        public struct MouseWheelEvent
        {
            public Vector2 position;
            public float wheel;
        }
        public IObservable<MouseWheelEvent> Wheel { get => wheelSubject; }

        int buttonId;
        EventSystem eventSystem;
        Subject<MouseWheelEvent> wheelSubject = new Subject<MouseWheelEvent>();

        protected override void Update()
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

            var wheel = Input.GetAxis("Mouse ScrollWheel");
            if(wheel < 0 || 0 < wheel) {
                wheelSubject.OnNext(new MouseWheelEvent()
                {
                    position = Input.mousePosition,
                    wheel = wheel
                });
            }
        }

        public override string ToString()
        {
            return $"MouseInput(buttonId={this.buttonId})";
        }

        public MouseInputObservable(MonoBehaviour behaviour, int buttonId, EventSystem eventSystem) : base(behaviour)
        {
            this.buttonId = buttonId;
            this.eventSystem = eventSystem;
        }
    }
}
