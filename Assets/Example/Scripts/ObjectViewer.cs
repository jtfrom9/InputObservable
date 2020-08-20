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
        _rigidbody.angularDrag = 1;
    }

    void Start()
    {
        var context = this.DefaultInputContext();
        var touch0 = context.GetObservable(0);
        var touch1 = context.GetObservable(1);

        touch0.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);
        touch1.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);

        // Stop Rotation
        touch0.Begin.Subscribe(_ =>
        {
            _rigidbody.angularVelocity = _rigidbody.angularVelocity * 0.1f;
        }).AddTo(this);

        // Rotate by Touch Operation
        touch0.ToEulerAngle(new Vector2 { x = -180, y = -180 })
            .Where(_ => !touch1.Began)
            .Subscribe(rot =>
            {
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
                    Debug.Log($"magnitude = {mag}");
                    _rigidbody.AddTorque(average.ToEulerAngle(new Vector2 { x = -180, y = -180 }) * (720 * mag));
                    // tween = target.DORotateQuaternion(
                    //     // target.rotation.eulerAngles + new Vector3(rotx, roty, 0),
                    //     target.rotation * Quaternion.Euler(rotx,roty,0),
                    //     mag)
                    //     // RotateMode.Fast)
                    //         .SetEase(Ease.OutExpo)
                    //         .OnComplete(() => Debug.Log("<color=red>Complete</color>"));
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
