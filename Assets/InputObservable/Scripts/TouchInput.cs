using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

namespace InputObservable
{
    public class TouchInputObservable : InputObservableBase
    {
        int index;
        EventSystem eventSystem;

        protected override void Update()
        {
            if (Input.touchCount == 0)
            {
                return;
            }
            var touch = Input.GetTouch(this.index);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
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
            return $"Touch({this.index})";
        }

        public TouchInputObservable(MonoBehaviour behaviour, int index, EventSystem eventSystem) : base(behaviour)
        {
            this.index = index;
            this.eventSystem = eventSystem;
        }
    }
}
