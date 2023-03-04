using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using CustomMapUtility.Caching;
using System.IO;
using uGuardian.Utilities;

namespace CustomMapUtility.Texture {
	internal sealed class TextureCache : FileCache<Texture2D> {
		protected override async Task<Texture2D> GetFile_Async_Internal(FileInfo file) {
			var fullName = file.FullName;
			var name = file.Name;

			Texture2D texture = CheckCache(file);
			if (texture != null) {
				return texture;
			}

			using (UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{fullName}")) {
				var source = www.SendWebRequest().GetWaitableTaskCompletionSource((operation) => {
					if (www.isNetworkError) {
						throw new InvalidOperationException($"{name}: {www.error}");
					}
					texture = DownloadHandlerTexture.GetContent(www);
					texture.name = file.Name;
					if (texture != null) {
						goto finishEntry;
					}
					else {
						throw new InvalidOperationException(name+": Image Returned Null");
					}

					finishEntry:
					dictionary[fullName] = texture;
					heldFile[fullName] = new WeakReference<Texture2D>(texture);
					return texture;
				});
				tasksInternal.Add(fullName, source);
				texture = await source.Task;
				tasksInternal.Remove(fullName);
				return texture;
			}
		}
		protected override Texture2D GetFile_Internal(FileInfo file) {
			var fullName = file.FullName;
			var name = file.Name;

			Texture2D texture = CheckCache(file);
			if (texture != null) {
				return texture;
			}

			using (UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{fullName}")) {
				var source = www.SendWebRequest();
				source.SpinWait();

				if (www.isNetworkError) {
					throw new InvalidOperationException($"{name}: {www.error}");
				}
				texture = DownloadHandlerTexture.GetContent(www);
				texture.name = file.Name;
				if (texture != null) {
					goto finishEntry;
				}
				else {
					throw new InvalidOperationException(name+": Image Returned Null");
				}

				finishEntry:
				dictionary[fullName] = texture;
				heldFile[fullName] = new WeakReference<Texture2D>(texture);
				return texture;
			}
		}
	}
}