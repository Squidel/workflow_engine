using CompatibiltityStateMachine.Interfaces;
using FundManagerStateMachine.Data;
using FundManagerStateMachine.Models;
using FundManagerStateMachine.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FundManagerStateMachine
{
    public class StateMachineRepo : IRepository
    {
        //private readonly Db_internalContextOptions<StateMachineDb_internalContext> _options;
        private DbContext _internalContext;

        //public StateMachineRepo(Db_internalContextOptions<StateMachineDb_internalContext> options)
        //{
        //    _options = options;
        //    EnsureTablesExist();
        //}

        public StateMachineRepo(DbContext _internalContext)
        {
            //var optionsBuilder = new Db_internalContextOptionsBuilder<StateMachineDb_internalContext>();
            //optionsBuilder.UseSqlServer(connectionString);
            //_options = optionsBuilder.Options;
            _internalContext = _internalContext;
            EnsureTablesExist();
        }

        private void EnsureTablesExist()
        {

                if (!_internalContext.Database.CanConnect())
                {
                    _internalContext.Database.EnsureCreated(); // Creates the database and tables if they don't exist
                }
            
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
                _internalContext.Set<Transition>().Add(transition);
                if (_internalContext.SaveChanges() > 0)
                {
                    if (dto.Conditions != null)
                    {
                        dto.Conditions.TransitionId = transition.TransitionId;
                        AddFilterConditions(dto.Conditions);
                    }
                }
            
        }

        public Transition GetNextState(int currentStateId)
        {
            if (currentStateId == 0)
                throw new InvalidOperationException("Current state id cannot be null");


                return _internalContext.Set<Transition>().Where(t => t.FromStateId == currentStateId).FirstOrDefault(t => t.FilterConditions.All(x => t.Evaluate(x)));
            
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
    int recordId,
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
                    EventType = 'T',
                    TableName = tableName,
                    RecordId = recordId.ToString(),
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
