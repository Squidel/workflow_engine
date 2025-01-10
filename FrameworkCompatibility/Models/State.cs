using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCompatibility.Models
{
    public class State
    {
        public int StateId { get; set; }
        public string Description { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class FilterCondition
    {
        public FilterCondition()
        {
            this.FilterConditions1 = new HashSet<FilterCondition>();
        }

        public int Id { get; set; }
        public string PropertyName { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public Nullable<int> ObjectTypeId { get; set; }
        public Nullable<int> ObjectId { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.Guid> EvaluatedLinkedId { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string LogicalOperator { get; set; }

        public int? TransitionId { get; set; }
        public virtual Transition Transition { get; set; }
        public virtual ICollection<FilterCondition> FilterConditions1 { get; set; }
        public virtual FilterCondition FilterCondition1 { get; set; }
    }

    public class Transition
    {
        public int TransitionId { get; set; }
        public State FromState { get; set; }
        public int FromStateId { get; set; }
        public State ToState { get; set; }
        public int ToStateId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public virtual ICollection<FilterCondition> FilterConditions { get; set; } = new HashSet<FilterCondition>();

    }
    public class Audit
    {
        public Audit()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string ColumnName { get; set; }
        public string OriginalValue { get; set; }
        public string NewValue { get; set; }
        public int? ModifiedById { get; set; }
        public int? CreatedById { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public int? Version { get; set; }
    }

}
