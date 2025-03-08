﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Constants;
using Eshava.Core.Linq.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Linq.Models;

namespace Eshava.Core.Linq
{
	public class TransformQueryEngine : ITransformQueryEngine
	{
		/// <summary>
		/// Transforms an expression from source to target data type
		/// </summary>
		/// <typeparam name="Source"></typeparam>
		/// <typeparam name="Target"></typeparam>
		/// <param name="expression"></param>
		/// <param name="ignoreMappings"></param>
		/// <returns></returns>
		/// 
		public Expression<Func<Target, bool>> Transform<Source, Target>(Expression<Func<Source, bool>> expression)
		{
			return Transform<Source, Target>(expression, false);
		}

		/// <summary>
		/// For test purpose only
		/// </summary>
		public Expression<Func<Target, bool>> Transform<Source, Target>(Expression<Func<Source, bool>> expression, bool ignoreMappings)
		{
			var targetType = typeof(Target);
			var sourceType = typeof(Source);

			var mappings = ignoreMappings
				? default(IMappingExpression)
				: MappingStore.Mappings.FirstOrDefault(m => m.SourceType == sourceType && m.TargetType == targetType);

			var parameterExpression = Expression.Parameter(targetType, "p");

			var result = ProcessExpression<Target>(expression.Body, mappings, parameterExpression);

			return Expression.Lambda<Func<Target, bool>>(result, parameterExpression);
		}

		public (MemberExpression Member, ParameterExpression Parameter) TransformMemberExpression<Source, Target>(MemberExpression memberExpression)
		{
			return TransformMemberExpression<Source, Target>(memberExpression, false);
		}

		/// <summary>
		/// For test purpose only
		/// </summary>
		public (MemberExpression Member, ParameterExpression Parameter) TransformMemberExpression<Source, Target>(MemberExpression memberExpression, bool ignoreMappings)
		{
			var targetType = typeof(Target);
			var sourceType = typeof(Source);

			var mappings = ignoreMappings
				? default
				: MappingStore.Mappings.FirstOrDefault(m => m.SourceType == sourceType && m.TargetType == targetType);

			var parameterExpression = Expression.Parameter(targetType, "p");

			var result = ProcessExpression<Target>(memberExpression, mappings, parameterExpression) as MemberExpression;

			return (result, parameterExpression);
		}

		private Expression ProcessExpression<Target>(Expression expression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var unaryExpression = expression as UnaryExpression;
			if (unaryExpression != default && expression.NodeType == ExpressionType.Not)
			{
				return ProcessUnaryExpressionNot<Target>(unaryExpression, mappingExpression, parameterExpression);
			}

			if (unaryExpression != default && expression.NodeType == ExpressionType.Convert)
			{
				return ProcessUnaryExpressionConvert<Target>(unaryExpression, mappingExpression, parameterExpression);
			}

			var binaryExpression = expression as BinaryExpression;
			if (binaryExpression != default)
			{
				return ProcessBinaryExpression<Target>(binaryExpression, mappingExpression, parameterExpression);
			}

			var memberExpression = expression as MemberExpression;
			if (memberExpression != default && expression.NodeType == ExpressionType.MemberAccess)
			{
				if (memberExpression.Expression == default)
				{
					return ProcessExpressionlessMemberExpression<Target>(memberExpression, mappingExpression, parameterExpression);
				}

				if (EndAsDisplayClassConstantExpression<Target>(memberExpression, new List<string>(), out var constantValue, mappingExpression, parameterExpression))
				{
					return constantValue;
				}

				return ProcessMemberExpression<Target>(memberExpression, mappingExpression, parameterExpression);
			}

			var constantExpression = expression as ConstantExpression;
			if (expression.NodeType == ExpressionType.Constant)
			{
				return constantExpression;
			}

			var methodCallExpression = expression as MethodCallExpression;
			if (methodCallExpression != default)
			{
				return ProcessMethodCallExpression<Target>(methodCallExpression, mappingExpression, parameterExpression);
			}

			if (expression.NodeType == ExpressionType.Parameter)
			{
				return expression.ChangeParameterExpression(parameterExpression.First(p => p.Type == expression.Type));
			}

			return expression;
		}

