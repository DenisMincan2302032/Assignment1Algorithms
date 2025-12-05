namespace Lab_Week_1_Friday
{
    public struct Coordinate
    {
        public int Row;
        public int Col;

        public Coordinate(int row, int col)
        {
            Row = row;
            Col = col;
        }

     
        public static bool operator ==(Coordinate a, Coordinate b) => a.Row == b.Row && a.Col == b.Col;
        public static bool operator !=(Coordinate a, Coordinate b) => !(a == b);

      
        public override bool Equals(object obj) => obj is Coordinate other && this == other;
        public override int GetHashCode() => (Row, Col).GetHashCode();
    }
}