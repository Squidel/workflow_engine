 var stateMachine = StateMachineFactory.Create("data source=FMDBDEV\\FMSQL;initial catalog=FundManager_UDC_Test;user id=delacon2;password=pa$$word;MultipleActiveResultSets=True;TrustServerCertificate=True");
                var test = stateMachine.GetNextState(21);
                stateMachine.PerformTransition(entities, request, all, (context, rec, nextState, related) =>
                {
                    rec.StateId = nextState;

                    foreach(var e in related)
                    {
                        e.RequestCode = "it wrk";
                    }

                    context.SaveChanges();
                });