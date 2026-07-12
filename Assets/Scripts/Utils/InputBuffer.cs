using System;
using UnityEngine;

namespace Decerno.Utils
{
    [Serializable]
    public class InputBuffer : InputBuffer<bool>
    {
        public void Set() => Set(true);
        public void TryConsume() => TryConsume(out _);
    }
    [Serializable]
    public class IntInputBuffer : InputBuffer<int>
    { }
    [Serializable]
    public class FloatInputBuffer : InputBuffer<float>
    { }
    [Serializable]
    public class Vector2InputBuffer : InputBuffer<Vector2>
    { }
    [Serializable]
    public class InputBuffer<T>
    {
        [SerializeField] private float bufferTime = 0.2f;
        [SerializeField] private T bufferedValue;

        float expireTime = float.NegativeInfinity;
        bool isBuffered = false;

        public bool IsBuffered => isBuffered && Time.time < expireTime;
        public float HoldTime => expireTime;
        public T Value => bufferedValue;

        public void Set(T value)
        {
            bufferedValue = value;
            isBuffered = true;
            expireTime = Time.time + bufferTime;
        }
        public void Consume()
        {
            isBuffered = false;
            expireTime = float.NegativeInfinity;
            bufferedValue = default;
        }
        public bool TryConsume(out T value) 
        {
            if (!IsBuffered) {
                value = default;
                return false;
            }
            value = bufferedValue;
            Consume();
            return true;
        }
        public void SetBufferTime(float seconds)
        {
            bufferTime = Math.Max(seconds, 0f);
        }
    }

}
