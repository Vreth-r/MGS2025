using System;

namespace Event
{
    public class EventDispatcher<T>
    {
        private event Action<T> Action;

        public void CallEvent(T data)
        {
            Action?.Invoke(data);
        }

        public void AddEventListener(Action<T> action)
        {
            this.Action += action;
        }

        public void RemoveEventListener(Action<T> action)
        {
            this.Action -= action;
        }

    }
}