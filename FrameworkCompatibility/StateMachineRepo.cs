using FrameworkCompatibility.Data;
using FrameworkCompatibility.Interfaces;
using FrameworkCompatibility.Models;
using FrameworkCompatibility.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrameworkCompatibility
{
    public class StateMachineRepo : IRepository
    {
        //private readonly Db_internalContextOptions<StateMachineDb_internalContext> _options;
        private StateMachineDbContext _internalContext;

        //public StateMachineRepo(Db_internalContextOptions<StateMachineDb_internalContext> options)
        //{
        //    _options = options;
        //    EnsureTablesExist();
        //}

        public StateMachineRepo(StateMachineDbContext internalContext)
        {
            //var optionsBuilder = new Db_internalContextOptionsBuilder<StateMachineDb_internalContext>();
            //optionsBuilder.UseSqlServer(connectionString);
            //_options = optionsBuilder.Options;
            _internalContext = internalContext;
            var test = _internalContext.States.ToList();
            var another = _internalContext.Transitions.ToList();
            var con = _internalContext.FilterConditions.ToList();
        }

        public void AddTransition(TransitionDTO dto)
        {

            var transition = new Transition
            {
                FromStateId = dto.FromStateId,
                ToStateId = dto.ToStateId,
                CreatedBy = dto.CreatedById,
                ModifiedBy = dto.ModifiedById.HasValue ? dto.ModifiedById.Value : 0,
                ModifiedDate = dto.ModifiedByDate
            };
            _internalContext.Transitions.Add(transition);
            if (_internalContext.SaveChanges() > 0)
            {
                if (dto.Conditions != null)
                {
                    foreach(var con in dto.Conditions)
                    {
                        con.TransitionId = transition.TransitionId;
                        AddFilterConditions(con);
                    }
                    
                }
            }

        }
        public bool HasNextState(int currentStateId)
        {
            bool response = false;
            if (currentStateId == 0)
                throw new InvalidOperationException("Current state id cannot be null");
            response = _internalContext.States.Any(x => x.StateId == currentStateId);
            return response;
        }
        public Transition GetNextState(int currentStateId)
        {
            if (currentStateId == 0)
                throw new InvalidOperationException("Current state id cannot be null");

            return _internalContext.Transitions.Where(t => t.FromStateId == currentStateId).FirstOrDefault();
            //return _internalContext.Transitions.Where(t => t.FromStateId == currentStateId).ToList().FirstOrDefault(t => t.FilterConditions.All(x => t.Evaluate(x)));

        }
        public Transition GetNextState(int currentStateId, object objectToBeEvaluated)
        {
            if (currentStateId <= 0)
                throw new InvalidOperationException("Current state id cannot be null");

            // First stage: Query the database
            var transitions = _internalContext.Transitions
                .Include("FilterConditions")
                .Where(t => t.FromStateId == currentStateId)
                .ToList();  // Materialize the query

            // Second stage: Apply custom evaluation in-memory
            return transitions.FirstOrDefault(t => t.FilterConditions.All(fc => objectToBeEvaluated.Evaluate(fc)));
        }
        public Transition GetPreviousState(int currentStateId, int recordId)
        {
            return GetTransitionHistoryAsync(recordId, "RollbackState").Select(dto => new Transition
            {
                TransitionId = dto.Id,
                FromStateId = dto.FromStateId,
                ToStateId = dto.ToStateId,
                CreatedBy = dto.CreatedById,
                CreatedDate = dto.CreatedByDate ?? DateTime.Now,  // Default to now if null
                ModifiedBy = dto.ModifiedById ?? 0,  // Default to 0 if null
                ModifiedDate = dto.ModifiedByDate,
                FilterConditions = dto.Conditions?.Select(c => new FilterCondition
                {
                    Id = c.Id,
                    PropertyName = c.PropertyName,
                    Operator = c.Operator,
                    Value = c.Value,
                    LogicalOperator = c.LogicalOperator
                }).ToList()
            }).LastOrDefault();
        }
        /// <summary>
        /// Get's the Last transition that meets criteria
        /// </summary>
        /// <param name="objectToBeEvaluated"></param>
        /// <param name="recordType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Transition GetEvaluatedTransitionByRecordType(object objectToBeEvaluated, int recordType)
        {
            if (objectToBeEvaluated== null)
                throw new InvalidOperationException("Evaluated Object cannot be null");

            try
            {
                // First stage: Query the database
                var test = _internalContext.Transitions.Include("FilterConditions").ToList();
                var transitions = _internalContext.Transitions
                    .Include("FilterConditions")
                    .Where(t => t.FilterConditions.Any(x => x.ObjectTypeId.HasValue && x.ObjectTypeId.Value == recordType))
                    .ToList();  // Materialize the query

                // Second stage: Apply custom evaluation in-memory
                return transitions.FirstOrDefault(t => t.FilterConditions.All(fc => objectToBeEvaluated.Evaluate(fc)));
            }
            catch (Exception ex)
            {

                throw;
            }
            return null;
        }

        public List<FilterCondition> GetFilterConditionsByRecordType(int recordType)
        {
            List<FilterCondition> response = new List<FilterCondition>();
            response = _internalContext.FilterConditions.Where(x => x.ObjectTypeId == recordType).ToList();
            return response;
        }
        public void AddFilterConditions(FilterDTO dto)
        {

            var filter = new FilterCondition
            {
                PropertyName = dto.PropertyName,
                Operator = dto.Operator,
                Value = dto.Value,
                ObjectId = dto.ObjectId,
                ObjectTypeId = dto.ObjectTypeId,
                IsActive = dto.IsActive,
                TransitionId = dto.TransitionId
            };
            _internalContext.Set<FilterCondition>().Add(filter);
            _internalContext.SaveChanges();

        }

        public void AddState(StateDTO dto)
        {

            var state = new State
            {
                StateId = dto.Id,
                Description = dto.Name,
                CreatedBy = dto.CreatedBy,
                CreatedDate = dto.CreatedByDate,
                ModifiedBy = dto.ModifiedBy,
                ModifiedDate = dto.ModifiedByDate
            };
            _internalContext.Set<State>().Add(state);
            _internalContext.SaveChanges();

        }

        public List<TransitionDTO> GetTransitionHistoryAsync(int recordId, string tableName)
        {

            List<TransitionDTO> transitions = new List<TransitionDTO>();

            int fromState = 0; // Assuming OriginalValue is the FromState
            int toState = 0;     // Assuming NewValue is the ToState
            int user = 0;        // Username who performed the transition
            transitions = _internalContext.Set<Audit>()
            .Where(a => a.RecordId == recordId.ToString() && a.TableName == tableName)
            .OrderBy(a => a.EventDate)
            .Select(entry => new TransitionDTO
            {
                FromStateId = int.TryParse(entry.OriginalValue, out fromState) ? fromState : 0,
                ToStateId = int.TryParse(entry.NewValue, out toState) ? toState : 0,
                CreatedById = int.TryParse(entry.Username, out user) ? user : 0,
                CreatedByDate = entry.EventDate,
                Comments = $"Transitioned by {entry.Username} on {entry.EventDate:yyyy-MM-dd HH:mm}"
            })
            .ToList();


            return transitions;

        }

        public void LogAuditAsync(
    string tableName,
    object recordId,
    string columnName,
    string originalValue,
    string newValue,
    int userId,
    string username)
        {

            var audit = new Audit
            {
                UserId = userId,
                Username = username,
                EventDate = DateTime.UtcNow,
                EventType = "T",
                TableName = tableName,
                RecordId = recordId.ToString() ?? string.Empty,
                ColumnName = columnName,
                OriginalValue = originalValue,
                NewValue = newValue,
                CreatedById = userId,
                ModifiedById = userId,
                ModifiedDateTime = DateTime.UtcNow,
                Version = 1 // Increment version logic can be added here.
            };

            _internalContext.Set<Audit>().Add(audit);
            _internalContext.SaveChanges();

        }

    }
}
