using Godot;
using System;

public partial class game_over_menu : CanvasLayer {
    const string RESTART_BUTTON_PATH = "RestartButton";

    [Signal]
    public delegate void RestartEventHandler(); // it needs to terminate in EventHandler, the name is just restart

    public Button RestartButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        RestartButton = GetNode<Button>(RESTART_BUTTON_PATH);
        RestartButton.Pressed += OnRestartButtonPressed;
    }

    public void OnRestartButtonPressed() {
        GD.Print("Restart signal emited");
        EmitSignal(SignalName.Restart);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }
}
