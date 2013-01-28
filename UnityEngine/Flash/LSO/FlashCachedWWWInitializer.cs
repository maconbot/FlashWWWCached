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
	public class FlashCachedWWWInitializer : IEnumerator
	{
		private readonly string _lsoName;
		private readonly int _minimumSize;
		private LSOCache _cache;
		private bool _callbackMade;
		public static LSOCache FlashWWWLSOCache;
		
		public FlashCachedWWWInitializer(string lsoName = "unity_cachedfiles", int minimumSize = 104857600)//By default, request 100mb
		{
			_lsoName = lsoName;
			_minimumSize = minimumSize;
		}

		private void CacheCallback(bool success)
		{
			_callbackMade = true;
			if (success)
				FlashWWWLSOCache = _cache;
		}

		public bool MoveNext()
		{
			if(_cache != null)
				return !_callbackMade;
			
			_cache = new LSOCache(_lsoName, _minimumSize, CacheCallback);
			_cache.RequestStorage();

			return true;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public object Current { get; private set; }
	}
}
