using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.IO;

public class myScript : MonoBehaviour {

	//static float dx = Screen.width / 1600.0f;
	//static float dy = Screen.height / 900.0f;
	//static float dx = 1.0f;
	//static float dy = 1.0f;

	public const int GAMESTATE_GO_TAKEPHOTO = 0;
	public const int GAMESTATE_TAKEPHOTO = 1;
	public const int GAMESTATE_GO_HOME = 2;
	public const int GAMESTATE_HOME = 3;
	public const int GAMESTATE_GO_MAINMENU = 4;
	public const int GAMESTATE_MAINMENU = 5;
	public const int GAMESTATE_GO_PLAY = 6;
	public const int GAMESTATE_PLAY = 7;
	public const int GAMESTATE_ENDGAME = 8;
	
	public static int GameState = 2;

	private GameObject obj_Model,obj_Model2,obj_Model3;
	private GameObject obj_BG,obj_confirmBtn,obj_video,obj_startBtn,
					   obj_menupointer,obj_time,obj_score,obj_remind,
					   obj_score_final,obj_blackBG;
	private GameObject[] choiceBtn = new GameObject[4];
	private int gameKind = 0;
	//private bool showHands_Flag = true;

	private GameObject leftHand;
	private GameObject rightHand;

	Texture2D[] bgTexture = new Texture2D[7];
	public static Object[] TexNumber;
	//Texture2D TexNumberFrame;
	//Texture2D TexTimerRecord;

	GameObject obj_btn,obj_leftHand,obj_rightHand,obj_kinect;
	int range = 10;
	Vector3[] buttonOriginalPosition = new Vector3[4];
	float kinectMoveTime = 0;
	Texture2D[] kinectTexture = new Texture2D[2];
	bool kinect_MouseFlag = true;
	Vector3 pre_mousePosition = Vector3.zero;

	float[] confirmPressedStartTime = {0,0,0,0};
	bool[] confirmButtonPressed = {false,false,false,false};
	bool[] isButtonPressed = {false,false,false,false};

	
	List<int> videoStopList = new List<int>();
	List<int> videoStopList_test = new List<int>();
	List<List<List<int>>> referencePoseList = new List<List<List<int>>>();
	List<List<List<int>>> poseScoreList = new List<List<List<int>>>();

	public static int videoTotalTime = 761;		//视频总时长
	Texture2D[] remindTexture = new Texture2D[3];

	public GameObject prefab_Number;
	List<GameObject> scoreNumberList = new List<GameObject>();
	List<GameObject> timeNumberList = new List<GameObject>();
	GameObject obj_PointNumberFrame,obj_PointNumber,obj_PointTimerRecord;
		
	public AudioSource music_click;
	public AudioSource music_chim;
	public AudioSource music_BaDuanJin;

	private bool autoJoinGame_Flag = true;
	private float autoJoinGame_time = 10.0f;

	public static bool isAdjustAngle = false;


	public void Awake() 
	{		
		Screen.SetResolution(1600, 900, true);//自己想要的分辨率，比如1024*768，true表示全屏
		//dx = Screen.width / 1600.0f;
		//dy = Screen.height / 900.0f;
		//dx = 1.0f;
		//dy = 1.0f;

		Screen.fullScreen = true;
	}
	// Use this for initialization
	void Start () {

		//Screen.showCursor = false;		//隐藏鼠标

		obj_Model = GameObject.Find ("Constructor");	//矿工模型
		obj_Model2 = GameObject.Find ("rainbowMan_v6");	//火柴人模型
		obj_Model3 = GameObject.Find ("Takato");		//卡通人
		obj_Model.SetActive (false);
		//obj_Model2.SetActive (false);
		//obj_Model3.SetActive (false);

		leftHand = GameObject.Find ("hand_left");
		rightHand = GameObject.Find ("hand_right");
		bgTexture[0]= (Texture2D)Resources.Load("Textures/Background/takephotobg");
		bgTexture[1]= (Texture2D)Resources.Load("Textures/Background/layout-01");
		bgTexture[2]= (Texture2D)Resources.Load("Textures/Background/layout-03");
		bgTexture[3]= (Texture2D)Resources.Load("Textures/Background/layout-06");
		bgTexture[4]= (Texture2D)Resources.Load("Textures/Background/layout-0C");
		bgTexture[5]= (Texture2D)Resources.Load("Textures/Background/layout-0D");
		bgTexture[6]= (Texture2D)Resources.Load("Textures/Background/layout-04");
		obj_BG= GameObject.Find("BG");
		obj_confirmBtn = GameObject.Find("confirmBtn");
		obj_confirmBtn.SetActive (false);
		obj_video = GameObject.Find("videoPlane");
		obj_video.SetActive (false);
		obj_startBtn = GameObject.Find("startBtn");
		obj_startBtn.SetActive (false);
		choiceBtn[0] = GameObject.Find("btn_1");
		choiceBtn [0].SetActive (false);
		choiceBtn[1] = GameObject.Find("btn_2");
		choiceBtn [1].SetActive (false);
		choiceBtn[2] = GameObject.Find("btn_3");
		choiceBtn [2].SetActive (false);
		choiceBtn[3] = GameObject.Find("btn_4");
		choiceBtn [3].SetActive (false);
		obj_menupointer = GameObject.Find ("menupointer");
		obj_menupointer.SetActive (false);
		obj_time = GameObject.Find("time");
		obj_time.SetActive (false);
		obj_score = GameObject.Find("score");
		obj_score.SetActive (false);
		obj_remind = GameObject.Find("remind");
		obj_remind.SetActive (false);
		remindTexture[0] = (Texture2D)Resources.Load("Textures/remind_green");
		remindTexture[1] = (Texture2D)Resources.Load("Textures/remind_yellow");
		remindTexture[2] = (Texture2D)Resources.Load("Textures/remind_red");
		obj_score_final = GameObject.Find("score_final");
		obj_score_final.SetActive (false);
		obj_blackBG = GameObject.Find ("blackBG");
		obj_blackBG.SetActive (false);

		obj_leftHand = leftHand;
		obj_rightHand = rightHand;
		obj_kinect = GameObject.Find("btn_kinect");
		obj_kinect.SetActive (false);
		kinectTexture[0] = (Texture2D)Resources.Load("Textures/kinect");
		kinectTexture[1] = (Texture2D)Resources.Load("Textures/kinect_red");

		TexNumber = Resources.LoadAll("Textures/number");
		//TexNumberFrame = (Texture2D)Resources.Load ("Textures/numberFrame");
		//TexTimerRecord = (Texture2D)Resources.Load ("Textures/timerRecord");

		obj_PointNumberFrame = GameObject.Find ("PointNumberFrame");
		obj_PointNumber = GameObject.Find ("PointNumber");
		obj_PointTimerRecord = GameObject.Find ("PointTimerRecord");
		obj_PointNumberFrame.SetActive (false);
		obj_PointNumber.SetActive (false);
		obj_PointTimerRecord.SetActive (false);

		GetVideoStopList ();
		planA_PassScore = scoreLevelList[0] * poseSkeletonNumber / 2 + scoreLevelList[1] * poseSkeletonNumber / 2;

		GetTextureByFile ();
	}

