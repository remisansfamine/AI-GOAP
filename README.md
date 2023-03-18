# Goal Oriented Action Planning - AI Planner - **C#**
### ISART DIGITAL GP3, School Project: *Rémi GINER*  
<br>

<!-- ABOUT THE PROJECT -->
# About The Project 
**Built with .NET 6.0**

The goal of this project is to develop a GOAP planner using a forward search. I made the choice not to waste time on the graphic aspect in order to concentrate on the architecture to make it as flexible as possible.

# Table of contents
1. [Features](#features)
2. [Controls](#controls)
3. [Details](#details)
    - [Usage](#usage)
    - [Basic Implementation](#basic-implementation)
    - [Plan Execution](#plan-execution)
    - [Backward Implementation](#backward-implementation)
    - [Performance](#performance)
4. [In the future](#itf)
5. [References](#references)
6. [Versionning](#versionning)
7. [Autors](#authors)


# Features
- Forward planning
- Backward planning
- Bitwise world states
- Pruning
- Dynamic action cost for forward planning
- Plan execution with continuous and discrete actions
- Opened architecture to any type of state structure

# Controls
During the execution, you will be asked questions in order to configure the construction of the plan.

# Details
## Usage
Before the plan is made, you can set the way the plan will be built. You will be asked: 
- If you want to add GarbageActions
- If yes, how many do you want to add
- If you want to use Forward planning or Backward planning
- If you want to use pruning

GarbageActions are useless actions used to test the performance of the algorithm by filling the action pool in order to slow down the development of the plan. If GarbageActions are used while the forward planning is used without pruning, this can considerably slow down its execution. 

## Basic Implementation

### **Templatization**

In order to have the most modular system possible, the WorldState is templatized. This allows the user to create his own class easily.

```cs
static public void BuildGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, GoalT goal, Func<StateT, GoalT, bool> goalChecker, ref int? cheapestCost)
```

### **Bitwise operations**

In the current context, EnumFlags are used as WorldState. This allows to use bitwise operations without verbosity by passing through operators (which Bitarrays do not offer).

```cs
public override ENPCStates ApplyEffects(in ENPCStates worldState)
{
    ENPCStates newWorldstate = worldState;
    newWorldstate |= ENPCStates.IS_NEAR_WEAPON;
    newWorldstate &= ~ENPCStates.IS_NEAR_PLAYER;

    return newWorldstate;
}
```

### **Dynamic costs**

When Forward Planning is used, action costs are retrieved dynamically.

```cs
class KillEnemy : GOAP.Action<ENPCStates>
{
    ... 

    public override int GetCost(in ENPCStates worldState) => (ENPCStates.HAS_WEAPON & worldState) != 0 ? 1 : 10;

    ...
}
```

### **Pruning**
In order to make the basic algorithm more efficient, I was able to do my own research. Inspired by the Minimax and A* algorithms, I was able to develop my own optimization which consists in pruning branches of the graph during its construction.
Throughout the elaboration of the plan, a minimum cost is kept. When a branch of the plan is completed, the cost of this branch is compared to the minimum cost in order to update it. When a new node is created from an action, its cumulative cost is compared to the minimum cost in order to stop its propagation if the current branch is too expensive.

This optimization allows a considerable saving of time in the context of Forward Planning.

```cs
static public void BuildGraph<StateT>(..., ref int cheapestCost)
{
    foreach (Action<StateT> action in availableActions)
    {
        if (!action.IsForwardValid(parent.worldState))
            continue;

            // Get the current node running cost
            int runningCost = ...

            // Prune branches which are more expensive than the cheapest one
            if (runningCost > cheapestCost)
                continue;

            Node<StateT> node = CreateNode(parent, newWorldState, action, runningCost);

            if (goalChecker(newWorldState, goal))
            {
                // Save cheapest cost when a branch of the plan is complete
                if (cheapestCost.HasValue)
                    cheapestCost = Math.Min(cheapestCost.Value, node.runningCost);

                leaves.Add(node);
                continue;
            }

                ...

                BuildGraph(..., ref cheapestCost);
            }
        }
```

## Plan Execution
The execution of the plan is based on the operating principle of a Behaviour Tree. An action can have three different states: Success, failure and running. When an action is executed, it returns its state. If the action returns SUCCESS, then the next action is called. If it returns RUNNING, then it will continue to be called. When an action returns FAILURE, the whole plan fails and is then stopped. 

## Backward Implementation
### **Reversed Behavior**
Backward planning works like Forward Planning by using preconditions and reverse effects.

```cs
class HealSelf : GOAP.Action<ENPCStates>
{
    ...    

    public override ENPCStates ApplyEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;

        // Set OFF bit at IS_HURT (1 << 3)
        newWorldstate &= ~ENPCStates.IS_HURT;

        return newWorldstate;
    }

    public override ENPCStates ApplyReversedEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;

        // Set ON bit at IS_HURT (1 << 3)
        newWorldstate |= ENPCStates.IS_HURT;

        return newWorldstate;
    }

    ...
}
```

### **Dynamic costs**
Because of the inverted nature of Backward Planning, I haven't found a way to do dynamic costs using the previous WorldState. This feature is therefore not used. 
A solution to make this work would be to calculate the cost of each branch after making the graph, but this would prevent pruning.

### **Pruning**
Since Backward Planning is already very efficient, pruning does not improve performance, and in some cases even slows it down.

## Performance
In order to have a vision of the performances, four tests were made to see which application is the faster. These tests were performed by populating 
the actions with 10 GarbageActions:

- Forward planning w/o pruning: ``more than 30min ``

- Backward planning w/o pruning: ``0021788ms``

- Forward planning w/ pruning: ``0078277ms``

- Backward planning w/ pruning: ``0020201ms``


## In the future:
In the future, this system will be ported to Unity to be used in more interesting contexts. It will also be rewritten in C++ to be implemented in an house-made engine.

## References:
General references:
- https://gamedevelopment.tutsplus.com/tutorials/goal-oriented-action-planning-for-a-smarter-ai--cms-20793

C# implementation:
- https://medium.com/@vedantchaudhari/goal-oriented-action-planning-34035ed40d0b
- https://learn.unity.com/tutorial/the-goap-planner-1?uv=2019.4

HTN Planner explanations:
- https://www.gameaipro.com/GameAIPro/GameAIPro_Chapter12_Exploring_HTN_Planners_through_Example.pdf

## Versionning
Git Lab for the versioning.

# Author
* **Rémi GINER**