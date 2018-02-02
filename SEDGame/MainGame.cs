using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SEDGame
{
    public class MainGame : Game
    {
        static float SolveStepTime = 0.5f;

        public static int TileSize = 32;

        public static int MaxMapWidth = 35;
        public static int MaxMapHeight = 20;

        static int ButtonUIY = 220;

        GraphicsDeviceManager graphics;
        SpriteBatch batch;

        SpriteFont font;

        Texture2D background;

        Texture2D warning;

        Texture2D menuBg;
        Rectangle menuBgRect;

        GridMap grid;

        //Buttons
        Button btn_hint;
        Button btn_save;
        Button btn_load;
        Button btn_reset;
        Button btn_solveStart;
        Button btn_solveStop;

        //Input
        MouseState _lastMouse;

        bool _canPlace;

        bool youWINXD = false;
        Texture2D winTexture;

        //Solve
        bool solve = false;
        float solveTime;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 900;

            IsMouseVisible = true;

            Window.Title = "Number picture stuff game - 2k18";
        }

        private void LoadEmptyGrid()
        {
            grid = new GridMap(MaxMapWidth, MaxMapHeight, TileSize, this);

            InitStuff();
        }

        private void LoadContentFileGrid(string file)
        {
            string realFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Content.RootDirectory);

            realFile = Path.Combine(realFile, file);

            _canPlace = true;

            LoadFileGrid(realFile);
        }

        private void LoadFileGrid(string file)
        {
            using (StreamReader sr = new StreamReader(file))
            {
                string[] xd = sr.ReadLine().Split(' ');

                int rows = Convert.ToInt32(xd[0]);
                int columns = Convert.ToInt32(xd[1]);

                grid = new GridMap(columns, rows, TileSize, this);

                sr.ReadLine(); //lol

                List<int> nums = new List<int>();

                for (int row = 0; row < rows; row++)
                {
                    nums.Clear();

                    string[] ln = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int l = 0; l < ln.Length; l++)
                        nums.Add(Convert.ToInt32(ln[l]));

                    nums.Reverse();
                    grid.CopyRowNumbers(nums, row);    
                }

                sr.ReadLine(); //lol

                for (int col = 0; col < columns; col++)
                {
                    nums.Clear();

                    string[] ln = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int l = 0; l < ln.Length; l++)
                        nums.Add(Convert.ToInt32(ln[l]));

                    nums.Reverse();
                    grid.CopyColumnNumbers(nums, col);
                }

                grid.CanHint = false;
                youWINXD = false;
                solve = false;
            }

            InitStuff();
        }

        private void LoadContentImageGrid(string file)
        {
            string realFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Content.RootDirectory);

            realFile = Path.Combine(realFile, file);

            LoadImageGrid(realFile);
        }

        private void LoadImageGrid(string file)
        {
            grid = MapImageConvert.LoadImage(file, this);

            _canPlace = true;
            youWINXD = false;
            solve = false;

            InitStuff();
        }

        private void LoadSaveFile(string file)
        {
            grid = GridMap.LoadState(file, this);

            _canPlace = true;
            youWINXD = false;
            solve = false;

            InitStuff();
        }

        private void InitStuff()
        {
            int size = 64;
            int pad = size + 6;
            int btnBase = ButtonUIY + 80;

            menuBgRect = new Rectangle(GraphicsDevice.Viewport.Width - menuBg.Width, 0, menuBg.Width, GraphicsDevice.Viewport.Height);

            int btnLeft = (menuBgRect.Width / 2) + menuBgRect.X - (size / 2);

            btn_save.Target = new Rectangle(btnLeft, btnBase, size, size);
            btn_load.Target = new Rectangle(btnLeft, btnBase + pad, size, size);
            btn_hint.Target = new Rectangle(btnLeft, btnBase + pad * 2, size, size);
            btn_reset.Target = new Rectangle(btnLeft, btnBase + pad * 3, size, size);

            btn_solveStart.Target = btn_solveStop.Target = new Rectangle(btnLeft, btnBase + pad * 4, size, size);

            btn_hint.Enabled = grid.CanHint;
            btn_solveStart.Enabled = btn_solveStop.Enabled = grid.CanHint;

            CheckDone();
        }

        protected override void LoadContent()
        {
            batch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>("bg");
            menuBg = Content.Load<Texture2D>("menu");
            winTexture = Content.Load<Texture2D>("uwon");
            warning = Content.Load<Texture2D>("warning");

            font = Content.Load<SpriteFont>("Font1");

            btn_hint = new SEDGame.Button(Content.Load<Texture2D>("hint"));
            btn_hint.OnClick += Btn_hint_OnClick;

            btn_load = new SEDGame.Button(Content.Load<Texture2D>("open"));
            btn_load.OnClick += Btn_load_OnClick;

            btn_save = new SEDGame.Button(Content.Load<Texture2D>("save"));
            btn_save.OnClick += Btn_save_OnClick;

            btn_reset = new SEDGame.Button(Content.Load<Texture2D>("reset"));
            btn_reset.OnClick += Btn_reset_OnClick;

            btn_solveStart = new SEDGame.Button(Content.Load<Texture2D>("play"));
            btn_solveStart.OnClick += Btn_solveStart_OnClick;

            btn_solveStop = new SEDGame.Button(Content.Load<Texture2D>("stop"));
            btn_solveStop.OnClick += Btn_solveStop_OnClick;

            //LoadEmptyGrid();
            //LoadContentFileGrid("Map.txt");
            LoadContentImageGrid("dzsingdzsang.png");
        }

        private void Btn_load_OnClick(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog opf = new System.Windows.Forms.OpenFileDialog())
            {
                opf.Filter = "All supported files|*.sav;*.png;*.jpg;*.bmp;*.txt|Save file (*.sav)|*.sav|Picture (*.png,*.jpg,*.bmp)|*.png;*.jpg;*.bmp|Txt files (*.txt)|*.txt";
                opf.InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Content.RootDirectory); 
                if (opf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string ext = Path.GetExtension(opf.FileName).ToLower();
                    switch (ext)
                    {
                        case ".sav":
                            LoadSaveFile(opf.FileName);
                            break;
                        case ".png":
                        case ".jpg":
                        case ".bmp":
                            LoadImageGrid(opf.FileName);
                            break;
                        case ".txt":
                            LoadFileGrid(opf.FileName);
                            break;
                    }
                }
            }
        }

        private void Btn_save_OnClick(object sender, EventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog svf = new System.Windows.Forms.SaveFileDialog())
            {
                svf.Filter = "Save file (*.sav)|*.sav";
                svf.InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Content.RootDirectory);
                if (svf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    grid.SaveState(svf.FileName);
                }
            }
        }

        private void Btn_reset_OnClick(object sender, EventArgs e)
        {
            grid.Reset();

            _canPlace = true;
            solve = false;

            youWINXD = false;

            InitStuff();
        }

        private void Btn_hint_OnClick(object sender, EventArgs e)
        {
            solve = false;

            grid.Hint();

            CheckDone();
        }

        private void CheckDone()
        {
            if (!grid.CheckDone())
                return;

            _canPlace = false;
            btn_hint.Enabled = false;
            btn_solveStart.Enabled = btn_solveStop.Enabled = false;

            grid.Finish();

            solve = false;

            youWINXD = true;
        }

        private void Btn_solveStart_OnClick(object sender, EventArgs e)
        {
            solve = true;
            solveTime = SolveStepTime;
        }

        private void Btn_solveStop_OnClick(object sender, EventArgs e)
        {
            solve = false;
        }

        protected override void Update(GameTime gameTime)
        { 
            MouseState mouse = Mouse.GetState();
            {
                grid.SelectedCell = grid.GetCellAt(mouse.Position);

                if (_canPlace)
                {
                    if (mouse.LeftButton == ButtonState.Pressed && _lastMouse.LeftButton == ButtonState.Released)
                    {
                        if (grid.SelectedCell.HasValue)
                        {
                            grid.SetColorUser(grid.SelectedCell.Value.X, grid.SelectedCell.Value.Y, true);

                            CheckDone();
                        }
                    }
                    if (mouse.RightButton == ButtonState.Pressed && _lastMouse.RightButton == ButtonState.Released)
                    {
                        if (grid.SelectedCell.HasValue)
                        {
                            grid.SetColorUser(grid.SelectedCell.Value.X, grid.SelectedCell.Value.Y, false);

                            CheckDone();
                        }
                    }

                    if (mouse.MiddleButton == ButtonState.Pressed && _lastMouse.MiddleButton == ButtonState.Released)
                    {
                        if (grid.SelectedCell.HasValue)
                        {
                            grid.SetMaybe(grid.SelectedCell.Value.X, grid.SelectedCell.Value.Y, !grid.GetMaybe(grid.SelectedCell.Value.X, grid.SelectedCell.Value.Y));
                        }
                    }
                }

                ButtonInput(mouse, _lastMouse);
            }
            _lastMouse = mouse;

            if (solve)
            {
                if ((solveTime -= gameTime.DeltaTime()) <= 0.0f)
                {
                    solveTime = SolveStepTime;

                    if (!grid.SolveCell())
                        solve = false;

                    CheckDone();
                }
            }

            base.Update(gameTime);
        }

        private void ButtonInput(MouseState ms, MouseState ls)
        {
            if (btn_hint.Input(ms, ls))
                return;

            if (btn_load.Input(ms, ls))
                return;

            if (btn_save.Input(ms, ls))
                return;

            if (btn_reset.Input(ms, ls))
                return;

            if (!solve && btn_solveStart.Input(ms, ls))
                return;

            if (solve && btn_solveStop.Input(ms, ls))
                return;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            batch.Begin();
            batch.Draw(background, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            //Menu
            batch.Draw(menuBg, menuBgRect, Color.Lerp(Color.White, Color.Transparent, 0.5f));

            //Score
            {
                string sctext = "Score:";
                Vector2 size = font.MeasureString(sctext);

                batch.DrawString(font, sctext, new Vector2(menuBgRect.X + (menuBgRect.Width / 2.0f) - (size.X / 2.0f), ButtonUIY), Color.White);
                float xd = (float)size.Y;

                sctext = grid.Score.ToString();
                size = font.MeasureString(sctext);
                batch.DrawString(font, sctext, new Vector2(menuBgRect.X + (menuBgRect.Width / 2.0f) - (size.X / 2.0f), ButtonUIY + xd * 1.2f), Color.White);
            }

            btn_hint.Draw(batch);
            btn_load.Draw(batch);
            btn_save.Draw(batch);
            btn_reset.Draw(batch);

            if (solve)
                btn_solveStop.Draw(batch);
            else
                btn_solveStart.Draw(batch);

            if (grid.HasWrongCell())
            {
                batch.Draw(warning, new Rectangle(GraphicsDevice.Viewport.Width - 64, 0, 64, 64), Color.White);
            }

            batch.End();

            grid.Draw(batch);

            if (youWINXD)
            {
                batch.Begin();
                batch.Draw(winTexture, new Vector2((GraphicsDevice.Viewport.Width / 2.0f) - (winTexture.Width / 2.0f), (GraphicsDevice.Viewport.Height / 2.0f) - (winTexture.Height / 2.0f)), Color.White);
                batch.End();
            }

            base.Draw(gameTime);
        }
    }
}
