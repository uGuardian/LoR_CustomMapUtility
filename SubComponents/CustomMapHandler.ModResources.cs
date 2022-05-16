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
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	public partial class CustomMapHandler {
		#region RESOURCES
		public static class ModResources {
			public class CacheInit : ModInitializer {
				#if !PRERELEASE
					#if !NOMP3
						public const string version = "2.5.0";
					#else
						public const string version = "2.5.0-NOMP3";
					#endif
				#else
					#warning PRERELEASE
					#if !NOMP3
						public const string version = "2.5.0-PRERELEASE";
					#else
						public const string version = "2.5.0-PRERELEASE-NOMP3";
					#endif
				#endif
				public override void OnInitializeMod() {
					var assembly = Assembly.GetExecutingAssembly();
					/*
					if (!string.Equals(assembly.GetName().Name, "ConfigAPI", StringComparison.Ordinal)) {
						var curDir = new DirectoryInfo(assembly.Location + "\\..\\..");
						Debug.Log($"CustomMapUtility Version \"{version}\" in Local Mode at {curDir.FullName}");
						_dirInfos = new DirectoryInfo[] { curDir };
					} else {
						_dirInfos =
							from modInfo in ModContentInfoLoader.LoadAllModInfos()
								// where modInfo.activated == true
							select modInfo.dirInfo;
						Debug.Log($"CustomMapUtility Version \"{version}\" in Global Mode");
					}
					*/
					
					_stagePaths = GetStageRootPaths();
					_bgms = GetStageBgmInfos();
					if (_stagePaths != null && _stagePaths.Count != 0) {
						string stagePathsDebug = "CustomMapUtility StageRootPaths: {";
						foreach (var dir in _stagePaths) {
							stagePathsDebug += $"{Environment.NewLine}	{dir.FullName}";
						}
						stagePathsDebug += Environment.NewLine+"}";
						Debug.Log(stagePathsDebug);
					}
					if (_bgms != null && _bgms.Count != 0) {
						string bgmsDebug = "CustomMapUtility BgmPaths: {";
						foreach (var path in _bgms) {
							bgmsDebug += $"{Environment.NewLine}	{path.FullName}";
						}
						bgmsDebug += Environment.NewLine+"}";
						Debug.Log(bgmsDebug);
					}
					#if !NOMP3
					Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => x.Contains("NAudio"));
					#endif
				}
			}
			public static string GetStagePath(string stageName) {
				IEnumerable<DirectoryInfo> stagePaths =
					from info in GetStageRootPaths()
					where string.Equals(info.Name, stageName, StringComparison.Ordinal)
					select info;
				string path = null;
				foreach (var dir in stagePaths) {
					if (path == null) {
						path = dir.FullName;
					} else {
						Debug.LogError($"Multiple stages share the name \"{stageName}\"");
					}
				}
				if (path == null) {
					throw new ArgumentNullException(nameof(stageName), "Stage does not exist");
				}
				Debug.Log("CustomMapUtility: StagePath: "+path);
				return path;
			}
			[Obsolete("Use GetStageBgmInfos() instead")]
			public static string[] GetStageBgmPaths() {
				string[] bgms = new string[_bgms.Count];
				int i = 0;
				foreach (var bgm in _bgms) {
					bgms[i] = bgm.FullName;
					i++;
				}
				return bgms;
			}
			private static List<FileInfo> _bgms;
			[Obsolete("Use GetStageBgmInfos(string[]) instead")]
			public static string[] GetStageBgmPaths(string[] bgmNames) {
				int debugCount = 0;
				foreach (var bgm in bgmNames) {
					Debug.Log($"CustomMapUtility: BGM{debugCount}: {bgm}{Environment.NewLine}");
					debugCount++;
				}
				IEnumerable<string> bgms =
					from file in GetStageBgmInfos()
					where bgmNames.Any(b => string.Equals(b, file.Name, StringComparison.OrdinalIgnoreCase))
					select file.FullName;
				return bgms.ToArray();
			}
			public static List<FileInfo> GetStageBgmInfos() {
				if (_bgms != null && _bgms.Count != 0) {
					return _bgms;
				}
				List<FileInfo> bgms = new List<FileInfo>();
				foreach (DirectoryInfo dir in _dirInfos) {
					DirectoryInfo bgmsPath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource/CustomAudio"));
					if (bgmsPath.Exists) {
						bgms.AddRange(bgmsPath.GetFiles());
					}
					bgmsPath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource/StageBgm"));
					if (bgmsPath.Exists) {
						Debug.LogWarning("CustomMapUtility: StageBgm folder is now obsolete, please use CustomAudio folder instead.");
						Singleton<ModContentManager>.Instance.GetErrorLogs().Add($"<color=yellow>(assembly: {Assembly.GetExecutingAssembly().GetName().Name}) CustomMapUtility: StageBgm folder is now obselete, please use CustomAudio folder instead.</color>");
						bgms.AddRange(bgmsPath.GetFiles());
					}
				}
				return bgms;
			}
			public static Dictionary<string, FileInfo> GetStageBgmInfos(string[] bgmNames) {
				IEnumerable<FileInfo> bgms =
					from file in GetStageBgmInfos()
					where bgmNames.Any(b => string.Equals(b, file.Name, StringComparison.OrdinalIgnoreCase))
					select file;
				return bgms.ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);
			}
			/// <summary>
			/// A debug function that resets the stage bgm path cache
			/// </summary>
			[Obsolete("This is a debug function and should not be used in final production")]
			public static void ResetStageBgmInfos() {
				_bgms = null;
			}
			private static IEnumerable<DirectoryInfo> _dirInfos;
			public static List<DirectoryInfo> GetStageRootPaths() {
				if (_stagePaths != null && _stagePaths.Count != 0) {
					return _stagePaths;
				}
				List<DirectoryInfo> paths = new List<DirectoryInfo>();
				foreach (DirectoryInfo dir in _dirInfos) {
					DirectoryInfo stagePath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource/Stage"));
					if (stagePath.Exists) {
						paths.Add(stagePath);
					}
				}
				List<DirectoryInfo> stagePaths = new List<DirectoryInfo>();
				foreach (var path in paths) {
					stagePaths.AddRange(path.GetDirectories());
				}
				return stagePaths;
			}
			private static List<DirectoryInfo> _stagePaths;
		}
		#endregion
	}
}