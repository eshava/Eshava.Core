using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.Core.Linq.Enums;

namespace Eshava.Core.Linq.Models
{
	internal class ExpressionDataContainer
	{
		public List<PropertyInfo> PropertyInfos { get; set; }
		public MemberExpression Member { get; set; }
		public ConstantExpression ConstantValue { get; set; }
		public ConstantExpression ConstantValueForNull { get; set; }
		public ParameterExpression Parameter { get; set; }
		public CompareOperator Operator { get; set; }
		public WhereQueryEngineOptions Options { get; set; }
	}
}
