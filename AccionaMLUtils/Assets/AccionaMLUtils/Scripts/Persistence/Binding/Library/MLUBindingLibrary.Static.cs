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

using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Acciona.MLUtils
{
	/// <summary>
	/// Static part of MLUBindingLibrary. Manages use of binding libraries (loading, global access with unique instances, etc).
	/// Appart from user loaded libraries it manages a Current library reference that MLUPersistentObjects are defaulted to.
	/// Current library can be changed in any moment, if never set it will automatically load DEFAULT_LIBRARY_ID constant (it will be also automatically saved on application quit).
	/// 
	/// BindingLibraryCreator is the delegate instance that the manager will use any time a new library is loaded/created
	/// 
	/// If developing a simple Magic Leap application that will be always used on same landscape with only one binding per persistent object, no parameters need to be set explicitly.
	/// Just add the MLUPersistentObject behaviour to any GameObject (with an explicit unique identifier or GameObject name) and it will persist as long as the recognized landscape by LuminOS doesn't change between executions.
	/// </summary>
	public abstract partial class MLUBindingLibrary
	{
		public const string DEFAULT_LIBRARY_ID = "DefaultLibrary";

		/// <summary>
		/// Delegate type used to define a MLUBindingLibrary type to be used by the MLUBindingLibrary static management system.
		/// </summary>
		public delegate MLUBindingLibrary BDCreator (string id, bool saveOnAppQuit = true);

		private static Dictionary<string, MLUBindingLibrary> loadedLibraries = new Dictionary<string, MLUBindingLibrary>();
		private static BDCreator bindingLibraryCreator;
		private static MLUBindingLibrary current;

		/// <summary>
		/// Current library being used by all MLUPersistentObjects where no custom library was defined.
		/// </summary>
		public static MLUBindingLibrary Current { get { return current ?? SetCurrentLibrary(DEFAULT_LIBRARY_ID, true, false); } }
		
		/// <summary>
		/// Current BDCreator delegate being used to instantiate new loaded/created libraries. Since MLUBindingLibrary is abstract, different implementations can be used by the application.
		/// MLUSecureSTorageDBD type will be used by default if this is never set, or set to null.
		/// </summary>
		public static BDCreator BindingLibraryCreator
		{
			get { return bindingLibraryCreator ?? MLUSecureStorageDBD.Creator; }
			set { bindingLibraryCreator = value; }
		}
		
		/// <summary>
		/// Returns an array of all currently loaded libraries (unique global instances for each library ID).
		/// </summary>
		public static MLUBindingLibrary[] LoadedLibraries
		{
			get
			{
				MLUBindingLibrary[] libraries = new MLUBindingLibrary[loadedLibraries.Count];
				int index = 0;

				foreach (var pair in loadedLibraries)
					libraries[index++] = pair.Value;
				
				return libraries;
			}
		}

		/// <summary>
		/// Sets the current library to the provided library unique identifier. If library didn't exist a new one will be created.
		/// </summary>
		public static MLUBindingLibrary SetCurrentLibrary (string id, bool saveOnAppQuit = true, bool reloadLibrary = false)
		{
			return current = GetLibrary(id, saveOnAppQuit, reloadLibrary);
		}

		/// <summary>
		/// Saves the current library to disk (performs MLUBindingLibrary.SaveLibrary()).
		/// </summary>
		public static void SaveCurrentLibrary ()
		{
			if (current != null)
				current.SaveLibrary();
		}

		/// <summary>
		/// Gets the library instance for the given library unique identifier. If no instance is currenlty loaded it will try to load it from disk (if didn't exist a new one will be created).
		/// SaveOnAppQuit library property can be overwritten if desired.
		/// If reloadLibrary == true it will try to reload the library data from disk (overwritting current unsaved library data if an instance was loaded).
		/// </summary>
		public static MLUBindingLibrary GetLibrary (string libraryID, bool saveOnAppQuit = true, bool reloadLibrary = false)
		{
			if (!string.IsNullOrEmpty(libraryID))
			{
				MLUBindingLibrary library;

				if (reloadLibrary || !loadedLibraries.TryGetValue(libraryID, out library))
				{
					library = BindingLibraryCreator(libraryID, saveOnAppQuit);

					if (!library.LoadLibrary())
						Debug.Log("MLUBindingLibrary: " + libraryID + " was not found, new instance will be created!");
					if (reloadLibrary)
						loadedLibraries.Remove(libraryID);

					loadedLibraries.Add(libraryID, library);
				}
				else
					library.SaveOnAppQuit = saveOnAppQuit; // override previous value since new one may have been specified

				return library;
			}
			else
				return null;
		}

		/// <summary>
		/// If the given library ID is currently loaded it will try to save it to disk.
		/// It does the same as calling SaveLibrary() if having the instance reference.
		/// Also the library instance can be unloaded from memory after saving.
		/// </summary>
		public static void SaveLibrary (string libraryID, bool unloadLibrary = false)
		{
			MLUBindingLibrary library;

			if (loadedLibraries.TryGetValue(libraryID, out library))
			{
				library.SaveLibrary();
				if (unloadLibrary) loadedLibraries.Remove(libraryID);
			}
		}

		/// <summary>
		/// Unloads given library ID from memory (if currently instanced). The library will be saved before unloading by default.
		/// </summary>
		public static void UnloadLibrary (string libraryID, bool saveLibrary = true)
		{
			MLUBindingLibrary library;

			if (loadedLibraries.TryGetValue(libraryID, out library))
			{
				if (saveLibrary) library.SaveLibrary();
				loadedLibraries.Remove(libraryID);
			}
		}

		/// <summary>
		/// Unloads from memory (if loaded) and deletes from disk (if exists) given library ID.
		/// </summary>
		/// <param name="libraryID"></param>
		public static void DeleteLibrary (string libraryID)
		{
			MLUBindingLibrary library = GetLibrary(libraryID);
			library.DeleteLibrary();
			loadedLibraries.Remove(libraryID);
		}

		/// <summary>
		/// Saves all currently loaded libraries to disk (calling SaveLibrary() method on each instance).
		/// Also libraries can be unloaded after saving if specified.
		/// </summary>
		public static void SaveAllLibraries (bool unloadLibraries = false)
		{
			foreach (var pair in loadedLibraries)
				if (pair.Value != null)
					pair.Value.SaveLibrary();

			if (unloadLibraries)
				loadedLibraries.Clear();
		}

		/// <summary>
		/// Unloads all currently loaded libraries from memory. Libraries will be saved before unloading by default.
		/// </summary>
		public static void UnloadAllLibraries (bool saveLibraries = true)
		{
			if (saveLibraries)
				SaveAllLibraries(true);
			else
				loadedLibraries.Clear();
		}

		/// <summary>
		/// Binary serializing method intended for utility on MLUBindingLibrary subclasses.
		/// </summary>
		protected byte[] Serialize (object obj, int dataSize = 0)
		{
			using (MemoryStream stream = dataSize > 0 ? new MemoryStream(dataSize) : new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, obj);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Binary deserializing method intended for utility on MLUBindingLibrary subclasses.
		/// </summary>
		protected static T Deserialize <T> (byte[] data) where T : class
		{
			using (MemoryStream stream = new MemoryStream(data))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				return formatter.Deserialize(stream) as T;
			}
		}
	}
}