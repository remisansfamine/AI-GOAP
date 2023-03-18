
namespace GOAP
{
    class Goal<StateT> 
    {
        public Goal(StateT activeStates, StateT inactiveStates)
        {
            this.activeStates = activeStates;
            this.inactiveStates = inactiveStates;
        }

        public StateT activeStates = default(StateT);
        public StateT inactiveStates = default(StateT);
    }

    class Node<StateT>
    {
        public StateT worldState = default(StateT);

        public Node<StateT> parent = null;

        public List<Node<StateT>> children = new List<Node<StateT>>();

        public Action<StateT> action = null;

        public int runningCost = 0;
    }

    public enum EActionState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    abstract class Action<StateT>
    {
        public virtual bool IsForwardValid(StateT worldState) => false;

        public virtual bool IsBackwardValid(StateT worldState) => false;

        public virtual StateT ApplyEffects(in StateT worldState) => worldState;

        public virtual StateT ApplyReversedEffects(in StateT worldState) => worldState;

        public abstract int GetCost(in StateT worldState);

        public virtual int GetReverseCost(in StateT worldState) => GetCost(worldState);

        public abstract EActionState Execute();
    }

    class Planner<GoalT>
    {
        static private List<Action<StateT>> CreateActionSubSet<StateT>(List<Action<StateT>> usableActions, Action<StateT> toRemoveAction)
        {
            return usableActions.Where(action => action != toRemoveAction).ToList();
        }

        static private Node<StateT> CreateNode<StateT>(Node<StateT> parent, StateT newWorldState, Action<StateT> action, int runningCost)
        {
            Node<StateT> newNode = new Node<StateT>();
            newNode.action = action;
            newNode.worldState = newWorldState;
            newNode.parent = parent;
            parent.children.Add(newNode);
            newNode.runningCost = runningCost;

            return newNode;
        }

        static public void BuildGraph<StateT>(StateT initialState, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT goal, Func<StateT, GoalT, bool> goalChecker, bool usePruning)
        {
            Node<StateT> parent = new Node<StateT>();
            parent.worldState = initialState;

            int? cheapestCost = usePruning ? int.MaxValue : null;
            BuildGraph(parent, leaves, availableActions, goal, goalChecker, ref cheapestCost);
        }

        static public void BuildGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT goal, Func<StateT, GoalT, bool> goalChecker, ref int? cheapestCost)
        {
            foreach (Action<StateT> action in availableActions)
            {
                if (!action.IsForwardValid(parent.worldState)) // Check preconditions
                    continue;

                int runningCost = parent.runningCost + action.GetCost(parent.worldState);
                StateT newWorldState = action.ApplyEffects(parent.worldState);

                // Prune branches which are more expensive than the cheapest one
                if (runningCost > cheapestCost)
                    continue;

                Node<StateT> node = CreateNode(parent, newWorldState, action, runningCost); // node.cost = parent.cost + action.cost(worldState)

                if (goalChecker(newWorldState, goal))
                {
                    // Save cheapest cost when a branch of the plan is complete
                    if (cheapestCost.HasValue)
                        cheapestCost = Math.Min(cheapestCost.Value, node.runningCost);

                    leaves.Add(node);
                    continue;
                }

                // Used action is removed
                List<Action<StateT>> actionsSubSet = CreateActionSubSet(availableActions, action);

                // Recursive call on the new node
                BuildGraph(node, leaves, actionsSubSet, goal, goalChecker, ref cheapestCost);
            }
        }

        static public void BuildReversedGraph<StateT>(StateT goal, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT initialState, Func<StateT, GoalT, bool> goalChecker, bool usePruning)
        {
            Node<StateT> parent = new Node<StateT>();
            parent.worldState = goal;

            int? cheapestCost = usePruning ? int.MaxValue : null;
            BuildReversedGraph(parent, leaves, availableActions, initialState, goalChecker, ref cheapestCost);
        }

        static public void BuildReversedGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT initialState, Func<StateT, GoalT, bool> goalChecker, ref int? cheapestCost)
        {
            foreach (Action<StateT> action in availableActions)
            {
                if (!action.IsBackwardValid(parent.worldState)) // Check preconditions
                    continue;

                int runningCost = parent.runningCost + action.GetReverseCost(parent.worldState);
                StateT newWorldState = action.ApplyReversedEffects(parent.worldState);

                // Prune branches which are more expensive than the cheapest one
                if (runningCost > cheapestCost)
                    continue;

                Node<StateT> node = CreateNode(parent, newWorldState, action, runningCost); // node.cost = parent.cost + action.cost

                if (goalChecker(newWorldState, initialState))
                {
                    if (cheapestCost.HasValue)
                        cheapestCost = Math.Min(cheapestCost.Value, node.runningCost);

                    leaves.Add(node);
                    continue;
                }

                // used action is removed
                List<Action<StateT>> actionsSubSet = CreateActionSubSet(availableActions, action);

                // recursive call on the new node
                BuildReversedGraph(node, leaves, actionsSubSet, initialState, goalChecker, ref cheapestCost);
            }
        }

        static public List<Node<StateT>> GetBestLeaves<StateT>(List<Node<StateT>> leaves) => leaves.Where((ws) => ws.runningCost == leaves.Min(ws => ws.runningCost)).ToList();

        static public List<Node<StateT>> UnrollLeaf<StateT>(Node<StateT> toUnroll, bool isReversed = true)
        {
            List<Node<StateT>> bestBranch = new List<Node<StateT>>();

            Node<StateT> currentNode = toUnroll;

            while (currentNode.parent is not null)
            {
                bestBranch.Add(currentNode);
                currentNode = currentNode.parent;
            }

            if (isReversed)
                bestBranch.Reverse();

            return bestBranch;
        }
    }
}
