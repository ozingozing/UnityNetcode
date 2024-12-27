using System.Collections;
using System.Collections.Generic;
using ChocoOzing.Utilities;
using UnityEngine;

/// <summary>
/// 경로(Path)를 정의하는 클래스. 
/// 주어진 웨이포인트를 기반으로 이동 경로와 회전 경계(turn boundaries)를 설정.
/// </summary>
public class Path
{
	// 읽기 전용 배열: 경로를 따라가야 할 좌표들(웨이포인트)
	public readonly Vector3[] lookPoints;

	// 읽기 전용 배열: 각 웨이포인트 사이의 회전 경계선을 저장
	public readonly Line[] turnBoundaries;

	// 경로의 마지막 경계선 인덱스
	public readonly int finishLineIndex;

	public readonly int slowDownIndex;

	/// <summary>
	/// Path 생성자. 웨이포인트와 시작 위치, 회전 거리(turn distance)를 이용해 경로를 설정.
	/// </summary>
	/// <param name="waypoints">경로를 이루는 웨이포인트 배열</param>
	/// <param name="startPos">경로 시작 위치</param>
	/// <param name="turnDst">회전 거리 (웨이포인트에서 회전 경계를 얼마나 떨어트릴지 결정)</param>
	public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst)
	{
		// 웨이포인트를 경로 점으로 설정
		this.lookPoints = waypoints;

		// 각 웨이포인트 간의 회전 경계선을 저장할 배열 초기화
		turnBoundaries = new Line[lookPoints.Length];

		// 마지막 경계선의 인덱스 계산
		finishLineIndex = turnBoundaries.Length - 1;

		// 시작 위치를 2D 벡터로 변환하여 이전 포인트로 설정
		Vector2 previousPoint = Vector3Extensions.Vector3ToVector2(startPos);

		// 모든 웨이포인트에 대해 반복
		for (int i = 0; i < lookPoints.Length; i++)
		{
			// 현재 웨이포인트를 2D 벡터로 변환
			Vector2 currentPoint = Vector3Extensions.Vector3ToVector2(lookPoints[i]);

			// 이전 포인트에서 현재 포인트로의 방향 벡터를 정규화
			Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;

			// 회전 경계 지점 설정
			// 마지막 웨이포인트는 회전 경계 없이 해당 지점이 그대로 경계가 됨
			// 그 외의 경우, 현재 포인트에서 회전 거리만큼 뒤로 이동한 위치를 경계로 설정
			Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;

			// 경계선을 설정 (turnBoundaryPoint를 기준으로 경계 생성)
			turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);

			// 이전 포인트를 업데이트
			previousPoint = turnBoundaryPoint;
		}

		float dstFromEndPoint = 0;
		for (int i = lookPoints.Length - 1; i > 0; i--)
		{
			dstFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
			if(dstFromEndPoint > stoppingDst)
			{
				slowDownIndex = i;
				break;
			}
		}
	}

	/// <summary>
	/// 경로와 경계선을 Gizmos를 통해 시각적으로 표시.
	/// </summary>
	public void DrawWithGizmos(Transform targetPos = null)
	{
		if(targetPos != null)
		{
			Gizmos.color = Color.cyan;
			DrawHexagon(GridGizmo.instance.NodeFromWorldPoint(targetPos.position).worldPosition + Vector3.up/2, GridGizmo.instance.hexRadius);
		}
		// 경로의 각 웨이포인트를 검은색 큐브로 표시
		Gizmos.color = Color.yellow;
		foreach (Vector3 p in lookPoints)
		{
			DrawHexagon(p + Vector3.up / 2, GridGizmo.instance.hexRadius); // 웨이포인트 위치 위로 살짝 띄워 표시
		}

		// 각 회전 경계선을 흰색으로 표시
		Gizmos.color = Color.white;
		foreach (Line l in turnBoundaries)
		{
			l.DrawWithGizmos(10); // 각 경계선을 길이 10으로 그림
		}
	}

	private void DrawHexagon(Vector3 center, float radius)
	{
		Vector3[] vertices = new Vector3[6];
		float rotationOffset = Mathf.Deg2Rad * -30; // -30°를 라디안으로 변환

		for (int i = 0; i < 6; i++)
		{
			float angle = Mathf.Deg2Rad * (60 * i) + rotationOffset; // 각도를 -30° 회전
			vertices[i] = new Vector3(
				center.x + radius * Mathf.Cos(angle),
				center.y,
				center.z + radius * Mathf.Sin(angle)
			);
		}

		// 여섯 개의 선으로 정육각형을 그림
		for (int i = 0; i < 6; i++)
		{
			Vector3 start = vertices[i];
			Vector3 end = vertices[(i + 1) % 6]; // 마지막 점에서 첫 번째 점으로 연결
			Gizmos.DrawLine(start, end);
		}
	}
}
