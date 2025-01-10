using FrameworkCompatibility.Models;
using FrameworkCompatibility.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCompatibility.Interfaces
{
    public interface IRepository
    {
        void AddTransition(TransitionDTO dto);

        bool HasNextState(int currentStateId);
        Transition GetNextState(int currentStateId);
        Transition GetNextState(int currentStateId, object objectToBeEvaluated);
        Transition GetEvaluatedTransitionByRecordType(object objectToBeEvaluated, int recordType);
        void AddFilterConditions(FilterDTO dto);
        void AddState(StateDTO dto);
        List<TransitionDTO> GetTransitionHistoryAsync(int recordId, string tableName);
        void LogAuditAsync(string tableName,object recordId,string columnName,string originalValue,string newValue,int userId,string username);
        List<FilterCondition> GetFilterConditionsByRecordType(int recordType);
    }
}
