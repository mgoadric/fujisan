using System;
namespace Fujisan
{
    public class Tile
    {

        public int[,] values;

        public Tile(int first, int second)
        {
            values = new int[2,2];
            values[0,0] = first;
            values[1,1] = first;
            values[0,1] = second;
            values[1,0] = second;
         }

        public void Rotate() {
            int temp = values[0,0];
            int temp2 = values[0,1];
            values[0,0] = temp2;
            values[1,1] = temp2;
            values[0,1] = temp;
            values[1,0] = temp;
        }

        public override string ToString() {
            return "" + values[0,0] + values[0,1] + "\n" +
                values[1,0] + values[1,1];
        }
    }
}
