using Godot;
using System;

public partial class PlayerController : Node
{
	public long NetworkId
	{
		get
		{
			return _networkId;
		}
		set
		{
			_networkId = value;
			SetMultiplayerAuthority((int)_networkId);
		}
	}
	long _networkId = -1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
