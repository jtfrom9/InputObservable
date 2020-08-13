using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using InputObservable;

public class GyroCameraController : MonoBehaviour
{
    public Text text;
    public Button resetButton;

    void Start()
    {
        // Cursor.visible = false;

        var gyro = new GyroInputObservable(this);

        gyro.EulerAngles.Subscribe(e =>
        {
            transform.rotation = Quaternion.Euler(e);
            text.text = $"Pos: {transform.position}, Rot: {transform.rotation.eulerAngles}";
        }).AddTo(this);

        // reset gyro rotation
        resetButton.OnClickAsObservable()
            .Subscribe(_ => { gyro.Reset(); })
            .AddTo(this);

        // Screen Touch (or Mouse) to gyro input emulation
        this.DefaultInputObservable(0, EventSystem.current)
            .ToEulerAngle(new Vector2 { x = -90, y = -90 })
            .Subscribe(rot => { gyro.AddRotate(rot); })
            .AddTo(this);
    }
}
