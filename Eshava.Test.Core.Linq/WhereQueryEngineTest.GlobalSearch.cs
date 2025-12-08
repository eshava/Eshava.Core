using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eshava.Core.Linq;
using Eshava.Core.Linq.Attributes;
using Eshava.Core.Linq.Models;
using Eshava.Test.Core.Linq.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Core.Linq
{
	public partial class WhereQueryEngineTest
	{
		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermCaseInsensitiveOneTest()
		{
			// Arrange
			var propertyValue = "Darkwing Duck";
			var queryParameter = new QueryParameters
			{
				SearchTerm = propertyValue.ToLower()
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyValue);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermCaseInsensitiveTwoTest()
		{
			// Arrange
			var propertyValue = "Darkwing Duck";
			var queryParameter = new QueryParameters
			{
				SearchTerm = propertyValue
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyValue.ToLower());
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermCaseSensitiveOneTest()
		{
			// Arrange
			var propertyValue = "Darkwing Duck";
			var queryParameter = new QueryParameters
			{
				SearchTerm = propertyValue.ToLower()
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyValue);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermCaseSensitiveTwoTest()
		{
			// Arrange
			var propertyValue = "Darkwing Duck";
			var queryParameter = new QueryParameters
			{
				SearchTerm = propertyValue
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyValue.ToLower());
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		[DataRow(false, DisplayName = "Deactivated split option")]
		[DataRow(true, DisplayName = "Activated split option")]
		public void BuildQueryExpressionsGlobalSearchTermWithSplitContainsOptionTest(bool containsSearchSplitBySpace)
		{
			// Arrange
			var classUnderTest = new WhereQueryEngine(new WhereQueryEngineOptions
			{
				UseUtcDateTime = true,
				ContainsSearchSplitBySpace = containsSearchSplitBySpace
			});

			var propertyContent = "Darkwing Duck";
			var queryParameter = new QueryParameters
			{
				SearchTerm = "Dark Du"
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyContent);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			if (containsSearchSplitBySpace)
			{
				resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			}
			else
			{
				resultWhere.Should().HaveCount(0);
			}
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		[DataRow(false, DisplayName = "Deactivated split option")]
		[DataRow(true, DisplayName = "Activated split option")]
		public void BuildQueryExpressionsGlobalSearchTermWithSplitContainsOptionCaseInsensitiveOneTest(bool containsSearchSplitBySpace)
		{
			// Arrange
			var classUnderTest = new WhereQueryEngine(new WhereQueryEngineOptions
			{
				UseUtcDateTime = true,
				ContainsSearchSplitBySpace = containsSearchSplitBySpace
			});

			var propertyContent = "Darkwing Duck";
			var queryParameter = new QueryParameters
			{
				SearchTerm = "Dark Du".ToLower()
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyContent);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			if (containsSearchSplitBySpace)
			{
				resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			}
			else
			{
				resultWhere.Should().HaveCount(0);
			}
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		[DataRow(false, DisplayName = "Deactivated split option")]
		[DataRow(true, DisplayName = "Activated split option")]
		public void BuildQueryExpressionsGlobalSearchTermWithSplitContainsOptionCaseInsensitiveTwoTest(bool containsSearchSplitBySpace)
		{
			// Arrange
			var classUnderTest = new WhereQueryEngine(new WhereQueryEngineOptions
			{
				UseUtcDateTime = true,
				ContainsSearchSplitBySpace = containsSearchSplitBySpace
			});

			var propertyContent = "Darkwing Duck".ToLower();
			var queryParameter = new QueryParameters
			{
				SearchTerm = "Dark Du"
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyContent);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			if (containsSearchSplitBySpace)
			{
				resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			}
			else
			{
				resultWhere.Should().HaveCount(0);
			}
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermWithSplitContainsOptionCaseSensitiveOneTest()
		{
			// Arrange
			var classUnderTest = new WhereQueryEngine(new WhereQueryEngineOptions
			{
				UseUtcDateTime = true,
				ContainsSearchSplitBySpace = true
			});

			var propertyContent = "Darkwing Duck";
			var queryParameter = new QueryParameters
			{
				SearchTerm = "Dark Du".ToLower()
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyContent);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermWithSplitContainsOptionCaseSensitiveTwoTest()
		{
			// Arrange
			var classUnderTest = new WhereQueryEngine(new WhereQueryEngineOptions
			{
				UseUtcDateTime = true,
				ContainsSearchSplitBySpace = true
			});

			var propertyContent = "Darkwing Duck".ToLower();
			var queryParameter = new QueryParameters
			{
				SearchTerm = "Dark Du"
			};

			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyContent);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = classUnderTest.BuildQueryExpressions<Alpha>(queryParameter, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
			propertyCountQueryIgnore.Should().Be(1);
		}

	}
}
