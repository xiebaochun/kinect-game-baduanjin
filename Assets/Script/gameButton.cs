using UnityEngine;
using System.Collections;

public class gameButton : MonoBehaviour {

	GameObject obj_btn,obj_leftHand,obj_rightHand;
	int range = 10;
	Vector3 buttonOriginalPosition; 

	float confirmPressedStartTime = 0;
	bool confirmButtonPressed = false;
	bool isButtonPressed = false;

	public gameButton(GameObject obj_btn,GameObject obj_leftHand,GameObject obj_rightHand)
	{
		this.obj_btn = obj_btn;
		this.obj_leftHand = obj_leftHand;
		this.obj_rightHand = obj_rightHand;
		buttonOriginalPosition = obj_btn.transform.position;
	}

	public bool checkButtonPressed()
	{
		if (checkButtonPressing())
		{
			System.Random RandNum = new System.Random();
			int rx = RandNum.Next(1, range);
			int ry = RandNum.Next(1, range);
			obj_btn.transform.position = new Vector3(buttonOriginalPosition.x + rx - range/2,
			                                    buttonOriginalPosition.y,
			                                    buttonOriginalPosition.z + ry - range/2);
			if (confirmPressedStartTime > 1)
			{				
				return true;
			}
		}
		
		return false;
	}

	public bool checkButtonPressing()
	{
		bool result = false;

		result = checkButtonPointedRectangle ();
		if (result) 
		{
			confirmPressedStartTime += Time.deltaTime;
			if (confirmButtonPressed == false && confirmPressedStartTime > 1)
			{
				confirmPressedStartTime = 0;
				confirmButtonPressed = true;
				//soundEffectChim.Play();	//播放按键按下的声音
			}
		}		
		return confirmButtonPressed;
	}

	public bool checkButtonPointedRectangle()
	{
		Rect confirmBtn_rect = new Rect(buttonOriginalPosition.x - obj_btn.transform.localScale.x*1.5f,
		                                buttonOriginalPosition.z - obj_btn.transform.localScale.z*1.5f,
		                                obj_btn.transform.localScale.x*3,obj_btn.transform.localScale.z*3);
		Vector2 leftHandPosition = new Vector2(obj_leftHand.transform.position.x,obj_leftHand.transform.position.z);
		Vector2 rightHandPosition = new Vector2(obj_rightHand.transform.position.x,obj_rightHand.transform.position.z);
		if ((confirmBtn_rect.Contains (leftHandPosition)) || 
		    (confirmBtn_rect.Contains (rightHandPosition)))
		{
			int sx = (int)(Mathf.Sin((Time.realtimeSinceStartup * 360 / 1000) * Mathf.PI / 180) * range);
			int sy = (int)(Mathf.Cos((Time.realtimeSinceStartup * 360 / 1000) * Mathf.PI / 180) * range);		
			obj_btn.transform.position = new Vector3(buttonOriginalPosition.x + sx - range/2,
			                                         buttonOriginalPosition.y,
			                                         buttonOriginalPosition.z + sy - range/2);
			if (isButtonPressed == false)
			{
				//soundEffectClick.Play();		//播放按键声音
				confirmPressedStartTime = 0;
				isButtonPressed = true;
			}
		}
		else
		{
			obj_btn.transform.position = buttonOriginalPosition;
			isButtonPressed = false;
		}
		
		return isButtonPressed;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
