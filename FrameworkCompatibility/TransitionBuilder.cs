using FrameworkCompatibility.Interfaces;
using FrameworkCompatibility.Models;
using FrameworkCompatibility.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCompatibility
{
    public class TransitionBuilder
    {
        private TransitionDTO _transition;
        private readonly IRepository _repo;
        public bool _isBuilt =false;

        public TransitionBuilder(IRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _transition = new TransitionDTO();
        }
        public TransitionBuilder FromState(int fromState) {
            _transition.FromStateId = fromState;
            return this;
        }
        public TransitionBuilder ToState(int toState)
        {
            _transition.ToStateId = toState;
            return this;
        }
        public TransitionBuilder WithCondition(FilterDTO filter)
        {
            _transition.Conditions.Add(filter);
            return this;
        }

        /// <summary>
        /// returns the built transition
        /// </summary>
        /// <returns></returns>
        public TransitionDTO Build()
        {
            if (_transition.FromStateId == 0 || _transition.ToStateId == 0)
            {
                throw new InvalidOperationException("Both FromStateId and ToStateId must be specified before building the transition.");
            }
            _isBuilt = true;
            return _transition;
        }
        /// <summary>
        /// saves the built transition to the database
        /// </summary>
        public void Save()
        {
            if (!_isBuilt)
            {
                throw new InvalidOperationException("The transition must be built using the Build() method before saving to the database.");
            }
            _repo.AddTransition(_transition);
        }
        public void Reset()
        {
            _isBuilt = false;
            _transition = new TransitionDTO();
        }
    }
}
