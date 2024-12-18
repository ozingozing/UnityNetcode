using UnityEngine;
using DG.Tweening;
using static LobbyManager;
using ChocoOzing.EventBusSystem;

public class InGamePlayerUIManager : MonoBehaviour
{
	public static InGamePlayerUIManager Instance { get; private set; }
	
	[SerializeField] private Transform playerSingleTemplate;
	[SerializeField] private Transform container;
	private RectTransform rectTransform;

	EventBinding<PlayerOnSpawnState> OnPlayerSpawnBinding;

	private void Awake()
	{
		Instance = this;
		rectTransform = GetComponent<RectTransform>();
		OnPlayerSpawnBinding = new EventBinding<PlayerOnSpawnState>(OnplayerSpanwed);
		EventBus<PlayerOnSpawnState>.Register(OnPlayerSpawnBinding);
	}

	private void OnDestroy()
	{
		EventBus<PlayerOnSpawnState>.Deregister(OnPlayerSpawnBinding);
		OnPlayerSpawnBinding = null;
	}

	void OnplayerSpanwed(PlayerOnSpawnState player)
	{
		if(player.state == ChocoOzing.EventBusSystem.PlayerState.Init)
		{
			Transform PlayerUI = Instantiate(playerSingleTemplate, container);
			PlayerUI.GetComponent<PlayerKDA>().TracePlayer(player.player);
		}
	}

	public void PanelFadeIn()
	{
		rectTransform.transform.localPosition = new Vector3(500, 0, 0);
		rectTransform.DOAnchorPos(new Vector2(0, 0), 0.5f, false).SetEase(Ease.OutElastic);
	}

	public void PanelFadeOut()
	{
		rectTransform.transform.localPosition = new Vector3(0, 0, 0);
		rectTransform.DOAnchorPos(new Vector2(500, 0), 0.5f, false).SetEase(Ease.InOutQuint);
	}
}
