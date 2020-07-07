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

        var gyro = new GyroController(this);

        gyro.EulerAngles.Subscribe(e =>
        {
            transform.rotation = Quaternion.Euler(e);
            text.text = $"Pos: {transform.position}, Rot: {transform.rotation.eulerAngles}";
        }).AddTo(this);

        resetButton.OnClickAsObservable().Subscribe(_ => { gyro.Reset(); }).AddTo(this);

        this.DefaultInputObservable(0, EventSystem.current)
        .AsRotate(90, 90)
        .Subscribe(rot =>
        {
            gyro.AddRotate(rot);
        })
        .AddTo(this);
    }
}
