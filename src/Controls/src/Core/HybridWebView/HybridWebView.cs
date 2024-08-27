﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="View"/> that presents local HTML content in a web view and allows JavaScript and C# code to
	/// communicate by using messages and by invoking methods.
	/// </summary>
	public class HybridWebView : View, IHybridWebView
	{
		/// <summary>Bindable property for <see cref="DefaultFile"/>.</summary>
		public static readonly BindableProperty DefaultFileProperty =
			BindableProperty.Create(nameof(DefaultFile), typeof(string), typeof(HybridWebView), defaultValue: "index.html");
		/// <summary>Bindable property for <see cref="HybridRoot"/>.</summary>
		public static readonly BindableProperty HybridRootProperty =
			BindableProperty.Create(nameof(HybridRoot), typeof(string), typeof(HybridWebView), defaultValue: "wwwroot");


		/// <inheritdoc/>
		public string? DefaultFile
		{
			get { return (string)GetValue(DefaultFileProperty); }
			set { SetValue(DefaultFileProperty, value); }
		}

		/// <inheritdoc/>
		public string? HybridRoot
		{
			get { return (string)GetValue(HybridRootProperty); }
			set { SetValue(HybridRootProperty, value); }
		}

		void IHybridWebView.RawMessageReceived(string rawMessage)
		{
			RawMessageReceived?.Invoke(this, new HybridWebViewRawMessageReceivedEventArgs(rawMessage));
		}

		/// <summary>
		/// Raised when a raw message is received from the web view. Raw messages are strings that have no additional processing.
		/// </summary>
		public event EventHandler<HybridWebViewRawMessageReceivedEventArgs>? RawMessageReceived;

		/// <summary>
		/// Sends a raw message to the code running in the web view. Raw messages have no additional processing.
		/// </summary>
		/// <param name="rawMessage"></param>
		public void SendRawMessage(string rawMessage)
		{
			Handler?.Invoke(
				nameof(IHybridWebView.SendRawMessage),
				new HybridWebViewRawMessage
				{
					Message = rawMessage,
				});
		}

		/// <inheritdoc/>
		public async Task<string?> InvokeJavaScriptAsync(string methodName, object?[]? paramValues, JsonTypeInfo?[]? paramJsonTypeInfos = null)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentException($"The method name cannot be null or empty.", nameof(methodName));
			}
			if (paramValues != null && paramJsonTypeInfos == null)
			{
				throw new ArgumentException($"The parameter values were provided, but the parameter JSON type infos were not.", nameof(paramJsonTypeInfos));
			}
			if (paramValues == null && paramJsonTypeInfos != null)
			{
				throw new ArgumentException($"The parameter JSON type infos were provided, but the parameter values were not.", nameof(paramValues));
			}
			if (paramValues != null && paramValues.Length != paramJsonTypeInfos!.Length)
			{
				throw new ArgumentException($"The number of parameter values does not match the number of parameter JSON type infos.", nameof(paramValues));
			}

			var result = await Handler?.InvokeAsync(
				nameof(IHybridWebView.InvokeJavaScriptAsync),
				new HybridWebViewInvokeJavaScriptRequest(methodName, paramValues, paramJsonTypeInfos))!;

			return result;
		}

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values specified
		/// by <paramref name="paramValues"/> by JSON-encoding each one.
		/// </summary>
		/// <typeparam name="TReturnType">The type of the return value to deserialize from JSON.</typeparam>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="returnTypeJsonTypeInfo">Metadata about deserializing the type of the return value specified by <typeparamref name="TReturnType"/>.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method by JSON-encoding each one.</param>
		/// <param name="paramJsonTypeInfos">Optional array of metadata about serializing the types of the parameters specified by <paramref name="paramValues"/>.</param>
		/// <returns>An object of type <typeparamref name="TReturnType"/> containing the return value of the called method.</returns>
		public async Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(string methodName, JsonTypeInfo<TReturnType?> returnTypeJsonTypeInfo, object?[]? paramValues = null, JsonTypeInfo?[]? paramJsonTypeInfos = null)
		{
			var stringResult = await InvokeJavaScriptAsync(methodName, paramValues, paramJsonTypeInfos);

			if (stringResult is null)
			{
				return default;
			}
			return JsonSerializer.Deserialize<TReturnType?>(stringResult, returnTypeJsonTypeInfo);
		}

		/// <inheritdoc/>
		public async Task<string?> EvaluateJavaScriptAsync(string script)
		{
			if (script == null)
			{
				return null;
			}

			var result = await Handler!.InvokeAsync(nameof(IHybridWebView.EvaluateJavaScriptAsync),
				new EvaluateJavaScriptAsyncRequest(script));

			return result;
		}
	}
}
