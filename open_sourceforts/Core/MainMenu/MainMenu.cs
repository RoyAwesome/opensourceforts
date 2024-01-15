using Godot;
using OSSForts;
using OSSForts.Globals;
using System;

public partial class MainMenu : Control
{
    Button? DisconnectFromServerButton;
	Button? JoinServerButton;
	Button? HostServerButton;
	Button? OptionsButton;
	Button? ExitButton;

    Panel? JoinGamePanel;
    Panel? HostGamePanel;
    Panel? OptionsPanel;

    OptionButton? MapList;

    TextEdit? ServerAddressEntry;

    Button? StartServerButton;
    Button? DoConnectToServerButton;

    MultiplayerManager? MultiplayerManager;

    SceneManager? SceneManager;
       


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        MultiplayerManager = GetNode<MultiplayerManager>("/root/MultiplayerManager");
        SceneManager = GetNode<SceneManager>("/root/SceneManager");

        DisconnectFromServerButton = GetNode<Button>("%DisconnectFromServerButton");
		JoinServerButton = GetNode<Button>("%JoinServerButton");
        HostServerButton = GetNode<Button>("%HostServerButton");
        OptionsButton = GetNode<Button>("%OptionsButton");
        ExitButton = GetNode<Button>("%ExitButton");

        JoinGamePanel = GetNode<Panel>("%JoinServerPanel");
        HostGamePanel = GetNode<Panel>("%HostServerPanel");
        OptionsPanel = GetNode<Panel>("%OptionsPanel");

        MapList = GetNode<OptionButton>("%MapOptionsList");

        ServerAddressEntry = GetNode<TextEdit>("%IPHostTextInput");

        StartServerButton = GetNode<Button>("%StartServerButton");
        DoConnectToServerButton = GetNode<Button>("%DoConnectToServerButton");

		JoinServerButton.Toggled += JoinServerButton_Toggled;
        HostServerButton.Toggled += HostServerButton_Toggled;
        OptionsButton.Toggled += OptionsButton_Toggled;

        ExitButton.Pressed += ExitButton_Pressed;

        StartServerButton.Pressed += StartServerButton_Pressed;
        DoConnectToServerButton.Pressed += DoConnectToServerButton_Pressed;

        Span<Panel> Panels = [JoinGamePanel, HostGamePanel, OptionsPanel];

        foreach(Panel panel in Panels)
        {
            panel.Visible = false;
        }
	}

    private void DoConnectToServerButton_Pressed()
    {
        GD.Print($"Connecting to Server {ServerAddressEntry?.Text ?? "null"}");
        if(!MultiplayerManager?.TryConnectToServer(ServerAddressEntry?.Text ?? "") ?? true)
        {
            GD.PrintErr("Unable to connect to server");
            return;
        }
        
        
    }

    private void StartServerButton_Pressed()
    {
        GD.Print($"Starting Server: {MapList?.GetItemText(MapList.Selected)}");

        if(MultiplayerManager == null || MapList == null || SceneManager == null)
        {
            GD.PrintErr("Unable to start server, null things");
            return;
        }

        Visible = false;
        Coroutine.Start(NetworkGame.StartLocalNetworkGame(MultiplayerManager, MapList.GetItemText(MapList.Selected)));

        //if (MultiplayerManager.HostServer())
        //{
        //    Visible = false;
        //    SceneManager.GoToGameplayScene(MapList.GetItemText(MapList.Selected));
        //}
    }

    private void ExitButton_Pressed()
    {
        GetTree().Quit();
    }

    private void OptionsButton_Toggled(bool toggledOn)
    {
        if(OptionsPanel == null)
        {
            return;
        }
        OptionsPanel.Visible = toggledOn;
    }

    private void HostServerButton_Toggled(bool toggledOn)
    {
        if(HostGamePanel == null)
        {
            return;
        }

        HostGamePanel.Visible = toggledOn;

        if(toggledOn && MapList != null)
        {
            MapList.Clear();
            var Maps = ProjectSettings.GetSettingWithOverride(GameSettings.MapListPath).AsStringArray();
            foreach(string map in Maps)
            {
                MapList.AddItem(map);
            }
        }
    }

    private void JoinServerButton_Toggled(bool toggledOn)
    {
        if (JoinGamePanel == null)
        {
            return;
        }

       JoinGamePanel.Visible = toggledOn;
    }

}
