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

namespace CustomMapUtility {
	#region OFFSETS
	/// <summary>
	/// Contains a set of image offsets ranging from 0 to 1 for initializing the stage
	/// </summary>
	[StructLayout(LayoutKind.Auto)]
	public readonly struct Offsets {
		/// <summary>
		/// Contains a set of image offsets ranging from 0 to 1 for initializing the stage
		/// </summary>
		/// <param name="bgOffsetX">Background x pivot</param>
		/// <param name="bgOffsetY">Background y pivot</param>
		/// <param name="floorOffsetX">Floor x pivot</param>
		/// <param name="floorOffsetY">Floor y pivot</param>
		/// <param name="underOffsetX">FloorUnder x pivot</param>
		/// <param name="underOffsetY">FloorUnder y pivot</param>
		public Offsets(float bgOffsetX = 0.5f, float bgOffsetY = 0.5f,
			float floorOffsetX = 0.5f, float floorOffsetY = (407.5f/1080f),
			float underOffsetX = 0.5f, float underOffsetY = (300f/1080f)) {
				BGOffset = new Vector2(bgOffsetX, bgOffsetY);
				FloorOffset = new Vector2(floorOffsetX, floorOffsetY);
				UnderOffset = new Vector2(underOffsetX, underOffsetY);
				// Debug.Log(BGOffset);
				// Debug.Log(FloorOffset);
				// Debug.Log(UnderOffset);
		}
		/// <summary>
		/// A pair of values defining the Background's x and y offset
		/// </summary>
		public readonly Vector2 BGOffset;
		/// <summary>
		/// A pair of values defining the Floor's x and y offset
		/// </summary>
		public readonly Vector2 FloorOffset;
		/// <summary>
		/// A pair of values defining the FloorUnder's x and y offset
		/// </summary>
		public readonly Vector2 UnderOffset;
	}
	#endregion
}