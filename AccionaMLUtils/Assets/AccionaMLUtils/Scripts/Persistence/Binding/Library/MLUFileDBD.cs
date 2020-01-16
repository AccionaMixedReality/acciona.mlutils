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
	/// Implementation of Dictionary Binding Library that saves/reads the library as binary data using .NET System.IO.File class.
	/// MLUFileDBD stands for Magic Leap Utils File Dictionary Binding Library
	/// </summary>
	[Serializable]
	public class MLUFileDBD : MLUDictionaryBD
	{
		/// <summary>
		/// Assign this creator to MLUBindingLibrary.BindingLibraryCreator static member if you want all loaded libraries to use this MLUBindingLibrary implementation.
		/// </summary>
		public static readonly MLUBindingLibrary.BDCreator Creator = (string libraryID, bool saveOnAppQuit) => new MLUFileDBD(libraryID, saveOnAppQuit, defaultLibraryFolderPath);

		/// <summary>
		/// Default folder path for all loaded MLUFileDBD type libraries. If null, persistent data path will be used instead.
		/// </summary>
		public static string defaultLibraryFolderPath = null;

		/// <summary>
		/// Current library folder path (library file will always be [libraryID].bld). If null Application.persistentDataPath + "/BindingLibrary/" will be used isntead.
		/// </summary>
		public string libraryFolderPath = null;

		/// <summary>
		/// Current library relative file path.
		/// </summary>
		public string LibraryFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(libraryID))
					return null;
				else
					return Path.Combine(LibraryDirectory, libraryID + ".bld"); // bld extension stands for binding library data
			}
		}

		public string LibraryDirectory { get { return libraryFolderPath ?? Path.Combine(Application.persistentDataPath,  "BindingLibrary"); } }

		protected MLUFileDBD (string libraryID, bool saveOnAppQuit = true, string libraryFolderPath = null) : base (libraryID, saveOnAppQuit)
		{
			this.libraryFolderPath = libraryFolderPath;
		}

		public override bool LoadLibrary ()
		{
			string filePath = LibraryFilePath;

			if (File.Exists(filePath))
			{
				try
				{
					byte[] data = File.ReadAllBytes(filePath);

					if (data != null && data.Length > 0)
					{
						MLUFileDBD library = Deserialize<MLUFileDBD>(data);
						this.PCFBindings = library.PCFBindings;
						this.landscapeBindings = library.landscapeBindings;
						Debug.Log("MLUBindingLibrary: " + libraryID + " library data successfully loaded. | PCF bindings: " + PCFBindings.Count + " | Landscape bindings: " + landscapeBindings.Count + " | Size: " + data.Length + " bytes");
						
						return true;
					}
					else
						Debug.Log("MLUBindingLibrary: couldn't load " + libraryID + " data from " + filePath + ". Read data is null or empty.");
				}
				catch (Exception exception)
				{
					Debug.LogError("MLUBindingLibrary: error while trying to load " + libraryID + ". " + exception.GetType() + ": " + exception.Message);
				}
			}
			else
			{
				Debug.Log("MLUBindingLibrary: couldn't load " + libraryID + " data from " + filePath + ". File doesn't exist!");
			}

			return false;
		}

		public override void SaveLibrary ()
		{
			string filePath = LibraryFilePath;

			try
			{
				byte[] data = Serialize(this, LibraryDataSizeEstimation);

				if (data != null && data.Length > 0)
				{
					string directory = LibraryDirectory;
					if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
					File.WriteAllBytes(filePath, data);
					Debug.Log("MLUBindingLibrary: " + libraryID + " library data saved successfully. Size: " + data.Length + " bytes");
				}
				else
					Debug.Log("MLUBindingLibrary: couldn't save " + libraryID + " data to " + filePath + ". Serialized data is null or empty.");
			}
			catch (Exception exception)
			{
				Debug.LogError("MLUBindingLibrary: error while saving " + libraryID + ". " + exception.GetType() + ": " + exception.Message);
			}
		}

		public override void DeleteLibrary ()
		{
			ClearLibrary();
			string filePath = LibraryFilePath;

			Debug.Log("test");

			try
			{
				if (File.Exists(filePath))
					File.Delete(filePath);
			}
			catch (Exception exception)
			{
				Debug.LogError("MLUBindingLibrary: couldn't delete library file " + filePath + ". " + exception.GetType() + ": " + exception.Message);
			}
		}
	}
}