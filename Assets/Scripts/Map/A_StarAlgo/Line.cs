using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// 2D ��(Line)�� ��Ÿ���� ����ü
/// </summary>
public struct Line
{
	// ���� ��(gradient)�� �ſ� ũ�ٰ� ������ �� ����� ��� ��
	const float verticalLineGradient = 1e5f;

	float gradient; // ���� ����(gradient)
	float y_intercept; // ���� y���� (y��� �����ϴ� ��)
	Vector2 pointOnLine_1; // �� ���� �� ��
	Vector2 pointOnLine_2; // �� ���� �� �ٸ� �� (���� ���� ���ǿ� ���)

	float gradientPerpendicular; // �� ���� ������ ���� ����(gradient)

	bool approachSide; // �־��� ���� ���� ��� �ʿ��� ���� ������ ��Ÿ��

	/// <summary>
	/// Line ����ü�� ������
	/// �� ���� �� ���� �� ���� ������ ���� ������� ���� �ʱ�ȭ
	/// </summary>
	/// <param name="pointOnLine">�� ���� �� ��</param>
	/// <param name="pointPerpendicularToLine">���� ������ ��</param>
	public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
	{
		// �� ���� x�� ���� ���
		float dx = pointOnLine.x - pointPerpendicularToLine.x;
		// �� ���� y�� ���� ���
		float dy = pointOnLine.y - pointPerpendicularToLine.y;

		// �������� ��� ���⸦ �ſ� ū ������ ����
		if (dx == 0)
		{
			gradientPerpendicular = verticalLineGradient;
		}
		else
		{
			// �������� �ƴ� ���, �������� ���⸦ ���
			gradientPerpendicular = dy / dx;
		}

		// �������� ���Ⱑ 0�� ���, �� ���� �������� ��
		if (gradientPerpendicular == 0)
		{
			gradient = verticalLineGradient;
		}
		else
		{
			// �׷��� ������ ���⸦ ��� (���� ���迡���� ���� ���� ���)
			gradient = -1 / gradientPerpendicular;
		}

		// y���� ��� (y = mx + b => b = y - mx)
		y_intercept = pointOnLine.y - gradient * pointOnLine.x;
		// �� ���� ù ��° �� ����
		pointOnLine_1 = pointOnLine;
		// �� ���� �� ��° �� ���� (���⸦ ������� ����)
		pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

		// �ʱ� approachSide ���� false�� ���� �� ���
		approachSide = false;
		approachSide = GetSide(pointPerpendicularToLine);
	}

	/// <summary>
	/// �־��� ���� ���� ��� �ʿ� ��ġ�ϴ��� �Ǵ�
	/// </summary>
	/// <param name="p">Ȯ���� ��</param>
	/// <returns>���� ���� ��� �ʿ� �ִ��� ���� (bool)</returns>
	bool GetSide(Vector2 p) =>
		(p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) >
		(p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);

	/// <summary>
	/// �־��� ���� ���� �Ѿ����� Ȯ��
	/// </summary>
	/// <param name="p">Ȯ���� ��</param>
	/// <returns>���� �ݴ������� �Ѿ���� ���� (bool)</returns>
	public bool HasCrossedLine(Vector2 p) => GetSide(p) != approachSide;

	/// <summary>
	/// Gizmos�� ����� ���� �ð������� ǥ��
	/// </summary>
	/// <param name="length">���� ����</param>
	public void DrawWithGizmos(int length)
	{
		// ���� ���� ��� (����ȭ ����)
		Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
		// ���� �߽� ��ġ ���
		Vector3 lineCenter = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
		// Gizmos�� ����Ͽ� �� �׸���
		Gizmos.DrawLine(lineCenter - lineDir * length / 2f, lineCenter + lineDir * length / 2f);
	}
}
