using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class GridCell
    {
        // Members
        private int row;
        private int col;

        // Constructor
        public GridCell(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        // Copy constructor
        public GridCell(GridCell b)
        {
            this.row = b.row;
            this.col = b.col;
        }

        // Copy constructor for list
        static public List<GridCell> cloneList(List<GridCell> c){
             return c.ConvertAll(cell => new GridCell(cell));
        }

        // String representation
        public override string ToString()
        {
            string s = "GridCell {row: " + row + ", col: " + col + "}";
            return s;
        }

        public int getRow() { return row; }
        public int getCol() { return col; }
    }
}
