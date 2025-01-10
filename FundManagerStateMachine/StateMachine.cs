using FundManagerStateMachine.Data;
using FundManagerStateMachine.Interfaces;
using FundManagerStateMachine.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FundManagerStateMachine.StateMachine;

namespace FundManagerStateMachine
{
    public class StateMachine
    {
        private readonly StateMachineRepo _repo;
        public delegate void OnTransactionComplete<T, TRelated>(DbContext pContext, T record, int nextStateId, IEnumerable<TRelated> relatedObjects);

        #region Constructors
        /// <summary>
        /// Default constructor. Initializes the repository using configuration from appsettings.json.
        /// </summary>
        public StateMachine()
            : this(BuildDefaultOptions())
        {
        }

        /// <summary>
        /// Constructor for providing custom DbContextOptions.
        /// </summary>
        /// <param name="options">The DbContextOptions for StateMachineDbContext.</param>
        public StateMachine(DbContextOptions<StateMachineDbContext> options)
        {
            _repo = new StateMachineRepo(options);
        }

        /// <summary>
        /// Constructor for injecting an existing repository instance.
        /// </summary>
        /// <param name="repo">An instance of StateMachineRepo.</param>
        public StateMachine(StateMachineRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }
        /// <summary>
        /// Constructor for getting the connection string from the calling project's configuration.
        /// </summary>
        /// <param name="configuration">The configuration object from the calling project.</param>
        public StateMachine(IConfiguration configuration)
            : this(BuildOptionsFromConfiguration(configuration))
        {
        }
        /// <summary>
        /// Constructor for providing a connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public StateMachine(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StateMachineDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            _repo = new StateMachineRepo(optionsBuilder.Options);
        }

        /// <summary>
        /// Builds default DbContextOptions using appsettings.json.
        /// </summary>
        private static DbContextOptions<StateMachineDbContext> BuildDefaultOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StateMachineDbContext>();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Ensure appsettings.json file is located
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("FMConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'FMConnection' is not configured in appsettings.json.");
            }

            optionsBuilder.UseSqlServer(connectionString);
            return optionsBuilder.Options;
        }



        /// <summary>
        /// Builds DbContextOptions using configuration from the calling project.
        /// </summary>
        private static DbContextOptions<StateMachineDbContext> BuildOptionsFromConfiguration(IConfiguration configuration)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StateMachineDbContext>();

            var connectionString = configuration.GetConnectionString("FMConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'FMConnection' is not configured in the calling project's configuration.");
            }

            optionsBuilder.UseSqlServer(connectionString);
            return optionsBuilder.Options;
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
        public void PerformTransition<TMain, TRelated>(DbContext context, TMain recordToTransition, IEnumerable<TRelated> relatedObjects, OnTransactionComplete<TMain, TRelated> customAction) where TMain : class where TRelated : class
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


            customAction?.Invoke(context, recordToTransition, transition.ToStateId, relatedObjects);
            _repo.LogAuditAsync("RollbackState", recordId.HasValue ? recordId.Value : 0, "StateId", transition.FromStateId.ToString(), transition.ToStateId.ToString(), 38, "Admin");
        }
        #endregion
    }

}
