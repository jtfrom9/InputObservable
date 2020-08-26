using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class ObjectViewer : MonoBehaviour
{
    public GameObject prefab;
    Transform _target;
    Rigidbody _rigidbody;

    void Awake()
    {
        var go = Instantiate(prefab);
        _target = go.transform;
        _rigidbody = go.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.angularDrag = 1f;
        _rigidbody.maxAngularVelocity = 20;
    }

    void Start()
    {
        var context = this.DefaultInputContext();
        var touch0 = context.GetObservable(0);
        var touch1 = context.GetObservable(1);

        touch0.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);
        touch1.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);

        // Stop Rotation by keep touch
        touch0.Keep(100, () => _rigidbody.angularVelocity.x > 0 || _rigidbody.angularVelocity.y > 0 || _rigidbody.angularVelocity.z > 0)
            .Subscribe(_ => {
                Debug.Log("break");
                var m = _rigidbody.angularVelocity.magnitude;
                _rigidbody.angularVelocity = (m < 0.1f) ? Vector3.zero : _rigidbody.angularVelocity * 0.1f;
            }).AddTo(this);

        // Rotate by Touch Operation
        var hratio = -180.0f / Screen.width;
        var vratio = -180.0f / Screen.height;
        touch0.Difference()
            .Where(_ => !touch1.Began)
            .Subscribe(v2 =>
            {
                var rot = v2.ToEulerAngle(hratio, vratio);
                _target.Rotate(rot, Space.World);
            }).AddTo(this);

        // Rotate Animation by Swipe operation
        touch0.Verocity(4)
            .Subscribe(vs =>
            {
                var average = new Vector2
                {
                    x = vs.Average(vi => vi.vector.x),
                    y = vs.Average(vi => vi.vector.y)
                };
                Debug.Log($"vector = {average}");
                var mag = average.magnitude;
                if (mag > 0.1f)
                {
                    var torque = average.ToEulerAngle(hratio, vratio) * mag;
                    _rigidbody.AddTorque(torque, ForceMode.VelocityChange);
                    Debug.Log($"magnitude = {mag}, torque={torque}");
                }
            }).AddTo(this);

        // Reset
        var orig_rotate = _target.rotation;
        var orig_scale = _target.localScale;
        touch0.DoubleSequence(200).Subscribe(_ =>
        {
            _target.rotation = orig_rotate;
            _target.localScale = orig_scale;
        }).AddTo(this);

        // Scale Object by Mouse Wheel
        var camera = Camera.main;
        (context as IMouseWheelObservable)?.Wheel.Subscribe(v =>
        {
            var scale = _target.localScale + Vector3.one * v.wheel;
            if(0.1 < scale.x && scale.x < 10)
                _target.localScale += Vector3.one * v.wheel;
        }).AddTo(this);

        // Scale Object by Pinch Operation
        RectangleObservable.From(touch0, touch1)
            .PinchSequence()
            .RepeatUntilDestroy(this)
            .Subscribe(diff =>
        {
            if (diff.x > 0 && diff.y > 0)
            {
                var v = Mathf.Max(diff.x / Screen.width, diff.y / Screen.height);
                var scale = _target.localScale + Vector3.one * v * 10;
                if (scale.x < 10)
                {
                    _target.localScale = scale;
                    Debug.Log($"pinch-out: diff={diff}, v={v}, localScale={_target.localScale}");
                }
            }
            else if (diff.x < 0 && diff.y < 0)
            {
                var v = Mathf.Min(diff.x / Screen.width, diff.y / Screen.height);
                var scale = _target.localScale + Vector3.one * v * 10;
                if (scale.x > 0.1)
                {
                    _target.localScale = scale;
                    Debug.Log($"pinch-in: diff={diff}, v={v}, localScale={_target.localScale}");
                }
            }
        }).AddTo(this);
    }
}
