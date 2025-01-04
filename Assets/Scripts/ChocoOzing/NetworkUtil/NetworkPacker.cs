using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChocoOzing.Network
{
	/// <summary>
	/// EncodeCoordinate value from float to short
	/// Compress vlue of short to int
	/// </summary>

	interface ICompressVector3
	{
		public float MAX { get; }
		public float MIN { get; }
		public short EncodeCoordinate(float value, float min, float max);
		public float DecodeCoordinate(int value, float min, float max);

		public int PackVector3(Vector3 position);
		public Vector3 UnpackVector3(int packed);
	}

	interface ICompressQuaternion
	{
		public int PackQuaternion(Quaternion quaternion);
		public Quaternion UnpackQuaternion(int packed);
	}

	public class Vector3Compressor : ICompressVector3
	{
		private float QuantizationValue = 1023f; //10bits

		public float MAX { get; }
		public float MIN { get; }
		
		public Vector3Compressor(float max, float min)
		{
			this.MAX = max;
			this.MIN = min;
		}

		public int PackVector3(Vector3 position)
		{
			int packed = 0;
			packed |= (EncodeCoordinate(position.x, MIN, MAX) & 0x3FF) << 20;
			packed |= (EncodeCoordinate(position.y, MIN, MAX) & 0x3FF) << 10;
			packed |= (EncodeCoordinate(position.z, MIN, MAX) & 0x3FF);
			return packed;
		}

		public Vector3 UnpackVector3(int packed)
		{
			float x = DecodeCoordinate((packed >> 20) & 0x3FF, MIN, MAX);
			float y = DecodeCoordinate((packed >> 10) & 0x3FF, MIN, MAX);
			float z = DecodeCoordinate(packed & 0x3FF, MIN, MAX);
			return new Vector3(x, y, z);
		}

		public short EncodeCoordinate(float value, float min, float max)
		{
			float normalized = Mathf.Clamp((value - min) / (max - min), 0f, 1f);
			return (short)(normalized * QuantizationValue);
		}

		public float DecodeCoordinate(int value, float min, float max)
		{
			float normalized = value / (float)QuantizationValue;
			return normalized * (max - min) + min;
		}
	}

	public class QuaternionCompressor : ICompressQuaternion
	{
		private float QuantizationValue = 1023f; //10bits
		public int PackQuaternion(Quaternion quaternion)
		{
			float largest = Mathf.Abs(quaternion.x);
			int largestIndex = 0;

			if (Mathf.Abs(quaternion.y) > largest) { largest = Mathf.Abs(quaternion.y); largestIndex = 1; };
			if (Mathf.Abs(quaternion.z) > largest) { largest = Mathf.Abs(quaternion.z); largestIndex = 2; };
			if (Mathf.Abs(quaternion.w) > largest) { largest = Mathf.Abs(quaternion.w); largestIndex = 3; };

			int sign = (quaternion[largestIndex] < 0) ? 1 : 0;

			float a = quaternion[(largestIndex + 1) % 4];
			float b = quaternion[(largestIndex + 2) % 4];
			float c = quaternion[(largestIndex + 3) % 4];

			int packedA = Mathf.RoundToInt((a + 1f) * QuantizationValue);
			int packedB = Mathf.RoundToInt((b + 1f) * QuantizationValue);
			int packedC = Mathf.RoundToInt((c + 1f) * QuantizationValue);

			return
				(largestIndex << 30) |
				(sign << 29) |
				(packedA << 20) |
				(packedB << 10) |
				packedC;
		}

		public Quaternion UnpackQuaternion(int packed)
		{
			int largestIndex = (packed >> 30) & 0x3;
			int sign = (packed >> 29) & 0x1;
			int packedA = (packed >> 20) & 0x3FF;
			int packedB = (packed >> 10) & 0x3FF;
			int packedC = packed & 0x3FF;

			//Restorate Quaternion value around [-1, 1]
			float a = (packedA / QuantizationValue) - 1f;
			float b = (packedB / QuantizationValue) - 1f;
			float c = (packedC / QuantizationValue) - 1f;
			//Restorate  Quaternion w value
			float w = Mathf.Sqrt(1f - a * a - b * b - c * c);

			Quaternion quaternion = new Quaternion();
			quaternion[(largestIndex + 1) % 4] = a;
			quaternion[(largestIndex + 2) % 4] = b;
			quaternion[(largestIndex + 3) % 4] = c;
			quaternion[largestIndex] = (sign == 1) ? -w : w;

			return quaternion;
		}
	}
}
