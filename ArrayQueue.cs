//------------------------------------------------------------------------------
//  <copyright from='2004' to='2005' company='Jerremy Koot and William Archbell'>
//    Copyright (c) Jerremy Koot and William Archbell. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://nzb.wordtgek.nl/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------
//
// File:    ArrayQueue.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;

namespace NZB_O_Matic
{
	/// <summary>
	/// A Queue and ArrayList hybrid. Supports queuing but is also enumerable.
	/// </summary>
	public class ArrayQueue : Queue, ICloneable, ICollection, IList, IEnumerable, IEnumerator
	{
		private ArrayList m_Array;
		private IEnumerator m_Enum;

		#region Queue Members

		public ArrayQueue()
		{
			lock(this)
			{
				m_Array = new ArrayList();
				m_Enum = m_Array.GetEnumerator();
			}
		}

		public override object Dequeue()
		{
			object ret;
			lock(this)
			{
				ret = m_Array[0];
				m_Array.RemoveAt(0);
			}
			return ret;
		}

		public override void Enqueue(object obj)
		{
			lock(this)
			{
				m_Array.Add(obj);
			}
		}

		public override bool Equals(object obj)
		{
			return m_Array.Equals(obj);
		}

		public override int GetHashCode()
		{
			return m_Array.GetHashCode();
		}

		public override object Peek()
		{
			return m_Array[m_Array.Count-1];
		}

		public override object[] ToArray()
		{
			return m_Array.ToArray();
		}

		public override string ToString()
		{
			return "ArrayQueue";
		}

		public override void TrimToSize()
		{
			lock(this)
			{
				m_Array.Capacity = m_Array.Count;
			}
		}



		#endregion

		#region ICloneable Members

		public new object Clone()
		{
			lock(this)
			{
				return m_Array.Clone();
			}
		}

		#endregion

		#region ICollection Members

		public new bool IsSynchronized
		{
			get
			{
				return m_Array.IsSynchronized;
			}
		}

		public new int Count
		{
			get
			{
				return m_Array.Count;
			}
		}

		public new void CopyTo(Array array, int index)
		{
			lock(this)
			{
				m_Array.CopyTo(array, index);
			}
		}

		public new object SyncRoot
		{
			get
			{
				return m_Array.SyncRoot;
			}
		}

		#endregion

		#region IEnumerable Members

		public new IEnumerator GetEnumerator()
		{
			return (IEnumerator)this; 
		}

		#endregion

		#region IList Members

		public bool IsReadOnly
		{
			get
			{
				return m_Array.IsReadOnly;
			}
		}

		public object this[int index]
		{
			get
			{
				return m_Array[index];
			}
			set
			{
				lock(this)
				{
					m_Array[index] = value;
				}
			}
		}

		public void RemoveAt(int index)
		{
			lock(this)
			{
				m_Array.RemoveAt(index);
			}
		}

		public void Insert(int index, object value)
		{
			lock(this)
			{
				m_Array.Insert(index, value);
			}
		}

		public void Remove(object value)
		{
			lock(this)
			{
				m_Array.Remove(value);
			}
		}

		public new bool Contains(object value)
		{
			return m_Array.Contains(value);
		}

		public new void Clear()
		{
			lock(this)
			{
				m_Array.Clear();
			}
		}

		public int IndexOf(object value)
		{
			return m_Array.IndexOf(value);
		}

		public int Add(object value)
		{
			return m_Array.Add(value);
		}

		public bool IsFixedSize
		{
			get
			{
				return m_Array.IsFixedSize;
			}
		}

		#endregion

		#region IEnumerator Members

		public void Reset()
		{
			lock(this)
			{
				m_Enum.Reset();
			}
		}

		public object Current
		{
			get
			{
				return m_Enum.Current;
			}
		}

		public bool MoveNext()
		{
			return m_Enum.MoveNext();
		}

		#endregion
	}
}
