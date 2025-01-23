using ChocoOzing.EventBusSystem;
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
	Vector3Int BlockSizeOffSet;
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
	const int batchSize = 10; // 한 번에 처리할 타일 개수
	int batchCount = 0;

	private void Awake()
	{
		width = Floor.GetComponent<Renderer>().bounds.size.x;
		height = Floor.GetComponent<Renderer>().bounds.size.y;
		depth = Floor.GetComponent<Renderer>().bounds.size.z;
	}

	EventBinding<LobbyEventArgs> eventBinding;
	private void Start()
	{
		eventBinding = new EventBinding<LobbyEventArgs>(Func);
		EventBus<LobbyEventArgs>.Register(new EventBinding<LobbyEventArgs>(Func));
	}

	private void OnDestroy()
	{
		if(eventBinding != null)
		{
			eventBinding.Remove(Func);
			EventBus<LobbyEventArgs>.Deregister(eventBinding);
			eventBinding = null;
		}
	}

	public async void Func(LobbyEventArgs e)
	{
		if (e.state == LobbyState.Start)
		{
			if (NetworkManager.Singleton.ServerIsHost)
			{
				await InitializeGrid();
				//Queue에 담아서 처리하는 형식은 local상으론 괜찮은데 Network상에선 데이터 전송에 부하가 생김
				//StartCoroutine(CreateMap());
				await CreatMapAsync();
				await GridGizmo.instance.DoCreateGrid();
				InGameManager.Instance.SpawnPlayer();
			}
		}
	}

	private async Task CreatMapAsync()
	{
		var tsc = new TaskCompletionSource<bool>();
		StartCoroutine(CreateMap(tsc));
		await tsc.Task; //CreateMap에서 상태완료/예외 반환전까지 대기
	}

	IEnumerator CreateMap(TaskCompletionSource<bool> tcs)
	{
		int TempCnt = 0;
		while (SpawnFloorPos.Count > 0)
		{
			GameObject GO = Instantiate(Floor, SpawnFloorPos.Dequeue(), Quaternion.identity, tileMap.transform);
			GO.GetComponent<NetworkObject>().Spawn();

			if (random.Next(0, 10) == 5
				&& gridHandler[random.Next(0, MapWidth), random.Next(0, MapHeight)] == Grid.FLOOR
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

		//StartCoroutine(CreateDeadline(new Vector3(TileCenter.x * width - BlockSizeOffSet.x - width / 2, 0, TileCenter.z * depth - BlockSizeOffSet.z - depth / 2)));
		StartCoroutine(CreateDeadline(new Vector3(TileCenter.x * width, 0, TileCenter.z * depth) - BlockSizeOffSet));

		yield return new WaitForSeconds(2);
		tcs.SetResult(true);
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
			BlockSizeOffSet = new Vector3Int(TileCenter.x * (int)width, 0, TileCenter.z * (int)depth);

			curWalker = new WalkerObject(TileCenter, GetDirection(), 0.5f);
			gridHandler[TileCenter.x, TileCenter.z] = Grid.FLOOR;
		

			SpawnFloorPos.Enqueue(new Vector3(TileCenter.x * width, 50, TileCenter.z * depth) - BlockSizeOffSet);
			Walkers.Add(curWalker);

			TileCount++;

			await CreateFloors();
			isGridInitialized = false; // Finish Create Flag
		});
	}

	IEnumerator CreateDeadline(Vector3 pinPoint)
	{
		// 각 방향으로 큐브 생성
		foreach (var direction in Directions)
		{
			Vector3 position = pinPoint + direction * ((gridHandler.GetLength(0) + 2) / 2) * width; // 중앙에서 각 방향으로 offset만큼 떨어진 위치
			GameObject GO = Instantiate(BigWall, position, Quaternion.identity); // 큐브 인스턴스화
			
			if (direction == Vector3.right || direction == Vector3.left)
			{
				GO.transform.localScale = new Vector3(1, (gridHandler.GetLength(0) + 2) * height, (gridHandler.GetLength(0) + 2) * depth);
			}
			else if (direction == Vector3.forward || direction == Vector3.back)
			{
				GO.transform.localScale = new Vector3((gridHandler.GetLength(0) + 2) * width, (gridHandler.GetLength(0) + 2) * height, 1);
			}
			else
			{
				GO.transform.localScale = new Vector3((gridHandler.GetLength(0) + 2) * width, 1, (gridHandler.GetLength(0) + 2) * depth);
			}

			GO.GetComponent<NetworkObject>().Spawn();

			yield return new WaitForSeconds(1);
		}

		//InGameManager.Instance.SpawnPlayer();
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
		await Task.Run(() =>
		{
			while ((float)TileCount / (float)gridHandler.Length < FillPercentage)
			{
				foreach (WalkerObject curWalker in Walkers)
				{
					Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, 0, (int)curWalker.Position.z);
					if (gridHandler[curPos.x, curPos.z] != Grid.FLOOR)
					{
						Vector3 pos = new Vector3(curPos.x * width, 50, curPos.z * depth) - BlockSizeOffSet;
						SpawnFloorPos.Enqueue(pos);

						TileCount++;
						gridHandler[curPos.x, curPos.z] = Grid.FLOOR;

						batchCount++;
						if (batchCount >= batchSize)
						{
							batchCount = 0;
						}
					}
				}

				//Walker Methods
				ChanceToRemove();
				ChanceToRedirect();
				ChanceToCreate();
				UpdatePosition();
			}
		});

		await CreateWalls();
	}

	void ChanceToRemove()
	{
		int updatedCount = Walkers.Count;
		for (int i = 0; i < updatedCount; i++)
		{
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
							wall = new Vector3((x + 1) * width , 50, y * depth) - BlockSizeOffSet;

							SpawnWallPos.Enqueue(wall);
							gridHandler[x + 1, y] = Grid.WALL;
						}
						if (gridHandler[x - 1, y] == Grid.EMPTY)
						{
							wall = new Vector3((x - 1) * width, 50, y * depth) - BlockSizeOffSet;

							SpawnWallPos.Enqueue(wall);
							gridHandler[x - 1, y] = Grid.WALL;
						}
						if (gridHandler[x, y + 1] == Grid.EMPTY)
						{
							wall = new Vector3(x * width, 50, (y + 1) * depth) - BlockSizeOffSet;

							SpawnWallPos.Enqueue(wall);
							gridHandler[x, y + 1] = Grid.WALL;
						}
						if (gridHandler[x, y - 1] == Grid.EMPTY)
						{
							wall = new Vector3(x * width, 50, (y - 1) * depth) - BlockSizeOffSet;

							SpawnWallPos.Enqueue(wall);
							gridHandler[x, y - 1] = Grid.WALL;
						}
					}
				}
			}
		});
	}

}