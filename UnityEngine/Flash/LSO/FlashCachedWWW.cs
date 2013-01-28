/**
 * Unity Flash Exporter LSO Helper Classes.
 * 
 * Copyright (C) 2013 Unity Technologies ApS
 *  
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections;

namespace UnityEngine.Flash.LSO
{
	public class FlashCachedWWW : IEnumerator
	{
		enum FlashCachedWwwState
		{
			Invalid,
			Init,
			WWW_StartDownload,
			WWW_Wait,
			WWW_Complete,
			AssetBundle_Create,
			AssetBundle_Complete,
			Complete
		}

		private static void _ActionScriptImports()
		{
			ActionScript.Import("com.unitzeroone.CRC32");
		}
		
		private readonly string _url;
		private readonly uint _crc;
		private readonly string _storageName;
		private FlashCachedWwwState _state = FlashCachedWwwState.Invalid;
		
		public bool dontCreateAssetBundle { get; set; }

		public string error { get; private set; }
		public bool isDone { get; private set; }
		public float progress { get; private set; }
		public AssetBundle assetBundle { get; private set; }
		public byte[] bytes { get; private set; }

		private WWW _www;
		private AssetBundleCreateRequest _request;
		
		public object Current { get; private set; }
		
		private FlashCachedWWW(string url, int version, uint crc)
		{
			_state = FlashCachedWwwState.Init;
			_url = url;
			_crc = crc;
			_storageName = GetStorageNameFor(GetFileNameWithExtensionFrom(url), version);
		}

		public static FlashCachedWWW LoadFromCacheOrDownload(string url, int version, uint crc = 0)
		{
			return new FlashCachedWWW(url, version, crc);
		}

		public bool MoveNext()
		{
			if (_state == FlashCachedWwwState.Invalid)
				throw new Exception("Invalid state");

			var cache = FlashCachedWWWInitializer.FlashWWWLSOCache;
			switch (_state)
			{
				case FlashCachedWwwState.Init:
					if (cache == null)
						return Advance();
					
					if (!cache.HasInCache(_storageName))
						return Advance();

					bytes = cache.GetFromCache(_storageName);
					return AdvanceOrBranch(CrcCheckSumCorrect(), FlashCachedWwwState.AssetBundle_Create);

				case FlashCachedWwwState.WWW_StartDownload:
					_www = new WWW(_url);
					return Advance();
				
				case FlashCachedWwwState.WWW_Wait:
					progress = _www.progress;
					error = _www.error;
					isDone = _www.isDone;
					return AdvanceIf(_www.isDone);
				
				case FlashCachedWwwState.WWW_Complete:
					//Dispose of WWW.
					if (_www.error != null)
						return AdvanceTo(FlashCachedWwwState.Complete);

					bytes = _www.bytes;
					_www.Dispose();
					if (cache != null && CrcCheckSumCorrect())
						cache.StoreInCache(_storageName, bytes);

					return Advance();

				case FlashCachedWwwState.AssetBundle_Create:
					if (dontCreateAssetBundle)
						return false;
					
					_request = AssetBundle.CreateFromMemory(bytes);
					return Advance();

				case FlashCachedWwwState.AssetBundle_Complete:
					if(_request == null)
						throw new Exception("Failed to load asset bundle ");

					if (_request.isDone)
						assetBundle = _request.assetBundle;
					
					return AdvanceIf(_request.isDone);
				case FlashCachedWwwState.Complete:
					_www = null;
					return false;
			}
			return false;
		}

		private bool AdvanceOrBranch(bool condition, FlashCachedWwwState branch)
		{
			if (condition)
				_state = branch;
			return true;
		}

		private bool AdvanceIf(bool condition)
		{
			if(condition)
				_state += 1;
			return true;
		}

		private bool Advance()
		{
			_state += 1;
			return true;
		}
		
		private bool AdvanceTo(FlashCachedWwwState c)
		{
			_state = c;
			return true;
		}

		private bool CrcCheckSumCorrect()
		{
			return _crc == 0 || _crc == ActionScript.Expression<uint>("com.unitzeroone.CRC32.checkSum({0}.elements)", bytes);
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public static FlashCachedWWWInitializer InitializeAndRequestCache()
		{
			return new FlashCachedWWWInitializer();
		}

		public static string GetFileNameWithExtensionFrom(string url)
		{
			return ActionScript.Expression<string>("{0}.match({1})[0]", url, ActionScript.Expression<object>(@"/(?<=\/)(\w+)((\.\w+(?=\?))|(\.\w+)$)/g"));
		}

		public static string GetStorageNameFor(string name, int version)
		{
			return "lso_name_" + name + "_version_" + version;
		}
	}
}

