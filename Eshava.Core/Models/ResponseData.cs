using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Eshava.Core.Models
{
	public class ResponseData<T>
	{
		private string _rawMessage;

		public ResponseData()
		{
			StatusCode = (int)HttpStatusCode.OK;
		}

		public ResponseData(T data) : this()
		{
			Data = data;
		}

		public ResponseData(T data, HttpStatusCode statusCode)
		{
			Data = data;
			StatusCode = (int)statusCode;
		}

		public bool IsFaulty { get; private set; }

		public T Data { get; set; }

		public string Message { get; private set; }

		public Guid? MessageGuid { get; private set; }

		public IEnumerable<ValidationError> ValidationErrors { get; private set; }

		public int StatusCode { get; private set; }

		public string GetRawMessage()
		{
			return _rawMessage;
		}

		/// <summary>
		/// Copies the data of this faulty response data instance into a new instance of a different type
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="messageGuid"></param>
		/// <returns></returns>
		public ResponseData<U> ConvertTo<U>(Guid? messageGuid = null)
		{
			return new ResponseData<U>
			{
				IsFaulty = true,
				MessageGuid = MessageGuid ?? messageGuid,
				Message = Message,
				ValidationErrors = ValidationErrors,
				StatusCode = StatusCode,
				_rawMessage = _rawMessage
			};
		}

		/// <summary>
		/// Adds a validation error to the current response data instance
		/// </summary>
		/// <param name="validationError"></param>
		/// <returns></returns>
		public ResponseData<T> AddValidationError(ValidationError validationError)
		{
			var errors = ValidationErrors?.ToList() ?? [];
			errors.Add(validationError);

			ValidationErrors = errors;

			return this;
		}

		/// <summary>
		/// Adds validation errors to the current response data instance
		/// </summary>
		/// <param name="validationErrors"></param>
		/// <returns></returns>
		public ResponseData<T> AddValidationErrors(IEnumerable<ValidationError> validationErrors)
		{
			var errors = ValidationErrors?.ToList() ?? [];
			errors.AddRange(validationErrors);

			ValidationErrors = errors;

			return this;
		}

		/// <summary>
		/// Adds a validation error to the current response data instance
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="errorType"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public ResponseData<T> AddValidationError(string propertyName, string errorType, object value = null, string methodType = null, string propertyNameFrom = null, string propertyNameTo = null)
		{
			return AddValidationError(new ValidationError
			{
				PropertyName = propertyName,
				PropertyNameFrom = propertyNameFrom,
				PropertyNameTo = propertyNameTo,
				ErrorType = errorType,
				MethodType = methodType,
				Value = value
			});
		}

		/// <summary>
		/// Adds a raw message
		/// </summary>
		/// <param name="rawMessage"></param>
		/// <returns></returns>
		public ResponseData<T> AddRawMessage(string rawMessage)
		{
			_rawMessage = rawMessage;

			return this;
		}

		/// <summary>
		/// Creates a faulty response data with status code <see cref="HttpStatusCode.BadRequest"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="messageGuid"></param>
		/// <returns></returns>
		public static ResponseData<T> CreateFaultyResponse(string message, Guid? messageGuid = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
		{
			return CreateFaultyResponse(message, statusCode, messageGuid);
		}

		/// <summary>
		/// Creates a faulty response data instance for status code <see cref="HttpStatusCode.InternalServerError"/>
		/// </summary>
		/// <param name="message">Message set to <see cref="Message"/></param>
		/// <param name="exception">Message set to <see cref="RawMessage"/></param>
		/// <param name="messageGuid">Guid set to <see cref="MessageGuid"/></param>
		/// <returns></returns>
		public static ResponseData<T> CreateInternalServerError(string message, Exception exception, Guid? messageGuid = null)
		{
			var responseData = CreateFaultyResponse(message, HttpStatusCode.InternalServerError, messageGuid);

			if (exception is not null)
			{
				responseData.AddRawMessage(exception.Message);
			}

			return responseData;
		}

		/// <summary>
		/// Creates a response data with message <see cref="MessageConstants.NOTEXISTING"/> and status code <see cref="HttpStatusCode.NotFound"/>
		/// </summary>
		/// <param name="messageGuid"></param>
		/// <returns></returns>
		public static ResponseData<T> CreateNotExistingResponse(Guid? messageGuid = null)
		{
			return CreateFaultyResponse(MessageConstants.NOTEXISTING, HttpStatusCode.NotFound, messageGuid);
		}

		/// <summary>
		/// Creates a response data with message <see cref="MessageConstants.INVALIDDATA"/> and status code <see cref="HttpStatusCode.BadRequest"/>
		/// </summary>
		/// <param name="messageGuid"></param>
		/// <returns></returns>
		public static ResponseData<T> CreateInvalidDataResponse(Guid? messageGuid = null)
		{
			return CreateFaultyResponse(MessageConstants.INVALIDDATA, HttpStatusCode.BadRequest, messageGuid);
		}

		/// <summary>
		/// Creates a response data with message <see cref="MessageConstants.IMMUTABLE"/> and status code <see cref="HttpStatusCode.BadRequest"/>
		/// </summary>
		/// <param name="messageGuid"></param>
		/// <returns></returns>
		public static ResponseData<T> CreateImmutable(Guid? messageGuid = null)
		{
			return CreateFaultyResponse(MessageConstants.IMMUTABLE, HttpStatusCode.BadRequest, messageGuid);
		}

		private static ResponseData<T> CreateFaultyResponse(string message, HttpStatusCode statusCode, Guid? messageGuid)
		{
			return new ResponseData<T>
			{
				IsFaulty = true,
				Message = message,
				MessageGuid = messageGuid,
				StatusCode = (int)statusCode
			};
		}

		private static class MessageConstants
		{
			public const string IMMUTABLE = "Immutable";
			public const string INVALIDDATA = "InvalidData";
			public const string NOTEXISTING = "NotExisting";
		}
	}
}