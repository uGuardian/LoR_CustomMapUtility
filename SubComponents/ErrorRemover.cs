using System.Collections.Generic;
using System.Linq;
using Mod;

namespace ErrorRemover {
	#pragma warning disable MA0048,MA0049
	public class AutoRemover : ModInitializer {
		public override void OnInitializeMod() => ErrorRemover.RemoveErrors();
	}
	public static class ErrorRemover {
		const string exists = "The same assembly name already exists. : "; 
		public static void RemoveErrors() {
			var dllList = new string[] {
				$"{exists}CustomMapUtility",
				$"{exists}NAudio",
			};
			Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => dllList.Any(x.Contains));
		}
		public static void RemoveErrors(string[] dllList)
			=> Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => dllList.Any(x.Contains));
		public static void RemoveErrors(List<string> dllList)
			=> Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => dllList.Any(x.Contains));
	}
}