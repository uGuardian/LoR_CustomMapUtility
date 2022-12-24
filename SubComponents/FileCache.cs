using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using uGuardian.Utilities;

namespace CustomMapUtility.Caching {
	public abstract class FileCache<T> : MonoBehaviour where T : class {
		protected static readonly Dictionary<string, WeakReference<T>> heldFile =
			new Dictionary<string, WeakReference<T>>(StringComparer.Ordinal);
		protected readonly ConcurrentDictionary<string, T> dictionary =
			new ConcurrentDictionary<string, T>(StringComparer.Ordinal);
		protected readonly Dictionary<string, Task<T>> tasks =
			new Dictionary<string, Task<T>>(StringComparer.Ordinal);
		protected readonly Dictionary<string, TaskCompletionSource<AsyncOperation>> tasksInternal =
			new Dictionary<string, TaskCompletionSource<AsyncOperation>>(StringComparer.Ordinal);

		public T CheckCache(FileInfo info) {
			var fullName = info.FullName;
			var name = info.Name;
			T result = null;
			if (dictionary.TryGetValue(fullName, out var file)) {
				result = file;
			}
			if (heldFile.ContainsKey(fullName)) {
				if (heldFile[fullName].TryGetTarget(out file) && file != null) {
					result = file;
				}
				#if DEBUG
				else {
					Debug.Log($"CustomMapUtility:FileCache: Cache for {name} was dropped from memory");
				}
				#endif
			}
			#if DEBUG
			if (result != null) {
				Debug.Log($"CustomMapUtility:FileCache: {name} retrieved from cache");
			}
			#endif
			return result;
		}
		public async Task<T> GetFile_Async(FileInfo file) {
			var fullName = file.FullName;
			var name = file.Name;
			T result = CheckCache(file);
			if (result != null) {

			} else if (tasks.TryGetValue(fullName, out var task)) {
				result = await task;
			} else {
				#if DEBUG
				Debug.Log($"CustomMapUtility:FileCache: Loading {name}");
				#endif
				task = GetFile_Async_Internal(file);
				tasks[fullName] = task;
				result = await task;
				tasks.Remove(fullName);
			}
			dictionary[fullName] = result;
			heldFile[fullName] = new WeakReference<T>(result);
			return result;
		}
		protected abstract Task<T> GetFile_Async_Internal(FileInfo file);
		public void AwaitCompletion(FileInfo file) {
			var fullName = file.FullName;
			if (tasks.TryGetValue(fullName, out var task)) {
				if (task.IsCompleted) {return;}
				SingletonBehavior<BattleSceneRoot>.Instance.StartCoroutine(AwaitCompletionRoutine(task));
			} else {
				task = GetFile_Async_Internal(file);
				tasks[fullName] = task;
				if (task.IsCompleted) {return;}
				SingletonBehavior<BattleSceneRoot>.Instance.StartCoroutine(AwaitCompletionRoutine(task));
			}
		}
		public static void AwaitCompletion(Task task) {
			if (task.IsCompleted) {return;}
			SingletonBehavior<BattleSceneRoot>.Instance.StartCoroutine(AwaitCompletionRoutine(task));
		}
		static IEnumerator AwaitCompletionRoutine(Task task) {
			while (!task.IsCompleted) {
				TimeManager.PauseTime();
				yield return null;
			}
			if (!SingletonBehavior<UI.UIPopupWindowManager>.Instance.AllCheckOpen()) {
				TimeManager.ResumeTime();
			}
		}
		public T GetFile(FileInfo file) {
			var fullName = file.FullName;
			var name = file.Name;
			T result = CheckCache(file);
			if (result != null) {

			} else if (tasks.TryGetValue(fullName, out var task)) {
				var internalTaskExists = tasksInternal.Remove(fullName, out var taskCompletionSource);
				if (task.Status == TaskStatus.WaitingForActivation) {
					#if DEBUG
					Debug.Log($"CustomMapUtility:FileCache: Loading {name}");
					#endif
					result = GetFile_Internal(file, out var operation);
					if (internalTaskExists) {
						taskCompletionSource.TrySetResult(operation);
					}
					goto complete;
				} else if (!task.IsCompleted && internalTaskExists) {
					Debug.Log($"CustomMapUtility:FileCache: Completing load of {name}");
					taskCompletionSource.SpinWait();
					if (!(task.AsyncState is UnityWebRequestAsyncOperation operation)) {
						throw new InvalidOperationException();
					}
				}
				result = task.Result;
			} else {
				#if DEBUG
				Debug.Log($"CustomMapUtility:FileCache: Loading {name}");
				#endif
				result = GetFile_Internal(file, out _);
			}

			complete:
			dictionary[fullName] = result;
			heldFile[fullName] = new WeakReference<T>(result);
			return result;
		}
		protected abstract T GetFile_Internal(FileInfo file, out UnityWebRequestAsyncOperation operation);
		protected void OnDisable() {
			dictionary.Clear();
			StopAllCoroutines();
			tasks.Clear();
			tasksInternal.Clear();
		}
	}
}