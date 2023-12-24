using Godot;
using System;

public partial class LoadingScreen : Control
{
	Label HintTextLabel;
	Label MapNameLabel;
	TextureRect MapPreviewImage;

	AnimationPlayer AnimationPlayer;

	public string HintText
	{
		get => HintTextLabel.Text;
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

	public Texture2D MapPreview
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
	}

	private void ResetAnim()
	{
        AnimationPlayer.AnimationFinished -= AnimationPlayer_AnimationFinished;
        AnimationPlayer.Play("RESET");
    }

	public void ShowOverScene()
	{
        ResetAnim();
        Visible = true;
        PlayingOut = false;
    }

	public void RemoveFromScene()
	{
		ResetAnim();

        PlayingOut = true;	
        AnimationPlayer.AnimationFinished += AnimationPlayer_AnimationFinished;
        AnimationPlayer.Play("FadeOut");
	}

    private void AnimationPlayer_AnimationFinished(StringName animName)
    {
        PlayingOut = false;
		Visible = false;
    }
}
