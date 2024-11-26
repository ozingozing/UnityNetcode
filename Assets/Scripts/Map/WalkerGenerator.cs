using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class WalkerGenerator : MonoBehaviour
{
	private SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

	private async Task WaitForCondition() => await semaphore.WaitAsync();
	public void SetCondition() => semaphore.Release();

	public enum Grid
	{
		FLOOR,
		WALL,
		EMPTY
	}

	//Variables
	Queue<Vector3> SpawnFloorPos = new Queue<Vector3>();
	Queue<Vector3> SpawnWallPos = new Queue<Vector3>();

	public int SpawnPointCount;
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
			Vector3.down,  // �Ʒ�
            Vector3.left,  // ����
            Vector3.right, // ������
            Vector3.forward,  // ��
            Vector3.back,     // ��
			Vector3.up,    // ��

	};
	System.Random random = new System.Random();
	private bool isGridInitialized = false;

	public int _playerCount;
	public int playerCount
	{
		get { return _playerCount; }
		set 
		{
			if (_playerCount < TotalPlayers)
			{
				_playerCount = value;
			}
			else if(_playerCount == TotalPlayers)
			{
				_playerCount = TotalPlayers;
				Invoke("SetCondition", 5);
			}
			else _playerCount = TotalPlayers;
		}
	}
	public int TotalPlayers;
	//batch 
	const int batchSize = 10; // �� ���� ó���� Ÿ�� ����
	int batchCount = 0;

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
		if (NetworkManager.Singleton.ServerIsHost)
		{
			await InitializeGrid();
			//Queue�� ��Ƽ� ó���ϴ� ������ local������ �������� Network�󿡼� ������ ���ۿ� ���ϰ� ����
			StartCoroutine(Test());
		}
	}

	IEnumerator Test()
	{
		int TempCnt = 0;
		while (SpawnFloorPos.Count > 0)
		{
			GameObject GO = Instantiate(Floor, SpawnFloorPos.Dequeue(), Quaternion.identity, tileMap.transform);
			GO.GetComponent<NetworkObject>().Spawn();

			if (gridHandler[random.Next(0, MapWidth), random.Next(0, MapHeight)] == Grid.FLOOR
				&&TempCnt++ < SpawnPointCount)
			{
				InGameManager.Instance.NetworkManager.GetComponent<SpawnPoint>().SpawnPoints.Add(GO.transform.position);
			}

			yield return null;
		}

		while (SpawnWallPos.Count > 0)
		{
			GameObject GO = Instantiate(Wall, SpawnWallPos.Dequeue(), Quaternion.identity, tileMap.transform);
			GO.GetComponent<NetworkObject>().Spawn();
			yield return null;
		}

		StartCoroutine(CreateDeadline(new Vector3(TileCenter.x * width - OffSet.x - width / 2, 0, TileCenter.z * depth - OffSet.z - depth / 2)));
	}

	public async Task InitializeGrid()
	{
		Debug.Log("semaphore down!!!");
		await WaitForCondition();
		Debug.Log("semaphore up!!!");
		if (isGridInitialized) return; // Already Finish Create Flag
		isGridInitialized = true;
		
		WalkerObject curWalker = null;
		await Task.Run(async () => {
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
		

			/*GameObject GO = Instantiate(Floor, new Vector3(TileCenter.x * width - OffSet.x, 50, TileCenter.z * depth - OffSet.z), Quaternion.identity, tileMap.transform);
			GO.GetComponent<NetworkObject>().Spawn();*/
			SpawnFloorPos.Enqueue(new Vector3(TileCenter.x * width - OffSet.x, 50, TileCenter.z * depth - OffSet.z));

			Walkers.Add(curWalker);

			TileCount++;

			await CreateFloors();
			isGridInitialized = false; // Finish Create Flag
		});
	}

	IEnumerator CreateDeadline(Vector3 pinPoint)
	{
		// �� �������� ť�� ����
		foreach (var direction in Directions)
		{
			Vector3 position = pinPoint + direction * TileCenter.x * width; // �߾ӿ��� �� �������� offset��ŭ ������ ��ġ
			GameObject GO = Instantiate(BigWall, position, Quaternion.identity); // ť�� �ν��Ͻ�ȭ
			
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

		InGameManager.Instance.SpawnPlayer();
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
	}
	
	public async Task CreateFloors()
	{
		int TempCnt = 0;

		await Task.Run(() =>
		{
			while ((float)TileCount / (float)gridHandler.Length < FillPercentage)
			{
				bool hasCreatedFloor = false;
				foreach (WalkerObject curWalker in Walkers)
				{
					Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, 0, (int)curWalker.Position.z);

					if (gridHandler[curPos.x, curPos.z] != Grid.FLOOR)
					{
						Vector3 pos = new Vector3(curPos.x * width - OffSet.x, 50, curPos.z * depth - OffSet.z);
						SpawnFloorPos.Enqueue(pos);
						/*GameObject GO = Instantiate(Floor, pos, Quaternion.identity, tileMap.transform);
						GO.GetComponent<NetworkObject>().Spawn();*/

						/*if (UnityEngine.Random.value < 0.1f)
						{
							if (TempCnt++ < SpawnPointCount)
							{
								InGameManager.Instance.NetworkManager.GetComponent<SpawnPoint>().SpawnPoints.Add(pos);
							}
						}*/

						TileCount++;
						gridHandler[curPos.x, curPos.z] = Grid.FLOOR;
						hasCreatedFloor = true;

						batchCount++;
						if (batchCount >= batchSize)
						{
							batchCount = 0;
							//yield return new WaitForSeconds(WaitTime);
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
					//yield return new WaitForSeconds(WaitTime);
				}
			}
		});

		//Queue�� ��Ƽ� ó���ϴ� ������ local������ �������� Network�󿡼� ������ ���ۿ� ���ϰ� ����
		/*while(SpawnPos.Count > 0)
		{
			GameObject GO = Instantiate(Floor, SpawnPos.Dequeue(), Quaternion.identity, tileMap.transform);
			GO.GetComponent<NetworkObject>().Spawn();
			batchCount++;
			if (batchCount >= batchSize)
			{
				batchCount = 0;
				//yield return new WaitForSeconds(WaitTime);
			}
		}*/

		//StartCoroutine(CreateWalls());
		await CreateWalls();
	}

	void ChanceToRemove()
	{
		int updatedCount = Walkers.Count;
		for (int i = 0; i < updatedCount; i++)
		{
			/*if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count > 1)
			{
				Walkers.RemoveAt(i);
				break;
			}*/
			if (random.Next(0,10) * 0.1f < Walkers[i].ChanceToChange && Walkers.Count > 1)
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
			/*if (UnityEngine.Random.value < Walkers[i].ChanceToChange)
			{
				WalkerObject curWalker = Walkers[i];
				curWalker.Direction = GetDirection();
				Walkers[i] = curWalker;
			}*/
			if (random.Next(0, 10) * 0.1f < Walkers[i].ChanceToChange)
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
			/*if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < MaximumWalkers)
			{
				Vector3 newDirection = GetDirection();
				Vector3 newPosition = Walkers[i].Position;

				WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
				Walkers.Add(newWalker);
			}*/
			if (random.Next(0, 10) * 0.1f < Walkers[i].ChanceToChange && Walkers.Count < MaximumWalkers)
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

	async Task CreateWalls()
	{
		await Task.Run(() =>
		{
			Vector3 wall;
			for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
			{
				for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
				{
					if (gridHandler[x, y] == Grid.FLOOR)
					{
						//bool hasCreatedWall = false;

						if (gridHandler[x + 1, y] == Grid.EMPTY)
						{
							//SpawnPos.Enqueue(new Vector3((x + 1) * width - OffSet.x, 50, y * depth - OffSet.z));
							wall = new Vector3((x + 1) * width - OffSet.x, 50, y * depth - OffSet.z);
							SpawnWallPos.Enqueue(wall);
							/*GameObject GO = Instantiate(Wall, new Vector3((x + 1) * width - OffSet.x, 50, y * depth - OffSet.z), Quaternion.identity, tileMap.transform);
							GO.GetComponent<NetworkObject>().Spawn();*/
							gridHandler[x + 1, y] = Grid.WALL;
							//hasCreatedWall = true;
						}
						if (gridHandler[x - 1, y] == Grid.EMPTY)
						{
							//SpawnPos.Enqueue(new Vector3((x - 1) * width - OffSet.x, 50, y * depth - OffSet.z));
							wall = new Vector3((x - 1) * width - OffSet.x, 50, y * depth - OffSet.z);
							SpawnWallPos.Enqueue(wall);
							/*GameObject GO = Instantiate(Wall, new Vector3((x - 1) * width - OffSet.x, 50, y * depth - OffSet.z), Quaternion.identity, tileMap.transform);
							GO.GetComponent<NetworkObject>().Spawn();*/
							gridHandler[x - 1, y] = Grid.WALL;
							//hasCreatedWall = true;
						}
						if (gridHandler[x, y + 1] == Grid.EMPTY)
						{
							//SpawnPos.Enqueue(new Vector3(x * width - OffSet.x, 50, (y + 1) * depth - OffSet.z));
							wall = new Vector3(x * width - OffSet.x, 50, (y + 1) * depth - OffSet.z);
							SpawnWallPos.Enqueue(wall);
							/*GameObject GO = Instantiate(Wall, new Vector3(x * width - OffSet.x, 50, (y + 1) * depth - OffSet.z), Quaternion.identity, tileMap.transform);
							GO.GetComponent<NetworkObject>().Spawn();*/
							gridHandler[x, y + 1] = Grid.WALL;
							//hasCreatedWall = true;
						}
						if (gridHandler[x, y - 1] == Grid.EMPTY)
						{
							//SpawnPos.Enqueue(new Vector3(x * width - OffSet.x, 50, (y - 1) * depth - OffSet.z));
							wall = new Vector3(x * width - OffSet.x, 50, (y - 1) * depth - OffSet.z);
							SpawnWallPos.Enqueue(wall);
							/*GameObject GO = Instantiate(Wall, new Vector3(x * width - OffSet.x, 50, (y - 1) * depth - OffSet.z), Quaternion.identity, tileMap.transform);
							GO.GetComponent<NetworkObject>().Spawn();*/
							gridHandler[x, y - 1] = Grid.WALL;
							//hasCreatedWall = true;
						}

						/*if (hasCreatedWall)
						{
							yield return new WaitForSeconds(WaitTime);
						}*/
					}
				}
			}
		});

		/*while(SpawnPos.Count > 0)
		{
			GameObject GO = Instantiate(Wall, SpawnPos.Dequeue(), Quaternion.identity, tileMap.transform);
			GO.GetComponent<NetworkObject>().Spawn(); batchCount++;
			if (batchCount >= batchSize)
			{
				batchCount = 0;
				yield return new WaitForSeconds(WaitTime);
			}
		}*/

		//TODO: Create Deadline
		//StartCoroutine(CreateDeadline(new Vector3(TileCenter.x * width - OffSet.x, 0, TileCenter.z * depth - OffSet.z)));
	}

}