	// Update is called once per frame
	void Update () {
		
		switch (GameState) {
			
		case GAMESTATE_GO_TAKEPHOTO:
			
			obj_Model.transform.position = new Vector3(-3900,-1000,obj_Model.transform.position.z);
			obj_Model2.transform.position = new Vector3(-6750,-1000,obj_Model2.transform.position.z);
			obj_Model3.transform.position = new Vector3(-3900,-1000,obj_Model3.transform.position.z);
			//showHands_Flag = true;
			leftHand.SetActive(true);
			rightHand.SetActive(true);
			obj_blackBG.SetActive(false);
			obj_BG.renderer.material.mainTexture = bgTexture[0];
			GameState = GAMESTATE_TAKEPHOTO;
			
			obj_confirmBtn.SetActive(true);
			buttonOriginalPosition[0] = obj_confirmBtn.transform.position;
			InitBtn();
			break;

		case GAMESTATE_TAKEPHOTO:

			if(checkButtonPressed(obj_confirmBtn,0))
			{
				obj_confirmBtn.SetActive(false);
				GameState = GAMESTATE_GO_HOME;
			}
			break;

		case GAMESTATE_GO_HOME:

			Screen.showCursor = true;		//显示鼠标
			obj_kinect.SetActive(true);

			music_BaDuanJin.Stop();
			videoPauseFlag = false;		//防止玩家在视频暂停的时候，切回主界面

			while(scoreNumberList.Count > 0) 
			{
				Destroy(scoreNumberList[0]);
				scoreNumberList.RemoveAt(0);
			}
			while(timeNumberList.Count > 0) 
			{
				Destroy(timeNumberList[0]);
				timeNumberList.RemoveAt(0);
			}
			obj_PointNumberFrame.SetActive(false);	//倒计时图片和数字
			obj_PointNumber.SetActive(false);
			obj_PointTimerRecord.SetActive (false);
			
			obj_video.SetActive (true);
			videoScript.videoTime = videoTotalTime;		//用来重置视频播放
			obj_time.SetActive(false);
			obj_score.SetActive(false);
			obj_Model.transform.position = new Vector3(-3900,-1000,obj_Model.transform.position.z);
			obj_Model2.transform.position = new Vector3(-6750,-1000,obj_Model2.transform.position.z);
			obj_Model3.transform.position = new Vector3(-3900,-1000,obj_Model3.transform.position.z);
			//showHands_Flag = true;
			leftHand.SetActive(true);
			rightHand.SetActive(true);
			obj_blackBG.SetActive(false);
			obj_BG.renderer.material.mainTexture = bgTexture[1];
			GameState = GAMESTATE_HOME;
			
			obj_startBtn.SetActive(true);
			buttonOriginalPosition[0] = obj_startBtn.transform.position;
			InitBtn();
			break;

		case GAMESTATE_HOME:
			
			if(checkButtonPressed(obj_startBtn,0))
			{
				videoScript.videoTime = 0;
				obj_startBtn.SetActive(false);
				GameState = GAMESTATE_GO_MAINMENU;

				isAdjustAngle = false;		//结束角度调节
			}

			if(Input.GetKeyDown (KeyCode.Escape)) 	//退出游戏
			{
				Application.Quit();
			}

			if(kinectMoveTime > 0)
			{
				kinectMoveTime -= Time.deltaTime;
				if(kinectMoveTime<=0)
				{
					kinectMoveTime = 0;
					obj_kinect.renderer.material.mainTexture = kinectTexture[0];//绿色图标
					
					int hr = Kinect.NativeMethods.NuiInitialize (Kinect.NuiInitializeFlags.UsesDepthAndPlayerIndex | Kinect.NuiInitializeFlags.UsesSkeleton | Kinect.NuiInitializeFlags.UsesColor);
					if (hr != 0) {
						Debug.Log("NuiInitialize Failed.");
					}
					
					hr = Kinect.NativeMethods.NuiSkeletonTrackingEnable (System.IntPtr.Zero, 0);
					if (hr != 0) {
						Debug.Log("Cannot initialize Skeleton Data.");
					}
					
					//Kinect.NativeMethods.NuiCameraSetAngle (0);	//开启角度调节前，把摄像机角度设置为0
					
					isAdjustAngle = true;		//开启角度调节
				}
			}
			else 
			{			
				Ray ray_start = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit_start;
				if (Physics.Raycast (ray_start, out hit_start)) 
				{
					if (((kinect_MouseFlag == true)&&(Input.GetMouseButtonUp (0))) ||
						((kinect_MouseFlag == false)&&(Input.mousePosition != pre_mousePosition)))
					{
						GameObject hit_Obj = hit_start.collider.gameObject;
						
						if(hit_Obj.name == "btn_kinect")
						{
							obj_kinect.renderer.material.mainTexture = kinectTexture[1];//红色图标

							kinectMoveTime = 1.0f;	//kinect按钮抖动时间
						}
					}
				}

				pre_mousePosition = Input.mousePosition;
			}

			break;

		case GAMESTATE_GO_MAINMENU:

			Screen.showCursor = false;		//隐藏鼠标
			obj_kinect.SetActive(false);

			autoJoinGame_time = 10.0f;
			autoJoinGame_Flag = true;		//自动进入游戏标志
			//showHands_Flag = true;
			leftHand.SetActive(true);
			rightHand.SetActive(true);
			obj_blackBG.SetActive(false);
			obj_menupointer.SetActive(true);
			obj_BG.renderer.material.mainTexture = bgTexture[2];
			GameState = GAMESTATE_MAINMENU;
			
			choiceBtn [0].SetActive (true);
			choiceBtn [1].SetActive (true);
			choiceBtn [2].SetActive (true);
			choiceBtn [3].SetActive (true);
			buttonOriginalPosition[0] = choiceBtn[0].transform.position;
			buttonOriginalPosition[1] = choiceBtn[1].transform.position;
			buttonOriginalPosition[2] = choiceBtn[2].transform.position;
			buttonOriginalPosition[3] = choiceBtn[3].transform.position;
			InitBtn();
			break;

		case GAMESTATE_MAINMENU:

			for(int i=0;i<choiceBtn.Length;i++)
			{
				if(checkButtonPressed(choiceBtn[i],i))
				{
					gameKind = 3 - i;					//由于更换顺序，所以做了这个转换
					choiceBtn [0].SetActive (false);	//游戏模式
					choiceBtn [1].SetActive (false);	//测试模式
					choiceBtn [2].SetActive (false);	//录像模式
					choiceBtn [3].SetActive (false);	//定点录像模式
					obj_menupointer.SetActive(false);
					GameState = GAMESTATE_GO_PLAY;
				}
			}

			if(autoJoinGame_Flag)
			{
				autoJoinGame_time -= Time.deltaTime;
				if(autoJoinGame_time <= 0)				//达到自动进入游戏时间
				{
					gameKind = 3;
					choiceBtn [0].SetActive (false);	//游戏模式
					choiceBtn [1].SetActive (false);	//测试模式
					choiceBtn [2].SetActive (false);	//录像模式
					choiceBtn [3].SetActive (false);	//定点录像模式
					obj_menupointer.SetActive(false);
					GameState = GAMESTATE_GO_PLAY;
				}
			}
			
			if(Input.GetKeyDown (KeyCode.Escape)) 	//退出游戏
			{
				Application.Quit();
			}
			break;

		case GAMESTATE_GO_PLAY:

			//showHands_Flag = false;
			leftHand.SetActive(false);
			rightHand.SetActive(false);
			obj_blackBG.SetActive(true);
			
			if(gameKind == 3)
				obj_BG.renderer.material.mainTexture = bgTexture[3];
			else if(gameKind == 2)
				obj_BG.renderer.material.mainTexture = bgTexture[4];
			else if(gameKind == 1)
				obj_BG.renderer.material.mainTexture = bgTexture[5];
			else if(gameKind == 0)
				obj_BG.renderer.material.mainTexture = bgTexture[6];

			if(gameKind == 0)
			{
				obj_Model.transform.position = new Vector3(0,50,obj_Model.transform.position.z);
				obj_Model2.transform.position = new Vector3(-6750,50,obj_Model2.transform.position.z);
				obj_Model3.transform.position = new Vector3(0,50,obj_Model3.transform.position.z);

				obj_PointNumberFrame.SetActive(true);	//倒计时图片和数字
				obj_PointNumber.SetActive(true);
				obj_PointNumberFrame.transform.position = new Vector3(-3300,1,-3300);
				obj_PointNumber.transform.position = new Vector3(-3300,2,-3300);
			}
			else 
			{
				obj_PointNumberFrame.transform.position = new Vector3(-6700,1,-3300);
				obj_PointNumber.transform.position = new Vector3(-6700,2,-3300);
				
				music_BaDuanJin.Play();
				obj_video.SetActive (true);
				obj_time.SetActive(true);
				if(gameKind != 1)
					obj_score.SetActive(true);

				obj_Model.transform.position = new Vector3(-3900,50,obj_Model.transform.position.z);
				obj_Model2.transform.position = new Vector3(-6750,50,obj_Model2.transform.position.z);
				obj_Model3.transform.position = new Vector3(-3900,50,obj_Model3.transform.position.z);
			}
			GameState = GAMESTATE_PLAY;


			if((gameKind == 0)||(gameKind == 1))
			{
				obj_PointTimerRecord.SetActive(true);	//记录次数
				if(gameKind == 0)
				{
					obj_PointTimerRecord.transform.position = new Vector3(6120,1,3000);
				}
				else //if(gameKind == 1)
				{
					obj_PointTimerRecord.transform.position = new Vector3(2125,1,3750);
				}
			}

			break;

		case GAMESTATE_PLAY:
			
			GetSkeletonList();
			IsPose();
			if(gameKind == 0)
			{
				Plan_PointTest();
			}
			else if(gameKind == 1)
			{
				Plan_Test();
			}
			else if(gameKind == 2)
			{
				Plan_A();
			}
			else //if(gameKind == 3)
			{
				Plan_B();
			}

			
			if(gameKind == 0)	//定点录像需要绘制的图标
			{
				obj_PointNumber.renderer.material.mainTexture = (Texture2D)TexNumber[(int)(pointTest_KeepTime + 0.9f)];
				DrawNumber(scoreNumberList,new Vector3(4700,2,3000),timerRecordTimes.ToString(),TexNumber,500,1.0f);
			}		
			
			if((gameKind == 1) || (gameKind == 2))
			{
				//绘制倒计时图标
				if (videoStopTime > 0)
				{
					obj_PointNumberFrame.SetActive(true);	//倒计时图片和数字
					obj_PointNumber.SetActive(true);
					obj_PointNumber.renderer.material.mainTexture = (Texture2D)TexNumber[(int)videoStopTime + 1];
				}
				else 
				{
					obj_PointNumberFrame.SetActive(false);	//倒计时图片和数字
					obj_PointNumber.SetActive(false);
				}
			}
			
			if(gameKind == 1)
			{
				//绘制记录次数
				DrawNumber(scoreNumberList,new Vector3(750,2,3750),timerRecordTimes.ToString(),TexNumber,500,1.0f);
				
			}
			else if((gameKind == 2) || (gameKind == 3))
			{
				//绘制分数
				if(plan_B_RecordTimes!=0)
				{
					obj_remind.SetActive(true);
					obj_remind.renderer.material.mainTexture = remindTexture[Mathf.Abs(plan_B_RecordTimes - 3)];
				}
				else 
				{
					obj_remind.SetActive(false);
					DrawNumber(scoreNumberList,new Vector3(750,2,3750),playerPoseScore.ToString(),TexNumber,500,1.0f);
				}
			}
			
			if((gameKind == 1) || (gameKind == 2) || (gameKind == 3))
			{
				//绘制视频播放时间
				string minutes = ((int)(videoScript.videoTime/60)).ToString();
				string seconds = ((int)(videoScript.videoTime%60)).ToString();
				if((int)(videoScript.videoTime/60) < 10)
				{
					minutes = "0" + minutes;
				}
				if((int)(videoScript.videoTime%60) < 10)
				{
					seconds = "0" + seconds;
				}
				DrawNumber(timeNumberList,new Vector3(750,2,2930),minutes + ":"+ seconds,TexNumber,500,1.0f);

			}

			if(videoScript.videoTime >= videoTotalTime)
			{
				if(music_BaDuanJin.isPlaying)
				{
					music_BaDuanJin.Stop();
				}

				obj_Model.transform.position = new Vector3(-3900,-1000,obj_Model.transform.position.z);
				obj_Model2.transform.position = new Vector3(-6750,-1000,obj_Model2.transform.position.z);//不再显示模型
				obj_Model3.transform.position = new Vector3(-3900,-1000,obj_Model3.transform.position.z);
				
				while(scoreNumberList.Count > 0) 
				{
					Destroy(scoreNumberList[0]);
					scoreNumberList.RemoveAt(0);
				}
				while(timeNumberList.Count > 0) 
				{
					Destroy(timeNumberList[0]);
					timeNumberList.RemoveAt(0);
				}
				obj_PointNumberFrame.SetActive (false);
				obj_PointNumber.SetActive (false);
				obj_PointTimerRecord.SetActive(false);

				poseTestRecord.Clear();
				waitPlayStop = true;				
				plan_B_RecordTimes = 0;
				plan_B_AnswerList.Clear();

				videoStopListRecord = 0;
				videoStopTime = 0;
				timerRecordTimes = 0;
				//showHands_Flag = true;
				leftHand.SetActive(true);
				rightHand.SetActive(true);
				obj_blackBG.SetActive(false);
				//obj_video.SetActive (false);
				if(gameKind != 1)
				{
					obj_score_final.SetActive(true);
				}
				obj_time.SetActive(false);
				obj_score.SetActive(false);
				obj_BG.renderer.material.mainTexture = bgTexture[2];
				GameState = GAMESTATE_ENDGAME;
				
				obj_confirmBtn.SetActive(true);
				buttonOriginalPosition[0] = obj_confirmBtn.transform.position;
				InitBtn();
			}

			break;

		case GAMESTATE_ENDGAME:

			if(checkButtonPressed(obj_confirmBtn,0))
			{
				videoScript.videoTime = 0;
				playerPoseScore = 0;
				playerTotalScore = 0;           //重新开始游戏，总分清零

				obj_confirmBtn.SetActive(false);
				obj_score_final.SetActive(false);
				GameState = GAMESTATE_GO_HOME;
			}
			
			if((gameKind == 2) || (gameKind == 3))
			{
				DrawNumber(scoreNumberList,new Vector3(-500,2,0),(playerTotalScore / videoStopList.Count).ToString(),TexNumber,900,1.8f);
			}
			break;
		}
		
		leftHand.transform.position = new Vector3 (KinectModelControllerV2.leftHandPosition.x, 3, 
		                                           KinectModelControllerV2.leftHandPosition.y);
		rightHand.transform.position = new Vector3 (KinectModelControllerV2.rightHandPosition.x, 3, 
		                                            KinectModelControllerV2.rightHandPosition.y);
		/*
		if (showHands_Flag) 
		{
			leftHand.transform.position = new Vector3 (KinectModelControllerV2.leftHandPosition.x, 3, 
                              					KinectModelControllerV2.leftHandPosition.y);
			rightHand.transform.position = new Vector3 (KinectModelControllerV2.rightHandPosition.x, 3, 
		                                        KinectModelControllerV2.rightHandPosition.y);
		} 
		else 
		{
			//leftHand.SetActive(false);
			//rightHand.SetActive(false);
			leftHand.transform.position = new Vector3 (KinectModelControllerV2.leftHandPosition.x, -1, 
			                                      KinectModelControllerV2.leftHandPosition.y);
			rightHand.transform.position = new Vector3 (KinectModelControllerV2.rightHandPosition.x, -1, 
			                                      KinectModelControllerV2.rightHandPosition.y);
		}
		*/

	}


