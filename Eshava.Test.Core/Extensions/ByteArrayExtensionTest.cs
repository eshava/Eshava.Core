using System;
using Eshava.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Core.Extensions
{
	[TestClass, TestCategory("Core.Extensions")]
	public class ByteArrayExtensionTest
	{
		[TestInitialize]
		public void Setup()
		{

		}

		[TestMethod]
		public void DecompressStringWithEmptyArrayTest()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				// Arrange
				var source = Array.Empty<byte>();

				// Act
				source.DecompressString();
			});
		}

		[TestMethod]
		public void DecompressStringWithNullArrayTest()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				// Arrange
				byte[] source = null;

				// Act
				source.DecompressString();
			});
		}
	}
}