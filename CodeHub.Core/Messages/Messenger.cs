using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Core.Messages
{
    public static class Messenger
    {
        private static readonly LinkedList<WeakReference> subscriptions = new LinkedList<WeakReference>();

        public static void Publish<T>(T obj)
        {
            foreach (var s in subscriptions.ToList())
            {
                if (s.IsAlive)
                {
                    var t = s.Target as Action<T>;
                    t?.Invoke(obj);
                }
                else
                {
                    subscriptions.Remove(s);
                }
            }
        }

        public static IDisposable Subscribe<T>(Action<T> action)
        {
            var wr = new WeakReference(action);
            var d = new Subscription(action, () => subscriptions.Remove(wr));
            subscriptions.AddLast(wr);
            return d;
        }

        private class Subscription : IDisposable
        {
            private readonly object _obj;
            private readonly Action _action;

            public Subscription(object obj, Action action)
            {
                _obj = obj;
                _action = action;
            }

            public void Dispose()
            {
                _action?.Invoke();
            }
        }
    }
}

