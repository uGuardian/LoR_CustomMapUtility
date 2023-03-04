using System;
using UnityEngine;
using UnityEngine.Networking;

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
}