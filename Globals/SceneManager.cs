using Godot;
using System;

public partial class SceneManager : Node
{
    MainMenu MainMenu;

    Node GameplayScene;
    Node LoadingScreen;

    public override void _Ready()
    {
        MainMenu = GetTree().Root.GetNode<MainMenu>("MainMenu");

        GD.Print($"SceneManager Ready {MainMenu?.GetPath() ?? "null"}");
    }

    public void GoToScene(string path)
    {

    }
}
