using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCompatibility
{
    public class Trigger<T>
    {
        public Func<T, bool>Condition { get; }
        public Action<T> Action { get; }
        public Trigger(Func<T, bool> condition, Action<T> action)
        {
            this.Condition = condition??throw new ArgumentNullException(nameof(condition));
            this.Action = action ?? throw new ArgumentNullException(nameof(action));
        }
    }
}
