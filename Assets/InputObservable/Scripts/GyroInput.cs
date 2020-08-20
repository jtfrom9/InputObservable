using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using InputObservable;

namespace InputObservable
{
    public class GyroInputObservable: IGyroInputObservable, IDisposable
    {
        MonoBehaviour behaviour;
        bool initialized;

#if !UNITY_EDITOR
        Vector3 initialRotation;
#endif
        Vector3 extraRotation;
        Subject<Vector3> rotation = new Subject<Vector3>();

        public IObservable<Vector3> EulerAngles { get => rotation; }

        private Quaternion GyroToUnity(Quaternion q)
        {
            q.x *= -1;
            q.y *= -1;
            return Quaternion.Euler(90, 0, 0) * q;
        }

        IEnumerator Start()
        {
            Input.gyro.enabled = true;
            Debug.Log($"updateInterval={Input.gyro.updateInterval}, enabled={Input.gyro.enabled}");
            yield return new WaitForSeconds(Input.gyro.updateInterval);
            Reset();
            initialized = true;
        }

        void Update()
        {
            Vector3 current;

#if !UNITY_EDITOR
            current = GyroToUnity(Input.gyro.attitude).eulerAngles - this.initialRotation;
#else
            current = Vector3.zero;
#endif
            rotation.OnNext(current + this.extraRotation);
        }

        public void AddRotate(Vector3 rotate)
        {
            this.extraRotation += rotate;
        }

        public void Reset()
        {
#if !UNITY_EDITOR
            this.initialRotation = GyroToUnity(Input.gyro.attitude).eulerAngles;
#endif
            this.extraRotation = Vector3.zero;
        }

        public override string ToString()
        {
            return $"GyroInput()";
        }

        public GyroInputObservable(MonoBehaviour behaviour)
        {
            this.behaviour = behaviour;

            behaviour.UpdateAsObservable()
                .Where(_ => initialized)
                .Subscribe(_ => Update()).AddTo(behaviour);
            behaviour.OnDestroyAsObservable()
                .Subscribe(_ => Dispose())
                .AddTo(behaviour);

            behaviour.StartCoroutine(Start());
        }

        public void Dispose()
        {
            rotation.Dispose();
        }
    }
}

