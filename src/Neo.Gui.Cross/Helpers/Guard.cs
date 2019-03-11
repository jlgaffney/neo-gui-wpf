using System;
using System.Linq.Expressions;
using Neo.Gui.Cross.Attributes;

namespace Neo.Gui.Cross.Helpers
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
            var pe = (MemberExpression) argumentExpression.Body;
            var parameterName = pe.Member.Name;

            // delegate validation to overload
            ArgumentIsNotNull(argumentValue, parameterName);
        }
    }
}