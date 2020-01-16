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
	/// Abstract class that provides basic functionality to get and save landscape bindings associated with a unique ID. Each library must have a unique ID.
	/// Static part manages loading/unloading libraries by ID.
	/// </summary>
	[Serializable]
	public abstract partial class MLUBindingLibrary
	{
		/// <summary>
		/// Unique library identifier.
		/// </summary>
		public readonly string libraryID;

		private bool saveOnAppQuit = false;

		/// <summary>
		/// Number of saved PCF bindings on this library.
		/// </summary>
		public abstract int PCFBindingsCount { get; }

		/// <summary>
		/// Number of saved landscape bindings on this library.
		/// </summary>
		public abstract int LandscapeBindingsCount { get; }

		/// <summary>
		/// Set this to true if you want the library to subscribe to the application quit event to call SaveLibrary(). Setting this to false will unsbscribe from the event (if previously subscribed).
		/// </summary>
		public bool SaveOnAppQuit
		{
			get{ return saveOnAppQuit; }
			
			set
			{
				// only perform operation if changing the value of saveOnAppQuit
				if (saveOnAppQuit != value)
				{
					if (saveOnAppQuit)
						Application.quitting -= SaveLibrary;
					else
						Application.quitting += SaveLibrary;
					
					saveOnAppQuit = value;
				}
			}
		}

		// ideally all subclasses should implement a protected constructor and provide a MLUBindingLibrary.BDCreator delegate instance so it can be registered on MLUBindingLibrary static library management.
		protected MLUBindingLibrary (string libraryID, bool saveOnAppQuit = true)
		{
			if (string.IsNullOrEmpty(libraryID))
				throw new ArgumentNullException("libraryID", "libraryID can't be null or empty!");
			
			this.libraryID = libraryID;
			SaveOnAppQuit = saveOnAppQuit;
		}
		
		/// <summary>
		/// Tries to retrieve a PCF binding with the given id.
		/// </summary>
		public abstract bool TryGetPCFBinding (string id, out MLUPCFBinding binding);

		/// <summary>
		/// Tries to retrieve a landscape binding with the given id.
		/// </summary>
		public abstract bool TryGetLandscapeBinding (string id, out MLULandscapeBinding binding);

		/// <summary>
		/// Saves the given binding with the given id to this library.
		/// </summary>
		public abstract void SetPCFBinding (string id, MLUPCFBinding binding);

		/// <summary>
		/// Removes the binding id from the library (if any).
		/// </summary>
		public abstract void RemovePCFBinding (string id);

		/// <summary>
		/// Saves the given binding with the given id to this library.
		/// </summary>
		public abstract void SetLandscapeBinding (string id, MLULandscapeBinding binding);

		/// <summary>
		/// Removes the binding id from the library (if any).
		/// </summary>
		public abstract void RemoveLandscapeBinding (string id);

		/// <summary>
		/// Clears current library data (does not save the clear to disk).
		/// </summary>
		public abstract void ClearLibrary ();

		/// <summary>
		/// Loads library data from disk (unsaved changes will be lost).
		/// Returns true if library loaded successfully.
		/// </summary>
		public abstract bool LoadLibrary ();

		/// <summary>
		/// Saves library data to disk (previous save will be overwritten).
		/// </summary>
		public abstract void SaveLibrary ();

		/// <summary>
		/// Clears current library data and deletes it from disk.
		/// </summary>
		public abstract void DeleteLibrary ();
	}
}