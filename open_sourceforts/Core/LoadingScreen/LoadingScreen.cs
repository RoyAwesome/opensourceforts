using Godot;
using System;
using System.Collections.Generic;

public partial class LoadingScreen : Control
{
	public static string HintText
	{
		get
		{
			return _hintText;
		}
		set
		{
			_hintText = value;
			if(_instance != null && _instance.HintTextLabel != null)
			{
                _instance.HintTextLabel.Text = value;
            }
		}
	}
	private static string _hintText = "";

	public static string LoadingJob
	{
		get => _loadingJob;
		set
		{
			_loadingJob = value;
			if(_instance != null && _instance.MapNameLabel != null)
			{
                _instance.MapNameLabel.Text = value;
            }
		}
	}
	private static string _loadingJob = "";

	private static Texture2D? _mapPreview;
	public static Texture2D? MapPreview
	{
        get => _mapPreview;
        set
		{
            _mapPreview = value;
            if(_instance != null && _instance.MapPreviewImage != null)
			{
                _instance.MapPreviewImage.Texture = value;
            }
        }
    }

	private static List<object> ObjectsHolding = new();
	public static void ShowLoadingScreen(object handle)
	{
		bool NoLoadingScreen = ObjectsHolding.Count == 0;
		ObjectsHolding.Add(handle);
		if(NoLoadingScreen && _instance != null)
		{
			_instance.ShowOverScene();
		}
	}

	public static void ReleaseLoadingScreen(object handle)
	{
		ObjectsHolding.Remove(handle);
		if(ObjectsHolding.Count == 0 && _instance != null)
		{
			_instance.FadeOutForPlay();
		}
	}

    public static bool PlayingOut
    {
        get;
        private set;
    } = false;

    private static LoadingScreen? _instance;

	Label? HintTextLabel;
	Label? MapNameLabel;
	TextureRect? MapPreviewImage;

	AnimationPlayer? AnimationPlayer;

    public static event Action? OnFadeOutComplete;

	const string SlowFadeOut = "FadeOut";
	const string FastFadeOut = "FadeOutFast";



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		HintTextLabel = GetNode<Label>("%HintTextLabel");
        MapNameLabel = GetNode<Label>("%MapNameLabel");
        MapPreviewImage = GetNode<TextureRect>("%MapPreviewImage");

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationPlayer.AnimationFinished += AnimationPlayer_AnimationFinished;

		_instance = this;
		HintTextLabel.Text = HintText;
		MapNameLabel.Text = LoadingJob;
		MapPreviewImage.Texture = MapPreview;

		GD.Print($"LoadingScreen Ready {AnimationPlayer?.GetPath() ?? "Null"}");
	}


    private void ResetAnim()
	{		
		if(AnimationPlayer != null)
		{
            AnimationPlayer.Play("RESET");
        }
    }

	public void ShowOverScene()
	{
        ResetAnim();
        Visible = true;
        PlayingOut = false;
    }

	public void FadeOutForPlay(bool Fast = true)
	{
		ResetAnim();

        PlayingOut = true;	
		if(AnimationPlayer != null)
		{			
            AnimationPlayer.Play(Fast ? FastFadeOut : SlowFadeOut);
        }
		else
		{
			AnimationPlayer_AnimationFinished(Fast ? FastFadeOut : SlowFadeOut);
		}

	}

    private void AnimationPlayer_AnimationFinished(StringName animName)
    {
		if(animName == "RESET")
		{
			return;
		}

        PlayingOut = false;
		Visible = false;

		OnFadeOutComplete?.Invoke();
    }
}