		private bool EndAsDisplayClassConstantExpression<Target>(Expression expression, List<string> nestedProperties, out Expression constantExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var memberExpression = expression as MemberExpression;
			if (memberExpression != default && expression.NodeType == ExpressionType.MemberAccess)
			{
				nestedProperties.Add(memberExpression.Member.Name);

				if (memberExpression.Expression.NodeType == ExpressionType.Constant)
				{
					constantExpression = ProcessDisplayClassConstantExpression<Target>(memberExpression, nestedProperties, mappingExpression, parameterExpression);

					return true;
				}

				return EndAsDisplayClassConstantExpression<Target>(memberExpression.Expression, nestedProperties, out constantExpression, mappingExpression, parameterExpression);
			}

			constantExpression = null;

			return false;
		}

		private UnaryExpression ProcessUnaryExpressionNot<Target>(UnaryExpression unaryExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var expressionResult = ProcessExpression<Target>(unaryExpression.Operand, mappingExpression, parameterExpression);

			return Expression.Not(expressionResult);
		}

		private Expression ProcessUnaryExpressionConvert<Target>(UnaryExpression unaryExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			return ProcessExpression<Target>(unaryExpression.Operand, mappingExpression, parameterExpression);
		}

		private Expression ProcessBinaryExpression<Target>(BinaryExpression binaryExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var left = ProcessExpression<Target>(binaryExpression.Left, mappingExpression, parameterExpression);
			var right = ProcessExpression<Target>(binaryExpression.Right, mappingExpression, parameterExpression);

			if (left.NodeType == ExpressionType.MemberAccess && left is MemberExpression)
			{
				right = CheckAndTransformExpressionDataType(right, left as MemberExpression);
			}

			if (right.NodeType == ExpressionType.MemberAccess && right is MemberExpression)
			{
				left = CheckAndTransformExpressionDataType(left, right as MemberExpression);
			}

			switch (binaryExpression.NodeType)
			{
				case ExpressionType.And:

					return Expression.And(left, right);

				case ExpressionType.AndAlso:

					return Expression.AndAlso(left, right);

				case ExpressionType.Equal:

					return Expression.Equal(left, right);

				case ExpressionType.GreaterThan:

					return Expression.GreaterThan(left, right);

				case ExpressionType.GreaterThanOrEqual:

					return Expression.GreaterThanOrEqual(left, right);

				case ExpressionType.LessThan:

					return Expression.LessThan(left, right);

				case ExpressionType.LessThanOrEqual:

					return Expression.LessThanOrEqual(left, right);

				case ExpressionType.NotEqual:

					return Expression.NotEqual(left, right);

				case ExpressionType.Or:

					return Expression.Or(left, right);

				case ExpressionType.OrElse:

					return Expression.OrElse(left, right);
			}

			return null;
		}

		private Expression ProcessExpressionlessMemberExpression<Target>(MemberExpression memberExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			if (memberExpression.Type == TypeConstants.DateTime)
			{
				(var value, var type) = GetValueFromDisplayClass(memberExpression.Member, null, null);
				var newConstantExpression = Expression.Constant(value, type);

				return ProcessExpression<Target>(newConstantExpression, mappingExpression, parameterExpression);
			}

			return memberExpression;
		}

		private Expression ProcessDisplayClassConstantExpression<Target>(MemberExpression memberExpression, List<string> nestedProperties, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var constantExpression = memberExpression.Expression as ConstantExpression;
			if (constantExpression.Value == default)
			{
				return constantExpression;
			}

			(var value, var type) = GetValueFromDisplayClass(memberExpression.Member, nestedProperties, constantExpression);
			constantExpression = Expression.Constant(value, type);

			return ProcessExpression<Target>(constantExpression, mappingExpression, parameterExpression);
		}

