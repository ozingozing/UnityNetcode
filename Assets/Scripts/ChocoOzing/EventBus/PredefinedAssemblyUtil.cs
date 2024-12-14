using System.Collections.Generic;
using System.Reflection;
using System;
/// <summary>
/// Unity에서 미리 정의된 어셈블리와 작업하기 위한 유틸리티 클래스.
/// 특정 조건에 따라 어셈블리에서 타입을 검색하는 기능을 제공합니다.
/// </summary>
public static class PredefinedAssemblyUtil
{
	/// <summary>
	/// Unity에서 사용되는 어셈블리 타입을 정의한 열거형.
	/// </summary>
	enum AssemblyType
	{
		AssemblyCSharp,                  // Assembly-CSharp
		AssemblyCSharpEditor,            // Assembly-CSharp-Editor
		AssemblyCSharpEditorFirstPss,    // Assembly-CSharp-Editor-firstpass
		AssemblyCSharpFirstPass,         // Assembly-CSharp-firstpass
	}

	/// <summary>
	/// 어셈블리 이름에 따라 AssemblyType을 반환.
	/// </summary>
	/// <param name="assemblyName">어셈블리 이름.</param>
	/// <returns>일치하는 AssemblyType. 없으면 null 반환.</returns>
	static AssemblyType? GetAssemblyType(string assemblyName)
	{
		return assemblyName switch
		{
			"Assembly-CSharp" => AssemblyType.AssemblyCSharp,
			"Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
			"Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPss,
			"Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
			_ => null
		};
	}

	/// <summary>
	/// 특정 인터페이스를 구현한 타입을 어셈블리에서 검색하여 컬렉션에 추가.
	/// </summary>
	/// <param name="assembly">검색할 어셈블리의 타입 배열.</param>
	/// <param name="types">추가할 대상 컬렉션.</param>
	/// <param name="interfaceType">검색할 인터페이스 타입.</param>
	static void AddTypesFromAssembly(Type[] assembly, ICollection<Type> types, Type interfaceType)
	{
		if (assembly == null) return;
		for (int i = 0; i < assembly.Length; i++)
		{
			Type type = assembly[i];
			if (type != interfaceType && interfaceType.IsAssignableFrom(type)) // 인터페이스 구현 여부 확인
			{
				types.Add(type);
			}
		}
	}

	/// <summary>
	/// Unity의 미리 정의된 어셈블리에서 특정 인터페이스를 구현하는 모든 타입을 검색.
	/// </summary>
	/// <param name="interfaceType">검색할 인터페이스 타입.</param>
	/// <returns>해당 인터페이스를 구현하는 타입 목록.</returns>
	public static List<Type> GetTypes(Type interfaceType)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies(); // 현재 도메인의 모든 어셈블리 가져오기

		Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
		List<Type> types = new List<Type>();
		foreach (Assembly assembly in assemblies)
		{
			AssemblyType? assemblyType = GetAssemblyType(assembly.GetName().Name); // 어셈블리 이름 매칭
			if (assemblyType != null)
			{
				assemblyTypes.Add((AssemblyType)assemblyType, assembly.GetTypes()); // 어셈블리의 타입 저장
			}
		}

		// 필요한 어셈블리에서 타입 검색 및 추가
		AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharp], types, interfaceType);
		AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharpEditor], types, interfaceType);

		return types;
	}
}
