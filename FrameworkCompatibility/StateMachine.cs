using FrameworkCompatibility.Interfaces;
using FrameworkCompatibility.Data;
using FrameworkCompatibility.Models.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static FrameworkCompatibility.StateMachine;
using System.ComponentModel.DataAnnotations;
using FrameworkCompatibility.Helpers;
using System.Diagnostics.SymbolStore;

namespace FrameworkCompatibility
{
    public class StateMachine
    {
        private readonly IRepository _repo;
        public delegate void OnTransactionComplete<T, TRelated>(ContextWrapper pContext, T record, int nextStateId, IEnumerable<TRelated> relatedObjects);

        #region Constructors

        /// <summary>
        /// Constructor for providing custom DbContextOptions.
        /// </summary>
        static StateMachine()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        /// <summary>
        /// Constructor for injecting an existing repository instance.
        /// </summary>
        /// <param name="repo">An instance of StateMachineRepo.</param>
        public StateMachine(IRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);

            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            return null;
        }

        #endregion
        #region Methods

        //TODO: add methods for getting all possible transitions
        public bool HasNextState(int currentState, out bool isError)
        {
            isError = false;
            bool hasNextState = false;
            try
            {
                hasNextState = _repo.HasNextState(currentState);
            }
            catch (Exception e)
            {
                isError = true;
                hasNextState = false;
            }
            return hasNextState;
        }
        public TransitionDTO GetNextState(int currentState)
        {
            var response = new TransitionDTO();
            try
            {
                var transition = _repo.GetNextState(currentState);

                return response = new TransitionDTO
                {
                    Id = transition.TransitionId,
                    FromStateId = transition.FromStateId,
                    ToStateId = transition.ToStateId,
                    CreatedById = transition.CreatedBy,
                    CreatedByDate = transition.CreatedDate,
                    ModifiedByDate = transition.ModifiedDate,
                    ModifiedById = transition.ModifiedBy,
                };
            }
            catch(InvalidOperationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;

        }

        public TransitionDTO GetNextState(int currentState, object evaluateO)
        {
            var response = new TransitionDTO();
            try
            {
                var transition = _repo.GetNextState(currentState, evaluateO);

                return response = new TransitionDTO
                {
                    Id = transition.TransitionId,
                    FromStateId = transition.FromStateId,
                    ToStateId = transition.ToStateId,
                    CreatedById = transition.CreatedBy,
                    CreatedByDate = transition.CreatedDate,
                    ModifiedByDate = transition.ModifiedDate,
                    ModifiedById = transition.ModifiedBy,
                };
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                response = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response = null;
            }
            return response;

        }

        public TransitionDTO GetLastState(int recordType, object objectToBeEvaluated)
        {
            var response = new TransitionDTO();
            try
            {
                var transition = _repo.GetEvaluatedTransitionByRecordType(objectToBeEvaluated, recordType);

                return response = new TransitionDTO
                {
                    Id = transition.TransitionId,
                    FromStateId = transition.FromStateId,
                    ToStateId = transition.ToStateId,
                    CreatedById = transition.CreatedBy,
                    CreatedByDate = transition.CreatedDate,
                    ModifiedByDate = transition.ModifiedDate,
                    ModifiedById = transition.ModifiedBy,
                };
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response = null;
            }
            return response;

        }

        public ResponseDTO AddTransition(TransitionRequest transitionRequest)
        {
            ResponseDTO response = new ResponseDTO();
            try
            {
                foreach (var item in transitionRequest.Transitions)
                {
                    item.Conditions.ForEach(condition => { condition.ObjectTypeId = transitionRequest.RequestTypeId; });
                    _repo.AddTransition(item);
                }
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                response.IsError = true;
                response.Message = ex.Message;
            }
            return response;
            }

        /// <summary>
        /// returns a boolean value if the record passes ANY of the conditions set for it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <param name="recordType"></param>
        /// <returns></returns>
        public bool EvaluateRecord<T>(T record, int recordType)
        {
            var conditions = _repo.GetFilterConditionsByRecordType( recordType);
            return conditions.Any(x => record.Evaluate(x));
        }
        public void PerformTransition<TMain, TRelated>(object context, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
        {
            if (recordToTransition == null)
                throw new ArgumentNullException(nameof(recordToTransition));
            var transitionRecordProp = typeof(TMain).GetProperty("StateId");
            var relatedObjectProp = typeof(TRelated).GetProperty("StateId");
            var recordId = recordToTransition.GetPrimaryKeyValue();
            if (transitionRecordProp == null)
                throw new Exception("Record doesn't have StateId;  StateId is needed when updating records");
            if (relatedObjectProp == null)
                throw new Exception("Related records doesn't have a StateId property; StateId is needed when updating records");

            int? currentState = transitionRecordProp.GetValue(recordToTransition) as int?;
            var transition = GetNextState(currentState.HasValue?currentState.Value:0);

            var newContext = new ContextWrapper(context);

            customAction?.Invoke(newContext, recordToTransition, transition.ToStateId, relatedObjects);
            _repo.LogAuditAsync("RollbackState", recordId != null? recordId:0, "StateId", transition.FromStateId.ToString(), transition.ToStateId.ToString(), 38, "Admin");
        }

        public void PerformTransition<TMain, TRelated>(object context, object objToEval, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
        {
            if (recordToTransition == null)
                throw new ArgumentNullException(nameof(recordToTransition));
            var transitionRecordProp = typeof(TMain).GetProperty("StateId");
            var relatedObjectProp = typeof(TRelated).GetProperty("StateId");
            var recordId = recordToTransition.GetPrimaryKeyValue();

            if (transitionRecordProp == null)
                throw new Exception("Record doesn't have StateId;  StateId is needed when updating records");
            if (relatedObjectProp == null)
                throw new Exception("Related records doesn't have a StateId property; StateId is needed when updating records");

            int? currentState = transitionRecordProp.GetValue(recordToTransition) as int?;
            var transition = GetNextState(currentState.HasValue ? currentState.Value : 0, objToEval);
            if (transition == null)
                throw new ArgumentNullException("No next state found that meets all criteria!");

            var newContext = new ContextWrapper(context);

            customAction?.Invoke(newContext, recordToTransition, transition.ToStateId, relatedObjects);
            _repo.LogAuditAsync("RollbackState", recordId != null? recordId:0, "StateId", transition.FromStateId.ToString(), transition.ToStateId.ToString(), 38, "Admin");
        }

        public void PerformTransition<TMain, TRelated>(object context, object objToEval, int recordType, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
        {
            if (recordToTransition == null)
                throw new ArgumentNullException(nameof(recordToTransition));
            var transitionRecordProp = typeof(TMain).GetProperty("StateId");
            var relatedObjectProp = typeof(TRelated).GetProperty("StateId");
            var recordId = recordToTransition.GetPrimaryKeyValue();
            if (transitionRecordProp == null)
                throw new Exception("Record doesn't have StateId;  StateId is needed when updating records");
            if (relatedObjectProp == null)
                throw new Exception("Related records doesn't have a StateId property; StateId is needed when updating records");

            int? currentState = transitionRecordProp.GetValue(recordToTransition) as int?;
            var lastTransition = GetLastState(recordType, objToEval);
            var unevaluatedTransition = GetNextState(currentState.HasValue ? currentState.Value : 0);
            if (lastTransition != null && unevaluatedTransition != null && lastTransition.Id == unevaluatedTransition.Id)
            {
                objToEval = MetaActions.SetProperty(objToEval, "IsLastMeeting", true);
            }
            var transition = GetNextState(currentState.HasValue ? currentState.Value : 0, objToEval);
            if (transition == null)
                throw new ArgumentNullException("No next state found that meets all criteria!");

            var newContext = new ContextWrapper(context);

            customAction?.Invoke(newContext, recordToTransition, transition.ToStateId, relatedObjects);
            _repo.LogAuditAsync("RollbackState", recordId != null? recordId:0, "StateId", transition.FromStateId.ToString(), transition.ToStateId.ToString(), 38, "Admin");
        }


        #endregion
    }

}
