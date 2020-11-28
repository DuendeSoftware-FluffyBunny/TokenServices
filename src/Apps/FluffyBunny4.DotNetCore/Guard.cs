using System;
using System.Collections.Generic;

namespace FluffyBunny4.DotNetCore
{
    /// <summary>
    ///     Guard Class
    /// </summary>
    public static class Guard
    {
        public static void ArguementEvaluate(string argumentName, Func<(bool success, string message)> condition)
        {
            var eval = condition.Invoke();
            if (!eval.success)
            {
                throw new ArgumentException(eval.message, argumentName);
            }
        }
        public static void ArgumentNotNull(string argumentName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
        public static void NotNull(string argumentName, object value)
        {
            if (value == null)
            {
                throw new NullReferenceException(argumentName);
            }
        }
        public static void ArgumentNotNullOrEmpty(string argumentName, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value cannot be an empty string.", argumentName);
            }
        }

        // Use IReadOnlyCollection<T> instead of IEnumerable<T> to discourage double enumeration
        public static void ArgumentNotNullOrEmpty<T>(string argumentName, IReadOnlyCollection<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            if (items.Count == 0)
            {
                throw new ArgumentException("Collection must contain at least one item.", argumentName);
            }
        }

        public static void ArgumentValid(bool valid, string argumentName, string exceptionMessage)
        {
            if (!valid)
            {
                throw new ArgumentException(exceptionMessage, argumentName);
            }
        }

        public static void OperationValid(bool valid, string exceptionMessage)
        {
            if (!valid)
            {
                throw new InvalidOperationException(exceptionMessage);
            }
        }
        /// <summary>
        ///     Test for a null value.
        /// </summary>
        /// <typeparam name="TData">Type of source.</typeparam>
        /// <param name="source">Source to test.</param>
        /// <param name="parameterName">Name of parameter.</param>
        /// <exception cref="ArgumentNullException">Exception thrown if null is found.</exception>
        public static void ForNull<TData>(TData source, string parameterName)
        {
            if (source == null)
                throw new ArgumentNullException(parameterName, ErrorMessages.ParameterNull
                    .Replace(Constants.Placeholders.ParameterName, parameterName));
        }

        /// <summary>
        ///     Test for null or default value.
        /// </summary>
        /// <typeparam name="TData">Type of source.</typeparam>
        /// <param name="source">Source to test.</param>
        /// <param name="parameterName">Name of parameter.</param>
        /// <exception cref="ArgumentNullOrDefaultException">Exception thrown if null or default value is found.</exception>
        public static void ForNullOrDefault<TData>(TData source, string parameterName)
        {
            if (source == null || Equals(source, default(TData)))
                throw new ArgumentNullOrDefaultException(parameterName, ErrorMessages.ParameterNullOrDefault
                    .Replace(Constants.Placeholders.ParameterName, parameterName));
        }

        /// <summary>
        ///     Test for value less than minimum value.
        /// </summary>
        /// <param name="sourceValue">Source to test.</param>
        /// <param name="minimumValue">Minimum value.</param>
        /// <param name="parameterName">Name of parameter.</param>
        /// <exception cref="ArgumentException">Exception thrown if value is less than minimum value.</exception>
        public static void ForValueLessThan(int sourceValue, int minimumValue, string parameterName)
        {
            if (sourceValue < minimumValue)
                throw new ArgumentException(
                    ErrorMessages.ValueLessThan.Replace(Constants.Placeholders.ParameterName, parameterName)
                        .Replace(Constants.Placeholders.MinimumValue, minimumValue.ToString()), parameterName);
        }
    }
}
