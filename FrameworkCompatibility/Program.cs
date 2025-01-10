﻿using FrameworkCompatibility;
using FrameworkCompatibility.Data;
using FrameworkCompatibility.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCompatibility
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sm = StateMachineFactory.Create("data source=FMDBDEV\\FMSQL;initial catalog=FundManager_UDC_Test;user id=delacon2;password=pa$$word;MultipleActiveResultSets=True;TrustServerCertificate=True");

            Console.WriteLine("StateMachine Created");
            //dynamic so = new ExpandoObject();
            //so.CorrectedAmount = 10000001;

            var so = new { CorrectedAmount = 10000000 - 1 };
            var test = sm.GetNextState(21, so);

            var entities = new StateMachineDbContext("data source=FMDBDEV\\FMSQL;initial catalog=FundManager_UDC_Test;user id=delacon2;password=pa$$word;MultipleActiveResultSets=True;TrustServerCertificate=True");
            var request = new State();
            request.StateId = 21;
            var all = new List<State>();
            sm.PerformTransition(entities, so, 6, request, all, (context, rec, nextState, related) =>
             {
                 Console.WriteLine("I've performed the action");
             });
            Console.WriteLine("Get Next State");
            //Console.WriteLine(test.Id.ToString());

            
        }
    }
   

}
