/*
 * KinectModelController.cs - Handles rotating the bones of a model to match 
 * 			rotations derived from the bone positions given by the kinect
 * 
 * 		Developed by Peter Kinney -- 6/30/2011
 * 
 */

using UnityEngine;
using System;
using System.Collections;

public class KinectModelControllerV2 : MonoBehaviour {
	
	//Assignments for a bitmask to control which bones to look at and which to ignore
	public enum BoneMask
	{
		None = 0x0,
		//EMPTY = 0x1,
		Spine = 0x2,
		Shoulder_Center = 0x4,
		Head = 0x8,
		Shoulder_Left = 0x10,
		Elbow_Left = 0x20,
		Wrist_Left = 0x40,
		Hand_Left = 0x80,
		Shoulder_Right = 0x100,
		Elbow_Right = 0x200,
		Wrist_Right = 0x400,
		Hand_Right = 0x800,
		Hips = 0x1000,
		Knee_Left = 0x2000,
		Ankle_Left = 0x4000,
		Foot_Left = 0x8000,
		//EMPTY = 0x10000,
		Knee_Right = 0x20000,
		Ankle_Right = 0x40000,
		Foot_Right = 0x80000,
		All = 0xEFFFE,
		Torso = 0x1000000 | Spine | Shoulder_Center | Head, //the leading bit is used to force the ordering in the editor
		Left_Arm = 0x1000000 | Shoulder_Left | Elbow_Left | Wrist_Left | Hand_Left,
		Right_Arm = 0x1000000 |  Shoulder_Right | Elbow_Right | Wrist_Right | Hand_Right,
		Left_Leg = 0x1000000 | Hips | Knee_Left | Ankle_Left | Foot_Left,
		Right_Leg = 0x1000000 | Hips | Knee_Right | Ankle_Right | Foot_Right,
		R_Arm_Chest = Right_Arm | Spine,
		No_Feet = All & ~(Foot_Left | Foot_Right),
		Upper_Body = Torso | Left_Arm | Right_Arm
	}
	
	public SkeletonWrapper sw;
	
	public GameObject Hip_Center;
	public GameObject Spine;
	public GameObject Shoulder_Center;
	public GameObject Head;
	public GameObject Collar_Left;
	public GameObject Shoulder_Left;
	public GameObject Elbow_Left;
	public GameObject Wrist_Left;
	public GameObject Hand_Left;
	public GameObject Fingers_Left; //unused
	public GameObject Collar_Right;
	public GameObject Shoulder_Right;
	public GameObject Elbow_Right;
	public GameObject Wrist_Right;
	public GameObject Hand_Right;
	public GameObject Fingers_Right; //unused
	public GameObject Hip_Override;
	public GameObject Hip_Left;
	public GameObject Knee_Left;
	public GameObject Ankle_Left;
	public GameObject Foot_Left;
	public GameObject Hip_Right;
	public GameObject Knee_Right;
	public GameObject Ankle_Right;
	public GameObject Foot_Right;
	
	public int player;
	public BoneMask Mask = BoneMask.All;
	public bool animated;
	public float blendWeight = 1;
	
	private GameObject[] _bones; //internal handle for the bones of the model
	private uint _nullMask = 0x0;
	
	private Quaternion[] _baseRotation; //starting orientation of the joints
	private Vector3[] _boneDir; //in the bone's local space, the direction of the bones
	private Vector3[] _boneUp; //in the bone's local space, the up vector of the bone
	private Vector3 _hipRight; //right vector of the hips
	private Vector3 _chestRight; //right vectory of the chest
	
	
	[HideInInspector]
	public static Vector2 leftHandPosition,rightHandPosition;
	public static Vector3[] skeletonPosition = new Vector3[14];

