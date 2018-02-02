using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEDGame
{
    class MapImageConvert
    {
        public static GridMap LoadImage(string file, MainGame game)
        {
            GridMap grid;

            using(Bitmap bmp = new Bitmap(file))
            {
                int w = Math.Min(bmp.Width, MainGame.MaxMapWidth);
                int h = Math.Min(bmp.Height, MainGame.MaxMapHeight);

                bool[] solveCells = new bool[w * h];
                Microsoft.Xna.Framework.Color[] revealColors = new Microsoft.Xna.Framework.Color[w * h];

                grid = new GridMap(w, h, MainGame.TileSize, game);

                List<int> nums = new List<int>();

                for (int row = 0; row < h; row++)
                {
                    nums.Clear();

                    int count = 0;
                    for(int x = 0; x < w; x++)
                    {
                        Color c = bmp.GetPixel(x, row);

                        if (CheckColor(c))
                        {
                            count++;

                            revealColors[x + row * w] = new Microsoft.Xna.Framework.Color(c.R, c.G, c.B);
                            solveCells[x + row * w] = true;
                        }
                        else
                        {
                            if (count > 0)
                            {
                                nums.Add(count);
                            }
                            count = 0;
                        }
                    }

                    if (count > 0)
                        nums.Add(count);

                    nums.Reverse();
                    grid.CopyRowNumbers(nums, row);
                }

                for (int col = 0; col < w; col++)
                {
                    nums.Clear();

                    int count = 0;
                    for (int y = 0; y < h; y++)
                    {
                        Color c = bmp.GetPixel(col, y);

                        if (CheckColor(c))
                        {
                            count++;

                            revealColors[col + y * w] = new Microsoft.Xna.Framework.Color(c.R, c.G, c.B);
                            solveCells[col + y * w] = true;
                        }
                        else
                        {
                            if (count > 0)
                            {
                                nums.Add(count);
                            }
                            count = 0;
                        }
                    }

                    if (count > 0)
                        nums.Add(count);

                    nums.Reverse();
                    grid.CopyColumnNumbers(nums, col);
                }

                grid.SetSolveCells(solveCells, revealColors);
            }

            return grid;
        }

        static bool CheckColor(Color c)
        {
            return (c.A > 150);
        }
    }
}
