using System.Collections.Generic;
using System.Linq;

namespace Neo.UI.Base.MVVM
{
    public class EventAggregator
    {
        public static readonly EventAggregator Current = new EventAggregator();

        // Prevent class from being instantiated outside of class
        private EventAggregator() { }

        private readonly List<IHandle> subscribers = new List<IHandle>();

        public void Subscribe(IHandle model)
        {
            this.subscribers.Add(model);
        }

        public void Unsubscribe(IHandle model)
        {
            this.subscribers.Remove(model);
        }

        public void Publish<T>(T message)
        {
            var messageSubscribers = this.subscribers.OfType<IHandle<T>>().ToList();

            foreach (var item in messageSubscribers)
            {
                item.Handle(message);
            }
        }
    }
}