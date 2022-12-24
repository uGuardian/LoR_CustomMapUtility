using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncOperation_Extensions {
	public static TaskCompletionSource<AsyncOperation> GetTaskCompletionSource(this UnityWebRequestAsyncOperation operation) {
		/* TaskCompletionSource doesn't appear to support these TaskCreationOptions
		var taskCompletionSource = new TaskCompletionSource<AsyncOperation>(operation,
			TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
		*/
		var taskCompletionSource = new TaskCompletionSource<AsyncOperation>(operation);
		operation.completed += (x) => taskCompletionSource.TrySetResult(x);

		return taskCompletionSource;
	}
	public static void SpinWait(this TaskCompletionSource<AsyncOperation> taskCompletionSource) {
		if (!(taskCompletionSource.Task.AsyncState is UnityWebRequestAsyncOperation operation)) {
			throw new InvalidOperationException();
		}
		operation.SpinWait();
		taskCompletionSource.TrySetResult(operation);
	}
	public static UnityWebRequestAsyncOperation SpinWait(this UnityWebRequestAsyncOperation operation) {
		System.Threading.SpinWait.SpinUntil(() => operation.webRequest.isDone);
		return operation;
	}
}