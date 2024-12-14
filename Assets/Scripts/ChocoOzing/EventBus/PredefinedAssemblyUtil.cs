using System.Collections.Generic;
using System.Reflection;
using System;
/// <summary>
/// Unity���� �̸� ���ǵ� ������� �۾��ϱ� ���� ��ƿ��Ƽ Ŭ����.
/// Ư�� ���ǿ� ���� ��������� Ÿ���� �˻��ϴ� ����� �����մϴ�.
/// </summary>
public static class PredefinedAssemblyUtil
{
	/// <summary>
	/// Unity���� ���Ǵ� ����� Ÿ���� ������ ������.
	/// </summary>
	enum AssemblyType
	{
		AssemblyCSharp,                  // Assembly-CSharp
		AssemblyCSharpEditor,            // Assembly-CSharp-Editor
		AssemblyCSharpEditorFirstPss,    // Assembly-CSharp-Editor-firstpass
		AssemblyCSharpFirstPass,         // Assembly-CSharp-firstpass
	}

	/// <summary>
	/// ����� �̸��� ���� AssemblyType�� ��ȯ.
	/// </summary>
	/// <param name="assemblyName">����� �̸�.</param>
	/// <returns>��ġ�ϴ� AssemblyType. ������ null ��ȯ.</returns>
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
	/// Ư�� �������̽��� ������ Ÿ���� ��������� �˻��Ͽ� �÷��ǿ� �߰�.
	/// </summary>
	/// <param name="assembly">�˻��� ������� Ÿ�� �迭.</param>
	/// <param name="types">�߰��� ��� �÷���.</param>
	/// <param name="interfaceType">�˻��� �������̽� Ÿ��.</param>
	static void AddTypesFromAssembly(Type[] assembly, ICollection<Type> types, Type interfaceType)
	{
		if (assembly == null) return;
		for (int i = 0; i < assembly.Length; i++)
		{
			Type type = assembly[i];
			if (type != interfaceType && interfaceType.IsAssignableFrom(type)) // �������̽� ���� ���� Ȯ��
			{
				types.Add(type);
			}
		}
	}

	/// <summary>
	/// Unity�� �̸� ���ǵ� ��������� Ư�� �������̽��� �����ϴ� ��� Ÿ���� �˻�.
	/// </summary>
	/// <param name="interfaceType">�˻��� �������̽� Ÿ��.</param>
	/// <returns>�ش� �������̽��� �����ϴ� Ÿ�� ���.</returns>
	public static List<Type> GetTypes(Type interfaceType)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies(); // ���� �������� ��� ����� ��������

		Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
		List<Type> types = new List<Type>();
		foreach (Assembly assembly in assemblies)
		{
			AssemblyType? assemblyType = GetAssemblyType(assembly.GetName().Name); // ����� �̸� ��Ī
			if (assemblyType != null)
			{
				assemblyTypes.Add((AssemblyType)assemblyType, assembly.GetTypes()); // ������� Ÿ�� ����
			}
		}

		// �ʿ��� ��������� Ÿ�� �˻� �� �߰�
		AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharp], types, interfaceType);
		AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharpEditor], types, interfaceType);

		return types;
	}
}
