using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChocoOzing.CoreSystem
{
	public class Movement : CoreComponent
	{
		public Rigidbody RB {  get; private set; }
		public Vector3 workSpace { get; private set; }
		public Vector3 CurrentVelocity { get; private set; }
		public bool canSetVelocity {  get; private set; }

		public Observer<bool> IsMoveLock;

		/// <summary>
		/// If you ingerit CoreComponent, ParentClass can
		/// add a list of child calsses to the parent class, 
		/// so you only have to find and use that list from the outside!!!
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			RB = Core.Root.GetComponent<Rigidbody>();
			canSetVelocity = true;
		}

		public override void OnNetworkSpawn()
		{
			if(IsLocalPlayer)
				IsMoveLock = new Observer<bool>(false, StopMove);
			base.OnNetworkSpawn();
		}

		public override void OnDestroy()
		{
			if(IsLocalPlayer)
				IsMoveLock.Dispose();
			base.OnDestroy();
		}

		public override void LogicUpdate()
		{
			CurrentVelocity = RB.velocity;
		}

		public void StopMove(bool value)
		{
			if(value)
			{
				RB.velocity = Vector3.zero;
				RB.angularVelocity = Vector3.zero;
			}
		}
	}
}
