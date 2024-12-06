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

		/// <summary>
		/// If you ingerit CoreComponent, ParentClass can
		/// add a list of child calsses to the parent class, 
		/// so you only have to find and use that list from the outside!!!
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			RB = GetComponentInParent<Rigidbody>();
			canSetVelocity = true;
		}

		public override void LogicUpdate()
		{
			CurrentVelocity = RB.velocity;
		}
	}
}
