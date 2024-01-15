using Godot;
using OSSForts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static System.Formats.Asn1.AsnWriter;

public partial class SceneManager : Node
{
    MainMenu? MainMenu;

    Node? GameplayScene;
  
    Dictionary<string, PackedScene?> LoadedScenes = new();

    List<(Node Child, Node Parent)> ScenesToMakeVisibleOnLoadingScreenComplete = new();

    const string MainMenuPath = "res://Core/MainMenu/MainMenu.tscn";
    const string GameplayHostPath = "res://Core/GameplayHost/GameplayHost.tscn";
    const string LandingScenePath = "";

    GameplayHost? GameplayHost;

    private static SceneManager? _instance;

    public override void _Ready()
    {
        _instance = this;
        LoadingScreen.OnFadeOutComplete += MakeScenesVisible;

        Coroutine.NextFrame(() =>
        {
            LoadingScreen.LoadingJob = "Main Menu";
            LoadingScreen.HintText = "This is a game about forts";
            LoadingScreen.ShowLoadingScreen(this);

            //Load the main menu        
            var packedScene = GD.Load<PackedScene>(MainMenuPath);

            MainMenu = packedScene.Instantiate<MainMenu>();
            ScenesToMakeVisibleOnLoadingScreenComplete.Add((MainMenu, GetTree().Root.GetNode("/root")));

            LoadingScreen.LoadingJob = "Initializing Gameplay Host";
            var PackedGameplayHost = GD.Load<PackedScene>(GameplayHostPath);
            GameplayHost = PackedGameplayHost.Instantiate<GameplayHost>();
            GetTree().Root.AddChild(GameplayHost);

            LoadingScreen.ReleaseLoadingScreen(this);
        });

        GD.Print($"SceneManager Ready {MainMenu?.GetPath() ?? "null"}");
    }  

    private static Godot.Collections.Array ProgressArray = new Godot.Collections.Array();

    public static IEnumerator LoadMapCoro(string path, Node Root, bool ShowLoadingScreen = true, bool VisibleOnLoad = true)
    {
        Error err = ResourceLoader.LoadThreadedRequest(path);


        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to load scene {path}, Error: {err.ToString()}");
            yield break;
        }

        if (ShowLoadingScreen)
        {
            LoadingScreen.ShowLoadingScreen(path);

            LoadingScreen.LoadingJob = path;
        }

        ResourceLoader.ThreadLoadStatus Status;       
        do
        {
            Status = ResourceLoader.LoadThreadedGetStatus(path, ProgressArray);
            switch (Status)
            {
                case ResourceLoader.ThreadLoadStatus.Failed:
                    GD.PrintErr($"Failed to load scene {path}");
                    yield break;

                case ResourceLoader.ThreadLoadStatus.InvalidResource:
                    GD.PrintErr($"Invalid resource {path}");
                    yield break;
                case ResourceLoader.ThreadLoadStatus.InProgress:
                    //Update the loading screen if we're showing the loading screen
                    if (ShowLoadingScreen)
                    {
                        LoadingScreen.LoadingJob = $"Loading Scene: {path} : {ProgressArray[0]}%";
                    }
                    break;
            }
           
            yield return Wait.NextFrame();
        } while (Status != ResourceLoader.ThreadLoadStatus.Loaded);
       
        var Scene = ResourceLoader.LoadThreadedGet(path) as PackedScene;

        
        if (Scene != null)
        {
            _instance?.LoadedScenes.Add(path, Scene);

            LoadingScreen.LoadingJob = "Instantiating Scene";
            var Node = Scene.Instantiate();

            if (ShowLoadingScreen)
            {
                if (VisibleOnLoad)
                {
                    _instance?.ScenesToMakeVisibleOnLoadingScreenComplete.Add((Node, Root));
                }
                LoadingScreen.ReleaseLoadingScreen(path);
            }        
            else if(VisibleOnLoad)
            {
                Root.AddChild(Node);
            }            
        }

        yield break;
    }

    public void GoToGameplayScene(string path,bool ShowLoadingScreen = true, bool VisibleOnLoad = true)
    {
        if(GameplayHost == null)
        {
            GD.PrintErr("GameplayHost is null");
            return;
        }

        Coroutine.Start(LoadMapCoro(path, GameplayHost, ShowLoadingScreen, VisibleOnLoad));     
    }

    private void MakeScenesVisible()
    {
        foreach (var n in ScenesToMakeVisibleOnLoadingScreenComplete)
        {
            n.Parent.AddChild(n.Child);
        }
    }


}
