using System;
using System.Diagnostics;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
	/// <summary>
	/// Starts the timer and invokes a  callback upon disposal. Provides a simple way of timing an operation by wrapping it in a <code>using</code> (C#) statement.
	/// </summary>
	/// <example>
	/// <code>
	/// Console.WriteLine("Testing Stopwatchdisposable, should be 567:");
	//  using (var timer = new DisposableTimer(result => Console.WriteLine("Took {0}ms", result)))
	//  {
	//      Thread.Sleep(567);
	//  }
	/// </code>
	/// </example>
	public class DisposableTimer : DisposableObject
	{
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private readonly Action<long> _callback;

		protected DisposableTimer(Action<long> callback)
		{
			_callback = callback;
		}

		public Stopwatch Stopwatch
		{
			get { return _stopwatch; }
		}

		/// <summary>
		/// Starts the timer and invokes the specified callback upon disposal.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <returns></returns>
		public static DisposableTimer Start(Action<long> callback)
		{
			return new DisposableTimer(callback);
		}

		public static DisposableTimer TraceDuration<T>(Func<string> startMessage, Func<string> completeMessage)
		{
			return TraceDuration(typeof(T), startMessage, completeMessage);
		}

		public static DisposableTimer TraceDuration(Type loggerType, Func<string> startMessage, Func<string> completeMessage)
		{
			LogHelper.Debug(loggerType, startMessage);
			return new DisposableTimer(x => LogHelper.Info(loggerType, () => completeMessage() + " (took " + x + "ms)"));
		}

		/// <summary>
		/// Adds a start and end log entry as Info and tracks how long it takes until disposed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startMessage"></param>
		/// <param name="completeMessage"></param>
		/// <returns></returns>
		public static DisposableTimer TraceDuration<T>(string startMessage, string completeMessage)
		{
			return TraceDuration(typeof(T), startMessage, completeMessage);
		}

		/// <summary>
		/// Adds a start and end log entry as Info and tracks how long it takes until disposed.
		/// </summary>
		/// <param name="loggerType"></param>
		/// <param name="startMessage"></param>
		/// <param name="completeMessage"></param>
		/// <returns></returns>
		public static DisposableTimer TraceDuration(Type loggerType, string startMessage, string completeMessage)
		{
			LogHelper.Info(loggerType, () => startMessage);
			return new DisposableTimer(x => LogHelper.Info(loggerType, () => completeMessage + " (took " + x + "ms)"));
		}

		/// <summary>
		/// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startMessage"></param>
		/// <param name="completeMessage"></param>
		/// <returns></returns>
		public static DisposableTimer DebugDuration<T>(string startMessage, string completeMessage)
		{
			return DebugDuration(typeof(T), startMessage, completeMessage);
		}

		/// <summary>
		/// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startMessage"></param>
		/// <param name="completeMessage"></param>
		/// <returns></returns>
		public static DisposableTimer DebugDuration<T>(Func<string> startMessage, Func<string> completeMessage)
		{
			return DebugDuration(typeof(T), startMessage, completeMessage);
		}

		/// <summary>
		/// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
		/// </summary>
		/// <param name="loggerType"></param>
		/// <param name="startMessage"></param>
		/// <param name="completeMessage"></param>
		/// <returns></returns>
		public static DisposableTimer DebugDuration(Type loggerType, string startMessage, string completeMessage)
		{
			LogHelper.Debug(loggerType, () => startMessage);
			return new DisposableTimer(x => LogHelper.Debug(loggerType, () => completeMessage + " (took " + x + "ms)"));
		}

		/// <summary>
		/// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
		/// </summary>
		/// <param name="loggerType"></param>
		/// <param name="startMessage"></param>
		/// <param name="completeMessage"></param>
		/// <returns></returns>
		public static DisposableTimer DebugDuration(Type loggerType, Func<string> startMessage, Func<string> completeMessage)
		{
			LogHelper.Debug(loggerType, startMessage);
			return new DisposableTimer(x => LogHelper.Debug(loggerType, () => completeMessage() + " (took " + x + "ms)"));
		}

		/// <summary>
		/// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
		/// </summary>
		protected override void DisposeResources()
		{
			_callback.Invoke(Stopwatch.ElapsedMilliseconds);
		}
	}
}