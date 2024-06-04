using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class level : Node {
    const string SCORE_LABEL_PATH = "Hud/ScoreLabel";
    const string MOVE_TIMER_PATH = "MoveTimer";
    const string FOOD_PATH = "Food";
    const string GAME_OVER_MENU_PATH = "GameOverMenu";


    [Export]
    public PackedScene SnakeScene;

    private int Score;
    private Label ScoreLabel;
    private bool GameStarted = false;
    private int Cell = 20; // grid cell in each roll/column
    private int CellSize = 50;
    private List<Vector2> OldData = new List<Vector2>();
    private List<Vector2> SnakeData = new List<Vector2>();
    private List<Node> Snake = new List<Node>();
    private Vector2 StartPosition = new Vector2(9, 9);
    private Vector2 Up = new Vector2(0, -1);
    private Vector2 Down = new Vector2(0, 1);
    private Vector2 Left = new Vector2(-1, 0);
    private Vector2 Right = new Vector2(1, 0);
    private Vector2 MoveDirection;
    private bool CanMove;
    private Timer MoveTimer;
    private Vector2 FoodPosition;
    private bool RegenFood = true;
    private Panel Food;
    private CanvasLayer GameOverMenu;

    private void AddSegment(Vector2 pos) {
        SnakeData.Add(pos);
        Node SnakeSegment = SnakeScene.Instantiate();
        SnakeSegment.Set("position", (pos * CellSize) + new Vector2(0, CellSize));
        AddChild(SnakeSegment);
        Snake.Add(SnakeSegment);
    }

    private void GenerateSnake() {
        OldData.Clear();
        SnakeData.Clear();
        Snake.Clear();
        for (int i = 0; i < 3; i++) {
            AddSegment(StartPosition + new Vector2(0, i));
        }
    }

    private void NewGame() {
        GetTree().Paused = false;
        // clear old segments
        GetTree().CallGroup("Segments", "queue_free"); // group of the Segments in godot editor
        GameOverMenu.Hide();
        Score = 0;
        ScoreLabel.Text = "Score: " + Score.ToString();
        MoveDirection = Up;
        CanMove = true;
        GenerateSnake();
        MoveFood();
    }

    private void MoveSnake() {
        if (CanMove) {
            if (Input.IsActionJustPressed("move_down") && MoveDirection != Up) {
                MoveDirection = Down;
                CanMove = false;
                if (!GameStarted) {
                    StartGame();
                }
            }
            if (Input.IsActionJustPressed("move_up") && MoveDirection != Down) {
                MoveDirection = Up;
                CanMove = false;
                if (!GameStarted) {
                    StartGame();
                }
            }
            if (Input.IsActionJustPressed("move_left") && MoveDirection != Right) {
                MoveDirection = Left;
                CanMove = false;
                if (!GameStarted) {
                    StartGame();
                }
            }
            if (Input.IsActionJustPressed("move_right") && MoveDirection != Left) {
                MoveDirection = Right;
                CanMove = false;
                if (!GameStarted) {
                    StartGame();
                }
            }
        }
    }

    private void StartGame() {
        GD.Print("Start");
        GameStarted = true;
        MoveTimer.Start();
    }

    private void OnMoveTimerTimeOut() {
        CanMove = true;
        // Without new c# references the original list, like a pointer
        // to clone we use new
        OldData = new List<Vector2>(SnakeData);
        SnakeData[0] += MoveDirection;

        for (int i = 0; i < SnakeData.Count; i++) {
            if (i > 0 && SnakeData[0] != OldData[0]) {
                // GD.Print("i: " + i);
                SnakeData[i] = OldData[i - 1];
            }
            Snake[i].Set("position", (SnakeData[i] * CellSize) + new Vector2(0, CellSize));
        }
        CheckOutOfBounds();
        CheckSelfEaten();
        CheckFoodEaten();
    }

    private void CheckOutOfBounds() {
        if (SnakeData[0].X < 0 || SnakeData[0].X > (Cell - 1)
            || SnakeData[0].Y < 0 || SnakeData[0].Y > (Cell - 1)) {
            EndGame();
        }
    }

    private void CheckSelfEaten() {
        for (int i = 1; i < SnakeData.Count; i++) {
            if (SnakeData[0] == SnakeData[i]) {
                EndGame();
            }
        }
    }

    private void CheckFoodEaten() {
        if (SnakeData[0] == FoodPosition) {
            Score += 1;
            ScoreLabel.Text = "Score: " + Score.ToString();
            AddSegment(OldData.Last());
            MoveFood();
        }
    }

    private void EndGame() {
        GameOverMenu.Show();
        MoveTimer.Stop();
        GameStarted = false;
        GetTree().Paused = true;
    }

    private void MoveFood() {
        while (RegenFood) {
            RegenFood = false;
            var rng = new RandomNumberGenerator();
            FoodPosition = new Vector2(rng.RandiRange(0, Cell - 1), rng.RandiRange(0, Cell - 1));
            foreach (var i in SnakeData) {
                if (FoodPosition == i) {
                    RegenFood = true;
                }
            }
        }
        Food.Set("position", (FoodPosition * CellSize) + new Vector2(0, CellSize));
        RegenFood = true;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        ScoreLabel = GetNode<Label>(SCORE_LABEL_PATH);
        MoveTimer = GetNode<Timer>(MOVE_TIMER_PATH);
        Food = GetNode<Panel>(FOOD_PATH);
        GameOverMenu = GetNode<CanvasLayer>(GAME_OVER_MENU_PATH);
        NewGame();
        MoveTimer.Timeout += OnMoveTimerTimeOut;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        MoveSnake();
    }

    // signal conected in godot editor
    private void _on_game_over_menu_restart() {
        NewGame();
    }
}


