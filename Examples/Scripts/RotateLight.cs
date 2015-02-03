using UnityEngine;
using System.Collections;
using LOS;

public class RotateLight : MonoBehaviour {

	public LOSLightBase coneLight;
	public Vector2 angleRange;
	public float speed;

	private bool _isIncreasing;
	
	void Start () {
		angleRange.x = SMath.ClampDegree0To360(angleRange.x);
		angleRange.y = SMath.ClampDegree0To360(angleRange.y);

		if (angleRange.x > angleRange.y) {
			float temp = angleRange.x;
			angleRange.x = angleRange.y;
			angleRange.y = temp;
		}

		if (coneLight.faceAngle < angleRange.x) {
			coneLight.faceAngle = angleRange.x;
			_isIncreasing = true;
		}
		else if (coneLight.faceAngle > angleRange.y){
			coneLight.faceAngle = angleRange.y;
			_isIncreasing = false;
		}
	}

	void Update () {
		if (_isIncreasing) {
			if (coneLight.faceAngle < angleRange.y) {
				coneLight.faceAngle += speed * Time.deltaTime;
			}
			else {
				_isIncreasing = false;
				coneLight.faceAngle -= speed * Time.deltaTime;
			}
		}
		else {
			if (coneLight.faceAngle > angleRange.x) {
				coneLight.faceAngle -= speed * Time.deltaTime;
			}
			else {
				_isIncreasing = true;
				coneLight.faceAngle += speed * Time.deltaTime;
			}
		}
	}
}
