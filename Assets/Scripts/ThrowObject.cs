using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    public Vector3 setStartPoint;
    public Vector3 setEndPoint;
    public float duration = 2.0f;
    public float gravity = -9.8f;
    public Vector3 velocity;

    public async void TrowInit(Transform start, Vector3 end)
    {
		setStartPoint = start.position;
		setEndPoint = end;

		//시작점, 끝점을 기준 초기 속도
		Vector3 displacement = setEndPoint - setStartPoint;
        float time = duration;

        //x, y방향 속도
        velocity.x = displacement.x / time;
        velocity.z = displacement.z / time;

        //y방향 속도
        velocity.y = displacement.y / time - .5f * gravity * time;

		await Throw();
    }

	Node node;
	async Task Throw()
	{
		float elapsed = 0f;

		while (true)
		{
			elapsed += Time.deltaTime;

			// 포물선 공식에 따라 위치 계산
			Vector3 currentPosition = setStartPoint
				+ velocity * elapsed
				+ 0.5f * new Vector3(0, gravity, 0) * Mathf.Pow(elapsed, 2);

			//transform.rotation = Quaternion.LookRotation(velocity);
			transform.position = currentPosition;

			// 목표 위치와 현재 위치의 거리가 충분히 가까워졌다면 루프 종료
			float threshold = Mathf.Max(0.1f, velocity.magnitude * Time.deltaTime);
			float distanceToTarget = Vector3.Distance(currentPosition, setEndPoint);
			if (distanceToTarget < threshold) // 0.1f는 오차 범위 (필요 시 조정)
			{
				break;
			}

			await Task.Yield(); // 다음 프레임까지 대기
		}

		// 정확히 도착점에 정렬
		transform.position = setEndPoint;
		node = await GridGizmo.instance.CheckAgain(transform.position);
	}

	private void OnDestroy()
	{
		if (node != null)
		{
			node.ReturnToOriginValue();
		}
	}
}
