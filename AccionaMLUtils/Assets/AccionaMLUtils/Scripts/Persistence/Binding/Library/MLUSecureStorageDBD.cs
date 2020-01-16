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
	/// Implementation of Dictionary Binding Library that saves/reads the library as binary data using MLSecureStorage API.
	/// MLUSecureStorageDBD stands for Magic Leap Utils SecureStorage Dictionary Binding Library.
	/// </summary>
	[Serializable]
	public class MLUSecureStorageDBD : MLUDictionaryBD
	{
		/// <summary>
		/// Assign this creator to MLUBindingLibrary.BindingLibraryCreator static member if you want all loaded libraries to use this MLUBindingLibrary implementation.
		/// </summary>
		public static readonly MLUBindingLibrary.BDCreator Creator = (string libraryID, bool saveOnAppQuit) => new MLUSecureStorageDBD(libraryID, saveOnAppQuit);

		/// <summary>
		/// MLSecureStorage data ID for this library's data.
		/// </summary>
		public string LibraryStorageID
		{
			get
			{
				if (string.IsNullOrEmpty(libraryID))
					return null;
				else
					return "BindingLibrary/" + libraryID;
			}
		}

		protected MLUSecureStorageDBD (string libraryID, bool saveOnAppQuit = true) : base (libraryID, saveOnAppQuit) { }

		public override bool LoadLibrary ()
		{
			try
			{
				byte[] data = null;
				MLResult result = MLSecureStorage.GetData(LibraryStorageID, ref data);

				if (result.IsOk)
				{
					MLUSecureStorageDBD library = Deserialize<MLUSecureStorageDBD>(data);
					this.PCFBindings = library.PCFBindings;
					this.landscapeBindings = library.landscapeBindings;
					Debug.Log("MLUBindingLibrary: " + libraryID + " library data successfully loaded. | PCF bindings: " + PCFBindings.Count + " | Landscape bindings: " + landscapeBindings.Count + " | Size: " + data.Length + " bytes");
					
					return true;
				}
				else
					Debug.Log("MLUBindingLibrary: couldn't load " + libraryID + " data from MLSecureStorage API. result: " + result);
			}
			catch (Exception exception)
			{
				Debug.LogError("MLUBindingLibrary: error while trying to load " + libraryID + ". " + exception.GetType() + ": " + exception.Message);
			}
			
			return false;
		}

		public override void SaveLibrary ()
		{
			try
			{
				byte[] data = Serialize(this, LibraryDataSizeEstimation);
				MLResult result = MLSecureStorage.StoreData(LibraryStorageID, data);

				if (result.IsOk)
					Debug.Log("MLUBindingLibrary: " + libraryID + " library data saved successfully. Size: " + data.Length + " bytes");
				else
					Debug.Log("MLUBindingLibrary: couldn't save " + libraryID + " data to MLSecureStorage API. result: " + result);
			}
			catch (Exception exception)
			{
				Debug.LogError("MLUBindingLibrary: error while saving " + libraryID + ". " + exception.GetType() + ": " + exception.Message);
			}
		}

		public override void DeleteLibrary ()
		{
			ClearLibrary();
			MLSecureStorage.DeleteData(LibraryStorageID);
		}
	}
}