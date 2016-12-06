using UnityEngine;
using System.Collections;

public class LaunchObb : MonoBehaviour {
	
	[Header("Settings")]
	public string GoogleKey;
	public bool UseObb;
	public int SceneToLoad = 1;


	[Header("Components")]
	public Material fadeMaterial;
	public SmoothScale progress;
	public TextMesh progressText;

	public virtual IEnumerator Start ()
	{
		yield return LoadObb();
		Application.LoadLevel(SceneToLoad);
	}

	protected Coroutine LoadObb()
	{
		return StartCoroutine(LoadObbEnum());
	}

	private IEnumerator LoadObbEnum()
	{
#if !UNITY_EDITOR && UNITY_ANDROID && USE_OBB

		if(UseObb)
		{
			GooglePlayDownloader.APPLICATION_PUBLIC_KEY = GoogleKey;

			if (!GooglePlayDownloader.RunningOnAndroid())
				yield break;
		
			string expPath = GooglePlayDownloader.GetExpansionFilePath();
			if (expPath == null)
				yield break;
		
			string mainOBBPath = GooglePlayDownloader.GetMainOBBPath(expPath);
			if (mainOBBPath == null)
			{
				GooglePlayDownloader.FetchOBB();			
				while (mainOBBPath == null)
				{
					yield return new WaitForEndOfFrame();
					mainOBBPath = GooglePlayDownloader.GetMainOBBPath(expPath);
				}
			}

			GooglePlayDownloader.DeleteOBBExceptCurrent(expPath);

			string uri = "file://" + mainOBBPath;
			Debug.Log("loading OBB : " + uri);

			if(progress)
				progress.SetTarget(0);
			if (progressText)
				progressText.text = "0%";

			WWW www = WWW.LoadFromCacheOrDownload(uri, 0);
			//while (www.progress < 1)
			//{
			//	if (progress)
			//		progress.SetTarget(www.progress);
			//	if (progressText)
			//		progressText.text = (int)(www.progress * 100) + "%";
			//	Debug.Log("OBB progress = " + www.progress);
			//
			//	yield return new WaitForEndOfFrame();
			//}
			//
			//if (progress)
			//	progress.SetTarget(1);
			//if (progressText)
			//	progressText.text = "100%";

			yield return www;
					
			Debug.Log("downloading finished " + uri);
			if (www.error != null)
			{
				Debug.Log("wwww error " + www.error);
				yield break;
			}
		}
#else
		yield break;
#endif
	}
}
