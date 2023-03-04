using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace uGuardian.Utilities
{
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