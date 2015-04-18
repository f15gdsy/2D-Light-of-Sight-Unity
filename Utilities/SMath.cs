using UnityEngine;
using System.Collections;

public static class SMath {
	public static Vector2 Vec3ToVec2 (Vector3 origin) {
		return new Vector2(origin.x, origin.y);
	}
	
	public static Vector3 Vec2ToVec3 (Vector2 origin, float z) {
		return new Vector3(origin.x, origin.y, z);
	}
	
	public static Vector3 Vec2ToVec3 (Vector2 origin) {
		return new Vector3(origin.x, origin.y, 0);
	}
	
	public static float Clamp (float min, float target, float max) {
		target = Mathf.Max(min, target);
		target = Mathf.Min(max, target);
		return target;
	}
	
	public static float GetManhattanDistance (Vector2 pos1, Vector2 pos2) {
		return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
	}
	
	public static Vector2 GetBezierPoint2 (float ratio, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
		float u = 1 - ratio;
		float tt = ratio * ratio;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * ratio;
		
		Vector2 p = uuu * p0;
		p += 3 * uu * ratio * p1;
		p += 3 * u * tt * p2;
		p += ttt * p3;
		
		return p;
	}
	
	public static Vector2 GetBezierPoint2 (float ratio, Vector2 p0, Vector2 p1, Vector2 p2) {
		return (1-ratio)*(1-ratio)*p0 + 2*(1-ratio)*ratio*p1 + ratio*ratio*p2;
	}
	
	public static Vector3 GetBezierPoint3 (float ratio, Vector3 p0, Vector3 p1, Vector3 p2) {
		return (1-ratio)*(1-ratio)*p0 + 2*(1-ratio)*ratio*p1 + ratio*ratio*p2;
	}
	
	public static float GetBezierPoint2Angle (float t, Vector2 p0, Vector2 p1, Vector2 p2) {
		Vector2 q0 = new Vector2((1 - t) * p0.x + t * p1.x, (1 - t) * p0.y + t * p1.y);
		Vector2 q1 = new Vector2((1 - t) * p1.x + t * p2.x, (1 - t) * p1.y + t * p2.y);
		
		float dX = q1.x - q0.x;
		float dY = q1.y - q0.y;
		float radian = Mathf.Atan2(dY, dX);
		float degree = Mathf.Rad2Deg * radian;
		
		return degree;
	}
	
	public static PositionSituation GetPositionSituation (Vector2 a, Vector2 b, float tolerance) {
		Vector2 offset = a - b;
		if (offset.x > tolerance && offset.y > tolerance) return PositionSituation.UpRight;
		if (offset.x > tolerance && offset.y < -tolerance) return PositionSituation.DownRight;
		if (offset.x < -tolerance && offset.y > tolerance) return PositionSituation.UpLeft;
		if (offset.x < -tolerance && offset.y < -tolerance) return PositionSituation.DownLeft;
		if (offset.x > tolerance) return PositionSituation.Up;
		if (offset.x < -tolerance) return PositionSituation.Down;
		if (offset.y > tolerance) return PositionSituation.Right;
		if (offset.y < -tolerance) return PositionSituation.Left;
		return PositionSituation.Center;
	}
	
	public static Vector2 PositionSituationToVector (PositionSituation situation) {
		Vector2 vector = Vector2.zero;
		switch (situation) {
		case PositionSituation.Up:
			vector = new Vector2(0, 1);
			break;
		case PositionSituation.UpRight:
			vector = new Vector2(0.75f, 0.75f);
			break;
		case PositionSituation.Right:
			vector = new Vector2(1, 0);
			break;
		case PositionSituation.DownRight:
			vector = new Vector2(0.75f, -0.75f);
			break;
		case PositionSituation.Down:
			vector = new Vector2(0, -1);
			break;
		case PositionSituation.DownLeft:
			vector = new Vector2(-0.75f, -0.75f);
			break;
		case PositionSituation.Left:
			vector = new Vector2(-1, 0);
			break;
		case PositionSituation.UpLeft:
			vector = new Vector2(-0.75f, 0.75f);
			break;
		}
		return vector;
	}

	/// <summary>
	/// Gets the degree between two index vectors.
	/// </summary>
	/// <returns>The degree between the two index vectors. 
	/// If target vector is on the left of origin, negative angle will be returned.
	/// If on the right, positive angle will be returned.
	/// </returns>
	/// <param name="origin">Origin.</param>
	/// <param name="target">Target.</param>
	public static float GetDegreeBetweenIndexVector (Vector2 origin, Vector2 target) {
		float degree = Mathf.Acos(origin.x * target.x + origin.y * target.y) * Mathf.Rad2Deg;
		Vector2 perpendicular = GetPerpendicular2(origin, true);
		float testCos = perpendicular.x * target.x + perpendicular.y * target.y;
		
		if (testCos > 0) {	// left
			degree = -Mathf.Abs(degree);	
		}
		else {
			degree = Mathf.Abs(degree);	
		}
		
		return degree;
	}
	
