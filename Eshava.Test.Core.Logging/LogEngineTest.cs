﻿using System;
using Eshava.Core.Logging;
using Eshava.Core.Logging.Interfaces;
using Eshava.Core.Logging.Models;
using Eshava.Test.Core.Logging.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eshava.Test.Core.Logging
{
	[TestClass, TestCategory("Core.Logging")]
	public class LogEngineTest
	{
		private LogEngine _classUnderTest;
		private ILogWriter _logWriterFake;

		[TestInitialize]
		public void Setup()
		{
			_logWriterFake = A.Fake<ILogWriter>();
			_classUnderTest = new LogEngine("DarkwingDuck", "1.0.0", LogLevel.Error, _logWriterFake, ReferenceLoopHandling.Ignore);
		}

		[DataTestMethod]
		[DataRow(LogLevel.Trace, false)]
		[DataRow(LogLevel.Debug, false)]
		[DataRow(LogLevel.Information, false)]
		[DataRow(LogLevel.Warning, false)]
		[DataRow(LogLevel.Error, true)]
		[DataRow(LogLevel.Critical, true)]
		public void IsEnabledTest(LogLevel logLevel, bool isEnabled)
		{
			// Act
			var result = _classUnderTest.IsEnabled(logLevel);

			// Assert
			result.Should().Be(isEnabled);
		}


		[TestMethod]
		public void LogTest()
		{
			// Arrange
			var eventId = new EventId(0, "MegaVolt");
			var alpha = new Alpha
			{
				Beta = 123,
				Gamma = "Super Hero",
			};

			var additionalInformation = new LogInformationDto
			{
				Class = "Villain",
				Method = "Attack",
				LineNumber = 666,
				Message = "DarkwingDuck is late",
				Information = alpha
			};
			var exception = new Exception("MegaVolt is here!", new NotSupportedException("Overload"));


			var logEntry = default(LogEntryDto);
			A.CallTo(() => _logWriterFake.Write(A<LogEntryDto>.Ignored)).Invokes(fakeCallObject =>
			{
				logEntry = fakeCallObject.Arguments[0] as LogEntryDto;
			});

			// Act
			_classUnderTest.Log(LogLevel.Error, eventId, additionalInformation, exception, null);

			// Assert
			logEntry.Host.HostName.Should().Be(Environment.MachineName);
			logEntry.Host.OperationSystem.Should().Be(Environment.OSVersion.VersionString);
			logEntry.Host.OperationSystem64Bit.Should().Be(Environment.Is64BitOperatingSystem);
			logEntry.Host.ProcessorCount.Should().Be(Environment.ProcessorCount);
			logEntry.Host.Culture.Should().Be(System.Globalization.CultureInfo.CurrentCulture.Name);

			logEntry.Process.ProcessName.Should().Be(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
			logEntry.Process.ProcessStartUtc.Should().Be(TimeZoneInfo.ConvertTimeToUtc(System.Diagnostics.Process.GetCurrentProcess().StartTime));
			logEntry.Process.Process64Bit.Should().Be(Environment.Is64BitProcess);
			logEntry.Process.MemoryUsage.Should().EndWith("MB");

			logEntry.LogLevel.Should().Be(LogLevel.Error.ToString().ToLower());
			logEntry.Category.Should().Be("DarkwingDuck");
			logEntry.Version.Should().Be("1.0.0");

			logEntry.Details.Should().BeEquivalentTo(JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(additionalInformation)));

			logEntry.Exception.Message.Should().Be(exception.Message);
			logEntry.Exception.StackTrace.Should().Be(exception.StackTrace);
			logEntry.Exception.InnerException.Message.Should().Be(exception.InnerException.Message);
			logEntry.Exception.InnerException.StackTrace.Should().Be(exception.InnerException.StackTrace);
			logEntry.TimestampUtc.Should().NotBe(DateTime.MinValue);
			logEntry.TimestampUtc.Should().NotBe(DateTime.MaxValue);
			logEntry.TimestampUtc.Kind.Should().Be(DateTimeKind.Utc);
		}

		[TestMethod]
		public void LogNoAdditionalInformationTest()
		{
			// Arrange
			var eventId = new EventId(0, "MegaVolt");

			var logEntry = default(LogEntryDto);
			A.CallTo(() => _logWriterFake.Write(A<LogEntryDto>.Ignored)).Invokes(fakeCallObject =>
			{
				logEntry = fakeCallObject.Arguments[0] as LogEntryDto;
			});

			// Act
			_classUnderTest.Log<LogInformationDto>(LogLevel.Error, eventId, null, null, null);

			// Assert
			logEntry.Details.Should().BeNull();
		}

		[TestMethod]
		public void LogNoExceptionTest()
		{
			// Arrange
			var eventId = new EventId(0, "MegaVolt");
			var alpha = new Alpha
			{
				Beta = 123,
				Gamma = "Super Hero",
			};
			var additionalInformation = new LogInformationDto
			{
				Class = "Villain",
				Method = "Attack",
				Message = "DarkwingDuck is late",
				Information = alpha
			};

			var logEntry = default(LogEntryDto);
			A.CallTo(() => _logWriterFake.Write(A<LogEntryDto>.Ignored)).Invokes(fakeCallObject =>
			{
				logEntry = fakeCallObject.Arguments[0] as LogEntryDto;
			});

			// Act
			_classUnderTest.Log(LogLevel.Error, eventId, additionalInformation, null, null);

			// Assert
			logEntry.Exception.Should().BeNull();
		}
	}
}