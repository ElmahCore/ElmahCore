using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ElmahCore
{



	/// <summary>
	/// Extension methods for <see cref="TaskCompletionSource{TResult}"/>.
	/// </summary>

	static class TaskCompletionSourceExtensions
	{
		/// <summary>
		/// Attempts to conclude <see cref="TaskCompletionSource{TResult}"/>
		/// as being canceled, faulted or having completed successfully
		/// based on the corresponding status of the given 
		/// <see cref="Task{T}"/>.
		/// </summary>

		public static bool TryConcludeFrom<T>(this TaskCompletionSource<T> source, Task<T> task)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (task == null) throw new ArgumentNullException(nameof(task));

			if (task.IsCanceled)
			{
				source.TrySetCanceled();
			}
			else if (task.IsFaulted)
			{
				var aggregate = task.Exception;
				Debug.Assert(aggregate != null);
				source.TrySetException(aggregate.InnerExceptions);
			}
			else if (TaskStatus.RanToCompletion == task.Status)
			{
				source.TrySetResult(task.Result);
			}
			else
			{
				return false;
			}
			return true;
		}
	}



	/// <summary>
	/// Extension methods for <see cref="Task"/>.
	/// </summary>

	static class TaskExtensions
	{
		/// <summary>
		/// Returns a <see cref="Task{T}"/> that can be used as the
		/// <see cref="IAsyncResult"/> return value from the method
		/// that begin the operation of an API following the 
		/// <a href="http://msdn.microsoft.com/en-us/library/ms228963.aspx">Asynchronous Programming Model</a>.
		/// If an <see cref="AsyncCallback"/> is supplied, it is invoked
		/// when the supplied task concludes (fails, cancels or completes
		/// successfully).
		/// </summary>

		public static Task<T> Apmize<T>(this Task<T> task, AsyncCallback callback, object state)
		{
			return Apmize(task, callback, state, null);
		}

		/// <summary>
		/// Returns a <see cref="Task{T}"/> that can be used as the
		/// <see cref="IAsyncResult"/> return value from the method
		/// that begin the operation of an API following the 
		/// <a href="http://msdn.microsoft.com/en-us/library/ms228963.aspx">Asynchronous Programming Model</a>.
		/// If an <see cref="AsyncCallback"/> is supplied, it is invoked
		/// when the supplied task concludes (fails, cancels or completes
		/// successfully).
		/// </summary>

		public static Task<T> Apmize<T>(this Task<T> task, AsyncCallback callback, object state, TaskScheduler scheduler)
		{
			var result = task;

			TaskCompletionSource<T> tcs = null;
			if (task.AsyncState != state)
			{
				tcs = new TaskCompletionSource<T>(state);
				result = tcs.Task;
			}

			Task t = task;
			if (tcs != null)
			{
				t = t.ContinueWith(delegate { tcs.TryConcludeFrom(task); },
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
			}
			if (callback != null)
			{
				// ReSharper disable RedundantAssignment
				t = t.ContinueWith(delegate { callback(result); }, // ReSharper restore RedundantAssignment
					CancellationToken.None,
					TaskContinuationOptions.None,
					scheduler ?? TaskScheduler.Default);
			}

			return result;
		}
	}
}