	public static Vector2 GetPerpendicular2 (Vector2 origin, bool isLeft) {
		Vector2 perpendicular = Vector2.zero;
		int quadrant = GetQuadrant(origin);
		
		if (isLeft) {
			switch (quadrant) {
			case 0:
				if (origin.y == 0) {
					if (origin.x > 0) perpendicular.y = 1;
					else perpendicular.y = -1;
				}
				else {
					if (origin.y > 0) perpendicular.x = -1;
					else perpendicular.x = 1;
				}
				break;
			case 1:
			case 2:
				perpendicular.x = -1;
				perpendicular.y = origin.x / origin.y;
				break;
			case 3:
			case 4:
				perpendicular.x = 1;
				perpendicular.y = -origin.x / origin.y;
				break;
			}
		}
		else {
			switch (quadrant) {
			case 0:
				if (origin.y == 0) {
					if (origin.x > 0) perpendicular.y = -1;
					else perpendicular.y = 1;
				}
				else {
					if (origin.y > 0) perpendicular.x = 1;
					else perpendicular.x = -1;
				}
				break;
			case 1:
			case 2:
				perpendicular.x = 1;
				perpendicular.y = -origin.x / origin.y;
				break;
			case 3:
			case 4:
				perpendicular.x = -1;
				perpendicular.y = origin.x / origin.y;
				break;
			}
		}
		
		return perpendicular;
	}
	
	public static int GetQuadrant (Vector2 point) {
		if (point.x > 0) {
			if (point.y > 0) return 1;
			else if (point.y == 0) return 0;
			else return 4;
		}
		else if (point.x == 0) {
			return 0;
		}
		else {
			if (point.y > 0) return 2;
			else if (point.y == 0) return 0;
			else return 3;	
		}
	}
	
	public static LineSituation IsBetween (Vector2 left, Vector2 target, Vector2 right) {	// 3 lines start from the same point
		Vector2 leftPerpendicular = SMath.GetPerpendicular2(left, true);
		if (Vector2.Dot(leftPerpendicular, target) < 0) {	// to the right of left line
			Vector2 rightPerpendicular = SMath.GetPerpendicular2(right, true);	
			if (Vector2.Dot(rightPerpendicular, target) > 0) {	// to the left of right line
				return LineSituation.Between;
			}
			else {	// to the right of the right line
				return LineSituation.OutRight;
			}
		}
		else {	// to the left of left line
			return LineSituation.OutLeft;
		}
	}

	/// <summary>
	/// Vectors to degree.
	/// </summary>
	/// <returns>The degree from 0 to 360.</returns>
	/// <param name="v">V.</param>
	public static float VectorToDegree (Vector2 v) {
		float radian = Mathf.Atan2(v.y, v.x);
		float degree = radian * Mathf.Rad2Deg;
		return ClampDegree0To360(degree);
	}

	public static Vector2 DegreeToUnitVector (float degree) {
		float radian = degree * Mathf.Deg2Rad;
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}

	public static float ClampDegree0To360 (float degree) {
		while (degree >= 360) {
			degree -= 360;
		}
		while (degree < 0) {
			degree += 360;
		}
		return degree;
	}
	
	public static bool CheckSamePoint (Vector2 p1, Vector2 p2, float tolerance) {
		return CheckSamePoint(SMath.Vec2ToVec3(p1), SMath.Vec2ToVec3(p2), tolerance);
	}
	
	public static bool CheckSamePoint (Vector3 p1, Vector3 p2, float tolerance) {
		return (p1 - p2).sqrMagnitude <= tolerance * tolerance;
	}
	
	public static float CrossProduct2D (Vector2 p, Vector2 q) {
		return p.x * q.y - p.y * q.x;
	}
	
	public static Vector3 RotatePointAroundPivot (Vector3 point, Vector3 pivot, Vector3 angles) {
		Vector3 direction = point - pivot;
		direction = Quaternion.Euler(angles) * direction;	// rotate it
		point = direction + pivot;
		return point;
	}
}

public enum PositionSituation {
	Up,
	UpRight,
	Right,
	DownRight,
	Down,
	DownLeft,
	Left,
	UpLeft,
	Center,
}

public enum LineSituation {
	Between = 0,
	OutLeft = 1,
	OutRight = 2,
	Same = 3,
}

public enum Relation {
	Greater,
	EqualTo,
	LessThan
}