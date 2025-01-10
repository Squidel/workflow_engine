using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundManagerStateMachine.Models.DTO
{
    public class TransitionDTO
    {
        public int Id { get; set; }
        public int FromStateId { get; set; }
        public int ToStateId { get; set; }
        public int CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        public DateTime? CreatedByDate { get; set; }
        public DateTime? ModifiedByDate { get; set; }
        public FilterDTO? Conditions { get; set; }
        public string Comments { get;set; }
    }

    public class FilterDTO
    {
        public int Id { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public bool IsActive { get; set; }
        public int? TransitionId { get; set; }
    }

    public class StateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedByDate { get; set; }
        public DateTime? ModifiedByDate { get; set; }
    }
}
