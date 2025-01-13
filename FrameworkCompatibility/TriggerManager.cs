using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCompatibility
{
    public class TriggerManager<T>
    {
        private readonly List<Trigger<T>> _triggers = new List<Trigger<T>>();

        public TriggerManager<T> AddTrigger(Func<T, bool> condition, Action<T> action)
        {
            _triggers.Add(new Trigger<T>(condition, action));
            return this;
        }
        public void EvaluateTriggers(T obj)
        {
            foreach (var trigger in _triggers)
            {
                if (trigger.Condition(obj))
                {
                    trigger.Action(obj);
                }
            }
        }
    }
}
