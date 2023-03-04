using System.Threading;
using System.Threading.Tasks;
namespace uGuardian.Utilities.Threading {
	public static class SynchronizationContext_Extensions {
		public static TaskScheduler GetTaskScheduler(this SynchronizationContext context) {
			TaskScheduler result;
			var current = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext(context);
			result = TaskScheduler.FromCurrentSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(current);
			return result;
		}
	}
}