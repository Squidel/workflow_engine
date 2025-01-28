using FrameworkCompatibility.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrameworkCompatibility.StateMachine;

namespace FrameworkCompatibility
{
    public class StateMachineManager<T>
    {
        private readonly StateMachine _stateMachine; // Existing StateMachine class
        private readonly TriggerManager<T> _triggerManager;

        public StateMachineManager(StateMachine stateMachine)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _triggerManager = new TriggerManager<T>();
        }

        public TransitionBuilder DefineTransition()
        {
            return _stateMachine.DefineTransition();
        }

        // Add triggers
        public StateMachineManager<T> AddTrigger(Func<T, bool> condition, Action<T> action)
        {
            _triggerManager.AddTrigger(condition, action);
            return this;
        }

        // Evaluate triggers
        public void EvaluateTriggers(T obj)
        {
            _triggerManager.EvaluateTriggers(obj);
        }

        // Delegate state management to the StateMachine
        public bool HasNext(int currentState, out bool isError)
        {
           return _stateMachine.HasNextState(currentState, out isError);
        }

        public TransitionDTO GetNextState(int currentState)
        {
            return _stateMachine.GetNextState(currentState);
        }

        public TransitionDTO GetNextState(int currentState, object evaluateO)
        {
            return _stateMachine.GetNextState(currentState, evaluateO);
        }

        public TransitionDTO GetLastState(int recordType, object objectToBeEvaluated)
        {
            return _stateMachine.GetLastState(recordType, objectToBeEvaluated);
        }
        public ResponseDTO AddTransition(TransitionRequest transitionRequest)
        {
            return _stateMachine.AddTransition(transitionRequest);
        }
        public bool EvaluateRecord<TE>(TE record, int recordType)
        {
            return _stateMachine.EvaluateRecord(record, recordType);
        }
        public void PerformTransition<TMain, TRelated>(object context, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
        {
            _stateMachine.PerformTransition(context, recordToTransition, relatedObjects, customAction);
        }
        public void PerformTransition<TMain, TRelated>(object context, object objToEval, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
        {
            _stateMachine.PerformTransition(context, objToEval, recordToTransition, relatedObjects, customAction);
        }
        public void PerformTransition<TMain, TRelated>(object context, object objToEval, int recordType, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
        {
            _stateMachine.PerformTransition(context, objToEval, recordType, recordToTransition, relatedObjects, customAction);
        }

        ///TODO: add logic to clear triggers
        public void ClearTriggers()
        {
            throw new NotImplementedException();
        }

        /// TODO: Add ability to enable logging
        public void EnableLogging()
        {
            throw new NotImplementedException();
        }


    }

}
