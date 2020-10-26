using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PoC.Reborn.EfCore
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<TTarget, bool>> Convert<TSource, TTarget>(this Expression<Func<TSource, bool>> root)
        {
            var visitor = new ParameterTypeVisitor<TSource, TTarget>();
            return (Expression<Func<TTarget, bool>>)visitor.Visit(root);
        }

        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>()
                                                               where TEntity : class
        {
            Expression<Func<TEntity, bool>> expression = null;

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                expression = e => !EF.Property<bool>(e, "IsDeleted");

            if (typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> multiTenantFilterExpression = e => EF.Property<Guid>(e, "TenantId") == Guider.GetTesterGuid();
                expression = expression == null ? multiTenantFilterExpression : CombineExpressions(expression, multiTenantFilterExpression);
            }

            return expression;
        }

        protected static Expression<Func<T, bool>> CombineExpressions<T>(this Expression<Func<T, bool>> expression1,
                                                                          Expression<Func<T, bool>> expression2) where T : class
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expression1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expression1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expression2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expression2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                {
                    return _newValue;
                }

                return base.Visit(node);
            }
        }


        class ParameterTypeVisitor<TSource, TTarget> : ExpressionVisitor
        {
            private ReadOnlyCollection<ParameterExpression> _parameters;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameters?.FirstOrDefault(p => p.Name == node.Name) ??
                                (node.Type == typeof(TSource) ? Expression.Parameter(typeof(TTarget), node.Name) : node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");
                return Expression.Lambda(Visit(node.Body), _parameters);
            }
        }
    }
}
