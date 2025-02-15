using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChocoOzing.Utilities
{
	public static class GameObjectExtensionMethod
	{
		public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component)
		{
			component = gameObject.GetComponentInChildren<T>();
			return component != null;
		}

		public static bool TryGetComponentInChildren<T>(this Component comp, out T component)
		{
			return TryGetComponentInChildren(comp.gameObject, out component);
		}

		public static bool TryGetComponenInParent<T>(this GameObject gameObject, out T component)
		{
			component = gameObject.GetComponentInParent<T>();
			return component != null;
		}

		public static bool TryGetComponenInParent<T>(this Component comp, out T component)
		{
			return TryGetComponenInParent(comp.gameObject, out component);
		}
	}

	public static class Vector3Extensions
	{
		/// <summary>
		/// Sets any values of the Vector3
		/// </summary>
		public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
		{
			return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
		}

		/// <summary>
		/// Adds to any values of the Vector3
		/// </summary>
		public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null)
		{
			return new Vector3(vector.x + (x ?? 0), vector.y + (y ?? 0), vector.z + (z ?? 0));
		}

		/// <summary>
		/// Vector3 change to Vector2
		/// </summary>
		/// <param name="vector3"></param>
		/// <returns></returns>
		public static Vector2 Vector3ToVector2(Vector3 vector3) => new Vector2(vector3.x, vector3.z);
	}
}
