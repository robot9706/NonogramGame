using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SEDGame
{
    class GridMap
    {
        //Static
        static Color GridColor = Color.DarkSlateGray;
        static Color EmptyCellBg = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);
        static Color SelectColor = Color.Lerp(Color.Blue, Color.Transparent, 0.75f);
        static Color EmptyNumBg = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
        public static Color BasicCellColor = new Color(50, 50, 60);
        static Color MaybeColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);
        static Color OkGreenColor = Color.Lerp(Color.Green, Color.Transparent, 0.5f);

        static int RowSize = 13;
        static int ColumnSize = 8;

        static Random _rnd = new Random();

        //Public
        public Point? SelectedCell;

        //Private
        private RenderTarget2D _view;

        private bool[] _solveCells;
        private Color[] _solveColor;

        private bool[] _cells;
        private Color[] _colors;

        private Point _revealSize;
        private bool[] _bigReveal;
        private bool[] _bigCellOK;

        private SpriteFont _font;

        private Point _size;
        private Point _pixelSize;
        private int _tileSize;

        private Texture2D _white;
        private Texture2D _button;
        private Texture2D _maybeTexture;

        private Point _gridLocation;

        private int[] _rowNumbers;
        private int[] _columnNumbers;

        private float _rowCellSize;
        private float _columnCellSize;

        private bool _viewDirty = true;

        private bool[] _maybe;

        private bool _canHint = true;
        public bool CanHint
        {
            get { return _canHint; }
            set { _canHint = value; }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public Point GridLocation
        {
            get { return _gridLocation; }
        }

        public Point GridPixelSize
        {
            get { return _pixelSize; }
        }

        public GridMap(int w, int h, int ts, Game game)
        {
            _score = 0;

            _view = new RenderTarget2D(game.GraphicsDevice, w, h);
            
            _font = game.Content.Load<SpriteFont>("Font1");
            _button = game.Content.Load<Texture2D>("button");
            _maybeTexture = game.Content.Load<Texture2D>("maybe");

            _size = new Point(w, h);

            _maybe = new bool[w * h];

            int mx = (int)Math.Ceiling(_size.X / 5.0f);
            int my = (int)Math.Ceiling(_size.Y / 5.0f);
            _revealSize = new Point(mx, my);
            _bigReveal = new bool[mx * my];
            _bigCellOK = new bool[mx * my];

            _rowNumbers = new int[h*RowSize];
            _columnNumbers = new int[w*ColumnSize];

            _cells = new bool[w * h];
            _colors = new Color[w * h];

            _solveCells = new bool[w * h];
            _solveColor = new Color[w * h];

            for (int x = 0; x < _colors.Length; x++)
            {
                _colors[x] = Color.Transparent;
                _solveColor[x] = BasicCellColor;
            }

            _tileSize = ts;
            _pixelSize = _size * new Point(_tileSize, _tileSize);

            _white = new Texture2D(game.GraphicsDevice, 1, 1);
            _white.SetData<Color>(new Color[] { Color.White });

            _gridLocation = new Point(10 * ts, 6 * ts);

            _rowCellSize = _tileSize * 0.65f;
            _columnCellSize = _tileSize * 0.6f;
        }

        public void UpdateView()
        {
            if (!_viewDirty)
                return;
            _viewDirty = false;

            _view.SetData<Color>(_colors);
        }

        public bool GetMaybe(int x, int y)
        {
            if (!IsInMap(x, y))
                return false;

            return _maybe[x + y * _size.X];
        }

        public void SetMaybe(int x, int y, bool m)
        {
            if (!IsInMap(x, y))
                return;

            _maybe[x + y * _size.X] = m;
        }

        public void SetSolveCells(bool[] cells, Color[] solveColor)
        { 
            _solveCells = cells;
            _solveColor = solveColor;
        }

        public void CopyRowNumbers(List<int> data, int row)
        {
            int ofs = row * RowSize;

            for (int x = 0; x < Math.Min(RowSize, data.Count); x++)
            {
                _rowNumbers[ofs + x] = data[x];
            }
        }

        public void CopyColumnNumbers(List<int> data, int col)
        {
            int ofs = col * ColumnSize;

            for (int x = 0; x < Math.Min(ColumnSize, data.Count); x++)
            {
                _columnNumbers[ofs + x] = data[x];
            }
        }

        public bool IsInMap(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < _size.X && y < _size.Y);
        }

        public void SetColorUser(int x, int y, bool set)
        {
            if (!IsInMap(x, y))
                return;

            int ofs = x + y * _size.X;

            if (set && !_cells[ofs])
            {
                if (_solveCells[ofs])
                {
                    _score += 100;
                }
                else
                {
                    _score -= 100;
                }
            }
            else if(!set && _cells[ofs])
            {
                _score -= 100;
            }

            _viewDirty = true;

            _colors[ofs] = (set ? _solveColor[ofs] : Color.Transparent);
            _cells[ofs] = set;

            CheckBigCell(x, y);
        }

        public Color GetColor(int x, int y)
        {
            if (!IsInMap(x, y))
                return Color.Transparent;

            return _colors[x + y * _size.X];
        }

        public void DrawCellColor(SpriteBatch batch, int x, int y, Color c)
        {
            batch.Draw(_white, new Rectangle(_gridLocation.X + x * _tileSize, _gridLocation.Y + y * _tileSize, _tileSize, _tileSize), c);
        }

        public void DrawCellTexture(SpriteBatch batch, int x, int y, Color c)
        {
            batch.Draw(_white, new Rectangle(_gridLocation.X + x * _tileSize, _gridLocation.Y + y * _tileSize, _tileSize, _tileSize), c);
            batch.Draw(_button, new Rectangle(_gridLocation.X + x * _tileSize, _gridLocation.Y + y * _tileSize, _tileSize, _tileSize), c);
        }

        public void DrawCellMaybe(SpriteBatch batch, int x, int y, Color c)
        {
            batch.Draw(_maybeTexture, new Rectangle(_gridLocation.X + x * _tileSize, _gridLocation.Y + y * _tileSize, _tileSize, _tileSize), c);
        }

        public void Draw(SpriteBatch batch)
        {
            UpdateView();

            batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            float wh = _view.Width / (float)_view.Height;
            float hw = _view.Height / (float)_view.Width;

            float height = (ColumnSize * _columnCellSize);
            float width = height * wh;

            batch.Draw(_view, new Rectangle((int)(_gridLocation.X - width), (int)(_gridLocation.Y - height), (int)width, (int)height), Color.White);

            batch.End();

            batch.Begin();
            for (int bx = 0; bx < _revealSize.X; bx++)
            {
                for (int by = 0; by < _revealSize.Y; by++)
                {
                    if (!_bigCellOK[bx + by * _revealSize.X])
                        continue;

                    batch.Draw(_white, new Rectangle(_gridLocation.X + bx * 5 * _tileSize, _gridLocation.Y + by * 5 * _tileSize, _tileSize * 5, _tileSize * 5), OkGreenColor);
                }
            }

            for (int y = 0; y < _size.Y; y++)
            {
                for (int x = 0; x < _size.X; x++)
                {
                    int ofs = x + y * _size.X;

                    if (_maybe[ofs])
                    {
                        DrawCellMaybe(batch, x, y, MaybeColor);
                    }

                    if (!_cells[ofs])
                    {
                        int bcx = (x / 5);
                        int bcy = (y / 5);

                        if (!_bigCellOK[bcx + bcy * _revealSize.X])
                        {
                            DrawCellColor(batch, x, y, EmptyCellBg);
                        }

                        continue;
                    }

                    DrawCellTexture(batch, x, y, _colors[ofs]);
                }
            }

            if(SelectedCell.HasValue)
            {
                DrawCellColor(batch, SelectedCell.Value.X, SelectedCell.Value.Y, SelectColor);
            }

            //Grid lines
            batch.Draw(_white, new Rectangle(_gridLocation.X - 1, _gridLocation.Y - 1, 2, _pixelSize.Y + 1), GridColor); //Left
            batch.Draw(_white, new Rectangle(_gridLocation.X + _pixelSize.X, _gridLocation.Y - 1, 2, _pixelSize.Y + 1), GridColor); //Right

            batch.Draw(_white, new Rectangle(_gridLocation.X - 1, _gridLocation.Y - 1, _pixelSize.X + 1, 2), GridColor); //Top
            batch.Draw(_white, new Rectangle(_gridLocation.X - 1, _gridLocation.Y + _pixelSize.Y, _pixelSize.X + 3, 2), GridColor); //Bottom

            for (int gx = 1; gx < _size.X; gx++)
                batch.Draw(_white, new Rectangle(_gridLocation.X + _tileSize * gx, _gridLocation.Y - 1, (gx % 5 == 0 ? 2 : 1), _pixelSize.Y + 1), GridColor);

            for (int gy = 1; gy < _size.Y; gy++)
                batch.Draw(_white, new Rectangle(_gridLocation.X - 1, _gridLocation.Y + _tileSize * gy, _pixelSize.X + 1, (gy % 5 == 0 ? 2 : 1)), GridColor);

            //Row numbers
            batch.Draw(_white, new Rectangle((int)(_gridLocation.X - RowSize * _rowCellSize), _gridLocation.Y - 1, (int)(_rowCellSize * RowSize), _pixelSize.Y), EmptyNumBg);

            batch.Draw(_white, new Rectangle((int)(_gridLocation.X - RowSize * _rowCellSize), _gridLocation.Y - 1, 2, _pixelSize.Y + 1), GridColor); //Left

            batch.Draw(_white, new Rectangle((int)(_gridLocation.X - RowSize * _rowCellSize), _gridLocation.Y - 1, (int)(_rowCellSize*RowSize), 2), GridColor); //Top
            batch.Draw(_white, new Rectangle((int)(_gridLocation.X - RowSize * _rowCellSize), _gridLocation.Y + _pixelSize.Y, (int)(_rowCellSize * RowSize), 2), GridColor); //Bottom

            for (int row = 0; row < _size.Y; row++)
            {
                if (row > 0)
                    batch.Draw(_white, new Rectangle((int)(_gridLocation.X - RowSize * _rowCellSize), _gridLocation.Y + _tileSize * row, (int)(_rowCellSize * RowSize), (row % 5 == 0 ? 2 : 1)), GridColor); //Bottom

                int ofs = row * RowSize;

                for (int column = 0; column < RowSize; column++)
                {
                    int idx = RowSize - 1 - column;

                    int num = _rowNumbers[ofs + idx];
                    if (num == 0)
                        continue;

                    Vector2 cellCenter = new Vector2(_gridLocation.X - _rowCellSize * (idx + 1) + (_rowCellSize / 2) - 3, _gridLocation.Y + _tileSize * row + (_tileSize / 2));

                    string txt = num.ToString();

                    batch.DrawString(_font, num.ToString(), cellCenter - (_font.MeasureString(txt) / 2), Color.White);
                }
            }

            //Column numbers
            batch.Draw(_white, new Rectangle(_gridLocation.X - 1, (int)(_gridLocation.Y - ColumnSize * _columnCellSize), _pixelSize.X + 1, (int)(ColumnSize * _columnCellSize)), EmptyNumBg);

            batch.Draw(_white, new Rectangle(_gridLocation.X - 1, (int)(_gridLocation.Y - ColumnSize * _columnCellSize), 2, (int)(ColumnSize * _columnCellSize)), GridColor); //Left
            batch.Draw(_white, new Rectangle(_gridLocation.X + _pixelSize.X, (int)(_gridLocation.Y - ColumnSize * _columnCellSize), 2, (int)(ColumnSize * _columnCellSize)), GridColor); //Right

            batch.Draw(_white, new Rectangle(_gridLocation.X, (int)(_gridLocation.Y - ColumnSize * _columnCellSize), _pixelSize.X, 2), GridColor); //Top

            for (int col = 0; col < _size.X; col++)
            {
                if(col > 0)
                    batch.Draw(_white, new Rectangle((int)(_gridLocation.X + _tileSize * col), (int)(_gridLocation.Y - ColumnSize * _columnCellSize), (col % 5 == 0 ? 2 : 1), (int)(ColumnSize * _columnCellSize)), GridColor);

                int ofs = col * ColumnSize;

                for (int row = 0; row < ColumnSize; row++)
                {
                    int idx = ColumnSize - 1 - row;

                    int num = _columnNumbers[ofs + idx];
                    if (num == 0)
                        continue;

                    Vector2 cellCenter = new Vector2(_gridLocation.X + col * _tileSize + (_tileSize / 2), _gridLocation.Y - _columnCellSize * (idx + 1) + (_rowCellSize / 2));

                    string txt = num.ToString();

                    batch.DrawString(_font, num.ToString(), cellCenter - (_font.MeasureString(txt) / 2), Color.White);
                }
            }

            batch.End();
        }

        public bool HasWrongCell()
        {
            for (int ofs = 0; ofs < _cells.Length; ofs++)
            {
                if (_cells[ofs] && !_solveCells[ofs])
                    return true;
            }

            return false;
        }

        public void Reset()
        {
            _score = 0;

            for (int ofs = 0; ofs < _cells.Length; ofs++)
            {
                _cells[ofs] = false;
                _colors[ofs] = EmptyCellBg;
                _maybe[ofs] = false;
            }

            for (int bf = 0; bf < _bigCellOK.Length; bf++)
            {
                _bigCellOK[bf] = false;
                _bigReveal[bf] = false;
            }

            _viewDirty = true;
        }

        public void Finish()
        {
            for (int ofs = 0; ofs < _cells.Length; ofs++)
            {
                if (_cells[ofs] && !_solveCells[ofs])
                {
                    _cells[ofs] = false;
                }
            }

            for (int bfs = 0; bfs < _bigCellOK.Length; bfs++)
                _bigCellOK[bfs] = true;
        }

        public Point? GetCellAt(Point screen)
        {
            Point p = ((screen - _gridLocation).ToVector() / _tileSize).ToPoint();
            if (p.X < 0 || p.Y < 0 || p.X >= _size.X || p.Y >= _size.Y)
                return null;

            return p;
        }

        public void CheckBigCell(int scx, int scy)
        {
            int cx = scx / 5;
            int cy = scy / 5;

            int sx = cx * 5;
            int sy = cy * 5;

            int ok = 0;
            int test = 0;
            for (int x = sx; x < sx + 5; x++)
            {
                for (int y = sy; y < sy + 5; y++)
                {
                    int ofs = x + y * _size.X;

                    if (_solveCells[ofs])
                    {
                        test++;

                        if (_cells[ofs])
                        {
                            ok++;
                        }
                    }
                }
            }

            _bigCellOK[cx + cy * _revealSize.X] = (ok >= test);
        }

        public void Hint()
        {
            List<Point> notRevealed = new List<Point>();

            for (int o = 0; o < (_revealSize.X * _revealSize.Y); o++)
            {
                if (_bigReveal[o])
                    continue;

                int rx = o % _revealSize.X;
                int ry = o / _revealSize.X;

                notRevealed.Add(new Point(rx, ry));
            }

            if (notRevealed.Count == 0)
                return;

            int rndX = _rnd.Next(0, notRevealed.Count);

            Point rev = notRevealed[rndX];

            _bigReveal[rev.X + rev.Y * _revealSize.X] = true;

            int posx = rev.X * 5;
            int posy = rev.Y * 5;

            _score -= 300;

            for (int rx = posx; rx < posx + 5; rx++)
            {
                for (int ry = posy; ry < posy + 5; ry++)
                {
                    int ofs = rx + ry * _size.X;

                    _cells[ofs] = _solveCells[ofs];
                    _colors[ofs] = _solveColor[ofs];

                    CheckBigCell(rx, ry);
                }
            }

            _viewDirty = true;
        }

        public bool SolveCell()
        {
            List<Point> unsolved = new List<Point>();

            int ofs;
            for (ofs = 0; ofs < _cells.Length; ofs++)
            {
                if(!_cells[ofs] && _solveCells[ofs])
                {
                    unsolved.Add(new Point(ofs % _size.X, ofs / _size.X));
                }
            }

            if (unsolved.Count == 0)
                return false;

            int rndX = _rnd.Next(0, unsolved.Count);
            Point solve = unsolved[rndX];

            ofs = solve.X + solve.Y * _size.X;

            _cells[ofs] = _solveCells[ofs];
            _colors[ofs] = _solveColor[ofs];

            _viewDirty = true;
            _score -= 10;

            CheckBigCell(solve.X, solve.Y);

            return true;
        }

        public bool CheckDone()
        {
            if (!_canHint)
                return false;

            for (int ofs = 0; ofs < _cells.Length; ofs++)
            {
                if (_cells[ofs] != _solveCells[ofs])
                    return false;
            }

            return true;
        }

        public void SaveState(string file)
        {
            using (Stream files = new FileStream(file, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(files))
                {
                    bw.Write(_size.X);
                    bw.Write(_size.Y);

                    for (int ofs = 0; ofs < (_size.X * _size.Y); ofs++)
                    {
                        bw.Write(_cells[ofs]);
                        Extensions.WriteColor(bw, _colors[ofs]);
                        bw.Write(_solveCells[ofs]);
                        Extensions.WriteColor(bw, _solveColor[ofs]);
                        bw.Write(_maybe[ofs]);
                    }

                    bw.Write(_revealSize.X);
                    bw.Write(_revealSize.Y);

                    for (int ofs = 0; ofs < (_revealSize.X * _revealSize.Y); ofs++)
                    {
                        bw.Write(_bigReveal[ofs]);
                        bw.Write(_bigCellOK[ofs]);
                    }

                    for (int x = 0; x < _rowNumbers.Length; x++)
                    {
                        bw.Write(_rowNumbers[x]);
                    }

                    for (int y = 0; y < _columnNumbers.Length; y++)
                    {
                        bw.Write(_columnNumbers[y]);
                    }

                    bw.Write(_score);
                    bw.Write(_canHint);
                }
            }
        }

        public static GridMap LoadState(string file, MainGame game)
        {
            GridMap map;

            using (Stream files = new FileStream(file, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(files))
                {
                    int sx = br.ReadInt32();
                    int sy = br.ReadInt32();

                    map = new SEDGame.GridMap(sx, sy, MainGame.TileSize, game);

                    for (int ofs = 0; ofs < (map._size.X * map._size.Y); ofs++)
                    {
                        map._cells[ofs] = br.ReadBoolean();
                        map._colors[ofs] = Extensions.ReadColor(br);
                        map._solveCells[ofs] = br.ReadBoolean();
                        map._solveColor[ofs] = Extensions.ReadColor(br);
                        map._maybe[ofs] = br.ReadBoolean();
                    }

                    map._revealSize = new Point(br.ReadInt32(), br.ReadInt32());

                    for (int ofs = 0; ofs < (map._revealSize.X * map._revealSize.Y); ofs++)
                    {
                        map._bigReveal[ofs] = br.ReadBoolean();
                        map._bigCellOK[ofs] = br.ReadBoolean();
                    }

                    for (int x = 0; x < map._rowNumbers.Length; x++)
                    {
                        map._rowNumbers[x] = br.ReadInt32();
                    }

                    for (int y = 0; y < map._columnNumbers.Length; y++)
                    {
                        map._columnNumbers[y] = br.ReadInt32();
                    }

                    map.Score = br.ReadInt32();
                    map._canHint = br.ReadBoolean();
                }
            }

            return map;
        }
    }
}
