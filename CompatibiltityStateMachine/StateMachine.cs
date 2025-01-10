using CompatibiltityStateMachine.Interfaces;
using FundManagerStateMachine.Data;
using FundManagerStateMachine.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static FundManagerStateMachine.StateMachine;

namespace FundManagerStateMachine
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
        public void PerformTransition<TMain, TRelated>(object context, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
        {
            if (recordToTransition == null)
                throw new ArgumentNullException(nameof(recordToTransition));
            var transitionRecordProp = typeof(TMain).GetProperty("StateId");
            var relatedObjectProp = typeof(TRelated).GetProperty("StateId");
            var transistionRecordIdProp = recordToTransition.GetPrimaryKeyValue()

            int? recordId = transistionRecordIdProp.GetValue(recordToTransition) as int?;

            if (transitionRecordProp == null)
                throw new Exception("Record doesn't have StateId;  StateId is needed when updating records");
            if (relatedObjectProp == null)
                throw new Exception("Related records doesn't have a StateId property; StateId is needed when updating records");

            int? currentState = transitionRecordProp.GetValue(recordToTransition) as int?;
            var transition = GetNextState(currentState.HasValue?currentState.Value:0);

            var newContext = new ContextWrapper(context);

            customAction?.Invoke(newContext, recordToTransition, transition.ToStateId, relatedObjects);
            _repo.LogAuditAsync("RollbackState", recordId.HasValue ? recordId.Value : 0, "StateId", transition.FromStateId.ToString(), transition.ToStateId.ToString(), 38, "Admin");
        }
        #endregion
    }

}
