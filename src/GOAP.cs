
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

    abstract class Action<StateT>
    {
        public virtual bool IsValid(StateT worldState) => false;

        public virtual bool IsReverseValid(StateT worldState) => false;

        public virtual StateT ApplyEffects(in StateT worldState) => worldState;

        public virtual StateT ApplyReversedEffects(in StateT worldState) => worldState;

        public abstract int GetCost(in StateT worldState);

        public abstract void Execute();
    }

    class Planner<GoalT>
    {
        static private List<Action<StateT>> CreateActionSubSet<StateT>(List<Action<StateT>> usableActions, Action<StateT> toRemoveAction)
        {
            return usableActions.Where(action => action != toRemoveAction).ToList();
        }

        static private Node<StateT> CreateNode<StateT>(Node<StateT> parent, StateT newWorldState, Action<StateT> action)
        {
            Node<StateT> newNode = new Node<StateT>();
            newNode.action = action;
            newNode.worldState = newWorldState;
            newNode.parent = parent;
            parent.children.Add(newNode);
            newNode.runningCost = parent.runningCost + action.GetCost(newWorldState);

            return newNode;
        }

        static public void BuildGraph<StateT>(StateT initialState, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT goal, Func<StateT, GoalT, bool> goalChecker)
        {
            Node<StateT> parent = new Node<StateT>();
            parent.worldState = initialState;

            int cheapestCost = int.MaxValue;
            BuildGraph(parent, leaves, availableActions, goal, goalChecker, ref cheapestCost);
        }

        static public void BuildGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT goal, Func<StateT, GoalT, bool> goalChecker)
        {
            int cheapestCost = int.MaxValue;
            BuildGraph(parent, leaves, availableActions, goal, goalChecker, ref cheapestCost);
        }

        static public void BuildGraph<StateT>(StateT initialState, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT goal, Func<StateT, GoalT, bool> goalChecker, ref int cheapestCost)
        {
            Node<StateT> parent = new Node<StateT>();
            parent.worldState = initialState;

            BuildGraph(parent, leaves, availableActions, goal, goalChecker, ref cheapestCost);
        }

        static public void BuildGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT goal, Func<StateT, GoalT, bool> goalChecker, ref int cheapestCost)
        {
            foreach (Action<StateT> action in availableActions)
            {
                if (!action.IsValid(parent.worldState)) // Check preconditions
                    continue;

                StateT newWorldState = action.ApplyEffects(parent.worldState);
                Node<StateT> node = CreateNode(parent, newWorldState, action); // node.cost = parent.cost + action.cost

                // Prune branches which are more expensive than the cheapest one
                if (node.runningCost > cheapestCost)
                    continue;

                if (goalChecker(newWorldState, goal))
                {
                    cheapestCost = Math.Min(cheapestCost, node.runningCost);

                    leaves.Add(node);
                    continue;
                }

                // used action is removed
                List<Action<StateT>> actionsSubSet = CreateActionSubSet(availableActions, action);

                // recursive call on the new node
                BuildGraph(node, leaves, actionsSubSet, goal, goalChecker, ref cheapestCost);
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

        static public void BuildReversedGraph<StateT>(StateT goal, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT initialState, Func<StateT, GoalT, bool> goalChecker)
        {
            Node<StateT> parent = new Node<StateT>();
            parent.worldState = goal;

            BuildReversedGraph(parent, leaves, availableActions, initialState, goalChecker);
        }

        static public void BuildReversedGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT initialState, Func<StateT, GoalT, bool> goalChecker)
        {
            foreach (Action<StateT> action in availableActions)
            {
                if (!action.IsReverseValid(parent.worldState)) // Check preconditions
                    continue;

                StateT newWorldState = action.ApplyReversedEffects(parent.worldState);
                Node<StateT> node = CreateNode(parent, newWorldState, action); // node.cost = parent.cost + action.cost

                if (goalChecker(newWorldState, initialState))
                {
                    leaves.Add(node);
                    continue;
                }

                // used action is removed
                List<Action<StateT>> actionsSubSet = CreateActionSubSet(availableActions, action);

                // recursive call on the new node
                BuildReversedGraph(node, leaves, actionsSubSet, initialState, goalChecker);
            }
        }
    }
}
