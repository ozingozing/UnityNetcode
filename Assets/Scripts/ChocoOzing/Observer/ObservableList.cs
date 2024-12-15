using System;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ObservableList Ŭ������ �� �ν��Ͻ��� �ʱ�ȭ�մϴ�.  
/// �� Ŭ������ ��� �ְų�, ������ ����Ʈ���� ��Ҹ� �����Ͽ� �ʱ�ȭ�˴ϴ�.
/// </summary>
/// <param name="initialList"> ��Ҹ� ������ ����Ʈ�Դϴ�. </param>    
public interface IObservableList<T>
{
	/// <summary>
	/// ObservableList�� �׸��� �߰��մϴ�.
	/// </summary>
	/// <param name="item">
	/// �߰��� �׸��Դϴ�.
	/// </param>        
	void Add(T item);

	/// <summary>
	/// ������ �ε����� �׸��� �����մϴ�.
	/// </summary>
	/// <param name="index"> �׸��� ������ 0���� �����ϴ� �ε����Դϴ�. </param>
	/// <param name="item"> ObservableList�� ������ �׸��Դϴ�. </param>        
	void Insert(int index, T item);

	/// <summary>
	/// ObservableList�� ��� �׸��� �����մϴ�.
	/// </summary>        
	void Clear();

	/// <summary>
	/// ObservableList�� Ư�� �׸��� ���ԵǾ� �ִ��� Ȯ���մϴ�.
	/// </summary>
	/// <param name="item"> Ȯ���� �׸��Դϴ�. </param>        
	bool Contains(T item);

	/// <summary>
	/// ObservableList���� Ư�� �׸��� �ε����� Ȯ���մϴ�.
	/// </summary>
	/// <param name="item"> ObservableList���� ��ġ�� Ȯ���� �׸��Դϴ�. </param>
	int IndexOf(T item);

	/// <summary>
	/// ObservableList�� ��Ҹ� �迭�� �����ϸ�, ������ �ε������� ���縦 �����մϴ�.
	/// </summary>
	/// <param name="array"> ObservableList���� ����� ��Ұ� ����� ��� 1���� �迭�Դϴ�. </param>
	/// <param name="arrayIndex"> �迭���� ���縦 ������ 0���� �����ϴ� �ε����Դϴ�. </param>        
	void CopyTo(T[] array, int arrayIndex);

	/// <summary>
	/// ObservableList���� Ư�� �׸��� ù ��° �׸��� �����մϴ�.
	/// </summary>
	/// <param name="item"> ObservableList���� ������ �׸��Դϴ�. </param>        
	bool Remove(T item);

	/// <summary>
	/// ObservableList�� ��ȸ�ϴ� ���׸� �����ڸ� ��ȯ�մϴ�.
	/// </summary>
	/// <returns> �÷����� ��ȸ�ϴ� �� ����� �� �ִ� ���׸� �������Դϴ�. </returns>        
	IEnumerator<T> GetEnumerator();

	/// <summary>
	/// ObservableList���� ������ �ε����� �׸��� �����մϴ�.
	/// </summary>
	/// <param name="index"> ������ �׸��� 0���� �����ϴ� �ε����Դϴ�. </param>        
	void RemoveAt(int index);
}

[Serializable]
public class ObservableList<T> : IList<T>, IObservableList<T>
{
	private readonly IList<T> list;
	public event Action<IList<T>> AnyValueChanged;

	/// <summary>
	/// �� ObservableList<T> �ν��Ͻ��� �ʱ�ȭ�մϴ�.  
	/// �ʱ� ����Ʈ�� �������� ������ ���������� �� ����Ʈ�� �����˴ϴ�.
	/// </summary>
	/// <param name="initialList"> �ʱ�ȭ�� ����� ����Ʈ�Դϴ�. </param>
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
	/// �̺�Ʈ AnyValueChanged�� ȣ���մϴ�.  
	/// ����Ʈ�� ���� ����� �� ȣ��˴ϴ�.
	/// </summary>
	public void Invoke() => AnyValueChanged?.Invoke(list);

	public int Count => list.Count;

	public bool IsReadOnly => list.IsReadOnly;

	/// <summary>
	/// ����Ʈ�� �׸��� �߰��ϰ� ���� �̺�Ʈ�� ȣ���մϴ�.
	/// </summary>
	/// <param name="item"> �߰��� �׸��Դϴ�. </param>
	public void Add(T item)
	{
		list.Add(item);
		Invoke();
	}

	/// <summary>
	/// ����Ʈ�� ��� �׸��� �����ϰ� ���� �̺�Ʈ�� ȣ���մϴ�.
	/// </summary>
	public void Clear()
	{
		list.Clear();
		Invoke();
	}

	/// <summary>
	/// ����Ʈ�� Ư�� �׸��� ���ԵǾ� �ִ��� Ȯ���մϴ�.
	/// </summary>
	/// <param name="item"> Ȯ���� �׸��Դϴ�. </param>
	/// <returns> �׸��� ���ԵǾ� ������ true, �ƴϸ� false�Դϴ�. </returns>
	public bool Contains(T item) => list.Contains(item);

	/// <summary>
	/// ����Ʈ�� �׸��� �迭�� �����մϴ�.
	/// </summary>
	/// <param name="array"> ���� ��� �迭�Դϴ�. </param>
	/// <param name="arrayIndex"> �迭���� ���縦 ������ �ε����Դϴ�. </param>
	public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

	/// <summary>
	/// ����Ʈ���� �׸��� �����ϰ�, �����ϸ� ���� �̺�Ʈ�� ȣ���մϴ�.
	/// </summary>
	/// <param name="item"> ������ �׸��Դϴ�. </param>
	/// <returns> ���ſ� �����ϸ� true, �ƴϸ� false�Դϴ�. </returns>
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
	/// ����Ʈ�� ��ȸ�ϴ� ���׸� �����ڸ� ��ȯ�մϴ�.
	/// </summary>
	/// <returns> ����Ʈ�� ��ȸ�� ���׸� �������Դϴ�. </returns>
	public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

	/// <summary>
	/// ����Ʈ�� ��ȸ�ϴ� �����׸� �����ڸ� ��ȯ�մϴ�.
	/// </summary>
	/// <returns> ����Ʈ�� ��ȸ�� �����׸� �������Դϴ�. </returns>
	IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

	/// <summary>
	/// ����Ʈ���� Ư�� �׸��� �ε����� ��ȯ�մϴ�.
	/// </summary>
	/// <param name="item"> Ȯ���� �׸��Դϴ�. </param>
	/// <returns> �׸��� �ε����Դϴ�. �׸��� ������ -1�� ��ȯ�մϴ�. </returns>
	public int IndexOf(T item) => list.IndexOf(item);

	/// <summary>
	/// ������ �ε����� �׸��� �����ϰ� ���� �̺�Ʈ�� ȣ���մϴ�.
	/// </summary>
	/// <param name="index"> ������ �ε����Դϴ�. </param>
	/// <param name="item"> ������ �׸��Դϴ�. </param>
	public void Insert(int index, T item)
	{
		list.Insert(index, item);
		Invoke();
	}

	/// <summary>
	/// ������ �ε������� �׸��� �����ϰ� ���� �̺�Ʈ�� ȣ���մϴ�.
	/// </summary>
	/// <param name="index"> ������ �׸��� �ε����Դϴ�. </param>
	public void RemoveAt(int index)
	{
		T item = list[index];
		list.RemoveAt(index);
		Invoke();
	}
}
