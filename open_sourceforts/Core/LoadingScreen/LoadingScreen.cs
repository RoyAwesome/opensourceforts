using Godot;
using System;

public partial class LoadingScreen : Control
{
	Label? HintTextLabel;
	Label? MapNameLabel;
	TextureRect? MapPreviewImage;

	AnimationPlayer? AnimationPlayer;

    public event Action? OnFadeOutComplete;

	const string SlowFadeOut = "FadeOut";
	const string FastFadeOut = "FadeOutFast";

	public string HintText
	{
		get => HintTextLabel?.Text ?? "null";
		set
		{
			if (HintTextLabel != null)
			{
                HintTextLabel.Text = value;
            }
		}
	}

	public string MapName
	{
		get => MapNameLabel?.Text ?? "null";
		set 
		{
			if (MapNameLabel != null) 
			{
				MapNameLabel.Text = value; 
			}
		}
	}

	public Texture2D? MapPreview
	{
        get => MapPreviewImage?.Texture;
        set
		{
            if (MapPreviewImage != null)
			{
                MapPreviewImage.Texture = value;
            }
        }
    }

	public bool PlayingOut
	{
		get;
		private set;
	} = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		HintTextLabel = GetNode<Label>("%HintTextLabel");
        MapNameLabel = GetNode<Label>("%MapNameLabel");
        MapPreviewImage = GetNode<TextureRect>("%MapPreviewImage");

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationPlayer.AnimationFinished += AnimationPlayer_AnimationFinished;

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
