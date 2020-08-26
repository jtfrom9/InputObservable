﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

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

    public interface IInputObservable
    {
        IObservable<InputEvent> OnBegin { get; }
        IObservable<InputEvent> OnEnd { get; }
        IObservable<InputEvent> OnMove { get; }
        bool IsBegin { get; }
    }

    public struct VerocityInfo
    {
        public InputEvent @event;
        public Vector2 vector;
        public override string ToString() { return $"<{@event},{vector}>"; }
    }

    public struct MouseWheelEvent
    {
        public Vector2 position;
        public float wheel;
    }

    public interface IMouseWheelObservable
    {
        IObservable<MouseWheelEvent> Wheel { get; }
    }

    public interface IGyroInputObservable
    {
        IObservable<Vector3> EulerAngles { get; }
        void AddRotate(Vector3 rotate);
        void Reset();
    }
}
