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
using System.Text;
#if !NOMP3
using NAudio.Wave;
#endif
using Mod;
using System.Xml.Serialization;
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	public partial class CustomMapHandler {
		#region RESOURCES
		public static class ModResources {
			public class CacheInit : ModInitializer {
				public const string version = "3.0.0";
				#if PRERELEASE
					#warning PRERELEASE
					public const string feature = "OVERHAUL";
				#endif
				static bool initialized = false;
				public override void OnInitializeMod() {
					if (initialized) {return;}
					#if !PRERELEASE
					Debug.Log($"CustomMapUtility Version \"{version}\"");
					#else
					Debug.Log($"CustomMapUtility Version \"{version}-PRERELEASE\" with feature \"{feature}\"");
					#endif
					var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
					var assemblyToken = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();
					var step1 = AppDomain.CurrentDomain.GetAssemblies()
						.AsParallel();
					var step2 = step1.Where(a => {
							var refAssemblies = a.GetReferencedAssemblies();
							return Array.Exists(refAssemblies, x =>
								string.Equals(assemblyName, x.Name, StringComparison.Ordinal) && assemblyToken.SequenceEqual(x.GetPublicKeyToken()));
						});
					var step3 = step2.Select(GetIdAndDirFromXml);
					step3.ForAll(x => {
					// foreach (var x in step5) {
						var (uniqueId, dirInfo) = x;

						var container = new CMUContainer(uniqueId, dirInfo);

						if(!containerDic.TryAdd(uniqueId, container)) {
							var oldContainer = containerDic[uniqueId];
							if (!oldContainer.ConfirmSameDirectory(container)) {
								// If this occurs it will attempt to compensate, but may error.
								AddErrorLog($"CustomMapUtility: ModID {uniqueId} exists in multiple directories");
							}
							// This shouldn't happen, but just to be sure this exists as a backup.
							lock (oldContainer) {
								oldContainer.CopyFrom(container);
							}
						}
					});
					/*
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
					*/
					StringBuilder sb = new StringBuilder("CustomMapUtility: Mods using this version: {");
					sb.AppendLine();
					foreach (var modId in containerDic.Keys) {
						sb.Append("	");
						sb.AppendLine(modId);
					}
					sb.Append("}");
					Debug.Log(sb);
					initialized = true;
				}
			}
			public static (string uniqueId, DirectoryInfo dirInfo) GetIdAndDirFromXml(Assembly assembly) {
				DirectoryInfo dirInfo = new FileInfo(assembly.Location).Directory.Parent;
				var files = dirInfo.EnumerateFiles("StageModInfo.xml");
				FileInfo stageModInfo;
				while ((stageModInfo = files.FirstOrDefault()) == null) {
					dirInfo = dirInfo.Parent;
					files = dirInfo.EnumerateFiles("StageModInfo.xml");
				}
				using (var streamReader = stageModInfo.OpenRead()) {
					Workshop.NormalInvitation invInfo = (Workshop.NormalInvitation) new XmlSerializer(typeof(Workshop.NormalInvitation)).Deserialize(streamReader);
					if (string.IsNullOrEmpty(invInfo.workshopInfo.uniqueId) || string.Equals(invInfo.workshopInfo.uniqueId, "-1", StringComparison.Ordinal)) {
						invInfo.workshopInfo.uniqueId = dirInfo.Name;
					}
					string uniqueId = invInfo.workshopInfo.uniqueId;
					return (uniqueId, dirInfo);
				}
			}
			internal static readonly ConcurrentDictionary<string, CMUContainer> containerDic = new ConcurrentDictionary<string, CMUContainer>();
			public class CMUContainer : IEquatable<CMUContainer>, IEquatable<string> {
				public readonly string uniqueId;
				public readonly DirectoryInfo directory;
				public readonly DirectoryInfo resourceDir;
				public readonly DirectoryInfo stageRootDir;
				public readonly Dictionary<string, DirectoryInfo> stageDic;
				public readonly DirectoryInfo audioRootDir;
				public readonly Dictionary<string, FileInfo> audioDic;

				public CMUContainer(string uniqueId, DirectoryInfo directory) {
					this.uniqueId = uniqueId;
					this.directory = directory;

					this.resourceDir = directory.EnumerateDirectories("Resource").FirstOrDefault();
					if (resourceDir == null) {return;}
					var resourceDirs = resourceDir.GetDirectories();
					this.stageRootDir = resourceDirs.FirstOrDefault(x => string.Equals(x.Name, "Stage", StringComparison.OrdinalIgnoreCase));
					this.audioRootDir = resourceDirs.FirstOrDefault(x => string.Equals(x.Name, "CustomAudio", StringComparison.OrdinalIgnoreCase));
					this.stageDic = stageRootDir?.EnumerateDirectories()
						.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase) ??
						new Dictionary<string, DirectoryInfo>(StringComparer.OrdinalIgnoreCase);
					this.audioDic = audioRootDir?.EnumerateFiles("*", SearchOption.AllDirectories)
						.ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase) ??
						new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
				}

				public bool Equals(CMUContainer target) {
					if (target == null) {
						return false;
					}
					return string.Equals(uniqueId, target.uniqueId, StringComparison.Ordinal);
				}
				public bool Equals(string target) {
					if (target == null) {
						return false;
					}
					return string.Equals(uniqueId, target, StringComparison.Ordinal);
				}
				public override bool Equals(object obj) {
					if (obj == null || !GetType().Equals(obj.GetType())) {
						return false;
					}
					var target = (CMUContainer)obj;
					return string.Equals(uniqueId, target.uniqueId, StringComparison.Ordinal);
				}
				public static bool operator ==(CMUContainer a, CMUContainer b) => a.Equals(b);
				public static bool operator !=(CMUContainer a, CMUContainer b) => !a.Equals(b);
				public static explicit operator string(CMUContainer a) => a.uniqueId;

				public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(uniqueId);

				public override string ToString() => uniqueId;

				[Obsolete("Use GetStageDir instead")]
				public string GetStagePath(string stageName) {
					if (!stageDic.TryGetValue(stageName, out var dir) && dir == null) {
						throw new ArgumentNullException(nameof(stageName), "Stage does not exist");
					}
					var path = dir.FullName;
					Debug.Log($"CustomMapUtility: StagePath: {path}");
					return path;
				}
				public DirectoryInfo GetStageDir(string stageName) {
					if (!stageDic.TryGetValue(stageName, out var dir) && dir == null) {
						throw new ArgumentNullException(nameof(stageName), "Stage does not exist");
					}
					Debug.Log($"CustomMapUtility: StagePath: {dir.FullName}");
					return dir;
				}
				public FileInfo GetStageBgmInfo(string bgmName) {
					if (audioDic.TryGetValue(bgmName, out var file)) {
						return file;
					} else {
						return null;
					}
				}
				public Dictionary<string, FileInfo> GetStageBgmInfos(IEnumerable<string> bgmNames) {
					var bgms = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
					foreach (var name in bgmNames) {
						if (audioDic.TryGetValue(name, out var file)) {
							bgms.Add(name, file);
						}
					}
					return bgms;
				}
				public Dictionary<string, FileInfo> GetStageBgmInfos(params string[] bgmNames) =>
					GetStageBgmInfos((IEnumerable<string>)bgmNames);

				public void CopyFrom(CMUContainer other) {
					foreach (var entry in other.stageDic) {
						if (!stageDic.TryAdd(entry.Key, entry.Value)) {
							if (!string.Equals(stageDic[entry.Key].FullName, entry.Value.FullName, StringComparison.Ordinal)) {
								AddErrorLog($"CustomMapUtility: Conflict for stage {entry.Key} occured during container copy");
							}
						}
					}
					foreach (var entry in other.audioDic) {
						if (!audioDic.TryAdd(entry.Key, entry.Value)) {
							if (!string.Equals(stageDic[entry.Key].FullName, entry.Value.FullName, StringComparison.Ordinal)) {
								AddErrorLog($"CustomMapUtility: Conflict for audio file {entry.Key} occured during container copy");
							}
						}
					}
				}
				public bool ConfirmSameDirectory(CMUContainer other) => string.Equals(directory.FullName, other.directory.FullName);
			}
			static void AddErrorLog(string msg) => Singleton<ModContentManager>.Instance.AddErrorLog(msg);
		}
		#endregion
	}
}