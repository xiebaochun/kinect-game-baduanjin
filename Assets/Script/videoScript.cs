using UnityEngine;
using System.Collections;

public class videoScript : MonoBehaviour {

	private MovieTexture movieTex;

	public static float videoTime = 0; 

	// Use this for initialization
	void Start () {
	
		movieTex = myScript.myMovie;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (movieTex.isPlaying) 
		{
			videoTime += Time.deltaTime;
		}

		renderer.material.mainTexture = movieTex;
		if (myScript.videoPauseFlag) 
		{
			movieTex.Pause();
		} 
		else 
		{
			movieTex.Play();
		}

		if (videoTime >= myScript.videoTotalTime) 
		{
			movieTex.Stop();
			this.gameObject.SetActive(false);
		}

	}

	void OnGUI()
	{
		GUI.color = Color.black;
		//GUILayout.Label ("时间：" + videoTime);
	}
}
