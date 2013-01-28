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

namespace UnityEngine.Flash.LSO
{
	public class LSOCache
	{
		private readonly string _name;
		private readonly int _minimumSize;
		private readonly Action<bool> _requestAction;
		private object _sharedObject;
		private Action<bool> _flushHandlerAction;
		private bool _canStoreInSharedObject;

		public static void _ActionScriptImports()
		{
			//Note, this function isn't used directly, but is needed for injecting the imports.
			ActionScript.Import("flash.net.SharedObject");
			ActionScript.Import("flash.net.SharedObjectFlushStatus");
			ActionScript.Import("flash.utils.ByteArray");
			ActionScript.Import("flash.events.NetStatusEvent");
		}

		public LSOCache(string sharedObjectName, int minimumSize, Action<bool> requestAction = null)
		{
			_name = sharedObjectName;
			_minimumSize = minimumSize;
			_requestAction = requestAction;
			
			var so = GetSharedObject(true);
			ActionScript.Statement("{0}.addEventListener(NetStatusEvent.NET_STATUS, asFlushEventHandler);", so);
		}

		public void RequestStorage()
		{
			object so = GetSharedObject(true);
			_flushHandlerAction = _requestAction;

			if (so == null)
				return;

			ActionScript.Statement("var success:Boolean = true; var status:String; try{status = {0}.flush({1})}catch(e:*){success = false}", so, _minimumSize);
			var st = ActionScript.Expression<string>("status");
			if (!ActionScript.Expression<bool>("success"))
				SetAndInvokeCanStoreInSharedObject(false);
			else
			{
				if (st != null && ActionScript.Expression<bool>("{0} == SharedObjectFlushStatus.FLUSHED", st))
					SetAndInvokeCanStoreInSharedObject(true);
			}
		}

		[NotRenamed]
		public void asFlushEventHandler(object o)
		{
			SetAndInvokeCanStoreInSharedObject(ActionScript.Expression<string>("{0}.info.code", o) == "SharedObject.Flush.Success");
		}

		private void SetAndInvokeCanStoreInSharedObject(bool canStoreInSharedObject)
		{
			_canStoreInSharedObject = canStoreInSharedObject;

			if (_flushHandlerAction != null)
				_flushHandlerAction(_canStoreInSharedObject);

			_flushHandlerAction = null;
		}

		public void Delete()
		{
			ActionScript.Statement("{0}.clear()", GetSharedObject());
		}

		public bool StoreInCache(string name, byte[] bytes)
		{
			var sharedObject = GetSharedObject();
			if (sharedObject == null)
				return false;

			ActionScript.Statement("{0}.data[{1}] = {2}.elements", sharedObject, name, bytes);
			return Flush();
		}

		public bool HasInCache(string name)
		{
			var sharedObject = GetSharedObject();
			if (sharedObject == null)
				return false;

			return ActionScript.Expression<bool>("{0} in ({1}.data)", name, sharedObject);
		}

		public byte[] GetFromCache(string name)
		{
			var sharedObject = GetSharedObject();
			if (sharedObject == null)
				return null;

			return !HasInCache(name) ? null : ActionScript.Expression<byte[]>("CLIByteArray.TakeOwnership({0} as ByteArray)", ActionScript.Expression<object>("{0}.data[{1}];", sharedObject, name));
		}

		public bool DeleteFileByName(string name)
		{
			var sharedObject = GetSharedObject();
			if (sharedObject == null)
				return false;

			if (!HasInCache(name))
				return false;

			ActionScript.Statement("{0}.data[{1}] = null", sharedObject, name);
			ActionScript.Statement("delete {0}.data[{1}]", sharedObject, name);
			return Flush();
		}

		private bool Flush()
		{
			var so = GetSharedObject();
			if (so == null)
				return false;

			ActionScript.Statement("var success:Boolean = true; try{{0}.flush()}catch(e:*){success = false}", so);
			return ActionScript.Expression<bool>("success");
		}

		private object GetSharedObject(bool force = false)
		{
			if (_sharedObject == null)
				_sharedObject = ActionScript.Expression<object>("SharedObject.getLocal({0});", _name);

			return (_canStoreInSharedObject | force) ? _sharedObject : null;
		}
	}
}
