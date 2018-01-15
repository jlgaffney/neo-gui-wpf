using System;
using System.Linq.Expressions;

namespace Neo.UI.Core.Helpers
{
    public static class Guard
    {
        public static void ArgumentIsNotNull<T>([ValidatedNotNull] T argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void ArgumentIsNotNull<T>(T argumentValue, Expression<Func<T>> argumentExpression) where T : class
        {
            ArgumentIsNotNull(argumentExpression, "argumentExpression");

            // process the expression in order to get parameter name and value
            MemberExpression pe = (MemberExpression)argumentExpression.Body;
            string parameterName = pe.Member.Name;

            // delegate validation to overload
            ArgumentIsNotNull(argumentValue, parameterName);
        }
    }
}