	// Use this for initialization
	void Start () {
		//store bones in a list for easier access, everything except Hip_Center will be one
		//higher than the corresponding Kinect.NuiSkeletonPositionIndex (because of the hip_override)
		_bones = new GameObject[(int)Kinect.NuiSkeletonPositionIndex.Count + 5] {
			null, Hip_Center, Spine, Shoulder_Center,
			Collar_Left, Shoulder_Left, Elbow_Left, Wrist_Left,
			Collar_Right, Shoulder_Right, Elbow_Right, Wrist_Right,
			Hip_Override, Hip_Left, Knee_Left, Ankle_Left,
			null, Hip_Right, Knee_Right, Ankle_Right,
			//extra joints to determine the direction of some bones
			Head, Hand_Left, Hand_Right, Foot_Left, Foot_Right};
		
		//determine which bones are not available
		for(int ii = 0; ii < _bones.Length; ii++)
		{
			if(_bones[ii] == null)
			{
				_nullMask |= (uint)(1 << ii);
			}
		}
		
		//store the base rotations and bone directions (in bone-local space)
		_baseRotation = new Quaternion[(int)Kinect.NuiSkeletonPositionIndex.Count];
		_boneDir = new Vector3[(int)Kinect.NuiSkeletonPositionIndex.Count];
		
		//first save the special rotations for the hip and spine
		_hipRight = Hip_Right.transform.position - Hip_Left.transform.position;
		_hipRight = Hip_Override.transform.InverseTransformDirection(_hipRight);
		
		_chestRight = Shoulder_Right.transform.position - Shoulder_Left.transform.position;
		_chestRight = Spine.transform.InverseTransformDirection(_chestRight);
		
		//get direction of all other bones
		for( int ii = 0; ii < (int)Kinect.NuiSkeletonPositionIndex.Count; ii++)
		{
			if((_nullMask & (uint)(1 << ii)) <= 0)
			{
				//save initial rotation
				_baseRotation[ii] = _bones[ii].transform.localRotation;
				
				//if the bone is the end of a limb, get direction from this bone to one of the extras (hand or foot).
				if(ii % 4 == 3 && ((_nullMask & (uint)(1 << (ii/4) + (int)Kinect.NuiSkeletonPositionIndex.Count)) <= 0))
				{
					_boneDir[ii] = _bones[(ii/4) + (int)Kinect.NuiSkeletonPositionIndex.Count].transform.position - _bones[ii].transform.position;
				}
				//if the bone is the hip_override (at boneindex Hip_Left, get direction from average of left and right hips
				else if(ii == (int)Kinect.NuiSkeletonPositionIndex.HipLeft && Hip_Left != null && Hip_Right != null)
				{
					_boneDir[ii] = ((Hip_Right.transform.position + Hip_Left.transform.position) / 2F) - Hip_Override.transform.position;
				}
				//otherwise, get the vector from this bone to the next.
				else if((_nullMask & (uint)(1 << ii+1)) <= 0)
				{
					_boneDir[ii] = _bones[ii+1].transform.position - _bones[ii].transform.position;
				}
				else
				{
					continue;
				}
				//Since the spine of the kinect data is ~40 degrees back from the hip,
				//check what angle the spine is at and rotate the saved direction back to match the data
				if(ii == (int)Kinect.NuiSkeletonPositionIndex.Spine)
				{
					float angle = Vector3.Angle(transform.up,_boneDir[ii]);
					_boneDir[ii] = Quaternion.AngleAxis(-40 + angle,transform.right) * _boneDir[ii];
				}
				//transform the direction into local space.
				_boneDir[ii] = _bones[ii].transform.InverseTransformDirection(_boneDir[ii]);
			}
		}
		//make _chestRight orthogonal to the direction of the spine.
		_chestRight -= Vector3.Project(_chestRight, _boneDir[(int)Kinect.NuiSkeletonPositionIndex.Spine]);
		//make _hipRight orthogonal to the direction of the hip override
		Vector3.OrthoNormalize(ref _boneDir[(int)Kinect.NuiSkeletonPositionIndex.HipLeft],ref _hipRight);
	}
	
