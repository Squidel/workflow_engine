using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CompatibiltityStateMachine.Interfaces
{
    public interface IContextWrapper
    {
        bool SaveChanges();
        //bool UpdateEntity<T>(T entity);
    }

    public class ContextWrapper : IContextWrapper
    {
        private Action _save;
        private Action _update;
        private MethodInfo _updateMethod;
        private object _context;
        public ContextWrapper(object ctx)
        {
            _context = ctx;
            var saveChangesMethod = ctx.GetType().GetMethod("SaveChanges");
            _save = () => saveChangesMethod?.Invoke(ctx, null);

            _updateMethod = ctx.GetType().GetMethod("UpdateChanges");
        }

        //public bool UpdateEntity<T>(T entity)
        //{
        //    bool success = false;
        //    if(_updateMethod != null)
        //    {
        //        _updateMethod?.Invoke(_context, entity);
        //    }
        //    return success;
        //}

        public bool SaveChanges()
        {
            bool success = false;
            if (_context != null)
            {
               var saveChangesMethod = _context.GetType().GetMethod("SaveChanges");
               saveChangesMethod?.Invoke(_context, null);
                success = true;
            }
            return success;
        }
    }
}
