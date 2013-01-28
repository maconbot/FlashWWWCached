/**
 * Unity Flash Exporter LSO Helper Classes.
 * 
 * Copyright (C) 2013 Unity Technologies ApS
 *  
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

/**
 * Example of usage
 */

using UnityEngine;
using System.Collections;
using UnityEngine.Flash.LSO;

public class FlashLoadFromLSOExample : MonoBehaviour
{
	public AssetBundle Bundle;
	// Use this for initialization
	IEnumerator Start () {
		const string bundle = "http://test.unitzeroone.com/myassetbundle.unity3d";
		
		#if UNITY_FLASH && !UNITY_EDITOR//Only when the target is Flash and we are not in the editor (thus, in the Flash VM) ActionScript code will be executed, only then this path is useful.
		/*
		 * This has to be done at the start of the application, or at least before loading the first data from cache. 
		 * It has to be ran only once per application. Inform the user up front that they will have to press allow.
		 * If the user hasn't been on the site before, or has denied access before, a Flash dialog for storage permissions is displayed.
		 * 
		 * You can optionally pass arguments as to set the requested size, filename and local paths for the Local Shared Object.
		 */
		yield return StartCoroutine(FlashCachedWWW.InitializeAndRequestCache());
		
		/*
		 * This returns a FlashCachedWWW object, that has similar properties to a WWW object.
		 * 
		 * The arguments have similar effects as the WWW.LoadFromCacheOrDownloadCall, but backing store is now provided by the LSO initialized by the previous call.
		 */
		var www = FlashCachedWWW.LoadFromCacheOrDownload(bundle, 3);
		
		//Now yield "normally", as we need to startcoroutine it.
		yield return StartCoroutine(www);
		#else
		//The path for other platforms that do support LoadFromCacheOrDownload natively.
		var www = WWW.LoadFromCacheOrDownload(bundle, 3);
		yield return www;

		#endif
		if (www.error != null)
			Debug.LogError("error occured while downloading from www");
		else
			Bundle = www.assetBundle;

		Debug.Log(www.assetBundle);
	}
	
	
}
