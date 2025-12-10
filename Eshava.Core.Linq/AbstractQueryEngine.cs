using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Eshava.Core.Linq
{
	public abstract class AbstractQueryEngine
	{
		protected MemberExpression GetMemberExpression<T>(Expression<Func<T, object>> funcExpression) where T : class
		{
			MemberExpression memberExpression = null;

			if (funcExpression.Body is UnaryExpression expBodyMemberExpression && expBodyMemberExpression.Operand is MemberExpression)
			{
				memberExpression = (MemberExpression)expBodyMemberExpression.Operand;
			}
			else if (funcExpression.Body is UnaryExpression expBodyBinaryExpression && expBodyBinaryExpression.Operand is BinaryExpression)
			{
				throw new NotSupportedException("Logical binary expressions are not supported");
			}
			else if (funcExpression.Body is MemberExpression expression)
			{
				memberExpression = expression;
			}
			
			return memberExpression;
		}

		

		protected void BuildPropertyChain(int index, string[] propertyParts, IEnumerable<PropertyInfo> propertyInfos, List<PropertyInfo> propertyInfoChain)
		{
			var propertyInfoPart = propertyInfos.SingleOrDefault(p => p.Name.Equals(propertyParts[index]));
			if (propertyInfoPart is null)
			{
				propertyInfoChain.Clear();

				return;
			}

			propertyInfoChain.Add(propertyInfoPart);

			index++;

			if (index < propertyParts.Length)
			{
				BuildPropertyChain(index, propertyParts, propertyInfoPart.PropertyType.GetProperties(), propertyInfoChain);
			}
		}
	}
}