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
    LoadingScreen? LoadingScreen;

  
    Dictionary<string, PackedScene?> LoadedScenes = new();

    List<(Node Child, Node Parent)> ScenesToMakeVisibleOnLoadingScreenComplete = new();

    const string MainMenuPath = "res://Core/MainMenu/MainMenu.tscn";
    const string GameplayHostPath = "res://Core/GameplayHost/GameplayHost.tscn";
    const string LandingScenePath = "";


    int ItemsHoldingLoadingScreen = 0;

    GameplayHost? GameplayHost;

    public override void _Ready()
    {
        LoadingScreen = GetTree().Root.GetNode<LoadingScreen>("LoadingScreen");
        LoadingScreen.OnFadeOutComplete += MakeScenesVisible;

        Coroutine.NextFrame(() =>
        {
            LoadingScreen.MapName = "Main Menu";
            LoadingScreen.HintText = "This is a game about forts";
            LoadingScreen.ShowOverScene();

            //Load the main menu        
            var packedScene = GD.Load<PackedScene>(MainMenuPath);

            MainMenu = packedScene.Instantiate<MainMenu>();
            ScenesToMakeVisibleOnLoadingScreenComplete.Add((MainMenu, GetTree().Root.GetNode("/root")));
            LoadingScreen.FadeOutForPlay();

            var PackedGameplayHost = GD.Load<PackedScene>(GameplayHostPath);
            GameplayHost = PackedGameplayHost.Instantiate<GameplayHost>();
            GetTree().Root.AddChild(GameplayHost);
        });

        GD.Print($"SceneManager Ready {MainMenu?.GetPath() ?? "null"}");
    }  

    Godot.Collections.Array ProgressArray = new Godot.Collections.Array();

    private IEnumerator LoadMapCoro(string path, Node Root, bool ShowLoadingScreen = true, bool VisibleOnLoad = true)
    {
        Error err = ResourceLoader.LoadThreadedRequest(path);


        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to load scene {path}, Error: {err.ToString()}");
            yield break;
        }

        if (ShowLoadingScreen && LoadingScreen != null)
        {
            LoadingScreen.ShowOverScene();
            ItemsHoldingLoadingScreen++;

            LoadingScreen.MapName = path;
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
                        //LoadingScreen?.Progr((int)ProgressArray[0]);
                    }
                    break;
            }

            yield return Wait.NextFrame();
        } while (Status != ResourceLoader.ThreadLoadStatus.Loaded);
       
        var Scene = ResourceLoader.LoadThreadedGet(path) as PackedScene;

        
        if (Scene != null)
        {
            LoadedScenes.Add(path, Scene);

            if(ShowLoadingScreen)
            {
                ItemsHoldingLoadingScreen--;
                if (ItemsHoldingLoadingScreen <= 0)
                {
                    LoadingScreen?.FadeOutForPlay();
                }
            }

            var Node = Scene.Instantiate();

            Root.AddChild(Node);

            //if (VisibleOnLoad)
            //{
            //    ScenesToMakeVisibleOnLoadingScreenComplete.Add((Node, Root));
            //}
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
