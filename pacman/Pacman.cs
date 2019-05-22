using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OGPLib;

namespace pacman {

    [Serializable]
	public class Direction
	{
		private int x;
		private int y;

		public Direction(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

        public static Direction UP { get => new Direction(0, -1); }
        public static Direction DOWN { get => new Direction(0, 1); }
        public static Direction LEFT { get => new Direction(-1, 0); }
        public static Direction RIGHT { get => new Direction(1, 0); }
        public static Direction RIGHTUP { get => RIGHT + UP; }
        public static Direction RIGHTDOWN { get => RIGHT + DOWN; }
        public static Direction LEFTUP { get => LEFT + UP; }
        public static Direction LEFTDOWN { get => LEFT + DOWN; }
        public static Direction NONE { get => new Direction(0, 0); }


        public static Direction operator+ (Direction d1, Direction d2) {
            return new Direction(d1.X + d2.X, d1.Y + d2.Y);
        }

        public static bool operator== (Direction d1, Direction d2) {

            return d1.X == d2.X && d1.Y == d2.Y;
        }

        public static bool operator!= (Direction d1, Direction d2) {
            return !(d1 == d2);
        }

        public Direction ReflectX() {
            return new Direction(-(this.x), this.y);
        }

        public Direction ReflectY() {
            return new Direction(this.x, -(this.y));
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            var direction = obj as Direction;
            return x == direction.x &&
                   y == direction.y;
        }

        public override int GetHashCode() {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public int X { get => x; set => x = value; }
		public int Y { get => y; set => y = value; }
	}

    public class BoardSize
	{
		public int xMax;
		public int yMax;
		public int xMin;
		public int yMin;

		public BoardSize(int xMax, int yMax, int xMin, int yMin)
		{
			this.xMax = xMax;
			this.yMax = yMax;
			this.xMin = xMin;
			this.yMin = yMin;
		}
	}
}
