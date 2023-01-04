using Mod;
using System.IO;

namespace Mod {
	internal static class CustomMapUtility_Extensions {
		public static DirectoryInfo GetLoadedModPath(this ModContentManager manager, string packageId) {
			ModContentInfo modContentInfo = manager._loadedContents.Find(x => x._modInfo.invInfo.workshopInfo.uniqueId == packageId)._modInfo;
			if (modContentInfo == null) {return null;}
			return modContentInfo.dirInfo;
		}
	}
}