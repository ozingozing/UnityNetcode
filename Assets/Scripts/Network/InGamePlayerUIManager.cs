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


	private void Awake()
	{
		Instance = this;
		rectTransform = GetComponent<RectTransform>();
	}
	EventBinding<PlayerOnSpawnEvent> OnPlayerSpawnBinding;
	private void OnEnable()
	{
		OnPlayerSpawnBinding = new EventBinding<PlayerOnSpawnEvent>(OnplayerSpanwed);
		EventBus<PlayerOnSpawnEvent>.Register(OnPlayerSpawnBinding);
	}

	private void OnDisable()
	{
		EventBus<PlayerOnSpawnEvent>.Deregister(OnPlayerSpawnBinding);
	}

	void OnplayerSpanwed(PlayerOnSpawnEvent player)
	{
		Transform PlayerUI = Instantiate(playerSingleTemplate, container);
		PlayerUI.GetComponent<PlayerKDA>().TracePlayer(player.player);
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
