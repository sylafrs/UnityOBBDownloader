﻿using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class GooglePlayDownloader
{
#if UNITY_ANDROID
	public static string APPLICATION_PUBLIC_KEY;
	
	//MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkmrmcuxbiRc01zMHjb0KlpSE4iqZHjsSmCL51s9vEkxIw8SYhuO81X0jtDts8/jNzjD3oa3Fo2LClRIQYU6yh0+YWt9OPhrAu34TsWuXTkvHXskupOE0uQZy6GFD/VOp//rQAUK+X8l/mB5aKW4fDv4t1PP0ZUsQ5NF4Jy5dM/htW7ZcW2FTaCrlIj2snLI4NK3ZVoJMtnMdr1yJXZm5X4BQl19Yp387ttyHALYSI1u70HiQH86otU7FZ1rhR5vczzV6xmHsF4bO+pbCAahWeJFP/b5sVXN1Q9uOkFTPSgkvKfvsIynm6Z+hokvOME1OmZW2xo8xlRr2KcpG+bYsXQIDAQAB
	
	private static AndroidJavaClass detectAndroidJNI;
	public static bool RunningOnAndroid()
	{
		if (detectAndroidJNI == null)
			detectAndroidJNI = new AndroidJavaClass("android.os.Build");
		return detectAndroidJNI.GetRawClass() != IntPtr.Zero;
	}
	
	private static AndroidJavaClass Environment;
	private const string Environment_MEDIA_MOUNTED = "mounted";

	static GooglePlayDownloader()
	{
		if (!RunningOnAndroid())
			return;

		Environment = new AndroidJavaClass("android.os.Environment");
		
		using (AndroidJavaClass dl_service = new AndroidJavaClass("com.unity3d.plugin.downloader.UnityDownloaderService"))
		{
	    // stuff for LVL -- MODIFY FOR YOUR APPLICATION!
			dl_service.SetStatic("BASE64_PUBLIC_KEY", APPLICATION_PUBLIC_KEY);
	    // used by the preference obfuscater
			dl_service.SetStatic("SALT", new byte[]{1, 43, 256-12, 256-1, 54, 98, 256-100, 256-12, 43, 2, 256-8, 256-4, 9, 5, 256-106, 256-108, 256-33, 45, 256-1, 84});
		}
	}
	
	public static string GetExpansionFilePath()
	{
		populateOBBData();

		if (Environment.CallStatic<string>("getExternalStorageState") != Environment_MEDIA_MOUNTED)
			return null;
			
		const string obbPath = "Android/obb";
			
		using (AndroidJavaObject externalStorageDirectory = Environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory"))
		{
			string root = externalStorageDirectory.Call<string>("getPath");
			return String.Format("{0}/{1}/{2}", root, obbPath, obb_package);
		}
	}
	public static string GetMainOBBPath(string expansionFilePath)
	{
		populateOBBData();

		if (expansionFilePath == null)
			return null;
		string main = String.Format("{0}/main.{1}.{2}.obb", expansionFilePath, obb_version, obb_package);
		if (!File.Exists(main))
			return null;
		return main;
	}
	public static string GetPatchOBBPath(string expansionFilePath)
	{
		populateOBBData();

		if (expansionFilePath == null)
			return null;
		string main = String.Format("{0}/patch.{1}.{2}.obb", expansionFilePath, obb_version, obb_package);
		if (!File.Exists(main))
			return null;
		return main;
	}
	public static void DeleteOBBExceptCurrent(string expansionFilePath)
	{
		string currentObb = String.Format("main.{0}.{1}.obb", obb_version, obb_package);
		DeleteAllFilesExcept(expansionFilePath, currentObb);
	}
	private static void DeleteAllFilesExcept(string expansionFilePath, string fileToKeep)
	{
		using (AndroidJavaObject expDir = new AndroidJavaObject("java.io.File", expansionFilePath))
		{
			string fileName;
			bool isDirectory;

			AndroidJavaObject [] files = expDir.Call<AndroidJavaObject[]>("listFiles");
			for(int i = 0; i < files.Length; i++)
			{
				fileName = files[i].Call<string>("getName");
				isDirectory = files[i].Call<bool>("isDirectory");

				if (!isDirectory && !fileName.Equals(fileToKeep))
					files[i].Call<bool>("delete");
			}
		}
	}
	public static void FetchOBB()
	{
		using (AndroidJavaClass unity_player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			AndroidJavaObject current_activity = unity_player.GetStatic<AndroidJavaObject>("currentActivity");
	
			AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent",
															current_activity,
															new AndroidJavaClass("com.unity3d.plugin.downloader.UnityDownloaderActivity"));
	
			int Intent_FLAG_ACTIVITY_NO_ANIMATION = 0x10000;
			intent.Call<AndroidJavaObject>("addFlags", Intent_FLAG_ACTIVITY_NO_ANIMATION);
			intent.Call<AndroidJavaObject>("putExtra", "unityplayer.Activity", 
														current_activity.Call<AndroidJavaObject>("getClass").Call<string>("getName"));
			current_activity.Call("startActivity", intent);
	
			if (AndroidJNI.ExceptionOccurred() != System.IntPtr.Zero)
			{
				Debug.LogError("Exception occurred while attempting to start DownloaderActivity - is the AndroidManifest.xml incorrect?");
				AndroidJNI.ExceptionDescribe();
				AndroidJNI.ExceptionClear();
			}
		}
	}
	
	// This code will reuse the package version from the .apk when looking for the .obb
	// Modify as appropriate
	private static string obb_package;
	private static int obb_version = 0;
	private static void populateOBBData()
	{
		if (obb_version != 0)
			return;
		using (AndroidJavaClass unity_player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			AndroidJavaObject current_activity = unity_player.GetStatic<AndroidJavaObject>("currentActivity");
			obb_package = current_activity.Call<string>("getPackageName");										// android.content.ContextWrapper.getPackageName
			AndroidJavaObject package_info = current_activity.Call<AndroidJavaObject>("getPackageManager")		// android.content.ContextWrapper.getPackageManager
				.Call<AndroidJavaObject>("getPackageInfo", obb_package, 0);
			obb_version = package_info.Get<int>("versionCode");

			Debug.Log("OBB Package is " + obb_package);
			Debug.Log("OBB Version is " + obb_version);
		}
	}
#endif
}
