/*
MIT License

Copyright (c) 2019 ACCIONA S.A.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
using UnityEngine.XR.MagicLeap;

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Acciona.MLUtils
{
	/// <summary>
	/// Abstract extension of MLUBindingLibrary that implements binding manipulation with dictionaries. Child classes will just have to implement how to load/save those dictionaries
	/// to disk. MLUDictionaryBD stands for Magic Leap Utils Dicitonary Binding Library.
	/// </summary>
	[Serializable]
	public abstract class MLUDictionaryBD : MLUBindingLibrary
	{
		private const int BYTES_PER_PCF_BINDING = 64; // my estimation of a MLUPCFBinding struct size in bytes
		private const int BYTES_PER_LANDSCAPE_BINDING = 4 * BYTES_PER_PCF_BINDING; // lets assume that a Magic Leap average session spawns 4 PCFs

		protected Dictionary<string, MLUPCFBinding> PCFBindings;
		protected Dictionary<string, MLULandscapeBinding> landscapeBindings;

		public override int PCFBindingsCount { get { return PCFBindings.Count; } }
		public override int LandscapeBindingsCount { get { return landscapeBindings.Count; } }

		protected int LibraryDataSizeEstimation { get { return BYTES_PER_PCF_BINDING * PCFBindings.Count + BYTES_PER_LANDSCAPE_BINDING * landscapeBindings.Count; } }

		protected MLUDictionaryBD (string libraryID, bool saveOnAppQuit = true) : base (libraryID, saveOnAppQuit)
		{
			PCFBindings = new Dictionary<string, MLUPCFBinding>();
			landscapeBindings = new Dictionary<string, MLULandscapeBinding>();
		}

		public override bool TryGetPCFBinding (string id, out MLUPCFBinding binding)
		{
			return PCFBindings.TryGetValue(id, out binding);
		}

		public override bool TryGetLandscapeBinding (string id, out MLULandscapeBinding binding)
		{
			return landscapeBindings.TryGetValue(id, out binding);
		}

		public override void SetPCFBinding (string id, MLUPCFBinding binding)
		{
			PCFBindings.Remove(id);
			PCFBindings.Add(id, binding);
		}

		public override void RemovePCFBinding (string id)
		{
			PCFBindings.Remove(id);
		}

		public override void SetLandscapeBinding (string id, MLULandscapeBinding binding)
		{
			landscapeBindings.Remove(id);
			landscapeBindings.Add(id, binding);
		}

		public override void RemoveLandscapeBinding (string id)
		{
			landscapeBindings.Remove(id);
		}

		public override void ClearLibrary ()
		{
			PCFBindings.Clear();
			landscapeBindings.Clear();
		}
	}
}