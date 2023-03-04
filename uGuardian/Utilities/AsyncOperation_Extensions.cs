using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace uGuardian.Utilities {
	public static class AsyncOperation_Extensions {
		public static WaitableTaskCompletionSource<AsyncOperation, T> GetWaitableTaskCompletionSource<T>(this AsyncOperation operation,
			Func<AsyncOperation, T> callback)
		{
			/* TaskCompletionSource doesn't appear to support these TaskCreationOptions
			var taskCompletionSource = new TaskCompletionSource<AsyncOperation>(operation,
				TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
			*/
			var taskCompletionSource = new WaitableTaskCompletionSource<AsyncOperation, T>(operation, callback);
			// var taskCompletionSource = new TaskCompletionSource<AsyncOperation>(operation);
			operation.completed += (_) => taskCompletionSource.Wait();

			return taskCompletionSource;
		}
		public static void SpinWait(this UnityWebRequestAsyncOperation operation) {
			if (operation.isDone) {return;}
			System.Threading.SpinWait.SpinUntil(() => operation.webRequest.isDone);
		}
		public static void SpinWait(this AsyncOperation operation) {
			Debug.LogWarning("Spinwait for generic AsyncOperations don't usually work");
			if (operation.isDone) {return;}
			System.Threading.SpinWait.SpinUntil(() => operation.isDone, 5000);
		}
	}
	public static class AsyncUtils {
		public static async Task<byte[]> LoadFile(FileInfo file) {
			byte[] result;
			using (var stream = file.OpenRead()) {
				result = new byte[stream.Length];
				await stream.ReadAsync(result, 0, (int)stream.Length).ConfigureAwait(false);
			}
			return result;
		}
	}
	public abstract class CustomTaskCompletionSource<TOutput> : TaskCompletionSource<TOutput> {
		public CustomTaskCompletionSource(object state) : base(state) {}

		public bool Completed {get; protected set;}

		public abstract void Wait();
		protected abstract void Complete();
	}
	public class WaitableTaskCompletionSource<TInput, TOutput> : CustomTaskCompletionSource<TOutput> where TInput : class {
		public WaitableTaskCompletionSource(TInput state, Func<TInput, TOutput> callback) : base(state) {
			this.state = state;
			this.callback = callback;
		}

		public readonly TInput state;
		public readonly Func<TInput, TOutput> callback;

		public override void Wait() {
			switch (state) {
				case null:
					break;
				case AsyncOperation operation:
					switch (operation) {
						case UnityWebRequestAsyncOperation webOperation:
							webOperation.SpinWait();
							break;
						default:
							operation.SpinWait();
							break;
					}
					break;
				case Task task:
					task.Wait();
					break;
			}
			Complete();
		}
		protected override void Complete() {
			if (Completed) {return;}
			if (TrySetResult(callback(state))) {
				Completed = true;
			}
		}
	}
}