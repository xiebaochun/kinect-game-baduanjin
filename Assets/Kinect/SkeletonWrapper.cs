using UnityEngine;
using System.Collections;

public class SkeletonWrapper : MonoBehaviour {
	
	public DeviceOrEmulator devOrEmu;
	private Kinect.KinectInterface kinect;
	
	private bool updatedSkeleton = false;
	private bool newSkeleton = false;
	
	[HideInInspector]
	public Kinect.NuiSkeletonTrackingState[] players;
	[HideInInspector]
	public int[] trackedPlayers;
	[HideInInspector]
	public Vector3[,] bonePos;
	[HideInInspector]
	public Vector3[,] bonePos_2;
	[HideInInspector]
	public Vector3[,] boneVel;
	[HideInInspector]
	public Kinect.NuiSkeletonPositionTrackingState[,] boneState;

	[HideInInspector]
	public Vector2 headPosition;
	private long set_KinectAngle = 0;
	private float set_KinectAngleKeepTime = 3.0f;
	private float set_KinectAngleTime = 2.0f;

	private bool needMove_Flag = true;
	private long needMove_Angle = -5;            //下移角度
	private float needMove_StartTime = 384.0f;       //下移开始时间点
	private float needMove_EndTime = 463.0f;         //下移结束时间点

	//private bool pre_isAdjustAngle = false;

	private System.Int64 ticks;
	private float deltaTime;
	private Matrix4x4 kinectToWorld;
	