	void Update () {
		//update the data from the kinect if necessary
		if(sw.pollSkeleton()){
			for( int ii = 0; ii < (int)Kinect.NuiSkeletonPositionIndex.Count; ii++)
			{
				if( ((uint)Mask & (uint)(1 << ii) ) > 0 && (_nullMask & (uint)(1 << ii)) <= 0 )
				{
					RotateJoint(ii);
				}
			}

			
			skeletonPosition[0]=new Vector3(1.0f,1.0f,-1.0f);	//缺少这行的时候，无法获得“头部”数据
			skeletonPosition[0]=sw.bonePos[player,3];
			skeletonPosition[1]=sw.bonePos[player,2];
			skeletonPosition[2]=sw.bonePos[player,4];
			skeletonPosition[3]=sw.bonePos[player,8];
			skeletonPosition[4]=sw.bonePos[player,5];
			skeletonPosition[5]=sw.bonePos[player,6];
			skeletonPosition[6]=sw.bonePos[player,9];
			skeletonPosition[7]=sw.bonePos[player,10];
			skeletonPosition[8]=sw.bonePos[player,0];
			skeletonPosition[9]=sw.bonePos[player,13];
			skeletonPosition[10]=sw.bonePos[player,14];
			skeletonPosition[11]=sw.bonePos[player,17];
			skeletonPosition[12]=sw.bonePos[player,18];
			skeletonPosition[13]=sw.bonePos[player,3];//头部骨骼信息
			/*
			//获得骨骼坐标信息（Z轴与之前的相反，所以加了"-"）
			skeletonPosition[0]=new Vector3(1.0f,1.0f,-1.0f);	//缺少这行的时候，无法获得“头部”数据
			skeletonPosition[0]=new Vector3(sw.bonePos[player,3].x,sw.bonePos[player,3].y,-sw.bonePos[player,3].z);
			skeletonPosition[1]=new Vector3(sw.bonePos[player,2].x,sw.bonePos[player,2].y,-sw.bonePos[player,2].z);
			skeletonPosition[2]=new Vector3(sw.bonePos[player,4].x,sw.bonePos[player,4].y,-sw.bonePos[player,4].z);
			skeletonPosition[3]=new Vector3(sw.bonePos[player,8].x,sw.bonePos[player,8].y,-sw.bonePos[player,8].z);
			skeletonPosition[4]=new Vector3(sw.bonePos[player,5].x,sw.bonePos[player,5].y,-sw.bonePos[player,5].z);
			skeletonPosition[5]=new Vector3(sw.bonePos[player,6].x,sw.bonePos[player,6].y,-sw.bonePos[player,6].z);
			skeletonPosition[6]=new Vector3(sw.bonePos[player,9].x,sw.bonePos[player,9].y,-sw.bonePos[player,9].z);
			skeletonPosition[7]=new Vector3(sw.bonePos[player,10].x,sw.bonePos[player,10].y,-sw.bonePos[player,10].z);
			skeletonPosition[8]=new Vector3(sw.bonePos[player,0].x,sw.bonePos[player,0].y,-sw.bonePos[player,0].z);
			skeletonPosition[9]=new Vector3(sw.bonePos[player,13].x,sw.bonePos[player,13].y,-sw.bonePos[player,13].z);
			skeletonPosition[10]=new Vector3(sw.bonePos[player,14].x,sw.bonePos[player,14].y,-sw.bonePos[player,14].z);
			skeletonPosition[11]=new Vector3(sw.bonePos[player,17].x,sw.bonePos[player,17].y,-sw.bonePos[player,17].z);
			skeletonPosition[12]=new Vector3(sw.bonePos[player,18].x,sw.bonePos[player,18].y,-sw.bonePos[player,18].z);
			skeletonPosition[13]=new Vector3(sw.bonePos[player,3].x,sw.bonePos[player,3].y,-sw.bonePos[player,3].z);//头部骨骼信息
			*/
			/*
			skeletonPosition[0]=sw.bonePos[player,3];
			skeletonPosition[1]=sw.bonePos[player,2];
			skeletonPosition[2]=sw.bonePos[player,4];
			skeletonPosition[3]=sw.bonePos[player,8];
			skeletonPosition[4]=sw.bonePos[player,5];
			skeletonPosition[5]=sw.bonePos[player,6];
			skeletonPosition[6]=sw.bonePos[player,9];
			skeletonPosition[7]=sw.bonePos[player,10];
			skeletonPosition[8]=sw.bonePos[player,0];
			skeletonPosition[9]=sw.bonePos[player,13];
			skeletonPosition[10]=sw.bonePos[player,14];
			skeletonPosition[11]=sw.bonePos[player,17];
			skeletonPosition[12]=sw.bonePos[player,18];
			*/
		}
	}


