using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// 2D 선(Line)을 나타내는 구조체
/// </summary>
public struct Line
{
	// 수직 선(gradient)이 매우 크다고 가정할 때 사용할 상수 값
	const float verticalLineGradient = 1e5f;

	float gradient; // 선의 기울기(gradient)
	float y_intercept; // 선의 y절편 (y축과 교차하는 점)
	Vector2 pointOnLine_1; // 선 위의 한 점
	Vector2 pointOnLine_2; // 선 위의 또 다른 점 (선의 방향 정의에 사용)

	float gradientPerpendicular; // 이 선에 수직인 선의 기울기(gradient)

	bool approachSide; // 주어진 점이 선의 어느 쪽에서 접근 중인지 나타냄

	/// <summary>
	/// Line 구조체의 생성자
	/// 선 위의 한 점과 이 선에 수직한 점을 기반으로 선을 초기화
	/// </summary>
	/// <param name="pointOnLine">선 위의 한 점</param>
	/// <param name="pointPerpendicularToLine">선에 수직한 점</param>
	public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
	{
		// 두 점의 x축 차이 계산
		float dx = pointOnLine.x - pointPerpendicularToLine.x;
		// 두 점의 y축 차이 계산
		float dy = pointOnLine.y - pointPerpendicularToLine.y;

		// 수직선의 경우 기울기를 매우 큰 값으로 설정
		if (dx == 0)
		{
			gradientPerpendicular = verticalLineGradient;
		}
		else
		{
			// 수직선이 아닌 경우, 수직선의 기울기를 계산
			gradientPerpendicular = dy / dx;
		}

		// 수직선의 기울기가 0인 경우, 이 선은 수직선이 됨
		if (gradientPerpendicular == 0)
		{
			gradient = verticalLineGradient;
		}
		else
		{
			// 그렇지 않으면 기울기를 계산 (수직 관계에서의 기울기 공식 사용)
			gradient = -1 / gradientPerpendicular;
		}

		// y절편 계산 (y = mx + b => b = y - mx)
		y_intercept = pointOnLine.y - gradient * pointOnLine.x;
		// 선 위의 첫 번째 점 설정
		pointOnLine_1 = pointOnLine;
		// 선 위의 두 번째 점 설정 (기울기를 기반으로 설정)
		pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

		// 초기 approachSide 값을 false로 설정 후 계산
		approachSide = false;
		approachSide = GetSide(pointPerpendicularToLine);
	}

	/// <summary>
	/// 주어진 점이 선의 어느 쪽에 위치하는지 판단
	/// </summary>
	/// <param name="p">확인할 점</param>
	/// <returns>점이 선의 어느 쪽에 있는지 여부 (bool)</returns>
	bool GetSide(Vector2 p) =>
		(p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) >
		(p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);

	/// <summary>
	/// 주어진 점이 선을 넘었는지 확인
	/// </summary>
	/// <param name="p">확인할 점</param>
	/// <returns>선의 반대쪽으로 넘어갔는지 여부 (bool)</returns>
	public bool HasCrossedLine(Vector2 p) => GetSide(p) != approachSide;

	/// <summary>
	/// Gizmos를 사용해 선을 시각적으로 표시
	/// </summary>
	/// <param name="length">선의 길이</param>
	public void DrawWithGizmos(int length)
	{
		// 선의 방향 계산 (정규화 벡터)
		Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
		// 선의 중심 위치 계산
		Vector3 lineCenter = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
		// Gizmos를 사용하여 선 그리기
		Gizmos.DrawLine(lineCenter - lineDir * length / 2f, lineCenter + lineDir * length / 2f);
	}
}
