#if DEBUG
#define PRERELEASE
#endif

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
#if !NOMP3
using NAudio.Wave;
#endif
using Mod;
using System.Xml.Serialization;

namespace CustomMapUtility {
	public partial class CustomMapHandler {
		public readonly static Dictionary<string, CustomMapHandler> instanceDic = new Dictionary<string, CustomMapHandler>(StringComparer.Ordinal);
		public readonly string modId;
		public readonly ModResources.CMUContainer container;
		public static CustomMapHandler GetCMU(string modId) {
			if (!instanceDic.TryGetValue(modId, out CustomMapHandler result)) {
				new ModResources.CacheInit().OnInitializeMod();
				if (!instanceDic.TryGetValue(modId, out result)) {
					result = new CustomMapHandler(modId);
					instanceDic.Add(modId, result);
				}
			}
			return result;
		}
		protected CustomMapHandler(string modId) {
			this.modId = modId;
			if (!ModResources.containerDic.TryGetValue(modId, out container)) {
				new ModResources.CacheInit().OnInitializeMod();
				if (!ModResources.containerDic.TryGetValue(modId, out container)) {
					throw new FileNotFoundException($"CustomMapUtility container for {modId} does not exist");
				}
			}
		}
	}
}