	void OnGUI()
	{
		GUI.color = Color.black;
		//GUILayout.Label ("" + new Rect(buttonOriginalPosition[0].x - obj_btn.transform.localScale.x*1.5f,
		//                               buttonOriginalPosition[0].z - obj_btn.transform.localScale.z*1.5f,
		//                               obj_btn.transform.localScale.x*3,obj_btn.transform.localScale.z*3));
		//GUILayout.Label ("" + new Vector2(obj_leftHand.transform.position.x,obj_leftHand.transform.position.z));
		//GUILayout.Label ("" + new Vector2(obj_rightHand.transform.position.x,obj_rightHand.transform.position.z));
		//GUILayout.Label ("" + KinectModelControllerV2.sw.bonePos [0, 7]);

		
		//GUILayout.Label ("时间：" + videoScript.videoTime);
		//GUILayout.Label ("" + playerPoseScore);
		//GUILayout.Label ("" + playerTotalScore);

		//for(int i=0;i<now_Pose.Count;i++)
		//{
		//	GUILayout.Label ("" + skeletonPosition[i,0] + skeletonPosition[i,1] + now_Pose[i]);
		//}

		switch (GameState) {

		case GAMESTATE_PLAY:

			if(gameKind == 0)	//定点录像需要绘制的图标
			{
				//GUI.color = Color.white;
				//GUI.DrawTexture(new Rect(1025 * dx, 32 * dy, 219 * dx, 175 * dy), TexNumberFrame);
				//DrawImageNumber (1056 * dx, 116 * dy, ((int)(pointTest_KeepTime + 0.9f)).ToString(), TexNumber, 1, 0, -30, 1.1f);
				
				//GUI.DrawTexture(new Rect(62 * dx, 715 * dy, 250 * dx, 70 * dy), TexTimerRecord);
				//DrawImageNumber (300 * dx, 750 * dy, timerRecordTimes.ToString(), TexNumber, 1, 0, -10, 0.5f);
			}		

			if((gameKind == 1) || (gameKind == 2))
			{
				//绘制倒计时图标
				//if (videoStopTime > 0)
				//{
				//	GUI.color = Color.white;
				//	GUI.DrawTexture(new Rect(1365 * dx, 32 * dy, 219 * dx, 175 * dy), TexNumberFrame);
				//	DrawImageNumber (1396 * dx, 116 * dy, ((int)videoStopTime + 1).ToString(), TexNumber, 1, 0, -30, 1.1f);
				//}
			}

			if(gameKind == 1)
			{
				//绘制记录次数
				//GUI.color = Color.white;
				//GUI.DrawTexture(new Rect(462 * dx, 791 * dy, 250 * dx, 70 * dy), TexTimerRecord);
				//DrawImageNumber (700 * dx, 826 * dy, timerRecordTimes.ToString(), TexNumber, 1, 0, -10, 0.5f);
				
			}
			else if((gameKind == 2) || (gameKind == 3))
			{
				//绘制分数
				//GUI.color = Color.white;
				//if(plan_B_RecordTimes!=0)
				//{
				//	obj_remind.SetActive(true);
				//	obj_remind.renderer.material.mainTexture = remindTexture[Mathf.Abs(plan_B_RecordTimes - 3)];
				//}
				//else 
				//{
				//	obj_remind.SetActive(false);
				//	DrawImageNumber (700 * dx, 826 * dy, playerPoseScore.ToString(), TexNumber, 1, 0, -10, 0.5f);
				//}
			}
			
			if((gameKind == 1) || (gameKind == 2) || (gameKind == 3))
			{
				//绘制视频播放时间
				//GUI.color = Color.white;
				//string minutes = ((int)(videoScript.videoTime/60)).ToString();
				//string seconds = ((int)(videoScript.videoTime%60)).ToString();
				//if((int)(videoScript.videoTime/60) < 10)
				//{
				//	minutes = "0" + minutes;
				//}
				//if((int)(videoScript.videoTime%60) < 10)
				//{
				//	seconds = "0" + seconds;
				//}
				//DrawImageNumber (700 * dx, 744 * dy, minutes + ":"+ seconds , TexNumber, 1, 0, -10, 0.5f);
			}

			break;

		case GAMESTATE_ENDGAME:

			if((gameKind == 2) || (gameKind == 3))
			{
				//GUI.color = Color.white;
				//DrawImageNumber (787 * dx, 445 * dy, (playerTotalScore / videoStopList.Count).ToString(), TexNumber, 1, 0, -30, 1.1f);
			}

			break;
		}
	}


