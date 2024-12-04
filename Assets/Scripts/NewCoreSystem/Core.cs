using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChocoOzing.CoreSystem
{
	public class Core : MonoBehaviour
	{
		private readonly List<CoreComponent> CoreComponents = new List<CoreComponent>();
		[field: SerializeField] public GameObject Root {  get; private set; }

		private void Awake()
		{
			Root = Root ? Root : transform.parent.gameObject;
		}

		private void Start()
		{
			Debug.Log($"{transform.parent.gameObject.name} : {CoreComponents.Count}");
		}

		public void LogicUpdate()
		{
			foreach ( CoreComponent component in CoreComponents )
			{
				component.LogicUpdate();
			}
		}

		public void AddComponent(CoreComponent coreComponent)
		{
			if(!CoreComponents.Contains(coreComponent))
				CoreComponents.Add(coreComponent);
		}

		public T GetCoreComponent<T>() where T : CoreComponent
		{
			var comp = CoreComponents.OfType<T>().FirstOrDefault();
			if (comp != null)
				return comp;

			comp = GetComponentInChildren<T>();
			if(comp != null)
				return comp;

			Debug.LogWarning($"{typeof(T)} Not Found on {transform.parent.name}");
			return null;
		}

		public T GetCoreComponent<T>(ref T value) where T : CoreComponent
		{
			value = GetCoreComponent<T>();
			return value;
		}
	}
}