using FrameworkCompatibility;
using FrameworkCompatibility.Data;
using System.Linq;

namespace FrameworkCompatibility
{
    public static class StateMachineFactory
    {
        public static StateMachine Create(string connectionString)
        {
            // Create the DbContext with the connection string
            var dbContext = new StateMachineDbContext(connectionString);

            var smtest = dbContext.States.ToList();
            // Instantiate the repository
            var repo = new StateMachineRepo(dbContext);

            // Return the StateMachine instance
            return new StateMachine(repo);
        }

        public static StateMachineManager<T> Create<T>(string connectionString)
        {
            var sm = StateMachineFactory.Create(connectionString);
            return new StateMachineManager<T>(sm);
        }

    }
}
