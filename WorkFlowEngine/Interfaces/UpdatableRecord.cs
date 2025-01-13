using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundManagerStateMachine.Interfaces
{
    public interface UpdatableRecord
    {
        public int StateId { get; set; }
    }
}