		private Expression ProcessMemberExpression<Target>(MemberExpression memberExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var targetType = typeof(Target);
			var mapping = mappingExpression?.GetMapping(memberExpression, parameterExpression.First(p => p.Type == targetType)) ?? (false, null);
			if (mapping.HasMapping)
			{
				return mapping.Expression;
			}

			var property = memberExpression.Member.Name;
			if (memberExpression.Expression is ParameterExpression)
			{
				var targetPropertyInfo = targetType.GetProperties().FirstOrDefault(p => p.Name == property);

				return Expression.MakeMemberAccess(parameterExpression.First(p => p.Type == targetType), targetPropertyInfo);
			}

			if (memberExpression.Expression is MemberExpression)
			{
				var parentMemberExpression = memberExpression.Expression as MemberExpression;
				var parent = ProcessMemberExpression<Target>(parentMemberExpression, mappingExpression, parameterExpression);
				var targetPropertyInfo = default(PropertyInfo);

				if (memberExpression.Expression.Type.IsDataTypeNullable())
				{
					if (parent.Type.IsDataTypeNullable())
					{
						targetPropertyInfo = parent.Type.GetProperties().FirstOrDefault(p => p.Name == property);
					}
					else
					{
						return parent;
					}
				}
				else
				{
					targetPropertyInfo = parent.Type.GetProperties().FirstOrDefault(p => p.Name == property);
				}

				return Expression.MakeMemberAccess(parent, targetPropertyInfo);
			}

			return memberExpression;
		}

		private Expression ProcessMethodCallExpression<Target>(MethodCallExpression methodCallExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var method = methodCallExpression.Method.Name.ToLower();
			if (method == MethodInfoConstants.ANY)
			{
				return ProcessMethodCallExpressionAny<Target>(methodCallExpression, mappingExpression, parameterExpression);
			}

			if (method == MethodInfoConstants.CONTAINS && methodCallExpression.Arguments.Count == 2)
			{
				return ProcessMethodCallExpressionContainedIn<Target>(methodCallExpression, mappingExpression, parameterExpression);
			}

			if (method == MethodInfoConstants.COMPARETO)
			{
				return ProcessMethodCallExpressionCompareTo<Target>(methodCallExpression, mappingExpression, parameterExpression);
			}

			if (method == MethodInfoConstants.PARSE)
			{
				return ProcessMethodCallExpressionParse<Target>(methodCallExpression, mappingExpression, parameterExpression);
			}

			if (method == MethodInfoConstants.TOLOWER)
			{
				return ProcessMethodCallExpressionToLower<Target>(methodCallExpression, mappingExpression, parameterExpression);
			}

			if (method == MethodInfoConstants.TOUPPER)
			{
				return ProcessMethodCallExpressionToUpper<Target>(methodCallExpression, mappingExpression, parameterExpression);
			}

			var memberExpression = ProcessExpression<Target>(methodCallExpression.Object, mappingExpression, parameterExpression);
			var valueExpression = ProcessExpression<Target>(methodCallExpression.Arguments.First(), mappingExpression, parameterExpression);

			return Expression.Call(memberExpression, methodCallExpression.Method, valueExpression);
		}

		private Expression ProcessMethodCallExpressionContainedIn<Target>(MethodCallExpression methodCallExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			//DisplayClass
			var valueExpression = ProcessExpression<Target>(methodCallExpression.Arguments.First(), mappingExpression, parameterExpression);
			var memberExpression = ProcessExpression<Target>(methodCallExpression.Arguments.Last(), mappingExpression, parameterExpression);

			if ((valueExpression.Type.GetDataTypeFromIEnumerable().IsDataTypeNullable() && !memberExpression.Type.IsDataTypeNullable())
				|| (!valueExpression.Type.GetDataTypeFromIEnumerable().IsDataTypeNullable() && memberExpression.Type.IsDataTypeNullable()))
			{
				valueExpression = ConvertArrayValuesToTargetDataType(valueExpression, memberExpression.Type);
			}

			return Expression.Call(MethodInfoConstants.GetGenericContains(memberExpression.Type), valueExpression, memberExpression);
		}

