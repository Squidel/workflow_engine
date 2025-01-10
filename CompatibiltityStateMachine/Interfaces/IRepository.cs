using FundManagerStateMachine.Models;
using FundManagerStateMachine.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompatibiltityStateMachine.Interfaces
{
    public interface IRepository
    {
        void AddTransition(TransitionDTO dto);
        Transition GetNextState(int currentStateId);
        void AddFilterConditions(FilterDTO dto);
        void AddState(StateDTO dto);
        List<TransitionDTO> GetTransitionHistoryAsync(int recordId, string tableName);
        void LogAuditAsync(string tableName,int recordId,string columnName,string originalValue,string newValue,int userId,string username);
    }
}
