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
        private readonly List<string> _logs = new List<string>();

        public TriggerManager<T> AddTrigger(Func<T, bool> condition, Action<T> action)
        {
            _triggers.Add(new Trigger<T>(condition, action));
            return this;
        }
        public TriggerManager<T> EvaluateTriggers(T obj)
        {
            StringBuilder logMessage = new StringBuilder();
            foreach (var trigger in _triggers)
            {
                logMessage.AppendLine($"About to evaluate condition: {trigger.Condition.Method.Name}");
                if (trigger.Condition(obj))
                {
                    logMessage.AppendLine($"  | trigger condition evaluated to true; about to execute {trigger.Action.Method.Name}");
                    trigger.Action(obj);
                    logMessage.AppendLine($"  | executed {trigger.Action.Method.Name}");
                }
                _logs.Add(logMessage.ToString());
            }
            return this;
        }

        public List<string> GetLogs() => _logs;
    }
}