	private void DrawNumber(List<GameObject> numberList, Vector3 postion, string Number,Object[] TexNum,int Interval,float scale)
	{
		char[] chars = (Number.ToString()).ToCharArray();

		while(numberList.Count < chars.Length) 
		{
			numberList.Add ((GameObject)Instantiate (prefab_Number));
		} 

		while(numberList.Count > chars.Length) 
		{
			Destroy(numberList[0]);
			numberList.RemoveAt(0);
		}

		for(int i=0;i<chars.Length;i++)
		{
			numberList[i].transform.position = postion + new Vector3(-Interval*i,0,0);
			numberList[i].transform.localScale =new Vector3(80*scale,1,80*scale);
			numberList[i].renderer.material.mainTexture = GetTex(chars[i],TexNum);
		}
		/*
		while(numberList.Count != 0)
		{
			Destroy(numberList[0]);
			numberList.RemoveAt(0);
		}
		for(int i=0;i<chars.Length;i++)
		{
			numberList.Add((GameObject)Instantiate(prefab_Number));
			numberList[i].transform.position = postion + new Vector3(-Interval*i,0,0);
			numberList[i].transform.localScale *=scale;
			numberList[i].renderer.material.mainTexture = GetTex(chars[i],TexNum);
		}
		*/
	}
	public static void DrawImageNumber(float x, float y, string Number, Object[] TexNum, int drawMode, int rotAngle, int Interval,float scale)    
	{
		/*
		Texture2D tex = (Texture2D)TexNum[0];
		float width = tex.width * dx * scale;
		float height = tex.height * dy * scale;
		float Draw_x = 0;
		float Draw_y = y - height / 2;
		char[] chars = (Number.ToString()).ToCharArray();
		
		if (drawMode == 0) Draw_x = x - (chars.Length * width + (chars.Length - 1) * Interval) / 2;//¾ÓÖÐ
		else if (drawMode == 1) Draw_x = x;//¿¿×ó
		else if (drawMode == 2) Draw_x = x - chars.Length * width - (chars.Length - 1) * Interval;//¿¿ÓÒ
		else { Debug.Log("绘制模式错误！"); Draw_x = x; }
		
		GUIUtility.RotateAroundPivot(rotAngle, new Vector2(x, y));//¸ù¾Ý»æÖÆµã£¬Ðý×ª¶ÔÓ³µÄ¶ÈÊý
		foreach (char s in chars)
		{
			GUI.DrawTexture(new Rect(Draw_x, Draw_y, width, height), GetTex(s, TexNum));
			Draw_x += (width + Interval);
		}
		
		GUIUtility.RotateAroundPivot(-rotAngle, new Vector2(x, y));//»¹Ô­GUIµÄÐý×ª
		*/
	}
	public static Texture2D GetTex(char s, Object[] TexNum)
	{
		try
		{
			if (s.ToString() == ":") return (Texture2D)TexNum[10];
			else return (Texture2D)TexNum[int.Parse(s.ToString())];
		}
		catch (System.Exception e)
		{
			Debug.Log("找不到对应编号图片！");
			Debug.Log(e);
			return null;
		}
	}


	Vector3[,] skeletonPosition = new Vector3[12, 2];
	List<int> now_Pose = new List<int>();
	List<int> pre_Pose = new List<int>();
	int videoStopListRecord = 0;
	int[] stopTime_Array = { 0, 0, 0, 0, 0 };             //保存附近的5个暂停时间点
	int plan_B_RecordTimes = 0;
	List<int> plan_B_RecordList = new List<int>();      //这会是一个12*5的链表，用来记录5组全身骨骼的情况
	List<int> plan_B_AnswerList = new List<int>();      //这会是一个12个元素的链表，用来记录5组全身骨骼中，每条骨骼的最高得分
	int playerPoseScore = 0;        //用来显示玩家得分
	int playerTotalScore = 0;       //用来显示玩家总得分
	//List<int> scoreLevelList = new List<int>();
	int[] scoreLevelList = {5,3,1};		//分数档
	const int poseSkeletonNumber = 12;

	bool waitPlayStop = true;
	float planA_KeepTime = 0;
	float videoStopTime = 0;
	float videoStopTime_temp = 0;         //在录像模式时，如果两个录像点相隔太近，需要用这个变量来辅助计时
	float videoMaxStopTime = 3.0f;
	public static bool videoPauseFlag = false;
	int planA_PassScore = 50;		//动作合格最低分数

	float pointTest_delayTime = 0;
	float pointTest_KeepTime = 5.0f;
	float setPointTest_KeepTime = 5.0f;	//每5秒记录一次
	int timerRecordTimes = 0;

	List<int> poseTestRecord = new List<int>();

	int changeTime = 0;
	Vector3[] changePosition = {new Vector3 (20, 0, 0),new Vector3 (14, 0, -14),
		new Vector3 (0, 0, 20),new Vector3 (-14, 0, -14),
		new Vector3 (-20, 0, 0),new Vector3 (-14, 0, 14),
		new Vector3 (0, 0, -20),new Vector3 (14, 0, 14)};



