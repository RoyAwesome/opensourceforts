using Godot;
using OSSForts;
using System;
using System.Collections.Generic;

public partial class CoroutineManager : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetTree().ProcessFrame += SceneManager_ProcessFrame;
    }

    private void SceneManager_ProcessFrame()
    {
       Coroutine.Process();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
