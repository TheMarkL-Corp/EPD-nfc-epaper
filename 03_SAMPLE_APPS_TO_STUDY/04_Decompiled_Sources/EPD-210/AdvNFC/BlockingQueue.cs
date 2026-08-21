using System;
using System.Collections.Generic;
using System.Threading;

public class BlockingQueue<T>
{
	private string m_name;

	private readonly int m_maxSize;

	private Queue<T> m_queue;

	private bool m_isRunning;

	private ManualResetEvent m_enqueueWait;

	private ManualResetEvent m_dequeueWait;

	public Action<string> m_actionOutLog;

	public int Count => m_queue.Count;

	public BlockingQueue(int maxSize, string name = "BlockingQueue", bool isRunning = false)
	{
		m_maxSize = maxSize;
		m_name = name;
		m_queue = new Queue<T>(m_maxSize);
		m_isRunning = isRunning;
		m_enqueueWait = new ManualResetEvent(false);
		m_dequeueWait = new ManualResetEvent(false);
	}

	private void OutLog(string message)
	{
	}

	public void Open()
	{
		m_isRunning = true;
	}

	public void Close()
	{
		m_isRunning = false;
		m_dequeueWait.Set();
	}

	public void Enqueue(T item)
	{
		if (m_isRunning)
		{
			while (true)
			{
				lock (m_queue)
				{
					if (m_queue.Count < m_maxSize)
					{
						m_queue.Enqueue(item);
						m_enqueueWait.Reset();
						m_dequeueWait.Set();
						OutLog(m_name + " 入队成功.");
						return;
					}
				}
				m_enqueueWait.WaitOne();
			}
		}
		OutLog(m_name + " 队列终止，不允许入队");
	}

	public bool Dequeue(ref T item)
	{
		while (m_isRunning)
		{
			lock (m_queue)
			{
				if (m_queue.Count > 0)
				{
					item = m_queue.Dequeue();
					m_dequeueWait.Reset();
					m_enqueueWait.Set();
					OutLog(m_name + " 出队成功.");
					return true;
				}
			}
			m_dequeueWait.WaitOne();
		}
		lock (m_queue)
		{
			return false;
		}
	}

	public void Clear()
	{
		m_queue.Clear();
	}
}
