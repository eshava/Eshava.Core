using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.Core.Linq.Attributes;
using Eshava.Core.Linq.Enums;
using Eshava.Core.Linq.Models;
using Eshava.Test.Core.Linq.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Core.Linq
{
	public partial class WhereQueryEngineTest
	{
		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 3,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				Gamma = new SpecialFilterField
				{
					Operator = CompareOperator.Equal,
					SearchTerm = "Darkwing Duck"
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.Contains,
					SearchTerm = "QuackFu"
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectCaseInsensitiveOneTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 3,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				Gamma = new SpecialFilterField
				{
					Operator = CompareOperator.Equal,
					SearchTerm = "DARKWING DUCK"
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.Contains,
					SearchTerm = "QUACKFU"
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectCaseInsensitiveTwoTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QUACKFU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 2,
					Gamma = "DARKWING DUCK",
					Delta = "QUACKFU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 3,
					Gamma = "DARKWING DUCK",
					Delta = "KUNGFU"
				}
			};

			var filter = new FilterModel
			{
				Gamma = new SpecialFilterField
				{
					Operator = CompareOperator.Equal,
					SearchTerm = "Darkwing Duck"
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.Contains,
					SearchTerm = "QuackFu"
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectCaseSensitiveOneTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 3,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				Gamma = new SpecialFilterField
				{
					Operator = CompareOperator.Equal,
					SearchTerm = "DARKWING DUCK"
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.Contains,
					SearchTerm = "QUACKFU"
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectCaseSensitiveTwoTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QUACKFU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 2,
					Gamma = "DARKWING DUCK",
					Delta = "QUACKFU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 3,
					Gamma = "DARKWING DUCK",
					Delta = "KUNGFU"
				}
			};

			var filter = new FilterModel
			{
				Gamma = new SpecialFilterField
				{
					Operator = CompareOperator.Equal,
					SearchTerm = "Darkwing Duck"
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.Contains,
					SearchTerm = "QuackFu"
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectWithFilterGroupTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "Quack_Fu better than Kung-Fu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than Kung-Fu"
				},
				new() {
					Beta = 3,
					Gamma = "DD",
					Delta = "Quack_Fu better than KungFu"
				},
				new() {
					Beta = 4,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than KungFu"
				},
				new() {
					Beta = 5,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than Kung-Fu"
				},
				new() {
					Beta = 6,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				ComplexFilterField = new ComplexFilterField
				{
					Operator = CompareOperator.None,
					LinkOperator = LinkOperator.And,
					LinkOperations = new List<ComplexFilterField>
					{
						new() {
							Operator = CompareOperator.None,
							LinkOperator = LinkOperator.Or,
							LinkOperations = new List<ComplexFilterField>
							{
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack_Fu"
								},
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack-Fu"
								}
							}
						},
						new() {
							Field = "Delta",
							Operator = CompareOperator.ContainsNot,
							SearchTerm = "KungFu"
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(1);
			resultWhere.Last().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectWithFilterGroupCaseInsensitiveOneTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "Quack_Fu better than Kung-Fu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than Kung-Fu"
				},
				new() {
					Beta = 3,
					Gamma = "DD",
					Delta = "Quack_Fu better than KungFu"
				},
				new() {
					Beta = 4,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than KungFu"
				},
				new() {
					Beta = 5,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than Kung-Fu"
				},
				new() {
					Beta = 6,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				ComplexFilterField = new ComplexFilterField
				{
					Operator = CompareOperator.None,
					LinkOperator = LinkOperator.And,
					LinkOperations = new List<ComplexFilterField>
					{
						new() {
							Operator = CompareOperator.None,
							LinkOperator = LinkOperator.Or,
							LinkOperations = new List<ComplexFilterField>
							{
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "QUACK_FU"
								},
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "QUACK-FU"
								}
							}
						},
						new() {
							Field = "Delta",
							Operator = CompareOperator.ContainsNot,
							SearchTerm = "KUNGFU"
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(1);
			resultWhere.Last().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectWithFilterGroupCaseInsensitiveTwoTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QUACK_FU BETTER THAN KUNG-FU"
				},
				new() {
					Beta = 2,
					Gamma = "DARKWING DUCK",
					Delta = "QUACK-FU BETTER THAN KUNG-FU"
				},
				new() {
					Beta = 3,
					Gamma = "DD",
					Delta = "QUACK_FU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 4,
					Gamma = "DARKWING DUCK",
					Delta = "QUACK-FU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 5,
					Gamma = "DARKWING DUCK",
					Delta = "QUACKFU BETTER THAN KUNG-FU"
				},
				new() {
					Beta = 6,
					Gamma = "DARKWING DUCK",
					Delta = "KUNGFU"
				}
			};

			var filter = new FilterModel
			{
				ComplexFilterField = new ComplexFilterField
				{
					Operator = CompareOperator.None,
					LinkOperator = LinkOperator.And,
					LinkOperations = new List<ComplexFilterField>
					{
						new() {
							Operator = CompareOperator.None,
							LinkOperator = LinkOperator.Or,
							LinkOperations = new List<ComplexFilterField>
							{
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack_Fu"
								},
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack-Fu"
								}
							}
						},
						new() {
							Field = "Delta",
							Operator = CompareOperator.ContainsNot,
							SearchTerm = "KungFu"
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(1);
			resultWhere.Last().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectWithFilterGroupCaseSensitiveOneTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "Quack_Fu better than Kung-Fu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than Kung-Fu"
				},
				new() {
					Beta = 3,
					Gamma = "DD",
					Delta = "Quack_Fu better than KungFu"
				},
				new() {
					Beta = 4,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than KungFu"
				},
				new() {
					Beta = 5,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than Kung-Fu"
				},
				new() {
					Beta = 6,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				ComplexFilterField = new ComplexFilterField
				{
					Operator = CompareOperator.None,
					LinkOperator = LinkOperator.And,
					LinkOperations = new List<ComplexFilterField>
					{
						new() {
							Operator = CompareOperator.None,
							LinkOperator = LinkOperator.Or,
							LinkOperations = new List<ComplexFilterField>
							{
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "QUACK_FU"
								},
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "QUACK-FU"
								}
							}
						},
						new() {
							Field = "Delta",
							Operator = CompareOperator.ContainsNot,
							SearchTerm = "KUNGFU"
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectWithFilterGroupCaseSensitiveTwoTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QUACK_FU BETTER THAN KUNG-FU"
				},
				new() {
					Beta = 2,
					Gamma = "DARKWING DUCK",
					Delta = "QUACK-FU BETTER THAN KUNG-FU"
				},
				new() {
					Beta = 3,
					Gamma = "DD",
					Delta = "QUACK_FU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 4,
					Gamma = "DARKWING DUCK",
					Delta = "QUACK-FU BETTER THAN KUNGFU"
				},
				new() {
					Beta = 5,
					Gamma = "DARKWING DUCK",
					Delta = "QUACKFU BETTER THAN KUNG-FU"
				},
				new() {
					Beta = 6,
					Gamma = "DARKWING DUCK",
					Delta = "KUNGFU"
				}
			};

			var filter = new FilterModel
			{
				ComplexFilterField = new ComplexFilterField
				{
					Operator = CompareOperator.None,
					LinkOperator = LinkOperator.And,
					LinkOperations = new List<ComplexFilterField>
					{
						new() {
							Operator = CompareOperator.None,
							LinkOperator = LinkOperator.Or,
							LinkOperations = new List<ComplexFilterField>
							{
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack_Fu"
								},
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack-Fu"
								}
							}
						},
						new() {
							Field = "Delta",
							Operator = CompareOperator.ContainsNot,
							SearchTerm = "KungFu"
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectWithFilterGroupNotAllowedFilterFieldTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "Quack_Fu better than Kung-Fu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than Kung-Fu"
				},
				new() {
					Beta = 3,
					Gamma = "DD",
					Delta = "Quack_Fu better than KungFu"
				},
				new() {
					Beta = 4,
					Gamma = "Darkwing Duck",
					Delta = "Quack-Fu better than KungFu"
				},
				new() {
					Beta = 5,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than Kung-Fu"
				},
				new() {
					Beta = 6,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				ComplexFilterField = new ComplexFilterField
				{
					Operator = CompareOperator.None,
					LinkOperator = LinkOperator.And,
					LinkOperations = new List<ComplexFilterField>
					{
						new() {
							Operator = CompareOperator.None,
							LinkOperator = LinkOperator.Or,
							LinkOperations = new List<ComplexFilterField>
							{
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack_Fu"
								},
								new() {
									Field = "Delta",
									Operator = CompareOperator.Contains,
									SearchTerm = "Quack-Fu"
								}
							}
						},
						new() {
							Field = "Gamma",
							Operator = CompareOperator.ContainsNot,
							SearchTerm = "KungFu"
						}
					}
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null);

			// Assert
			result.IsFaulty.Should().BeTrue();
			result.Data.Should().BeNull();

			result.ValidationErrors.Should().HaveCount(1);
			result.ValidationErrors.First().PropertyName.Should().Be("Gamma");
			result.ValidationErrors.First().ErrorType.Should().Be("NotAllowed");
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByFilterObjectWithNotAllowedCompareOperationsTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Gamma = "DD",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than KungFu"
				},
				new() {
					Beta = 3,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var filter = new FilterModel
			{
				Gamma = new SpecialFilterField
				{
					Operator = CompareOperator.StartsWith,
					SearchTerm = "Darkwing Duck"
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.EndsWith,
					SearchTerm = "QuackFu"
				}
			};

			Expression<Func<Alpha, bool>> expectedResultGamma = p => p.Gamma == "Darkwing Duck";
			Expression<Func<Alpha, bool>> expectedResultDelta = p => p.Delta != null && p.Delta.Contains("QuackFu");

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null);

			// Assert
			result.IsFaulty.Should().BeTrue();
			result.Message.Should().Be("InvalidData");

			result.ValidationErrors.Should().HaveCount(2);
			result.ValidationErrors.First().PropertyName.Should().Be(nameof(FilterModel.Gamma));
			result.ValidationErrors.First().ErrorType.Should().Be("InvalidOperator");
			result.ValidationErrors.First().Value.Should().Be(CompareOperator.StartsWith.ToString());

			result.ValidationErrors.Last().PropertyName.Should().Be(nameof(FilterModel.Delta));
			result.ValidationErrors.Last().ErrorType.Should().Be("InvalidOperator");
			result.ValidationErrors.Last().Value.Should().Be(CompareOperator.EndsWith.ToString());
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermByFilterObjectTest()
		{
			// Arrange
			var filter = new FilterModel
			{
				SearchTerm = "Darkwing Duck"
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, filter.SearchTerm);
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, filter.SearchTerm);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermByFilterObjectCaseInsensitiveOneTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var propertyValue = "Darkwing Duck";
			var filter = new FilterModel
			{
				SearchTerm = propertyValue.ToUpper()
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
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, filter.SearchTerm, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermByFilterObjectCaseInsensitiveTwoTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = true
			};

			var propertyValue = "Darkwing Duck";
			var filter = new FilterModel
			{
				SearchTerm = propertyValue
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyValue.ToUpper());
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, filter.SearchTerm, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(properties.Count - propertyCountQueryIgnore);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermByFilterObjectCaseSensitiveOneTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var propertyValue = "Darkwing Duck";
			var filter = new FilterModel
			{
				SearchTerm = propertyValue.ToUpper()
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
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, filter.SearchTerm, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermByFilterObjectCaseSensitiveTwoTest()
		{
			// Arrange
			var options = new WhereQueryEngineOptions
			{
				CaseInsensitive = false
			};

			var propertyValue = "Darkwing Duck";
			var filter = new FilterModel
			{
				SearchTerm = propertyValue
			};

			var typeString = typeof(string);
			var properties = typeof(Alpha).GetProperties().Where(p => p.PropertyType == typeString && p.CanWrite).ToList();
			var propertyCountQueryIgnore = 0;

			var exampleList = new List<Alpha>();
			foreach (var propertyInfo in properties)
			{
				var alpha = new Alpha();
				propertyInfo.SetValue(alpha, propertyValue.ToUpper());
				exampleList.Add(alpha);

				if (propertyInfo.GetCustomAttribute<QueryIgnoreAttribute>() != null)
				{
					propertyCountQueryIgnore++;
				}
			}

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, filter.SearchTerm, options: options);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).ToList();
			resultWhere.Should().HaveCount(0);
			propertyCountQueryIgnore.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsPropertyMappingByFilterObjectTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Kappa = new Omega
					{
						Psi = ""
					}
				},
				new() {
					Beta = 2,
					Kappa = new Omega
					{
						Psi = null
					}
				},
				new() {
					Beta = 3,
					Kappa = new Omega
					{
						Psi = "Darkwing Duck"
					}
				},
				new() {
					Beta = 4,
					Kappa = new Omega
					{
						Psi = "QuackFu"
					}
				}
			};

			var mappings = new Dictionary<string, List<Expression<Func<Alpha, object>>>>
			{
				{ nameof(Alpha.Chi), new List<Expression<Func<Alpha, object>>> { p => p.Kappa.Psi } }
			};

			var filter = new FilterModel
			{
				Chi = new SpecialFilterField
				{
					Operator = CompareOperator.Equal,
					SearchTerm = "Darkwing Duck"
				}
			};

			Expression<Func<Alpha, bool>> expectedResultEqual = p => p.Kappa.Psi == "Darkwing Duck";

			// Act
			var result = _classUnderTest.BuildQueryExpressions(filter, null, mappings);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);
			result.Data.First().Should().BeEquivalentTo(expectedResultEqual);

			exampleList.Where(result.Data.First().Compile()).Single().Beta.Should().Be(3);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByNestedFilterObjectTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Delta = "QuackFu better than KungFu",
					Kappa = new Omega
					{
						Chi = "DD",
					}
				},
				new() {
					Beta = 2,
					Delta = "QuackFu better than KungFu",
					Kappa = new Omega
					{
						Chi = "Darkwing Duck",
					}
				},
				new() {
					Beta = 3,
					Delta = "KungFu",
					Kappa = new Omega
					{
						Chi = "Darkwing Duck",
					}
				}
			};

			var filter = new FilterModel
			{
				Kappa = new FilterModelOmega
				{
					Chi = new SpecialFilterField
					{
						Operator = CompareOperator.Equal,
						SearchTerm = "Darkwing Duck"
					}
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.Contains,
					SearchTerm = "QuackFu"
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyByNestedFilterObjectWithMappingTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new() {
					Beta = 1,
					Delta = "QuackFu better than KungFu",
					Kappa = new Omega
					{
						Chi = "DD",
					}
				},
				new() {
					Beta = 2,
					Delta = "QuackFu better than KungFu",
					Kappa = new Omega
					{
						Chi = "Darkwing Duck",
					}
				},
				new() {
					Beta = 3,
					Delta = "KungFu",
					Kappa = new Omega
					{
						Chi = "Darkwing Duck",
					}
				}
			};

			var filter = new FilterModel
			{
				Kappa = new FilterModelOmega
				{
					MappingNeeded = new SpecialFilterField
					{
						Operator = CompareOperator.Equal,
						SearchTerm = "Darkwing Duck"
					}
				},
				Delta = new FilterField
				{
					Operator = CompareOperator.Contains,
					SearchTerm = "QuackFu"
				}
			};

			var mappings = new Dictionary<string, List<Expression<Func<Alpha, object>>>>
			{
				{$"{nameof(FilterModel.Kappa)}.{ nameof(FilterModelOmega.MappingNeeded)}", [p => p.Kappa.Chi] }
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(filter, null, mappings: mappings);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);

			var resultWhere = exampleList.Where(result.Data.First().Compile()).Where(result.Data.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}
	}
}