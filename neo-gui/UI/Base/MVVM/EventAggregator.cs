using System;
using System.Collections.Generic;
using System.Linq;

//  PLEASE DON'T USE THIS OBJECT

namespace Neo.UI.Base.MVVM
{
    [Obsolete(
        "This class will be replaced and moved to Helpers projects in the solution. " +
        "This was not deleted yet in order to keep the code compiling. " +
        "When all the refereces are replaced, this class will the deleted.")]
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