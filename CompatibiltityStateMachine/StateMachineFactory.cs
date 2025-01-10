using FundManagerStateMachine;
using FundManagerStateMachine.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace CompatibiltityStateMachine
{
    public static class StateMachineFactory
    {
        public static StateMachine Create(string connectionString)
        {
            // Build DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<StateMachineDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Create the DbContext
            var dbContext = new StateMachineDbContext(optionsBuilder.Options);

            var repo = new StateMachineRepo(dbContext);
            return new StateMachine(repo);
        }
    }
}
