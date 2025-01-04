using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChocoOzing.CoreSystem
{
	public class CoreComponent : MonoBehaviour, ILogicUpdate
	{
		protected Core Core { get => core = (core != null) ? core : GetComponentInParent<Core>(); }
		private Core core;

		protected virtual void Awake()
		{
			//Debug.LogAssertion(this.gameObject.transform.parent.name);
			Core.AddComponent(this);
		}

		public virtual void LogicUpdate() { }
	}
}