		private Expression ProcessMethodCallExpressionCompareTo<Target>(MethodCallExpression methodCallExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			//DisplayClass
			var valueExpression = ProcessExpression<Target>(methodCallExpression.Arguments.First(), mappingExpression, parameterExpression);
			var memberExpression = ProcessExpression<Target>(methodCallExpression.Object, mappingExpression, parameterExpression);

			valueExpression = AddValueAccessToNullableProperty(valueExpression);
			memberExpression = AddValueAccessToNullableProperty(memberExpression);

			return Expression.Call(valueExpression, MethodInfoConstants.GetCompareTo(memberExpression.Type), memberExpression);
		}

		private Expression ProcessMethodCallExpressionToLower<Target>(MethodCallExpression methodCallExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var memberExpression = ProcessExpression<Target>(methodCallExpression.Object, mappingExpression, parameterExpression);
			memberExpression = AddValueAccessToNullableProperty(memberExpression);

			return Expression.Call(memberExpression, MethodInfoConstants.StringToLower);
		}

		private Expression ProcessMethodCallExpressionToUpper<Target>(MethodCallExpression methodCallExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			var memberExpression = ProcessExpression<Target>(methodCallExpression.Object, mappingExpression, parameterExpression);
			memberExpression = AddValueAccessToNullableProperty(memberExpression);

			return Expression.Call(memberExpression, MethodInfoConstants.StringToUpper);
		}

		private Expression ProcessMethodCallExpressionParse<Target>(MethodCallExpression methodCallExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			//DisplayClass
			var valueExpression = ProcessExpression<Target>(methodCallExpression.Arguments.First(), mappingExpression, parameterExpression) as ConstantExpression;
			object parsedValue;

			if (methodCallExpression.Type == TypeConstants.Guid)
			{
				parsedValue = Guid.Parse(valueExpression.Value as string);
			}
			else
			{
				return null;
			}

			return Expression.Constant(parsedValue, methodCallExpression.Type);
		}

		private Expression ProcessMethodCallExpressionAny<Target>(MethodCallExpression methodCallExpression, IMappingExpression mappingExpression, params ParameterExpression[] parameterExpression)
		{
			// Display Class
			var memberExpression = methodCallExpression.Arguments.First() as MemberExpression;
			var constantExpression = memberExpression.Expression as ConstantExpression;

			var innerType = memberExpression.Type.GetDataTypeFromIEnumerable();
			var innerParameterExpression = Expression.Parameter(innerType, "p" + DateTime.UtcNow.Millisecond);

			var parameterExpressions = new List<ParameterExpression>(parameterExpression)
			{
				innerParameterExpression
			};

			// Inner function expression
			var lambdaExpression = methodCallExpression.Arguments.Last() as LambdaExpression;
			var lambdaAsQuery = ProcessExpression<Target>(lambdaExpression.Body, mappingExpression, parameterExpressions.ToArray());
			lambdaExpression = Expression.Lambda(lambdaAsQuery, innerParameterExpression);

			(var valueArray, var typeArray) = GetValueFromDisplayClass(memberExpression.Member, null, constantExpression);
			constantExpression = Expression.Constant(valueArray, typeArray);

			var method = MethodInfoConstants.GetGenericAny(innerType);

			return Expression.Call(method, constantExpression, lambdaExpression);
		}

		private Expression AddValueAccessToNullableProperty(Expression expression)
		{
			if (expression.NodeType == ExpressionType.MemberAccess && expression is MemberExpression && expression.Type.IsDataTypeNullable())
			{
				var targetPropertyInfo = expression.Type.GetProperties().FirstOrDefault(p => p.Name == "Value");
				expression = Expression.MakeMemberAccess(expression, targetPropertyInfo);
			}

			return expression;
		}