	void OnGUI()
	{
		GUI.color = Color.black;

		for (int ii = 0; ii < (int)Kinect.NuiSkeletonPositionIndex.Count; ii++) 
		{
			//GUILayout.Label ("" + sw.bonePos[player,ii]);
			if(ii==7)
			{
				//GUILayout.Label ("" + sw.bonePos[player,ii]);
				leftHandPosition = new Vector2((((-1 * sw.bonePos[player,ii].x) + 0.5f) * 3840*2 - 4000), (((-1 * sw.bonePos[player,ii].y) + 0.8f) * 2304*2 + 2000));
			}
			else if(ii==11)
			{
				//GUILayout.Label ("" + sw.bonePos[player,ii]);
				rightHandPosition = new Vector2((((-1 * sw.bonePos[player,ii].x) + 0.5f) * 3840*2 - 4000), (((-1 * sw.bonePos[player,ii].y) + 0.8f) * 2304*2 + 2000));
			}
		}
		/*
		GUILayout.Label ("" + sw.bonePos[player,3]);
		GUILayout.Label ("" + sw.bonePos[player,2]);
		GUILayout.Label ("" + sw.bonePos[player,4]);
		GUILayout.Label ("" + sw.bonePos[player,8]);
		GUILayout.Label ("" + sw.bonePos[player,5]);
		GUILayout.Label ("" + sw.bonePos[player,6]);
		GUILayout.Label ("" + sw.bonePos[player,9]);
		GUILayout.Label ("" + sw.bonePos[player,10]);
		GUILayout.Label ("" + sw.bonePos[player,0]);
		GUILayout.Label ("" + sw.bonePos[player,13]);
		GUILayout.Label ("" + sw.bonePos[player,14]);
		GUILayout.Label ("" + sw.bonePos[player,17]);
		GUILayout.Label ("" + sw.bonePos[player,18]);
		*/
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



		/*
		skeletonPosition[0] = new Vector3(sw.bonePos[player,3].x,sw.bonePos[player,3].y,sw.bonePos[player,3].z);		//头
		skeletonPosition[1] = new Vector3(sw.bonePos[player,1].x,sw.bonePos[player,1].y,sw.bonePos[player,1].z);		//肩膀中心
		skeletonPosition[2] = new Vector3(sw.bonePos[player,4].x,sw.bonePos[player,4].y,sw.bonePos[player,4].z);;		//左肩
		skeletonPosition[3] = new Vector3(sw.bonePos[player,8].x,sw.bonePos[player,8].y,sw.bonePos[player,8].z);;		//右肩
		skeletonPosition[4] = new Vector3(sw.bonePos[player,5].x,sw.bonePos[player,5].y,sw.bonePos[player,5].z);;		//左手肘
		skeletonPosition[5] = new Vector3(sw.bonePos[player,6].x,sw.bonePos[player,6].y,sw.bonePos[player,6].z);;		//左手腕
		skeletonPosition[6] = new Vector3(sw.bonePos[player,9].x,sw.bonePos[player,9].y,sw.bonePos[player,9].z);;		//右手肘
		skeletonPosition[7] = new Vector3(sw.bonePos[player,10].x,sw.bonePos[player,10].y,sw.bonePos[player,10].z);;	//右手腕
		skeletonPosition[8] = new Vector3(sw.bonePos[player,0].x,sw.bonePos[player,0].y,sw.bonePos[player,0].z);;		//臀部中心
		skeletonPosition[9] = new Vector3(sw.bonePos[player,13].x,sw.bonePos[player,13].y,sw.bonePos[player,13].z);;	//左膝盖
		skeletonPosition[10] = new Vector3(sw.bonePos[player,14].x,sw.bonePos[player,14].y,sw.bonePos[player,14].z);;	//左脚踝
		skeletonPosition[11] = new Vector3(sw.bonePos[player,17].x,sw.bonePos[player,17].y,sw.bonePos[player,17].z);;	//右膝盖
		skeletonPosition[12] = new Vector3(sw.bonePos[player,18].x,sw.bonePos[player,18].y,sw.bonePos[player,18].z);;	//右脚踝
		*/
	}
	
