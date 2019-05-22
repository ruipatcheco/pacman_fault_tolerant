using OGPLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pacman {


    static class GuidGenerator
    {
        private static int guid = 0;
        public static int getGuid()
        {
            return ++guid;
        }
    }
    /// <summary>
    /// Represents an abstract game entity. Any class that extends this class must implement a read-only
    /// Picture property, which must return a Bitmap object corresponding to the picture that will be rendered
    /// in the game screen.
    /// </summary>
    [Serializable]
    public abstract class GameEntity {
        private Coordinate2D coordinates;
        private string guid;

        [NonSerialized]
        private PictureBox model;

        public GameEntity() : this(new Coordinate2D(0, 0)) {
        }

        public GameEntity(Coordinate2D coords) {
            Coordinates = coords;
            guid = GuidGenerator.getGuid().ToString();
        }

        public Coordinate2D Coordinates { get => coordinates; set => coordinates = value; }

        public virtual void draw(Form form) {
            Utils.doInGUI(form, delegate () {
                form.Controls.Add(this.Model);
            });
        }

        protected abstract Bitmap Picture {
            get;
        }

        protected abstract Size Size {
            get;
        }

        public PictureBox Model {
            get {
                if (this.model == null) {
                    this.model = new PictureBox();
                    this.model.BackColor = this.BackgroundColor;
                    this.model.Location = new Point(this.coordinates.X, this.coordinates.Y);
                    this.model.Size = this.Size;
                    this.model.SizeMode = PictureBoxSizeMode.StretchImage;
                    this.model.Name = this.Guid;
                    if (this.Picture == null)
                        this.Model.Image = null;
                    else {
                        lock (this) {
                            this.Model.Image = new Bitmap(this.Picture);
                        }
                    }
                }
                return this.model;
            }
            set => model = value;
        }

        public virtual Color BackgroundColor { get => Color.Transparent; }
        public string Guid { get => guid; set => guid = value; }

        public int Left(BoardSize container) {
            return this.Model.Bounds.Left - container.xMin;
        }

        public int Right(BoardSize container) {
            return this.Model.Bounds.Right - container.xMin;
        }

        public int Top(BoardSize container) {
            return this.Model.Bounds.Top;
        }

        public int Bottom(BoardSize container) {
            return this.Model.Bounds.Bottom - container.yMin;
        }


        public bool Move(Direction direction, int speed, BoardSize board) {
            if (direction == Direction.LEFT && this.Left(board) > board.xMin ||
                direction == Direction.RIGHT && this.Right(board) < board.xMax ||
                direction == Direction.UP && this.Top(board) > board.yMin ||
                direction == Direction.DOWN && this.Bottom(board) < board.yMax ||

                direction == Direction.LEFTUP && (this.Left(board) > board.xMin && this.Top(board) > board.yMin) ||
                direction == Direction.LEFTDOWN && (this.Left(board) > board.xMin && this.Bottom(board) < board.yMax) ||
                direction == Direction.RIGHTUP && (this.Right(board) < board.xMax && this.Top(board) > board.yMin) ||
                direction == Direction.RIGHTDOWN && (this.Right(board) < board.xMax && this.Bottom(board) < board.yMax)) {
                this.Coordinates.X += direction.X * speed;
                this.Coordinates.Y += direction.Y * speed;
                this.Model.Location = new Point(this.Coordinates.X, this.Coordinates.Y);
                return true;
            }
            return false;
        }

    }

    [Serializable]
    public class Player : GameEntity {

        private OGPPlayer player1;
        private Direction direction;
        private int score;
        private bool isDead = false;



        [NonSerialized]
        private static readonly Direction DEFAULT_DIRECTION = Direction.LEFT;

        [NonSerialized]
        private static readonly Size size = new Size(25, 25);

        [NonSerialized]
        private static readonly Dictionary<Direction, Bitmap> directions = new Dictionary<Direction, Bitmap>() {
            {Direction.LEFT , global::pacman.Properties.Resources.Left},
            {Direction.RIGHT, global::pacman.Properties.Resources.Right },
            {Direction.UP, global::pacman.Properties.Resources.Up },
            {Direction.DOWN, global::pacman.Properties.Resources.down }
        };

        public Player() : base() {
            this.PlayerDirection = pacman.Player.DEFAULT_DIRECTION;
        }

        public Player(OGPPlayer player, Coordinate2D coords, Direction direction, int score) : base(coords) {
            this.PlayerDirection = direction;
            this.Player1 = player;
            this.Score = score;
        }

        public string Username { get => Player1.Username; set => Player1.Username = value; }
        public string ChatUri { get => Player1.ChatUri; set => Player1.ChatUri = value; }
        public Direction PlayerDirection { get => direction; set => direction = value; }

        protected override Bitmap Picture => directions[this.PlayerDirection];
        protected override Size Size => pacman.Player.size;

        public int Score { get => score; set => score = value; }
        public OGPPlayer Player1 { get => player1; set => player1 = value; }
        public bool IsDead { get => isDead; set => isDead = value; }


    }

    [Serializable]
    public class Monster : GameEntity {
        public enum MonsterColor {
            Yellow, Pink, Red
        }
        private MonsterColor color;

        [NonSerialized]
        private Direction direction;

        [NonSerialized]
        private static readonly Dictionary<MonsterColor, Bitmap> images = new Dictionary<MonsterColor, Bitmap>() {
            {MonsterColor.Yellow, global::pacman.Properties.Resources.yellow_guy },
            {MonsterColor.Pink, global::pacman.Properties.Resources.pink_guy},
            {MonsterColor.Red, global::pacman.Properties.Resources.red_guy }
        };

        [NonSerialized]
        private static readonly MonsterColor DEFAULT_COLOR = MonsterColor.Red;

        [NonSerialized]
        private static readonly Size size = new Size(30, 30);

        public Monster() : base() {
            this.Color = Monster.DEFAULT_COLOR;
        }

        public Monster(Coordinate2D coords, MonsterColor color, Direction dir) : base(coords) {
            this.Direction = dir;
            this.color = color;
        }

        public MonsterColor Color { get => color; set => color = value; }

        protected override Bitmap Picture => images[this.color];

        protected override Size Size => Monster.size;

        public Direction Direction { get => direction; set => direction = value; }

        internal void InvertDirectionX() {
            this.Direction.X = -this.Direction.X;
        }

        internal void InvertDirectionY() {
            this.Direction.Y = -this.Direction.Y;
        }


    }

    [Serializable]
    public class Coin : GameEntity {
        public Coin(Coordinate2D coords) : base(coords) { }

        [NonSerialized]
        private static readonly Size size = new Size(15, 15);

        protected override Bitmap Picture => global::pacman.Properties.Resources.bcoin;

        protected override Size Size => size;
    }

    [Serializable]
    public class Wall : GameEntity {
        private int width;
        private int height;

        public Wall(Coordinate2D coords, int width, int height) : base(coords) {
            this.width = width;
            this.height = height;
        }

        public override Color BackgroundColor { get => Color.MidnightBlue; }

        protected override Bitmap Picture => null;

        protected override Size Size => new Size(this.width, this.height);

    }
}
