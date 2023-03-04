using System;
using System.Collections.Concurrent;
using System.Threading;

namespace uGuardian.Utilities.Threading {
	public class ImmediateUnitySynchronizationContext : SynchronizationContext {
		static int mainThreadId;
		static SynchronizationContext unityContext;
		bool activePost;
		readonly ConcurrentQueue<(SendOrPostCallback callback, object state)> queue;
		public ImmediateUnitySynchronizationContext() : this(new ConcurrentQueue<(SendOrPostCallback callback, object state)>()) {}
		private ImmediateUnitySynchronizationContext(ConcurrentQueue<(SendOrPostCallback callback, object state)> queue) {
			this.queue = queue;
		}
		internal static void InitializeSynchronizationContext(SynchronizationContext unitySyncContext) {
			mainThreadId = Thread.CurrentThread.ManagedThreadId;
			unityContext = unitySyncContext;
		}
		public override void Send(SendOrPostCallback callback, object state) {
			if (mainThreadId == Thread.CurrentThread.ManagedThreadId) {
				queue.Enqueue((callback, state));
				ExecuteAll();
			} else {
				Post(callback, state);
			}
		}

		public override void Post(SendOrPostCallback d, object state) {
			queue.Enqueue((d, state));
			CheckUnityPost();
		}

		void CheckUnityPost() {
			if (!activePost) {
				unityContext.Post(unityPostCallback, this);
				activePost = true;
			}
		}
		static readonly SendOrPostCallback unityPostCallback = new SendOrPostCallback(UnityPost);
		static void UnityPost(object state) {
			var context = (ImmediateUnitySynchronizationContext)state;
			context.ExecuteAll();
			context.activePost = false;
		}

		public override SynchronizationContext CreateCopy() => new ImmediateUnitySynchronizationContext(
			new ConcurrentQueue<(SendOrPostCallback callback, object state)>(queue));
		public void ExecuteAll() {
			if (mainThreadId == Thread.CurrentThread.ManagedThreadId) {
				ExecuteAll_Internal();
			} else {
				UnityEngine.Debug.LogError("ExecuteAll not called on main thread, queueing");
				CheckUnityPost();
			}
		}
		private void ExecuteAll_Internal() {
			while (queue.TryDequeue(out var callbackTuple)) {
				callbackTuple.callback(callbackTuple.state);
			}
		}
	}
}