using Godot;
using System;
using System.Collections.Generic;

public enum GameplayState
{
	None,
	Transiting,
	WaitingToStart,
	Playing,
	WaitingToEnd,
}

public partial class GameplayHost : Node
{
	public Node3D? DynamicLayer
	{
		get;
		private set;
	}

	public MultiplayerSpawner? Spawner
	{
		get;
		private set;
	}
	const string PlayerCharacterPath = "res://Gameplay/Character/PlayerCharacter.tscn";
	PackedScene? PlayerCharacterScene;
	Dictionary<PlayerController, PlayerCharacter> PlayerCharacters = new();

	MultiplayerManager? MPManager;


	public GameplayState GameState
	{
        get => _currentState;
        set
		{
			GameplayState oldState = _currentState;
			_currentState = value;
			TransitionGameplayState(oldState);
		}
    }
	GameplayState _currentState = GameplayState.None;

	public event Action<GameplayState>? LeavingGameplayState;
	public event Action<GameplayState>? EnteringGameplayState;

	public override void _Ready()
	{
		PlayerCharacterScene = GD.Load<PackedScene>(PlayerCharacterPath);

		DynamicLayer = GetNode<Node3D>("%DynamicLayer");
		Spawner = GetNode<MultiplayerSpawner>("%MultiplayerSpawner");
		MPManager = MultiplayerManager.Get(this);

		EnteringGameplayState += OnEnterGameplayState;

		TransitionGameplayState(GameplayState.None);
	}


	public PlayerCharacter? CreatePlayerCharacterFor(PlayerController controller)
	{
		if(PlayerCharacters.ContainsKey(controller))
		{
			PlayerCharacters[controller].QueueFree();	
		}

		if(PlayerCharacterScene != null)
		{
            var playerCharacter = PlayerCharacterScene.Instantiate<PlayerCharacter>();            
			PlayerCharacters[controller] = playerCharacter;
            DynamicLayer?.AddChild(playerCharacter);
            return playerCharacter;
        }

		return null;
	}

	public void TransitionGameplayState(GameplayState PreviousState)
	{
		LeavingGameplayState?.Invoke(PreviousState);
		EnteringGameplayState?.Invoke(GameState);
	}

	private void OnEnterGameplayState(GameplayState State)
	{
		if(State == GameplayState.None)
		{
			//Destroy all player characters
			foreach(var KV in PlayerCharacters)
			{
				KV.Value.QueueFree();
			}
		}
		else if(State == GameplayState.WaitingToStart)
		{
			//HACKHACK: A ruleset should govern this (should we even have a waiting to start game state?)
			//but for now, just start the game;
			GameState = GameplayState.Playing;
		}
		else if(State == GameplayState.Playing)
		{
			//HACKHACK A ruleset should govern this, but for now, just spawn a player character for each player
			if(MPManager != null)
			{
                foreach (PlayerController controller in MPManager.AllPlayers())
                {
                    CreatePlayerCharacterFor(controller);
                }
            }
		}
	}
}
