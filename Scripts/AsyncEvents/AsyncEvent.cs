using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.AsyncEvents;
public interface IAsyncDispatcher
{
	public Task DispatchParallel();
	public Task DispatchSequential();
}

public interface IAsyncHandlerCollection
{
	public void Subscribe(Func<Task> awaitable);
	public void Unsubscribe(Func<Task> awaitable);
}
public class AsyncEvent : IAsyncDispatcher,IAsyncHandlerCollection
{
	readonly List<Func<Task>> subscribers = [];
	public void Subscribe(Func<Task> awaitable)
	{
		lock (subscribersLock){
			subscribers.Add(awaitable);
		}
	}

	public void Unsubscribe(Func<Task> awaitable)
	{
		lock (subscribersLock){
			subscribers.Remove(awaitable);
		}
	}

	readonly object subscribersLock = new();

	public async Task DispatchParallel()
	{
		Func<Task>[] snapshot;
		lock (subscribersLock){
			snapshot = subscribers.ToArray();
		}
		Task[] tasks = new Task[ snapshot.Length];
		for (int i = 0; i < snapshot.Length; i++){
			Func<Task> func = snapshot[i];
			tasks[i] = func();
		}
		await Task.WhenAll(tasks);
	}

	public async Task DispatchSequential()
	{

		Func<Task>[] snapshot;
		lock (subscribersLock){
			snapshot = subscribers.ToArray();
		}
		foreach (Func<Task> t in snapshot){
			await t.Invoke();
		}
	}
}