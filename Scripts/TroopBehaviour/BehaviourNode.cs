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

public readonly struct NodeContext(TroopManager.IReadonlyTroopInfo troop, Goal goal, WorldState state)
{
	public TroopManager.IReadonlyTroopInfo Troop { get; } = troop;
	public Goal Goal { get; } = goal;
	public WorldState State { get; } = state;
}

public class NodeEvaluation
{
	NodeEvaluation(ResultType type, IGameAction? action)
	{
		Type = type;
		Result = action;
	}
	public enum ResultType
	{
		Ongoing,
		Idle,
		Failure,
	}
	public ResultType Type { get; }
	public IGameAction? Result { get; }

	public static NodeEvaluation Fail()
	{
		return new NodeEvaluation(ResultType.Failure, null);
	}

	public static NodeEvaluation Idle()
	{
		return new NodeEvaluation(ResultType.Idle, null);
	}

	public static NodeEvaluation FromAction(IGameAction action)
	{
		return new NodeEvaluation(ResultType.Ongoing, action);
	}
}

// public static class BehaviourNodeUtils{
//
// 	public enum ActionOnFailure
// 	{
// 		Retry,
// 		Skip,
// 		Fail
// 	}
// 	public static IEnumerable<NodeEvaluation> SkipNullEvaluations(IBehaviourNode node, NodeContext context)
// 	{
// 		foreach (NodeEvaluation evaluation in node.EvaluateActions(context)){
// 			if (evaluation.Result != null){
// 				yield return evaluation;
// 			}
//
//
// 		}
// 	}
// }

[GlobalClass]
public abstract partial class BehaviourNode : Resource, IBehaviourNode
{

	public abstract IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context);


}