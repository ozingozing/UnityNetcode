using System.Collections;
using System.Collections.Generic;
using ChocoOzing.Utilities;
using UnityEngine;

/// <summary>
/// ���(Path)�� �����ϴ� Ŭ����. 
/// �־��� ��������Ʈ�� ������� �̵� ��ο� ȸ�� ���(turn boundaries)�� ����.
/// </summary>
public class Path
{
	// �б� ���� �迭: ��θ� ���󰡾� �� ��ǥ��(��������Ʈ)
	public readonly Vector3[] lookPoints;

	// �б� ���� �迭: �� ��������Ʈ ������ ȸ�� ��輱�� ����
	public readonly Line[] turnBoundaries;

	// ����� ������ ��輱 �ε���
	public readonly int finishLineIndex;

	public readonly int slowDownIndex;

	/// <summary>
	/// Path ������. ��������Ʈ�� ���� ��ġ, ȸ�� �Ÿ�(turn distance)�� �̿��� ��θ� ����.
	/// </summary>
	/// <param name="waypoints">��θ� �̷�� ��������Ʈ �迭</param>
	/// <param name="startPos">��� ���� ��ġ</param>
	/// <param name="turnDst">ȸ�� �Ÿ� (��������Ʈ���� ȸ�� ��踦 �󸶳� ����Ʈ���� ����)</param>
	public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst)
	{
		// ��������Ʈ�� ��� ������ ����
		this.lookPoints = waypoints;

		// �� ��������Ʈ ���� ȸ�� ��輱�� ������ �迭 �ʱ�ȭ
		turnBoundaries = new Line[lookPoints.Length];

		// ������ ��輱�� �ε��� ���
		finishLineIndex = turnBoundaries.Length - 1;

		// ���� ��ġ�� 2D ���ͷ� ��ȯ�Ͽ� ���� ����Ʈ�� ����
		Vector2 previousPoint = Vector3Extensions.Vector3ToVector2(startPos);

		// ��� ��������Ʈ�� ���� �ݺ�
		for (int i = 0; i < lookPoints.Length; i++)
		{
			// ���� ��������Ʈ�� 2D ���ͷ� ��ȯ
			Vector2 currentPoint = Vector3Extensions.Vector3ToVector2(lookPoints[i]);

			// ���� ����Ʈ���� ���� ����Ʈ���� ���� ���͸� ����ȭ
			Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;

			// ȸ�� ��� ���� ����
			// ������ ��������Ʈ�� ȸ�� ��� ���� �ش� ������ �״�� ��谡 ��
			// �� ���� ���, ���� ����Ʈ���� ȸ�� �Ÿ���ŭ �ڷ� �̵��� ��ġ�� ���� ����
			Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;

			// ��輱�� ���� (turnBoundaryPoint�� �������� ��� ����)
			turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);

			// ���� ����Ʈ�� ������Ʈ
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
	/// ��ο� ��輱�� Gizmos�� ���� �ð������� ǥ��.
	/// </summary>
	public void DrawWithGizmos(Transform targetPos = null)
	{
		if(targetPos != null)
		{
			Gizmos.color = Color.cyan;
			DrawHexagon(GridGizmo.instance.NodeFromWorldPoint(targetPos.position).worldPosition + Vector3.up/2, GridGizmo.instance.hexRadius);
		}
		// ����� �� ��������Ʈ�� ������ ť��� ǥ��
		Gizmos.color = Color.yellow;
		foreach (Vector3 p in lookPoints)
		{
			DrawHexagon(p + Vector3.up / 2, GridGizmo.instance.hexRadius); // ��������Ʈ ��ġ ���� ��¦ ��� ǥ��
		}

		// �� ȸ�� ��輱�� ������� ǥ��
		Gizmos.color = Color.white;
		foreach (Line l in turnBoundaries)
		{
			l.DrawWithGizmos(10); // �� ��輱�� ���� 10���� �׸�
		}
	}

	private void DrawHexagon(Vector3 center, float radius)
	{
		Vector3[] vertices = new Vector3[6];
		float rotationOffset = Mathf.Deg2Rad * -30; // -30�Ƹ� �������� ��ȯ

		for (int i = 0; i < 6; i++)
		{
			float angle = Mathf.Deg2Rad * (60 * i) + rotationOffset; // ������ -30�� ȸ��
			vertices[i] = new Vector3(
				center.x + radius * Mathf.Cos(angle),
				center.y,
				center.z + radius * Mathf.Sin(angle)
			);
		}

		// ���� ���� ������ ���������� �׸�
		for (int i = 0; i < 6; i++)
		{
			Vector3 start = vertices[i];
			Vector3 end = vertices[(i + 1) % 6]; // ������ ������ ù ��° ������ ����
			Gizmos.DrawLine(start, end);
		}
	}
}