		private Expression CheckAndTransformExpressionDataType(Expression expression, MemberExpression memberExpression)
		{
			if (expression is ConstantExpression)
			{
				var c = expression as ConstantExpression;

				if (expression.Type.GetDataType() != memberExpression.Type.GetDataType())
				{
					object convertedValue;
					if (memberExpression.Type.GetDataType().IsEnum && expression.Type.GetDataType() == TypeConstants.Int)
					{
						convertedValue = Enum.Parse(memberExpression.Type.GetDataType(), c.Value?.ToString() ?? "");
					}
					else
					{
						convertedValue = Convert.ChangeType(c.Value, memberExpression.Type, CultureInfo.InvariantCulture);
					}

					expression = Expression.Constant(convertedValue, memberExpression.Type);
				}
				else if (memberExpression.Type.IsDataTypeNullable())
				{
					expression = Expression.Constant(c.Value, memberExpression.Type);
				}
				else if (expression.Type.IsDataTypeNullable() && !memberExpression.Type.IsDataTypeNullable())
				{
					expression = Expression.Constant(c.Value, memberExpression.Type);
				}
			}
			else if (expression is ParameterExpression)
			{
				var p = expression as ParameterExpression;
				if (memberExpression.Type.IsDataTypeNullable() && !expression.Type.IsDataTypeNullable())
				{
					expression = p.ToNullableType();
				}
				else if (!memberExpression.Type.IsDataTypeNullable() && expression.Type.IsDataTypeNullable())
				{
					expression = p.FromNullableType();
				}
			}

			return expression;
		}

		private Expression ConvertArrayValuesToTargetDataType(Expression valueExpression, Type targetType)
		{
			var childsToAdd = ConvertArray(valueExpression as ConstantExpression, targetType);
			var arrayType = targetType.MakeArrayType();

			return Expression.Constant(childsToAdd, arrayType);
		}

		private Array ConvertArray(ConstantExpression constantExpression, Type targetDataType)
		{
			var valueArray = (IEnumerable)constantExpression.Value;
			var arrayLength = 0;
			foreach (var newItem in valueArray)
			{
				if (newItem == null && !targetDataType.IsDataTypeNullable())
				{
					continue;
				}

				arrayLength++;
			}

			var targetArray = Array.CreateInstance(targetDataType, arrayLength);
			var index = 0;
			foreach (var newItem in valueArray)
			{
				if (newItem == null && !targetDataType.IsDataTypeNullable())
				{
					continue;
				}

				if (targetDataType.IsDataTypeNullable() || newItem == null)
				{
					targetArray.SetValue(newItem, index);
				}
				else
				{
					targetArray.SetValue(Convert.ChangeType(newItem, targetDataType, CultureInfo.InvariantCulture), index);
				}

				index++;
			}

			return targetArray;
		}

		private (object Value, Type Type) GetValueFromDisplayClass(MemberInfo memberInfo, List<string> nestedProperties, ConstantExpression constantExpression)
		{
			var value = default(object);
			var type = default(Type);

			if (memberInfo.MemberType == MemberTypes.Field)
			{
				value = ((FieldInfo)memberInfo).GetValue(constantExpression?.Value);
				type = ((FieldInfo)memberInfo).FieldType;
			}
			else if (memberInfo.MemberType == MemberTypes.Property)
			{
				value = ((PropertyInfo)memberInfo).GetValue(constantExpression?.Value);
				type = ((PropertyInfo)memberInfo).PropertyType;
			}

			return GetValueFromObject(value, type, nestedProperties);
		}

		private (object Value, Type Type) GetValueFromObject(object value, Type type, List<string> nestedProperties)
		{
			if ((nestedProperties?.Count ?? 0) == 0)
			{
				return (value, type);
			}

			for (var i = (nestedProperties.Count - 2); i >= 0; i--)
			{
				var propertyInfo = type.GetProperty(nestedProperties[i]);
				type = propertyInfo.PropertyType;

				if (value is not null)
				{
					value = propertyInfo.GetValue(value);
				}
			}

			return (value, type);
		}
	}
}