using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSSForts.Globals;

public static class NetworkGame
{

    public static IEnumerator StartLocalNetworkGame(MultiplayerManager MM, string MapPath)
    {       
        //Get the Gameplay Host
        GameplayHost? Host = MM.GetTree().Root.GetNode<GameplayHost>("/root/GameplayHost");
        if (Host == null)
        {
            GD.PrintErr("Failed to get GameplayHost");
            yield break;
        }

        Host.GameState = GameplayState.Transiting;

        // Bring up the loading screen
        object handle = new();
        LoadingScreen.ShowLoadingScreen(handle);
        LoadingScreen.LoadingJob = "Starting Server";
               
        //Wait for the server to start
        if (!MM.HostServer())
        {
            GD.PrintErr("Failed to host server");
            LoadingScreen.ReleaseLoadingScreen(handle);
            yield break;
        }
        MM.AcceptingClients = false;

       

        //Load the map
        yield return SceneManager.LoadMapCoro(MapPath, Host);

        //Accept clients after the map is loaded
        MM.AcceptingClients = true;

        //Bring down the loading screen
        LoadingScreen.ReleaseLoadingScreen(handle);


        //Indicate that all is ready to the GameplayHost
        Host.GameState = GameplayState.WaitingToStart;
    }

}
