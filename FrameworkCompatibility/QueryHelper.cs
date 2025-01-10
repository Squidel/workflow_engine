using FrameworkCompatibility.Models;
using FrameworkCompatibility.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCompatibility
{
    public static class QueryHelper
    {
        public static IEnumerable<T> WhereBy<T>(this IEnumerable<T> list, IEnumerable<FilterDTO> conditions)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression expression = null;

            if (conditions == null && !conditions.Any())
            {
                return list;
            }

            foreach (var condition in conditions)
            {
                Expression property = parameter;

                foreach (var member in condition.PropertyName.Split('.'))
                {
                    property = Expression.Property(property, member);
                }
                ConstantExpression constant;
                try
                {
                    constant = Expression.Constant(Convert.ChangeType(condition.Value, property.Type));
                }
                catch (Exception ex)
                {
                    if (property.Type == typeof(bool))
                    {
                        // Handle decimal-to-boolean conversion: assuming 1 for true, 0 for false.
                        decimal decimalValue = Convert.ToDecimal(condition.Value);
                        constant = Expression.Constant(decimalValue == 1, typeof(bool));
                    }
                    else
                    {
                        constant = Expression.Constant(decimal.TryParse(condition.Value, out var decimalResult) ? (object)decimalResult : null);
                    }

                }

                BinaryExpression body;

                switch (condition.Operator)
                {
                    case "==":
                        body = Expression.Equal(property, constant);
                        break;
                    case "!=":
                        body = Expression.NotEqual(property, constant);
                        break;
                    case ">":
                        body = Expression.GreaterThan(property, constant);
                        break;
                    case "<":
                        body = Expression.LessThan(property, constant);
                        break;
                    case ">=":
                        body = Expression.GreaterThanOrEqual(property, constant);
                        break;
                    case "<=":
                        body = Expression.LessThanOrEqual(property, constant);
                        break;
                    default:
                        throw new NotSupportedException($"Operator {condition.Operator} is not supported");
                }
                if (expression == null)
                {
                    expression = body;
                }
                else
                {
                    switch (condition.Operator)
                    {
                        case "AND":
                            expression = Expression.AndAlso(expression, body);
                            break;
                        case "OR":
                            expression = Expression.OrElse(expression, body);
                            break;
                        default:
                            expression = Expression.OrElse(expression, body);
                            break;
                    }

                }
            }

            var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
            return list.AsQueryable().Where(lambda);
        }


        public static bool Evaluate<T>(this T obj, IEnumerable<FilterDTO> conditions, out string additionId)
        {
            bool response = true;
            additionId = string.Empty;
            foreach (var condition in conditions)
            {
                var propertyInfo = typeof(T).GetProperty(condition.PropertyName);
                if (propertyInfo == null)
                    throw new ArgumentException($"Property '{condition.PropertyName}' not found on type {typeof(T)}");

                var propertyValue = propertyInfo.GetValue(obj);

                bool conditionMet = true;

                object parsedValue;

                try
                {
                    // Attempt to parse the condition value based on the type of the property
                    parsedValue = Convert.ChangeType(condition.Value, propertyValue.GetType());
                }
                catch (Exception ex)
                {
                    if (propertyValue.GetType() == typeof(bool))
                    {
                        // Handle decimal-to-boolean conversion: assuming 1 for true, 0 for false.
                        decimal decimalValue = Convert.ToDecimal(condition.Value);
                        parsedValue = decimalValue == 1;
                    }
                    else
                    {
                        throw new ArgumentException($"Failed to parse value '{condition.Value}' to type '{propertyValue.GetType()}': {ex.Message}");
                    }

                }
                switch (condition.Operator)
                {
                    case "==":
                        if (!Equals(propertyValue, parsedValue)) conditionMet = false;
                        break;
                    case "!=":
                        if (Equals(propertyValue, parsedValue)) conditionMet = false;
                        break;
                    case ">":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) <= 0) conditionMet = false;
                        break;
                    case "<":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) >= 0) conditionMet = false;
                        break;
                    case ">=":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) < 0) conditionMet = false;
                        break;
                    case "<=":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) > 0) conditionMet = false;
                        break;
                    default:
                        throw new ArgumentException($"Operator '{condition.Operator}' not supported");
                }

                if (conditionMet)
                {
                    //additionId = condition.EvaluatedLinkedId.ToString(); not needed as we just need the true or false value

                }

                // Combine the result with the specified logical operator
                switch (condition.Operator)
                {
                    case "AND":
                        response = response && conditionMet;
                        break;
                    case "OR":
                        response = response || conditionMet;
                        break;
                    default:
                        response = response || conditionMet;
                        break;
                }

                // Short-circuit if result is determined for OR logic
                if (condition.Operator == "OR" && response) break;
            }
            return response;
        }

        public static bool Evaluate<T>(this T obj, FilterCondition condition)
        {
            bool response = true;
            try
            {
                object propertyValue = null;
                var propertyInfo = typeof(T).GetProperty(condition.PropertyName);
                if (propertyInfo == null)
                {
                    if (obj is IDictionary<string, object> asDict)
                    {
                        propertyValue = asDict.TryGetValue(condition.PropertyName, out var value) ? value : null;
                    }
                    else
                    {
                        propertyInfo = obj.GetType().GetProperty(condition.PropertyName);
                        if (propertyInfo == null)
                        {
                            throw new ArgumentException($"Property '{condition.PropertyName}' not found on type {typeof(T)}");
                        }
                        propertyValue = propertyInfo.GetValue(obj);
                    }
                }
                else
                {
                    propertyValue = propertyInfo.GetValue(obj);
                }

                bool conditionMet = true;

                object parsedValue;

                try
                {
                    // Attempt to parse the condition value based on the type of the property
                    parsedValue = Convert.ChangeType(condition.Value, propertyValue.GetType());
                }
                catch (Exception ex)
                {
                    if (propertyValue.GetType() == typeof(bool))
                    {
                        // Handle decimal-to-boolean conversion: assuming 1 for true, 0 for false.
                        decimal decimalValue = Convert.ToDecimal(condition.Value);
                        parsedValue = decimalValue == 1;
                    }
                    else
                    {
                        throw new ArgumentException($"Failed to parse value '{condition.Value}' to type '{propertyValue.GetType()}': {ex.Message}");
                    }

                }
                switch (condition.Operator)
                {
                    case "==":
                        if (!Equals(propertyValue, parsedValue)) conditionMet = false;
                        break;
                    case "!=":
                        if (Equals(propertyValue, parsedValue)) conditionMet = false;
                        break;
                    case ">":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) <= 0) conditionMet = false;
                        break;
                    case "<":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) >= 0) conditionMet = false;
                        break;
                    case ">=":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) < 0) conditionMet = false;
                        break;
                    case "<=":
                        if (Comparer<object>.Default.Compare(propertyValue, parsedValue) > 0) conditionMet = false;
                        break;
                    default:
                        throw new ArgumentException($"Operator '{condition.Operator}' not supported");
                }

                if (conditionMet)
                {
                    //additionId = condition.EvaluatedLinkedId.ToString(); not needed as we just need the true or false value

                }

                // Combine the result with the specified logical operator
                switch (condition.LogicalOperator)
                {
                    case "AND":
                        response = response && conditionMet;
                        break;
                    case "OR":
                        response = response || conditionMet;
                        break;
                    default:
                        response = response || conditionMet;
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                response = false;
            }

            return response;
        }
    }

}
