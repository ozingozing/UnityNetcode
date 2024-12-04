using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChocoOzing.CoreSystem
{
	public class CoreComponent : MonoBehaviour, ILogicUpdate
	{
		protected Core core;

		protected virtual void Awake()
		{
			core = transform.parent.GetComponent<Core>();
			//Debug.LogAssertion(this.gameObject.transform.parent.name);
			if(core == null)
			{
				Debug.LogError("There isn't Core on the parent");
			}
			core.AddComponent(this);
		}

		public virtual void LogicUpdate() { }
	}
}