	void RotateJoint(int bone) {
		//if blendWeight is 0 there is no need to compute the rotations
		if( blendWeight <= 0 ){ return; }
		Vector3 upDir = new Vector3();
		Vector3 rightDir = new Vector3();
		if(bone == (int)Kinect.NuiSkeletonPositionIndex.Spine)
		{
			upDir = ((Hip_Left.transform.position + Hip_Right.transform.position) / 2F) - Hip_Override.transform.position;
			rightDir = Hip_Right.transform.position - Hip_Left.transform.position;
		}
		
		//if the model is not animated, reset rotations to fix twisted joints
		if(!animated){_bones[bone].transform.localRotation = _baseRotation[bone];}
		//if the required bone data from the kinect isn't available, return
		if( sw.boneState[player,bone] == Kinect.NuiSkeletonPositionTrackingState.NotTracked)
		{
			return;
		}
		
		//get the target direction of the bone in world space
		//for the majority of bone it's bone - 1 to bone, but Hip_Override and the outside
		//shoulders are determined differently.
		
		Vector3 dir = _boneDir[bone];
		Vector3 target;
		
		//if bone % 4 == 0 then it is either an outside shoulder or the hip override
		if(bone % 4 == 0)
		{
			//hip override is at Hip_Left
			if(bone == (int)Kinect.NuiSkeletonPositionIndex.HipLeft)
			{
				//target = vector from hip_center to average of hips left and right
				target = ((sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.HipLeft] + sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.HipRight]) / 2F) - sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.HipCenter];
			}
			//otherwise it is one of the shoulders
			else
			{
				//target = vector from shoulder_center to bone
				target = sw.bonePos[player,bone] - sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.ShoulderCenter];
			}
		}
		else
		{
			//target = vector from previous bone to bone
			target = sw.bonePos[player,bone] - sw.bonePos[player,bone-1];
		}
		//transform it into bone-local space (independant of the transform of the controller)
		target = transform.TransformDirection(target);
		target = _bones[bone].transform.InverseTransformDirection(target);
		//create a rotation that rotates dir into target
		Quaternion quat = Quaternion.FromToRotation(dir,target);
		//if bone is the spine, add in the rotation along the spine
		if(bone == (int)Kinect.NuiSkeletonPositionIndex.Spine)
		{
			//rotate the chest so that it faces forward (determined by the shoulders)
			dir = _chestRight;
			target = sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.ShoulderRight] - sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.ShoulderLeft];
			
			target = transform.TransformDirection(target);
			target = _bones[bone].transform.InverseTransformDirection(target);
			target -= Vector3.Project(target,_boneDir[bone]);
			
			quat *= Quaternion.FromToRotation(dir,target);
			
		}
		//if bone is the hip override, add in the rotation along the hips
		else if(bone == (int)Kinect.NuiSkeletonPositionIndex.HipLeft)
		{
			//rotate the hips so they face forward (determined by the hips)
			dir = _hipRight;
			target = sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.HipRight] - sw.bonePos[player,(int)Kinect.NuiSkeletonPositionIndex.HipLeft];
			
			target = transform.TransformDirection(target);
			target = _bones[bone].transform.InverseTransformDirection(target);
			target -= Vector3.Project(target,_boneDir[bone]);
			
			quat *= Quaternion.FromToRotation(dir,target);
		}
		
		//reduce the effect of the rotation using the blend parameter
		quat = Quaternion.Lerp(Quaternion.identity, quat, blendWeight);
		//apply the rotation to the local rotation of the bone
		_bones[bone].transform.localRotation = _bones[bone].transform.localRotation  * quat;
		
		if(bone == (int)Kinect.NuiSkeletonPositionIndex.Spine)
		{
			restoreBone(_bones[(int)Kinect.NuiSkeletonPositionIndex.HipLeft],_boneDir[(int)Kinect.NuiSkeletonPositionIndex.HipLeft],upDir);
			restoreBone(_bones[(int)Kinect.NuiSkeletonPositionIndex.HipLeft],_hipRight,rightDir);
		}
		
		return;
	}
	
	void restoreBone(GameObject bone,Vector3 dir, Vector3 target)
	{
		//transform target into bone-local space (independant of the transform of the controller)
		//target = transform.TransformDirection(target);
		target = bone.transform.InverseTransformDirection(target);
		//create a rotation that rotates dir into target
		Quaternion quat = Quaternion.FromToRotation(dir,target);
		bone.transform.localRotation *= quat;
	}
}


