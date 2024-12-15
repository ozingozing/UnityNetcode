using System;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ObservableList 클래스의 새 인스턴스를 초기화합니다.  
/// 이 클래스는 비어 있거나, 지정된 리스트에서 요소를 복사하여 초기화됩니다.
/// </summary>
/// <param name="initialList"> 요소를 복사할 리스트입니다. </param>    
public interface IObservableList<T>
{
	/// <summary>
	/// ObservableList에 항목을 추가합니다.
	/// </summary>
	/// <param name="item">
	/// 추가할 항목입니다.
	/// </param>        
	void Add(T item);

	/// <summary>
	/// 지정된 인덱스에 항목을 삽입합니다.
	/// </summary>
	/// <param name="index"> 항목을 삽입할 0부터 시작하는 인덱스입니다. </param>
	/// <param name="item"> ObservableList에 삽입할 항목입니다. </param>        
	void Insert(int index, T item);

	/// <summary>
	/// ObservableList의 모든 항목을 제거합니다.
	/// </summary>        
	void Clear();

	/// <summary>
	/// ObservableList에 특정 항목이 포함되어 있는지 확인합니다.
	/// </summary>
	/// <param name="item"> 확인할 항목입니다. </param>        
	bool Contains(T item);

	/// <summary>
	/// ObservableList에서 특정 항목의 인덱스를 확인합니다.
	/// </summary>
	/// <param name="item"> ObservableList에서 위치를 확인할 항목입니다. </param>
	int IndexOf(T item);

	/// <summary>
	/// ObservableList의 요소를 배열로 복사하며, 지정된 인덱스에서 복사를 시작합니다.
	/// </summary>
	/// <param name="array"> ObservableList에서 복사된 요소가 저장될 대상 1차원 배열입니다. </param>
	/// <param name="arrayIndex"> 배열에서 복사를 시작할 0부터 시작하는 인덱스입니다. </param>        
	void CopyTo(T[] array, int arrayIndex);

	/// <summary>
	/// ObservableList에서 특정 항목의 첫 번째 항목을 제거합니다.
	/// </summary>
	/// <param name="item"> ObservableList에서 제거할 항목입니다. </param>        
	bool Remove(T item);

	/// <summary>
	/// ObservableList를 순회하는 제네릭 열거자를 반환합니다.
	/// </summary>
	/// <returns> 컬렉션을 순회하는 데 사용할 수 있는 제네릭 열거자입니다. </returns>        
	IEnumerator<T> GetEnumerator();

	/// <summary>
	/// ObservableList에서 지정된 인덱스의 항목을 제거합니다.
	/// </summary>
	/// <param name="index"> 제거할 항목의 0부터 시작하는 인덱스입니다. </param>        
	void RemoveAt(int index);
}

[Serializable]
public class ObservableList<T> : IList<T>, IObservableList<T>
{
	private readonly IList<T> list;
	public event Action<IList<T>> AnyValueChanged;

	/// <summary>
	/// 새 ObservableList<T> 인스턴스를 초기화합니다.  
	/// 초기 리스트가 제공되지 않으면 내부적으로 빈 리스트가 생성됩니다.
	/// </summary>
	/// <param name="initialList"> 초기화에 사용할 리스트입니다. </param>
	public ObservableList(IList<T> initialList = null)
	{
		list = initialList ?? new List<T>();
	}

	public T this[int index]
	{
		get => list[index];
		set
		{
			list[index] = value;
			Invoke();
		}
	}

	/// <summary>
	/// 이벤트 AnyValueChanged를 호출합니다.  
	/// 리스트의 값이 변경될 때 호출됩니다.
	/// </summary>
	public void Invoke() => AnyValueChanged?.Invoke(list);

	public int Count => list.Count;

	public bool IsReadOnly => list.IsReadOnly;

	/// <summary>
	/// 리스트에 항목을 추가하고 변경 이벤트를 호출합니다.
	/// </summary>
	/// <param name="item"> 추가할 항목입니다. </param>
	public void Add(T item)
	{
		list.Add(item);
		Invoke();
	}

	/// <summary>
	/// 리스트의 모든 항목을 제거하고 변경 이벤트를 호출합니다.
	/// </summary>
	public void Clear()
	{
		list.Clear();
		Invoke();
	}

	/// <summary>
	/// 리스트에 특정 항목이 포함되어 있는지 확인합니다.
	/// </summary>
	/// <param name="item"> 확인할 항목입니다. </param>
	/// <returns> 항목이 포함되어 있으면 true, 아니면 false입니다. </returns>
	public bool Contains(T item) => list.Contains(item);

	/// <summary>
	/// 리스트의 항목을 배열로 복사합니다.
	/// </summary>
	/// <param name="array"> 복사 대상 배열입니다. </param>
	/// <param name="arrayIndex"> 배열에서 복사를 시작할 인덱스입니다. </param>
	public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

	/// <summary>
	/// 리스트에서 항목을 제거하고, 성공하면 변경 이벤트를 호출합니다.
	/// </summary>
	/// <param name="item"> 제거할 항목입니다. </param>
	/// <returns> 제거에 성공하면 true, 아니면 false입니다. </returns>
	public bool Remove(T item)
	{
		var result = list.Remove(item);
		if (result)
		{
			Invoke();
		}

		return result;
	}

	/// <summary>
	/// 리스트를 순회하는 제네릭 열거자를 반환합니다.
	/// </summary>
	/// <returns> 리스트를 순회할 제네릭 열거자입니다. </returns>
	public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

	/// <summary>
	/// 리스트를 순회하는 비제네릭 열거자를 반환합니다.
	/// </summary>
	/// <returns> 리스트를 순회할 비제네릭 열거자입니다. </returns>
	IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

	/// <summary>
	/// 리스트에서 특정 항목의 인덱스를 반환합니다.
	/// </summary>
	/// <param name="item"> 확인할 항목입니다. </param>
	/// <returns> 항목의 인덱스입니다. 항목이 없으면 -1을 반환합니다. </returns>
	public int IndexOf(T item) => list.IndexOf(item);

	/// <summary>
	/// 지정된 인덱스에 항목을 삽입하고 변경 이벤트를 호출합니다.
	/// </summary>
	/// <param name="index"> 삽입할 인덱스입니다. </param>
	/// <param name="item"> 삽입할 항목입니다. </param>
	public void Insert(int index, T item)
	{
		list.Insert(index, item);
		Invoke();
	}

	/// <summary>
	/// 지정된 인덱스에서 항목을 제거하고 변경 이벤트를 호출합니다.
	/// </summary>
	/// <param name="index"> 제거할 항목의 인덱스입니다. </param>
	public void RemoveAt(int index)
	{
		T item = list[index];
		list.RemoveAt(index);
		Invoke();
	}
}
