#nullable enable
using System;
using System.Collections.Generic;
using Game.State;
using Godot;

namespace Game.TroopBehaviour;

public interface IBehaviourNode
{
	public IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context);
}

public readonly struct NodeContext(TroopManager.TroopInfo troop, Goal goal, WorldState state)
{
	public TroopManager.TroopInfo Troop { get; } = troop;
	public Goal Goal { get; } = goal;
	public WorldState State { get; } = state;
}

public readonly struct NodeEvaluation(NodeEvaluation.ResultType type, IGameAction? result)
{
	public enum ResultType
	{
		Success,
		Failure,
	}
	public ResultType Type { get; } = type;
	public IGameAction? Result { get; } = result;
}

public static class BehaviourNodeUtils{

	public enum ActionOnFailure
	{
		Retry,
		Skip,
		Fail
	}
	public static IEnumerable<NodeEvaluation> EvaluateNode(IBehaviourNode node, NodeContext context, ActionOnFailure onFail)
	{
		foreach (NodeEvaluation evaluation in node.EvaluateActions(context)){
			if (evaluation.Type == NodeEvaluation.ResultType.Failure){
				switch (onFail){
					case ActionOnFailure.Retry:
						continue;
					case ActionOnFailure.Skip:
						yield break;
					case ActionOnFailure.Fail:
						yield return evaluation;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			yield return evaluation;


		}
	}
}

public abstract partial class BehaviourNode : Resource, IBehaviourNode
{

	public abstract IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context);


}