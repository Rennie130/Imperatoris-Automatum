using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachine<T, TEnum> where TEnum : Enum
    {
        private readonly TEnum _baseState;
        private readonly T _source;
        public State CurrentState { get; private set; }
        private Dictionary<TEnum, State> _availableStates = new();

        public StateMachine(T source, TEnum startingState)
        {
            _source = source;
            _baseState = startingState;
            
            //init all values with a default state to prevent the SM from exploding if you don't register a state
            foreach (var enumValue in  Enum.GetValues(typeof(TEnum)).Cast<TEnum>()) 
            {
                ForState(enumValue);
            }
        }

        public TEnum StateID => CurrentState.ID;

        public State ForState(TEnum state)
        {
            if (!_availableStates.ContainsKey(state))
                _availableStates.Add(state, new State{ID = state});

            if(CurrentState == null && state.Equals(_baseState))
                SetState(_baseState, false);
            
            return _availableStates[state];
        }

        public void Tick()
        {
            UpdateState();
            CurrentState?.DoTick(_source);
        }

        private void UpdateState()
        {
            foreach (var transition in CurrentState.Transitions)
            {
                if(transition.Condition.Invoke(_source))
                {
                    SetState(transition.StateId, true);
                    return;
                }
            }
        }

        public void SetState(TEnum stateID, bool shouldReapplyState)
        {
            //if the state matches and we are reconciling, just ignore the request
            var reapply = shouldReapplyState || CurrentState != _availableStates[stateID];
        
            if(reapply)
                CurrentState?.HandleExit?.Invoke(_source);
            CurrentState = _availableStates[stateID];
            if(reapply)
                CurrentState?.HandleEnter?.Invoke(_source);
        }
        
      
        public class State
        {
            internal TEnum ID;
            internal Action<T> HandleTick;
            internal Action<T> HandleEnter;
            internal Action<T> HandleExit;

            internal readonly List<StateTransition> Transitions = new();

            public void DoTick(T source) => HandleTick?.Invoke(source);
            
            public State OnTick(Action<T> onTickAction)
            {
                HandleTick = onTickAction;
                return this;
            }
            
            public State OnEnter(Action<T> onEnterAction)
            {
                HandleEnter = onEnterAction;
                return this;
            }
            
            public State OnExit(Action<T> onEnterAction)
            {
                HandleExit = onEnterAction;
                return this;
            }

            public State WithTransition(TEnum state, Predicate<T> condition)
            {
                Transitions.Add(new StateTransition(){ Condition = condition, StateId = state});
                return this;
            }
            
        }

        public struct StateTransition
        {
            public Predicate<T> Condition;
            public TEnum StateId;
        }
    }

