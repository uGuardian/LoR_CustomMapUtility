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
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	#pragma warning disable IDE0051, IDE0059, CS0219
	class A_ReadMe {
		protected const string text = "If you're reading this in a decompiler, please get the source code off github";
		protected const string text2 = "It's easier to read, and less likely to have a faulty output. It'll also be up to date.";
		protected const string URL = "https://github.com/uGuardian/LoR_CustomMapUtility";
	}
	#pragma warning restore IDE0051,IDE0059,CS0219,IDE1006
	public partial class CustomMapHandler {
		#region OBSOLETESTRINGS
		public const string obsoleteString = "Remove (new MapManager) and change to InitCustomMap<MapManager>";
		public const string obsoleteString2 = "Using InitCustomMap<MapManager> and removing typeof(MapManager) is new preferred";
		public const string obsoleteStringGeneric = "Remove (new MapManager) and change to <MapManager> generic method";
		public const string obsoleteStringGeneric2 = "Using <MapManager> generic method and removing typeof(MapManager) is new preferred";
		#endregion

		#pragma warning disable IDE0051,IDE0059,CS0219,IDE1006
		sealed class A_ReadMe : CustomMapUtility.A_ReadMe {
			const string _text = text;
			const string _text2 = text2;
			const string _URL = URL;
		}
		#pragma warning restore IDE0051,IDE0059,CS0219,IDE1006
	}
}