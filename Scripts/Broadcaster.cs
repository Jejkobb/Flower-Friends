using System;
using System.Collections.Generic;
using System.Reflection;

public static class Broadcaster
{
    private class WeakAction
    {
        private readonly WeakReference _target;
        private readonly MethodInfo _methodInfo;

        public WeakAction(Delegate action)
        {
            _target = new WeakReference(action.Target);
            _methodInfo = action.Method;
        }

        public void Invoke(object param)
        {
            if (_target.IsAlive)
            {
                _methodInfo.Invoke(_target.Target, new[] { param });
            }
        }

        public bool IsAlive => _target.IsAlive;
    }

    private static Dictionary<string, List<WeakAction>> eventDictionary = new Dictionary<string, List<WeakAction>>();

    public static void Broadcast(string topic, object value)
    {
        if (eventDictionary.TryGetValue(topic, out var actions))
        {
            for (int i = actions.Count - 1; i >= 0; i--)
            {
                var weakAction = actions[i];
                if (weakAction.IsAlive)
                {
                    weakAction.Invoke(value);
                }
                else
                {
                    // Remove the action if the target object is dead
                    actions.RemoveAt(i);
                }
            }
        }
    }

    public static void AddListener(string topic, Action<object> listener)
    {
        if (!eventDictionary.TryGetValue(topic, out var actions))
        {
            actions = new List<WeakAction>();
            eventDictionary[topic] = actions;
        }

        actions.Add(new WeakAction(listener));
    }
}
