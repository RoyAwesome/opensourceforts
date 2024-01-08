using Godot;
using System;
using System.Collections.Generic;

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

	const string PlayerCharacterPath = "res://Gameplay/Player/PlayerCharacter.tscn";

	PackedScene? PlayerCharacterScene;

	Dictionary<PlayerController, PlayerCharacter> PlayerCharacters = new();

	public override void _Ready()
	{
		PlayerCharacterScene = GD.Load<PackedScene>(PlayerCharacterPath);

		DynamicLayer = GetNode<Node3D>("%DynamicLayer");
		Spawner = GetNode<MultiplayerSpawner>("%MultiplayerSpawner");
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

}
