using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class SliderController : MonoBehaviour
{
    public Text stringText;
    public Text intervalText;
    public Slider slider;
    public Toggle toggle;

    ReactiveProperty<float> _value = new ReactiveProperty<float>();
    ReactiveProperty<bool> _enabled = new ReactiveProperty<bool>();

    void Start()
    {
        slider.OnValueChangedAsObservable().Subscribe(v =>
        {
            intervalText.text = $"{v} (ms)";
            _value.Value = v;
        }).AddTo(this);

        toggle.OnValueChangedAsObservable().SubscribeToInteractable(slider);

        string back = intervalText.text;
        toggle.OnValueChangedAsObservable().Subscribe(v => {
            if (!v)
            {
                back = intervalText.text;
                intervalText.text = "";
            } else {
                intervalText.text = back;
            }
            _enabled.Value = v;
        }).AddTo(this);
    }

    public IReadOnlyReactiveProperty<float> Value { get => _value; }
    public IReadOnlyReactiveProperty<bool> Enabled { get => _enabled; }
    public string Text { set => stringText.text = value; }
}
