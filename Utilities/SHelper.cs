using UnityEngine;
//using UnityEditor;
//using UnityEditorInternal;

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;


public static class SHelper {

	public static Vector2 GetMousePositionInWorld2D () {
		Vector3 mousePosInWorld = GetMousePositionInWorld();
		return SMath.Vec3ToVec2(mousePosInWorld);
	}

	public static Vector3 GetMousePositionInWorld () {
		float distanceToCamera = Mathf.Abs(Camera.main.transform.position.z);
		Vector3 mousePosOnScreen = Input.mousePosition;
		Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosOnScreen.x, mousePosOnScreen.y, distanceToCamera));
		return mousePosInWorld;
	}

	public static Vector3 GetTouchPositionInWorld2D (Vector2 pos) {
		float distanceToCamera = Mathf.Abs(Camera.main.transform.position.z);
		Vector2 positionInWorld = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, distanceToCamera));
		return positionInWorld;
	}

	public static Rect CreateRect (Vector2 p1, Vector2 p2) {
		float minX = p1.x < p2.x ? p1.x : p2.x;
		float minY = p1.y < p2.y ? p1.y : p2.y;
		float w = Mathf.Abs(p1.x - p2.x);
		float h = Mathf.Abs(p1.y - p2.y);

		return new Rect(minX, minY, w, h);
	}

	public static void SetAnimationTrigger (Animator animator, string toState, string triggerName) {
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName(toState)) {
			animator.SetTrigger(triggerName);
		}
	}

	public static bool Gamble (float successRate) {
		if (successRate >= 100) return true;
		if (successRate <= 0) return false;
		return UnityEngine.Random.Range(0, 100) <= successRate;
	}

	public static Vector3 LockToPixel (Vector3 point) {
		point = Camera.main.WorldToScreenPoint(point);
		point.x = Mathf.Round(point.x);
		point.y = Mathf.Round(point.y);
		point = Camera.main.ScreenToWorldPoint(point);
		return point;
	}

	public static void FlipX2d (Transform trans) {
		Vector3 angle = trans.localEulerAngles;
		angle.y += 180;
		trans.localEulerAngles = angle;
	}

	public static Vector2 GetScreenSizeInWorld (Camera cam) {
		float height = cam.orthographicSize;
		float width = height * Camera.main.aspect;
		return new Vector2(2 * width, 2 * height);
	}

	public static bool CheckWithinScreen (Vector2 position, Camera camera, float distance) {
		Vector3 minScreenPosition = camera.WorldToScreenPoint(new Vector2(position.x - distance, position.y - distance));
		Vector3 screenPosition = camera.WorldToScreenPoint(position);
		float distanceInScreen = Math.Abs(screenPosition.x - minScreenPosition.x);
		return !(screenPosition.x + distanceInScreen <= 0 || screenPosition.x - distanceInScreen >= camera.pixelWidth || 
		         screenPosition.y + distanceInScreen <= 0 || screenPosition.y - distanceInScreen >= camera.pixelHeight);
	}

	public static bool CheckGameObjectInLayer (GameObject go, int layerMask) {
		return (layerMask & (1 << go.layer)) > 0;
	}
}