	private void Plan_PointTest()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			pointTest_delayTime = 0;
			pointTest_KeepTime = setPointTest_KeepTime;
			timerRecordTimes = 0;
			GameState = GAMESTATE_GO_HOME;
			return;
		}
		
		if (pointTest_delayTime > 0)
		{
			pointTest_delayTime -= Time.deltaTime;
			if (pointTest_delayTime <= 0)
			{
				pointTest_KeepTime = setPointTest_KeepTime;
			}
		}
		
		pointTest_KeepTime -= Time.deltaTime;
		if ((pointTest_KeepTime <= 0) && (pointTest_delayTime <= 0))
		{
			pointTest_delayTime = 1.0f;
			timerRecordTimes++;
			
			int i = 0;
			MakeMyFile(0,"Head-ShoulderCenter:         " + now_Pose[i++]);
			MakeMyFile(0,"ShoulderCenter-ShoulderLeft: " + now_Pose[i++]);
			MakeMyFile(0,"ShoulderCenter-ShoulderRight:" + now_Pose[i++]);
			
			MakeMyFile(0,"ShoulderLeft-ElbowLeft:      " + now_Pose[i++]);
			MakeMyFile(0,"ElbowLeft-WristLeft:         " + now_Pose[i++]);
			
			MakeMyFile(0,"ShoulderRight-ElbowRight:    " + now_Pose[i++]);
			MakeMyFile(0,"ElbowRight-WristRight:       " + now_Pose[i++]);
			
			MakeMyFile(0,"ShoulderCenter-HipCenter:    " + now_Pose[i++]);
			
			MakeMyFile(0,"HipCenter-KneeLeft:          " + now_Pose[i++]);
			MakeMyFile(0,"KneeLeft-AnkleLeft:          " + now_Pose[i++]);
			
			MakeMyFile(0,"HipCenter-KneeRight:         " + now_Pose[i++]);
			MakeMyFile(0,"KneeRight-AnkleRight:        " + now_Pose[i++]);
			MakeMyFile(0,"-------------------------");//写入一条新log 
		}
	}
	
	private void Plan_Test()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			playerPoseScore = 0;
			playerTotalScore = 0;           //重新开始游戏，总分清零
			videoStopTime = 0;
			videoStopListRecord = 0;
			timerRecordTimes = 0;
			poseTestRecord.Clear();
			GameState = GAMESTATE_GO_HOME;
			return;
		}
		
		if((int)(videoScript.videoTime) == (videoStopList_test[videoStopListRecord] - videoMaxStopTime))
		{
			//waitPlayStop = false;
			videoStopListRecord++;
			if (videoStopListRecord >= videoStopList_test.Count)
			{
				videoStopListRecord = 0;
				//videoTimes++;
				//recordFlag = true;
			}
			if (videoStopTime <= 0)     //前一个记录点已经记录完成
			{
				videoStopTime = videoMaxStopTime;
			}
			else                       //前一个记录点没有记录完成，用辅助计时器，暂存间隔时间
			{
				videoStopTime_temp = videoMaxStopTime;
			}
			//videoPlayer.Pause();
		}
		
		if (videoStopTime_temp > 0)
		{
			videoStopTime_temp -= Time.deltaTime;
			if (videoStopTime <= 0)
			{
				videoStopTime = videoStopTime_temp;
				videoStopTime_temp = 0;
			}
		}
		
		if (videoStopTime > 0)
		{
			videoStopTime -= Time.deltaTime;
			if (videoStopTime <= 0)
			{
				//waitPlayStop = true;
				//videoPlayer.Resume();
				
				timerRecordTimes++;
				if (timerRecordTimes > videoStopList_test.Count)
				{
					timerRecordTimes = 0;
				}

				for (int iii = 0; iii < now_Pose.Count; iii++)
				{
					poseTestRecord.Add(now_Pose[iii]);
					pre_Pose[iii] = now_Pose[iii];
				}
			}
		}
		//else if (recordFlag)
		else if (videoScript.videoTime >= videoTotalTime)
		{
			#region 一遍视频播放完毕后，玩家整套动作的记录
			
			MakeMyFile(1,"--------------------------------------------------------------------------------------------------------");
			MakeMyFile(1,"--------------------------------Record Time:" + System.DateTime.Now.ToString() + "------------------------------------------");
			MakeMyFile(1,"--------------------------------------------------------------------------------------------------------");
			MakeMyFile(1,"<Node>");
			MakeMyFile(1,"   <Poses>");
			//for (int poseNumber = 0; poseNumber < videoStopList_test.Count; poseNumber++)
			int count = poseTestRecord.Count / now_Pose.Count;
			for (int poseNumber = 0; poseNumber < count; poseNumber++)
			{
				MakeMyFile(1,"      <Pose id=\"" + poseNumber + "\">");
				MakeMyFile(1,"          <time>" + videoStopList_test[poseNumber] + "</time>");
				MakeMyFile(1,"          <Head-ShoulderCenter>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 0]);
				MakeMyFile(1,"          </Head-ShoulderCenter>");
				MakeMyFile(1,"          <ShoulderCenter-ShoulderLeft>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 1]);
				MakeMyFile(1,"          </ShoulderCenter-ShoulderLeft>");
				MakeMyFile(1,"          <ShoulderCenter-ShoulderRight>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 2]);
				MakeMyFile(1,"          </ShoulderCenter-ShoulderRight>");
				MakeMyFile(1,"          <ShoulderLeft-ElbowLeft>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 3]);
				MakeMyFile(1,"          </ShoulderLeft-ElbowLeft>");
				MakeMyFile(1,"          <ElbowLeft-WristLeft>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 4]);
				MakeMyFile(1,"          </ElbowLeft-WristLeft>");
				MakeMyFile(1,"          <ShoulderRight-ElbowRight>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 5]);
				MakeMyFile(1,"          </ShoulderRight-ElbowRight>");
				MakeMyFile(1,"          <ElbowRight-WristRight>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 6]);
				MakeMyFile(1,"          </ElbowRight-WristRight>");
				MakeMyFile(1,"          <ShoulderCenter-HipCenter>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 7]);
				MakeMyFile(1,"          </ShoulderCenter-HipCenter>");
				MakeMyFile(1,"          <HipCenter-KneeLeft>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 8]);
				MakeMyFile(1,"          </HipCenter-KneeLeft>");
				MakeMyFile(1,"          <KneeLeft-AnkleLeft>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 9]);
				MakeMyFile(1,"          </KneeLeft-AnkleLeft>");
				MakeMyFile(1,"          <HipCenter-KneeRight>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 10]);
				MakeMyFile(1,"          </HipCenter-KneeRight>");
				MakeMyFile(1,"          <KneeRight-AnkleRight>");
				MakeMyFile(1,"          " + poseTestRecord[poseNumber * now_Pose.Count + 11]);
				MakeMyFile(1,"          </KneeRight-AnkleRight>");
				MakeMyFile(1,"      </Pose>");
			}
			MakeMyFile(1,"   </Poses>");
			MakeMyFile(1,"</Node>");
			
			
			//保存scv文档
			MakeMyFile(2,"RecordTime,VideoTime,Head-ShoulderCenter,ShoulderCenter-ShoulderLeft,ShoulderCenter-ShoulderRight,ShoulderLeft-ElbowLeft,ElbowLeft-WristLeft,ShoulderRight-ElbowRight,ElbowRight-WristRight,ShoulderCenter-HipCenter,HipCenter-KneeLeft,KneeLeft-AnkleLeft,HipCenter-KneeRight,KneeRight-AnkleRight");
			for (int poseNumber = 0; poseNumber < count; poseNumber++)
			{
				MakeMyFile(2,"" + System.DateTime.Now.ToString() + ","
				             + videoStopList_test[poseNumber] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 0] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 1] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 2] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 3] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 4] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 5] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 6] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 7] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 8] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 9] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 10] + ","
				             + poseTestRecord[poseNumber * now_Pose.Count + 11]);
			}
			
			
			poseTestRecord.Clear();
			#endregion
			
		}
		
	}

	private void Plan_A()
	{		
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			playerPoseScore = 0;
			playerTotalScore = 0;           //重新开始游戏，总分清零
			
			videoStopTime = 0;
			waitPlayStop = true;
			videoStopListRecord = 0;
			GameState = GAMESTATE_GO_HOME;
			return;
		}
		
		if ((waitPlayStop == true) &&
		    ((int)(videoScript.videoTime) == videoStopList[videoStopListRecord]))
		{
			waitPlayStop = false;
			planA_KeepTime = 0;
			videoStopListRecord++;
			videoStopTime = videoMaxStopTime;
			//videoPlayer.Pause();
			videoPauseFlag = true;
		}
		
		int now_PoseScore = 0;              //用来记录12条骨骼的累计得分
		if (waitPlayStop == false)
		{
			videoStopTime -= Time.deltaTime;
			planA_KeepTime -= Time.deltaTime;
			if (planA_KeepTime <= 0)
			{
				planA_KeepTime = 0.5f;		//0.5秒
				for (int i = 0; i < now_Pose.Count; i++)
				{
					int thisSkeletonScore = 0;      //用来暂时记录单条骨骼的分数
					for (int j = 0; j < referencePoseList[videoStopListRecord - 1][i].Count; j++)
					{
						int temp_score = GetOneSkeletonScore(now_Pose[i], referencePoseList[videoStopListRecord - 1][i][j]);
						if (temp_score > thisSkeletonScore)
						{
							thisSkeletonScore = temp_score;
						}
					}
					now_PoseScore += thisSkeletonScore;
				}
				playerPoseScore = (now_PoseScore * 100) / (scoreLevelList[0] * poseSkeletonNumber);   //将得分换算成百分制
				
				if ((now_PoseScore >= planA_PassScore) || (videoStopTime <= 0))          //动作得分符合要求，或者超过最长等待时间，就继续播放视频
				{
					playerTotalScore += playerPoseScore;        //将玩家此次得分，加入总得分
					
					videoStopTime = 0;
					waitPlayStop = true;
					//videoPlayer.Resume();
					videoPauseFlag = false;
					
					if (videoStopListRecord >= videoStopList.Count)
					{
						videoStopListRecord = 0;
					}
				}
			}
		}
		
	}
	
	private void Plan_B()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			playerPoseScore = 0;
			playerTotalScore = 0;           //重新开始游戏，总分清零
			
			plan_B_RecordTimes = 0;
			videoStopListRecord = 0;
			plan_B_AnswerList.Clear();
			GameState = GAMESTATE_GO_HOME;
			return;
		}
		
		int videoTime_Milliseconds = (int)(videoScript.videoTime * 1000);
		
		
		if (videoTime_Milliseconds <= 1000)     //以视频播放时长小于1秒，来重置计时标志
		{
			videoStopListRecord = 0;
			for (int i = 0; i < stopTime_Array.Length; i++)
			{
				stopTime_Array[i] = videoStopList[videoStopListRecord] * 1000 - 1000 + i * 500;
			}
		}
		
		
		if ((videoTime_Milliseconds > stopTime_Array[plan_B_RecordTimes]) && (videoStopListRecord < videoStopList.Count))
		{
			plan_B_RecordTimes++;
			for (int j = 0; j < now_Pose.Count; j++)
			{
				plan_B_RecordList.Add(now_Pose[j]);
			}
			
			if (plan_B_RecordTimes == stopTime_Array.Length)       //已经记录下5组骨骼数据
			{
				plan_B_RecordTimes = 0;
				plan_B_AnswerList.Clear();          //清空上一个计时点，玩家骨骼的得分链表
				for (int skeletonNumber = 0; skeletonNumber < now_Pose.Count; skeletonNumber++) //遍历12条骨骼
				{
					int thisSkeletonScore = 0;      //用来暂时记录单条骨骼的分数
					for (int k = 0; k < stopTime_Array.Length; k++)                             //遍历玩家单条骨骼的5次记录
					{
						for (int l = 0; l < referencePoseList[videoStopListRecord][skeletonNumber].Count; l++)  //遍历单条骨骼的所有正确答案
						{
							int temp_score = GetOneSkeletonScore(plan_B_RecordList[skeletonNumber + k * now_Pose.Count], referencePoseList[videoStopListRecord][skeletonNumber][l]);
							if (temp_score > thisSkeletonScore)
							{
								thisSkeletonScore = temp_score;
							}
						}
					}
					plan_B_AnswerList.Add(thisSkeletonScore);
				}
				
				videoStopListRecord++;          //进入下一个动作
				if (videoStopListRecord < videoStopList.Count)
				{
					for (int i = 0; i < stopTime_Array.Length; i++)
					{
						stopTime_Array[i] = videoStopList[videoStopListRecord] * 1000 - 1000 + i * 500;
					}
				}
				plan_B_RecordList.Clear();      //上一个动作的5套记录清除

				playerPoseScore = 0;
				for (int i = 0; i < plan_B_AnswerList.Count; i++)
				{
					playerPoseScore += plan_B_AnswerList[i];
				}
				playerPoseScore = (playerPoseScore * 100) / (scoreLevelList[0] * poseSkeletonNumber);   //将得分换算成百分制
				playerTotalScore += playerPoseScore;        //将玩家此次得分，加入总得分
			}
		}

	}
	
	private void MakeMyFile(int recordKind, string info)
	{		
		string path = Application.dataPath + "//Resources//Record";

		//System.IO.Directory.CreateDirectory (path);
		DirectoryInfo dir = new DirectoryInfo (path);
		if (!dir.Exists) 
		{
			dir.Create();
		}
		//if (!File.Exists (path)) 
		//{
		//	File.Create(path);
		//}

		string name = "PointTestRecord." + 
			System.DateTime.Now.Year + "-" +
				System.DateTime.Now.Month + "-" +
				System.DateTime.Now.Day + ".txt";

		if (recordKind == 1) 
		{
			name = "TestRecord." + 
					System.DateTime.Now.Year + "-" +
					System.DateTime.Now.Month + "-" +
					System.DateTime.Now.Day + ".txt";
		} 
		else if (recordKind == 2) 
		{
			name = "csv_TestRecord." + 
				System.DateTime.Now.Year + "-" +
					System.DateTime.Now.Month + "-" +
					System.DateTime.Now.Day + ".csv";
		}
		
		StreamWriter sw;
		FileInfo t = new FileInfo (path + "//" + name);
		if (!t.Exists)
		{
			sw = t.CreateText ();
		}
		else 
		{
			sw = t.AppendText();
		}
		sw.WriteLine (info);
		sw.Close ();
		sw.Dispose ();
	}

	private int GetOneSkeletonScore(int playerSkeleton,int answerSkeleton)
	{
		if (playerSkeleton == answerSkeleton)       //玩家动作符合最佳答案，返回3分
		{
			return scoreLevelList[0];
		}
		else
		{
			for (int i = 0; i < poseScoreList.Count; i++)
			{
				if (poseScoreList[i][0][0] == answerSkeleton)
				{
					for (int j = 0; j < poseScoreList[i][1].Count; j++)
					{
						if (poseScoreList[i][1][j] == playerSkeleton)   //玩家动作符合优秀答案，返回2分
						{
							return scoreLevelList[1];
						}
					}
					
					for (int j = 0; j < poseScoreList[i][2].Count; j++) //玩家动作符合一般答案，返回1分
					{
						if (poseScoreList[i][2][j] == playerSkeleton)
						{
							return scoreLevelList[2];
						}
					}
				}
			}
			return 0;                                                   //玩家动作不符合任何答案，返回0分
		}
	}

	void GetSkeletonList()
	{
		/*
		skeletonPosition[0] = sw.bonePos[player,3];		//头
		skeletonPosition[1] = sw.bonePos[player,1];		//肩膀中心
		skeletonPosition[2] = sw.bonePos[player,4];		//左肩
		skeletonPosition[3] = sw.bonePos[player,8];		//右肩
		skeletonPosition[4] = sw.bonePos[player,5];		//左手肘
		skeletonPosition[5] = sw.bonePos[player,6];		//左手腕
		skeletonPosition[6] = sw.bonePos[player,9];		//右手肘
		skeletonPosition[7] = sw.bonePos[player,10];	//右手腕
		skeletonPosition[8] = sw.bonePos[player,0];		//臀部中心
		skeletonPosition[9] = sw.bonePos[player,13];	//左膝盖
		skeletonPosition[10] = sw.bonePos[player,14];	//左脚踝
		skeletonPosition[11] = sw.bonePos[player,17];	//右膝盖
		skeletonPosition[12] = sw.bonePos[player,18];	//右脚踝
		*/
		skeletonPosition [0, 0] = KinectModelControllerV2.skeletonPosition [13];
		skeletonPosition [0, 1] = KinectModelControllerV2.skeletonPosition [1];

		skeletonPosition [1, 0] = KinectModelControllerV2.skeletonPosition [1];
		skeletonPosition [1, 1] = KinectModelControllerV2.skeletonPosition [2];
		
		skeletonPosition [2, 0] = KinectModelControllerV2.skeletonPosition [1];
		skeletonPosition [2, 1] = KinectModelControllerV2.skeletonPosition [3];
		
		skeletonPosition [3, 0] = KinectModelControllerV2.skeletonPosition [2];
		skeletonPosition [3, 1] = KinectModelControllerV2.skeletonPosition [4];
		
		skeletonPosition [4, 0] = KinectModelControllerV2.skeletonPosition [4];
		skeletonPosition [4, 1] = KinectModelControllerV2.skeletonPosition [5];
		
		skeletonPosition [5, 0] = KinectModelControllerV2.skeletonPosition [3];
		skeletonPosition [5, 1] = KinectModelControllerV2.skeletonPosition [6];
		
		skeletonPosition [6, 0] = KinectModelControllerV2.skeletonPosition [6];
		skeletonPosition [6, 1] = KinectModelControllerV2.skeletonPosition [7];
		
		skeletonPosition [7, 0] = KinectModelControllerV2.skeletonPosition [1];
		skeletonPosition [7, 1] = KinectModelControllerV2.skeletonPosition [8];
		
		skeletonPosition [8, 0] = KinectModelControllerV2.skeletonPosition [8];
		skeletonPosition [8, 1] = KinectModelControllerV2.skeletonPosition [9];
		
		skeletonPosition [9, 0] = KinectModelControllerV2.skeletonPosition [9];
		skeletonPosition [9, 1] = KinectModelControllerV2.skeletonPosition [10];
		
		skeletonPosition [10, 0] = KinectModelControllerV2.skeletonPosition [8];
		skeletonPosition [10, 1] = KinectModelControllerV2.skeletonPosition [11];
		
		skeletonPosition [11, 0] = KinectModelControllerV2.skeletonPosition [11];
		skeletonPosition [11, 1] = KinectModelControllerV2.skeletonPosition [12];
		/*
		skeletonList [0].Add (KinectModelControllerV2.skeletonPosition [0]);
		skeletonList [0].Add (KinectModelControllerV2.skeletonPosition [1]);

		skeletonList [1].Add (KinectModelControllerV2.skeletonPosition [1]);
		skeletonList [1].Add (KinectModelControllerV2.skeletonPosition [2]);
		
		skeletonList [2].Add (KinectModelControllerV2.skeletonPosition [1]);
		skeletonList [2].Add (KinectModelControllerV2.skeletonPosition [3]);
		
		skeletonList [3].Add (KinectModelControllerV2.skeletonPosition [2]);
		skeletonList [3].Add (KinectModelControllerV2.skeletonPosition [4]);
		
		skeletonList [4].Add (KinectModelControllerV2.skeletonPosition [4]);
		skeletonList [4].Add (KinectModelControllerV2.skeletonPosition [5]);
		
		skeletonList [5].Add (KinectModelControllerV2.skeletonPosition [3]);
		skeletonList [5].Add (KinectModelControllerV2.skeletonPosition [6]);
		
		skeletonList [6].Add (KinectModelControllerV2.skeletonPosition [6]);
		skeletonList [6].Add (KinectModelControllerV2.skeletonPosition [7]);
		
		skeletonList [7].Add (KinectModelControllerV2.skeletonPosition [1]);
		skeletonList [7].Add (KinectModelControllerV2.skeletonPosition [8]);
		
		skeletonList [8].Add (KinectModelControllerV2.skeletonPosition [8]);
		skeletonList [8].Add (KinectModelControllerV2.skeletonPosition [9]);
		
		skeletonList [9].Add (KinectModelControllerV2.skeletonPosition [9]);
		skeletonList [9].Add (KinectModelControllerV2.skeletonPosition [10]);
		
		skeletonList [10].Add (KinectModelControllerV2.skeletonPosition [8]);
		skeletonList [10].Add (KinectModelControllerV2.skeletonPosition [11]);
		
		skeletonList [11].Add (KinectModelControllerV2.skeletonPosition [11]);
		skeletonList [11].Add (KinectModelControllerV2.skeletonPosition [12]);
		*/
	}

	void IsPose()
	{
		int location = 0;

		for(int i=0; i<poseSkeletonNumber; i++)
		{
			location = GetJointLocation(skeletonPosition[i,0],skeletonPosition[i,1]);
						
			if (now_Pose.Count < poseSkeletonNumber)
			{
				now_Pose.Add(location);     //确定链表元素数量
				pre_Pose.Add(0);
			}
			else
			{
				now_Pose[i] = location;         //记录动作
			}
		}
	}

	int GetJointLocation(Vector3 centerJoint,Vector3 angleJoint)
	{
		float vector3D_x = angleJoint.x - centerJoint.x;
		float vector3D_y = angleJoint.y - centerJoint.y;
		float vector3D_z = angleJoint.z - centerJoint.z;
		
		float angle_x, angle_y, angle_z;
		
		angle_x = GetAngle(vector3D_y, vector3D_z, vector3D_x);
		angle_y = GetAngle(vector3D_x, vector3D_z, vector3D_y);
		angle_z = GetAngle(vector3D_x, vector3D_y, vector3D_z);
		
		
		int result_X = HelpJudge(angle_x);
		int result_Y = HelpJudge(angle_y);
		int result_Z = HelpJudge(angle_z);

		int[] temp_x = new int[25];
		int[] temp_y = new int[25];
		int[] temp_z = new int[25];
		
		if (result_X == 1)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_x[i] = 105 + i * 5;
				temp_x[i + 5] = 205 + i * 5;
				temp_x[i + 10] = 305 + i * 5;
				temp_x[i + 15] = 405 + i * 5;
				temp_x[i + 20] = 505 + i * 5;
			}
		}
		else if (result_X == 2)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_x[i] = 104 + i * 5;
				temp_x[i + 5] = 204 + i * 5;
				temp_x[i + 10] = 304 + i * 5;
				temp_x[i + 15] = 404 + i * 5;
				temp_x[i + 20] = 504 + i * 5;
			}
		}
		else if (result_X == 3)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_x[i] = 103 + i * 5;
				temp_x[i + 5] = 203 + i * 5;
				temp_x[i + 10] = 303 + i * 5;
				temp_x[i + 15] = 403 + i * 5;
				temp_x[i + 20] = 503 + i * 5;
			}
		}
		else if (result_X == 4)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_x[i] = 102 + i * 5;
				temp_x[i + 5] = 202 + i * 5;
				temp_x[i + 10] = 302 + i * 5;
				temp_x[i + 15] = 402 + i * 5;
				temp_x[i + 20] = 502 + i * 5;
			}
		}
		else if (result_X == 5)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_x[i] = 101 + i * 5;
				temp_x[i + 5] = 201 + i * 5;
				temp_x[i + 10] = 301 + i * 5;
				temp_x[i + 15] = 401 + i * 5;
				temp_x[i + 20] = 501 + i * 5;
			}
		}
		
		if (result_Y == 1)
		{
			for (int i = 0; i < 25; i++)
			{
				temp_y[i] = 101 + i;
			}
		}
		else if (result_Y == 2)
		{
			for (int i = 0; i < 25; i++)
			{
				temp_y[i] = 201 + i;
			}
		}
		else if (result_Y == 3)
		{
			for (int i = 0; i < 25; i++)
			{
				temp_y[i] = 301 + i;
			}
		}
		else if (result_Y == 4)
		{
			for (int i = 0; i < 25; i++)
			{
				temp_y[i] = 401 + i;
			}
		}
		else if (result_Y == 5)
		{
			for (int i = 0; i < 25; i++)
			{
				temp_y[i] = 501 + i;
			}
		}
		
		if (result_Z == 1)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_z[i] = 121 + i;
				temp_z[i + 5] = 221 + i;
				temp_z[i + 10] = 321 + i;
				temp_z[i + 15] = 421 + i;
				temp_z[i + 20] = 521 + i;
			}
		}
		else if (result_Z == 2)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_z[i] = 116 + i;
				temp_z[i + 5] = 216 + i;
				temp_z[i + 10] = 316 + i;
				temp_z[i + 15] = 416 + i;
				temp_z[i + 20] = 516 + i;
			}
		}
		else if (result_Z == 3)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_z[i] = 111 + i;
				temp_z[i + 5] = 211 + i;
				temp_z[i + 10] = 311 + i;
				temp_z[i + 15] = 411 + i;
				temp_z[i + 20] = 511 + i;
			}
		}
		else if (result_Z == 4)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_z[i] = 106 + i;
				temp_z[i + 5] = 206 + i;
				temp_z[i + 10] = 306 + i;
				temp_z[i + 15] = 406 + i;
				temp_z[i + 20] = 506 + i;
			}
		}
		else if (result_Z == 5)
		{
			for (int i = 0; i < 5; i++)
			{
				temp_z[i] = 101 + i;
				temp_z[i + 5] = 201 + i;
				temp_z[i + 10] = 301 + i;
				temp_z[i + 15] = 401 + i;
				temp_z[i + 20] = 501 + i;
			}
		}
		
		int temp = 0;
		bool getResult = false;
		for (int i = 0; i < 25 && getResult == false; i++)
		{
			for (int j = 0; j < 25 && getResult == false; j++)
			{
				if (temp_x[i] == temp_y[j])
				{
					temp = temp_y[j];
					for (int k = 0; k < 25 && getResult == false; k++)
					{
						if (temp == temp_z[k])
						{
							getResult = true;
						}
					}
				}
			}
		}
		
		return temp;
	}
	
	private float GetAngle(float a, float b, float c)
	{
		float duibian = c;
		float linbian = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
		
		float angleRad = Mathf.Atan(duibian / linbian);
		float angleDeg = angleRad * 180 / Mathf.PI;
		
		return angleDeg;
	}
	
	private int HelpJudge(double angle)
	{
		if (angle > 54)
		{
			return 1;
		}
		else if (angle > 18)
		{
			return 2;
		}
		else if (angle > -18)
		{
			return 3;
		}
		else if (angle > -54)
		{
			return 4;
		}
		else
		{
			return 5;
		}
	}

	void InitBtn()
	{
		for (int i=0; i<confirmPressedStartTime.Length; i++) 
		{
			confirmPressedStartTime [i] = 0;
			confirmButtonPressed [i] = false;
			isButtonPressed [i] = false;
		}
	}

	public bool checkButtonPressed(GameObject btn,int number)
	{
		obj_btn = btn;
		if (checkButtonPressing(number))
		{
			System.Random RandNum = new System.Random();
			int rx = RandNum.Next(1, range);
			int ry = RandNum.Next(1, range);
			obj_btn.transform.position = new Vector3(buttonOriginalPosition[number].x + rx*12 - range*6,
			                                         buttonOriginalPosition[number].y,
			                                         buttonOriginalPosition[number].z + ry*12 - range*6);
			if (confirmPressedStartTime[number] > 1)
			{				
				return true;
			}
		}
		
		return false;
	}
	
	public bool checkButtonPressing(int number)
	{
		bool result = false;
		
		result = checkButtonPointedRectangle (number);
		if (result) 
		{
			obj_menupointer.transform.position = new Vector3(obj_menupointer.transform.position.x,
			                                                 obj_menupointer.transform.position.y,
			                                                 -1737 + 1055 * number);

			confirmPressedStartTime[number] += Time.deltaTime;
			if (confirmButtonPressed[number] == false && confirmPressedStartTime[number] > 2) 
			{
				confirmPressedStartTime[number] = 0;
				confirmButtonPressed[number] = true;
					//soundEffectChim.Play();	//播放按键按下的声音
				music_chim.Play();
			}
		} 
		else if (confirmButtonPressed[number] == true) 
		{
			confirmPressedStartTime[number] += Time.deltaTime;
		}
		return confirmButtonPressed[number];
	}
	
	public bool checkButtonPointedRectangle(int number)
	{
		Rect confirmBtn_rect = new Rect(buttonOriginalPosition[number].x - obj_btn.transform.localScale.x*6.0f,
		                                buttonOriginalPosition[number].z - obj_btn.transform.localScale.z*6.0f,
		                                obj_btn.transform.localScale.x*12.0f,obj_btn.transform.localScale.z*12.0f);
		Vector2 leftHandPosition = new Vector2(obj_leftHand.transform.position.x,obj_leftHand.transform.position.z - 300);
		Vector2 rightHandPosition = new Vector2(obj_rightHand.transform.position.x,obj_rightHand.transform.position.z - 300);
		if ((confirmBtn_rect.Contains (leftHandPosition)) || 
		    (confirmBtn_rect.Contains (rightHandPosition)))
		{
			changeTime ++;
			//int sx = (int)(Mathf.Sin((changeTime * 360 / 1000) * Mathf.PI / 180) * range);
			//int sy = (int)(Mathf.Cos((changeTime * 360 / 1000) * Mathf.PI / 180) * range);		
			//obj_btn.transform.position = new Vector3(buttonOriginalPosition[number].x + sx*12 - range*6,
			//                                         buttonOriginalPosition[number].y,
			//                                         buttonOriginalPosition[number].z + sy*12 - range*6);
			obj_btn.transform.position = buttonOriginalPosition[number] + changePosition[(changeTime/4)%8];
			if (isButtonPressed[number] == false)
			{
				if(number != 0)					//除非玩家按到的是“游戏模式”
					autoJoinGame_Flag = false;	//否则都算玩家有按过按键，取消自动进入游戏标志
				//soundEffectClick.Play();		//播放按键声音
				music_click.Play();
				confirmPressedStartTime[number] = 0;
				isButtonPressed[number] = true;
			}
		}
		else
		{
			obj_btn.transform.position = buttonOriginalPosition[number];
			isButtonPressed[number] = false;
		}
		
		return isButtonPressed[number];
	}

	private void GetVideoStopList()
	{
		XmlDocument xmldoc = new XmlDocument ();
		XmlNode root;
		xmldoc.Load (Application.dataPath + "/Resources/points.xml");
		root = xmldoc.SelectSingleNode("Node");
		XmlNodeList pointNodeList = root.SelectSingleNode("points").ChildNodes;
		
		for (int i = 0; i < pointNodeList.Count; i++)
		{
			poseScoreList.Add(new List<List<int>>());
			
			poseScoreList[i].Add(GetList(pointNodeList[i]["correctpoints"].InnerText));
			poseScoreList[i].Add(GetList(pointNodeList[i]["goodpoints"].InnerText));
			poseScoreList[i].Add(GetList(pointNodeList[i]["okpoints"].InnerText));
		}

		xmldoc.Load(Application.dataPath + "/Resources/BaDuanJin.xml");
		root = xmldoc.SelectSingleNode("Node");
		XmlNodeList poseNodeList = root.SelectSingleNode("Poses").ChildNodes;
		
		for (int i = 0; i < poseNodeList.Count; i++)
		{
			videoStopList.Add(int.Parse(poseNodeList[i]["time"].InnerText));
			
			referencePoseList.Add(new List<List<int>>());
			
			referencePoseList[i].Add(GetList(poseNodeList[i]["Head-ShoulderCenter"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["ShoulderCenter-ShoulderLeft"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["ShoulderCenter-ShoulderRight"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["ShoulderLeft-ElbowLeft"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["ElbowLeft-WristLeft"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["ShoulderRight-ElbowRight"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["ElbowRight-WristRight"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["ShoulderCenter-HipCenter"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["HipCenter-KneeLeft"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["KneeLeft-AnkleLeft"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["HipCenter-KneeRight"].InnerText));
			referencePoseList[i].Add(GetList(poseNodeList[i]["KneeRight-AnkleRight"].InnerText));
		}

		xmldoc.Load(Application.dataPath + "/Resources/setting.xml");
		root = xmldoc.SelectSingleNode("Node");
		string poseRecordTime = root ["poseRecordTime"].InnerText;
		videoStopList_test = GetList (poseRecordTime);

		string tempFlag = root ["isMouse"].InnerText;
		if (tempFlag == "true") 
		{
			kinect_MouseFlag = true;
		} 
		else 
		{
			kinect_MouseFlag = false;
		}

		videoTotalTime = int.Parse (root ["videoTime"].InnerText);
	}
	
	private List<int> GetList(string str)
	{
		List<string> strList = str.Split(',').ToList();
		List<int> intList = new List<int>();
		for (int i = 0; i < strList.Count; i++)
		{
			intList.Add(int.Parse(strList[i]));
		}
		return intList;
	}

	public static MovieTexture myMovie;
	private void GetTextureByFile()
	{
		WWW www = new WWW ("file://");
		string texturePath = "" + Application.dataPath + "/Resources/BG_Texture";
		List<string> textureList = GetNameList (texturePath);
			
		if (textureList.Count >= 4) 
		{
			www = new WWW ("file://" + texturePath + "//" + textureList [0]);
			//yield return www;
			bgTexture [3] = www.texture;

			www = new WWW ("file://" + texturePath + "//" + textureList [1]);
			bgTexture [4] = www.texture;

			www = new WWW ("file://" + texturePath + "//" + textureList [2]);
			bgTexture [5] = www.texture;

			www = new WWW ("file://" + texturePath + "//" + textureList [3]);
			bgTexture [6] = www.texture;
		} 
		else 
		{
			Debug.Log("need more BG texture!");
		}

		string videoPath = "" + Application.dataPath + "/Resources/BG_Texture/video";
		List<string> videoList = GetNameList (videoPath);

		if (videoList.Count > 0) 
		{
			www = new WWW ("file://" + videoPath + "//" + videoList [0]);
			myMovie = www.movie;
			music_BaDuanJin.clip = myMovie.audioClip;
		} 
		else 
		{
			Debug.Log("need more video!");
		}
	}

	private List<string> GetNameList(string path)
	{
		DirectoryInfo dir = new DirectoryInfo (path);
		List<string> tempList = new List<string> ();
		foreach (FileInfo fi in dir.GetFiles()) 
		{
			if(fi.Extension != ".meta")
				tempList.Add(fi.Name);
		}

		return tempList;
	}

}
