using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class WalkerGenerator : MonoBehaviour
{
	public enum Grid
	{
		FLOOR,
		WALL,
		EMPTY
	}

	//Variables
	public Grid[,] gridHandler;
	public List<WalkerObject> Walkers;
	public GameObject tileMap;
	public GameObject Floor;
	public GameObject Wall;
	public GameObject BigWall;
	public int MapWidth = 30;
	public int MapHeight = 30;

	public int MaximumWalkers = 10;
	public int TileCount = default;
	public float FillPercentage = 0.4f;
	public float WaitTime = 0.05f;

	public float width;
	public float height;
	public float depth;
	Vector3Int TileCenter;
	Vector3Int OffSet;
	Vector3[] Directions = {
			Vector3.down,  // 아래
            Vector3.left,  // 왼쪽
            Vector3.right, // 오른쪽
            Vector3.forward,  // 앞
            Vector3.back,     // 뒤
			Vector3.up,    // 위

	};
	System.Random random = new System.Random();
	private bool isGridInitialized = false;

	private void Awake()
	{
		width = Floor.GetComponent<Renderer>().bounds.size.x;
		height = Floor.GetComponent<Renderer>().bounds.size.y;
		depth = Floor.GetComponent<Renderer>().bounds.size.z;
	}

	private void Start()
	{
		LobbyManager.Instance.OnGameStarted += Func;
	}

	public async void Func(object sender, System.EventArgs e)
	{
		if (NetworkManager.Singleton.IsHost)
			await InitializeGrid();
	}

	public async Task InitializeGrid()
	{
		if (isGridInitialized) return; // Already Finish Create Flag
		isGridInitialized = true;
		
		WalkerObject curWalker = null;
		await Task.Run(() => {
			gridHandler = new Grid[MapWidth, MapHeight];

			for (int x = 0; x < gridHandler.GetLength(0); x++)
			{
				for (int y = 0; y < gridHandler.GetLength(1); y++)
				{
					gridHandler[x, y] = Grid.EMPTY;
				}
			}

			Walkers = new List<WalkerObject>();

			TileCenter = new Vector3Int(gridHandler.GetLength(0) / 2, 0, gridHandler.GetLength(1) / 2);
			OffSet = new Vector3Int(TileCenter.x * (int)width, 0, TileCenter.z * (int)depth);

			curWalker = new WalkerObject(TileCenter, GetDirection(), 0.5f);
			gridHandler[TileCenter.x, TileCenter.z] = Grid.FLOOR;
		});

		GameObject GO = Instantiate(Floor, new Vector3(TileCenter.x * width - OffSet.x, -height / 2, TileCenter.z * depth - OffSet.z), Quaternion.identity, tileMap.transform);
		GO.GetComponent<NetworkObject>().Spawn();
		Walkers.Add(curWalker);

		TileCount++;

		StartCoroutine(CreateFloors());
		isGridInitialized = false; // Finish Create Flag
	}

	IEnumerator CreateDeadline(Vector3 pinPoint)
	{
		// 각 방향으로 큐브 생성
		foreach (var direction in Directions)
		{
			Vector3 position = pinPoint + direction * TileCenter.x * width; // 중앙에서 각 방향으로 offset만큼 떨어진 위치
			GameObject GO = Instantiate(BigWall, position, Quaternion.identity); // 큐브 인스턴스화
			if (direction == Vector3.right || direction == Vector3.left)
			{
				GO.transform.localScale = new Vector3(1, gridHandler.GetLength(0) * height, gridHandler.GetLength(0) * depth);
			}
			else if (direction == Vector3.forward || direction == Vector3.back)
			{
				GO.transform.localScale = new Vector3(gridHandler.GetLength(0) * width, gridHandler.GetLength(0) * height, 1);
			}
			else
			{
				GO.transform.localScale = new Vector3(gridHandler.GetLength(0) * width, 1, gridHandler.GetLength(0) * depth);
			}

			GO.GetComponent<NetworkObject>().Spawn();

			yield return new WaitForSeconds(1);
		}
	}

	Vector3 GetDirection()
	{
		int choice = random.Next(0, 4);
		return choice switch
		{
			0 => Vector3.back,
			1 => Vector3.forward,
			2 => Vector3.left,
			3 => Vector3.right,
			_ => Vector3.zero,
		};

		/*int choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

		switch (choice)
		{
			case 0:
				return Vector3.back;
			case 1:
				return Vector3.left;
			case 2:
				return Vector3.forward;
			case 3:
				return Vector3.right;
			default:
				return Vector3.zero;
		}*/
	}

	IEnumerator CreateFloors()
	{
		//batch 
		const int batchSize = 10; // 한 번에 처리할 타일 개수
		int batchCount = 0;

		while ((float)TileCount / (float)gridHandler.Length < FillPercentage)
		{
			bool hasCreatedFloor = false;
			foreach (WalkerObject curWalker in Walkers)
			{
				Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, 0, (int)curWalker.Position.z);

				if (gridHandler[curPos.x, curPos.z] != Grid.FLOOR)
				{
					GameObject GO = Instantiate(Floor, new Vector3(curPos.x * width - OffSet.x, -height / 2, curPos.z * depth - OffSet.z), Quaternion.identity, tileMap.transform);
					GO.GetComponent<NetworkObject>().Spawn();
					TileCount++;
					gridHandler[curPos.x, curPos.z] = Grid.FLOOR;
					hasCreatedFloor = true;

					batchCount++;
					if(batchCount >= batchSize)
					{
						batchCount = 0;
						yield return new WaitForSeconds(WaitTime);
					}
				}
			}

			//Walker Methods
			ChanceToRemove();
			ChanceToRedirect();
			ChanceToCreate();
			UpdatePosition();

			if (hasCreatedFloor)
			{
				yield return new WaitForSeconds(WaitTime);
			}
		}

		StartCoroutine(CreateWalls());
	}

	void ChanceToRemove()
	{
		int updatedCount = Walkers.Count;
		for (int i = 0; i < updatedCount; i++)
		{
			if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count > 1)
			{
				Walkers.RemoveAt(i);
				break;
			}
		}
	}

	void ChanceToRedirect()
	{
		for (int i = 0; i < Walkers.Count; i++)
		{
			if (UnityEngine.Random.value < Walkers[i].ChanceToChange)
			{
				WalkerObject curWalker = Walkers[i];
				curWalker.Direction = GetDirection();
				Walkers[i] = curWalker;
			}
		}
	}

	void ChanceToCreate()
	{
		int updatedCount = Walkers.Count;
		for (int i = 0; i < updatedCount; i++)
		{
			if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < MaximumWalkers)
			{
				Vector3 newDirection = GetDirection();
				Vector3 newPosition = Walkers[i].Position;

				WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
				Walkers.Add(newWalker);
			}
		}
	}

	void UpdatePosition()
	{
		for (int i = 0; i < Walkers.Count; i++)
		{
			WalkerObject FoundWalker = Walkers[i];
			FoundWalker.Position += FoundWalker.Direction;
			FoundWalker.Position.x = Mathf.Clamp(FoundWalker.Position.x, 1, gridHandler.GetLength(0) - 2);
			FoundWalker.Position.z = Mathf.Clamp(FoundWalker.Position.z, 1, gridHandler.GetLength(1) - 2);
			Walkers[i] = FoundWalker;
		}
	}

	IEnumerator CreateWalls()
	{
		for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
		{
			for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
			{
				if (gridHandler[x, y] == Grid.FLOOR)
				{
					bool hasCreatedWall = false;

					if (gridHandler[x + 1, y] == Grid.EMPTY)
					{
						GameObject GO = Instantiate(Wall, new Vector3((x + 1) * width - OffSet.x, -height / 2, y * depth - OffSet.z), Quaternion.identity, tileMap.transform);
						GO.GetComponent<NetworkObject>().Spawn();
						gridHandler[x + 1, y] = Grid.WALL;
						hasCreatedWall = true;
					}
					if (gridHandler[x - 1, y] == Grid.EMPTY)
					{
						GameObject GO = Instantiate(Wall, new Vector3((x - 1) * width - OffSet.x, -height / 2, y * depth - OffSet.z), Quaternion.identity, tileMap.transform);
						GO.GetComponent<NetworkObject>().Spawn();
						gridHandler[x - 1, y] = Grid.WALL;
						hasCreatedWall = true;
					}
					if (gridHandler[x, y + 1] == Grid.EMPTY)
					{
						GameObject GO = Instantiate(Wall, new Vector3(x * width - OffSet.x, -height / 2, (y + 1) * depth - OffSet.z), Quaternion.identity, tileMap.transform);
						GO.GetComponent<NetworkObject>().Spawn();
						gridHandler[x, y + 1] = Grid.WALL;
						hasCreatedWall = true;
					}
					if (gridHandler[x, y - 1] == Grid.EMPTY)
					{
						GameObject GO = Instantiate(Wall, new Vector3(x * width - OffSet.x, -height / 2, (y - 1) * depth - OffSet.z), Quaternion.identity, tileMap.transform);
						GO.GetComponent<NetworkObject>().Spawn();
						gridHandler[x, y - 1] = Grid.WALL;
						hasCreatedWall = true;
					}

					if (hasCreatedWall)
					{
						yield return new WaitForSeconds(WaitTime);
					}
				}
			}
		}

		//TODO: Create Deadline
		StartCoroutine(CreateDeadline(new Vector3(TileCenter.x * width - OffSet.x, 0, TileCenter.z * depth - OffSet.z)));
	}

}