	// Use this for initialization
	void Start () {
		kinect = devOrEmu.getKinect();
		players = new Kinect.NuiSkeletonTrackingState[Kinect.Constants.NuiSkeletonCount];
		trackedPlayers = new int[Kinect.Constants.NuiSkeletonMaxTracked];
		trackedPlayers[0] = -1;
		trackedPlayers[1] = -1;
		bonePos = new Vector3[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		bonePos_2 = new Vector3[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		boneVel = new Vector3[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		boneState = new Kinect.NuiSkeletonPositionTrackingState[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		
		//create the transform matrix that converts from kinect-space to world-space
		Matrix4x4 trans = new Matrix4x4();
		trans.SetTRS( new Vector3(-kinect.getKinectCenter().x,
		                          kinect.getSensorHeight()-kinect.getKinectCenter().y,
		                          -kinect.getKinectCenter().z),
		             Quaternion.identity, Vector3.one );
		Matrix4x4 rot = new Matrix4x4();
		Quaternion quat = new Quaternion();
		double theta = Mathf.Atan((kinect.getLookAt().y+kinect.getKinectCenter().y-kinect.getSensorHeight()) / (kinect.getLookAt().z + kinect.getKinectCenter().z));
		float kinectAngle = (float)(theta * (180 / Mathf.PI));
		quat.eulerAngles = new Vector3(-kinectAngle, 0, 0);
		rot.SetTRS( Vector3.zero, quat, Vector3.one );
		Matrix4x4 flip = Matrix4x4.identity;
		flip[2,2] = -1;
		//final transform matrix offsets the rotation of the kinect, then translates to a new center
		kinectToWorld = flip*trans*rot;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (myScript.GameState != myScript.GAMESTATE_PLAY) 
		{
			set_KinectAngleKeepTime -= Time.deltaTime;
			if (set_KinectAngleKeepTime <= 0) 
			{
				set_KinectAngleKeepTime = set_KinectAngleTime;

				/*
				if((pre_isAdjustAngle == false) && (myScript.isAdjustAngle == true))
				{
					pre_isAdjustAngle = true;
					Kinect.NativeMethods.NuiCameraSetAngle(-15);
				}
				else if(myScript.isAdjustAngle)
				{
					adjustKinectSensor ();
				}
				*/
				if(myScript.isAdjustAngle)
				{
					adjustKinectSensor ();
				}
			}
		} 
		else 
		{
			if((videoScript.videoTime >= needMove_StartTime) && (videoScript.videoTime < needMove_EndTime)
			   &&(needMove_Flag == true))
			{
				needMove_Flag = false;
				if((set_KinectAngle + needMove_Angle) >= -15)
				{
					Kinect.NativeMethods.NuiCameraSetAngle(set_KinectAngle + needMove_Angle);
				}
				else 
				{
					Kinect.NativeMethods.NuiCameraSetAngle(-15);
				}
			}
			else if((videoScript.videoTime >= needMove_EndTime)&&(needMove_Flag == false))
			{
				needMove_Flag = true;
				Kinect.NativeMethods.NuiCameraSetAngle(set_KinectAngle);
			}
		}
	}

	void OnGUI()
	{
		//GUI.color = Color.black;
		//GUILayout.Label ("headPosition:" + headPosition.y);
	}
	
	void LateUpdate () {
		updatedSkeleton = false;
		newSkeleton = false;
	}
	
	/// <summary>
	/// First call per frame checks if there is a new skeleton frame and updates,
	/// returns true if there is new data
	/// Subsequent calls do nothing have the same return as the first call.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	public bool pollSkeleton () {
		if (!updatedSkeleton)
		{
			updatedSkeleton = true;
			if (kinect.pollSkeleton())
			{
				newSkeleton = true;
				System.Int64 cur = kinect.getSkeleton().liTimeStamp;
				System.Int64 diff = cur - ticks;
				ticks = cur;
				deltaTime = diff / (float)1000;
				processSkeleton();
			}
		}
		return newSkeleton;
	}
	
	private void processSkeleton () {
		int[] tracked = new int[Kinect.Constants.NuiSkeletonMaxTracked];
		tracked[0] = -1;
		tracked[1] = -1;
		int trackedCount = 0;
		//update players
		for (int ii = 0; ii < Kinect.Constants.NuiSkeletonCount; ii++)
		{
			players[ii] = kinect.getSkeleton().SkeletonData[ii].eTrackingState;
			if (players[ii] == Kinect.NuiSkeletonTrackingState.SkeletonTracked)
			{
				tracked[trackedCount] = ii;
				trackedCount++;
			}
		}
		//this should really use trackingID instead of index, but for now this is fine
		switch (trackedCount)
		{
		case 0:
			trackedPlayers[0] = -1;
			trackedPlayers[1] = -1;
			break;
		case 1:
			//last frame there were no players: assign new player to p1
			if (trackedPlayers[0] < 0 && trackedPlayers[1] < 0)
				trackedPlayers[0] = tracked[0];
			//last frame there was one player, keep that player in the same spot
			else if (trackedPlayers[0] < 0) 
				trackedPlayers[1] = tracked[0];
			else if (trackedPlayers[1] < 0)
				trackedPlayers[0] = tracked[0];
			//there were two players, keep the one with the same index (if possible)
			else
			{
				if (tracked[0] == trackedPlayers[0])
					trackedPlayers[1] = -1;
				else if (tracked[0] == trackedPlayers[1])
					trackedPlayers[0] = -1;
				else
				{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = -1;
				}
			}
			break;
		case 2:
			//last frame there were no players: assign new players to p1 and p2
			if (trackedPlayers[0] < 0 && trackedPlayers[1] < 0)
			{
				trackedPlayers[0] = tracked[0];
				trackedPlayers[1] = tracked[1];
			}
			//last frame there was one player, keep that player in the same spot
			else if (trackedPlayers[0] < 0)
			{
				if (trackedPlayers[1] == tracked[0])
					trackedPlayers[0] = tracked[1];
				else{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = tracked[1];
				}
			}
			else if (trackedPlayers[1] < 0)
			{
				if (trackedPlayers[0] == tracked[1])
					trackedPlayers[1] = tracked[0];
				else{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = tracked[1];
				}
			}
			//there were two players, keep the one with the same index (if possible)
			else
			{
				if (trackedPlayers[0] == tracked[1] || trackedPlayers[1] == tracked[0])
				{
					trackedPlayers[0] = tracked[1];
					trackedPlayers[1] = tracked[0];
				}
				else
				{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = tracked[1];
				}
			}
			break;
		}
		
		//update the bone positions, velocities, and tracking states)
		for (int player = 0; player < 2; player++)
		{
			if (trackedPlayers[player] >= 0)
			{
				for (int bone = 0; bone < (int)Kinect.NuiSkeletonPositionIndex.Count; bone++)
				{
					Vector3 oldpos = bonePos[player,bone];
					bonePos[player,bone] = kinectToWorld.MultiplyPoint3x4(kinect.getSkeleton().SkeletonData[trackedPlayers[player]].SkeletonPositions[bone]);

					bonePos_2[player,bone] = bonePos[player,bone];
					bonePos[player,bone] = new Vector3(bonePos[player,bone].x,bonePos[player,bone].y,-bonePos[player,bone].z);

					boneVel[player,bone] = (bonePos[player,bone] - oldpos) / deltaTime;
					boneState[player,bone] = kinect.getSkeleton().SkeletonData[trackedPlayers[player]].eSkeletonPositionTrackingState[bone];

				}
				headPosition = new Vector2((0.5f + bonePos[0,3].x)*1600,
				                           (-1.15f + bonePos[0,3].y)*900);

			}
		}
	}
	private void adjustKinectSensor()
	{
		if (headPosition.y > 350) 
		{
			try
			{
				if(set_KinectAngle < 15)
				{
					set_KinectAngle += 3;
					Kinect.NativeMethods.NuiCameraSetAngle(set_KinectAngle);
					return;
				}
			}
			catch
			{
				Debug.Log("Angle is wrong!");
				return;
			}
		}

		if (headPosition.y < 0) 
		{
			try
			{
				if(set_KinectAngle > -15)
				{
					set_KinectAngle -= 3;
					Kinect.NativeMethods.NuiCameraSetAngle(set_KinectAngle);
					return;
				}
			}
			catch
			{
				Debug.Log("Angle is wrong!");
				return;
			}
		}

		//myScript.isAdjustAngle = false;		//角度调节完毕
		//pre_isAdjustAngle = false;
	}
}
