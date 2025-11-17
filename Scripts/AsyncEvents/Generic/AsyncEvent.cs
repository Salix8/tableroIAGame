using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.AsyncEvents.Generic;

public interface IAsyncDispatcher<in TArgs>
{
	public Task DispatchParallel(TArgs args);
	public Task DispatchSequential(TArgs args);
}

public interface IAsyncHandlerCollection<out TArgs>
{
	public void Subscribe(Func<TArgs, Task> awaitable);
	public void Unsubscribe(Func<TArgs, Task> awaitable);
}

public class AsyncEvent<TArgs> : IAsyncHandlerCollection<TArgs>, IAsyncDispatcher<TArgs>
{
	readonly List<Func<TArgs, Task>> subscribers = [];
	public void Subscribe(Func<TArgs, Task> awaitable)
	{
		lock (subscribersLock){
			subscribers.Add(awaitable);
		}
	}

	public void Unsubscribe(Func<TArgs, Task> awaitable)
	{
		lock (subscribersLock){
			subscribers.Remove(awaitable);
		}
	}

	readonly object subscribersLock = new();

	public async Task DispatchParallel(TArgs args)
	{
		Func<TArgs, Task>[] snapshot;
		lock (subscribersLock){
			snapshot = subscribers.ToArray();
		}
		Task[] tasks = new Task[ snapshot.Length];
		for (int i = 0; i < snapshot.Length; i++){
			Func<TArgs, Task> func = snapshot[i];
			tasks[i] = func(args);
		}
		await Task.WhenAll(tasks);
	}

	public async Task DispatchSequential(TArgs args)
	{

		Func<TArgs, Task>[] snapshot;
		lock (subscribersLock){
			snapshot = subscribers.ToArray();
		}
		foreach (Func<TArgs, Task> t in snapshot){
			await t.Invoke(args);
		}
	}
}
