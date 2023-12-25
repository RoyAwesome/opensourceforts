using Godot;
using OSSForts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public partial class SceneManager : Node
{
    MainMenu? MainMenu;

    Node? GameplayScene;
    LoadingScreen? LoadingScreen;

    struct LoadingSceneInfo
    {
        public string Path;
        public Node? Node;
        public int LoadPercent;
        public bool VisibleOnLoad;
    }
    Dictionary<string, PackedScene?> LoadedScenes = new();

    List<LoadingSceneInfo> LoadingScenes = new();

    List<Node> ScenesToMakeVisibleOnLoadingScreenComplete = new();

    const string MainMenuPath = "res://Core/MainMenu/MainMenu.tscn";
    const string LandingScenePath = "";


    int ItemsHoldingLoadingScreen = 0;

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
            ScenesToMakeVisibleOnLoadingScreenComplete.Add(MainMenu);
            LoadingScreen.FadeOutForPlay();          

            //GoToScene(LandingScenePath);
        });

        GD.Print($"SceneManager Ready {MainMenu?.GetPath() ?? "null"}");
    }

    private void SceneManager_ProcessFrame()
    {
        if(LoadingScenes.Count > 0)
        {            
            for (int i = 0; i < LoadingScenes.Count; i++)
            {
                if (ProcessSceneLoad(ref CollectionsMarshal.AsSpan(LoadingScenes)[i]))
                {
                    LoadingScenes.RemoveAt(i);
                    i--;
                }
            }
        }       
    }

    Godot.Collections.Array ProgressArray = new Godot.Collections.Array();

    public void GoToScene(string path,bool ShowLoadingScreen = true, bool VisibleOnLoad = true)
    {
        
        Error err = ResourceLoader.LoadThreadedRequest(path);
        if(err != Error.Ok)
        {
            GD.PrintErr($"Failed to load scene {path}, Error: {err.ToString()}");
            return;
        }

        var Status = ResourceLoader.LoadThreadedGetStatus(path, ProgressArray);
        
        switch (Status)
        {
            case ResourceLoader.ThreadLoadStatus.Failed:
                GD.PrintErr($"Failed to load scene {path}");
                return;
           
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                GD.PrintErr($"Invalid resource {path}");
                return;
            case ResourceLoader.ThreadLoadStatus.InProgress:
                LoadingSceneInfo sceneInfo = new();
                sceneInfo.Path = path;
                sceneInfo.LoadPercent = 0;
                sceneInfo.VisibleOnLoad = VisibleOnLoad;
                LoadingScenes.Add(sceneInfo);
                if (ShowLoadingScreen)
                {
                    LoadingScreen?.ShowOverScene();
                    ItemsHoldingLoadingScreen++;
                }
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                PackedScene? Scene = ResourceLoader.LoadThreadedGet(path) as PackedScene;
                LoadedScenes.Add(path, Scene);
                if(Scene != null)
                {
                    Coroutine.NextFrame(() =>
                    {
                        AddSceneToTree(Scene);
                    });
                }              
                break;
            default:
                break;
        }   

     
    }

    private void AddSceneToTree(PackedScene Scene)
    {
        var Node = Scene.Instantiate();
        ScenesToMakeVisibleOnLoadingScreenComplete.Add(Node);

        ItemsHoldingLoadingScreen--;
        if(ItemsHoldingLoadingScreen <=0)
        {
            LoadingScreen?.FadeOutForPlay();
        }
    }

    private void MakeScenesVisible()
    {
        foreach (Node n in ScenesToMakeVisibleOnLoadingScreenComplete)
        {
            GetTree().Root.AddChild(n);
        }
    }

    private bool ProcessSceneLoad(ref LoadingSceneInfo SceneInfo)
    {
        return false;
    }

}
