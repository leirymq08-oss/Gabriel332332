using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisSimple
{
    public partial class Form1 : Form
    {
        const int Cols = 10;
        const int Rows = 20;
        const int CellSize = 28;

        readonly int[,] board = new int[Rows, Cols];
        readonly System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();
        readonly Random rnd = new Random();

        static readonly int[][,] Shapes = new int[][,]
        {
            new int[,] { {0,0},{1,0},{0,1},{1,1} },
            new int[,] { {0,0},{1,0},{2,0},{3,0} },
            new int[,] { {0,0},{0,1},{1,1},{2,1} },
            new int[,] { {2,0},{0,1},{1,1},{2,1} },
            new int[,] { {1,0},{2,0},{0,1},{1,1} },
            new int[,] { {0,0},{1,0},{1,1},{2,1} },
            new int[,] { {1,0},{0,1},{1,1},{2,1} }
        };

        static readonly Color[] Colors = new Color[]
        {
            Color.Yellow, Color.Cyan, Color.Blue, Color.Orange,
            Color.Green, Color.Red, Color.Purple
        };

        int currentShape;
        List<Point> currentCells = new List<Point>();
        int currentColor;
        int score;
        bool gameOver;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            ClientSize = new Size(Cols * CellSize + 150, Rows * CellSize + 40);
            Text = "Tetris Sencillo";
            KeyDown += Form1_KeyDown;
            Paint += Form1_Paint;

            gameTimer.Interval = 500;
            gameTimer.Tick += GameTimer_Tick;

            SpawnPiece();
            gameTimer.Start();
        }

        void SpawnPiece()
        {
            currentShape = rnd.Next(Shapes.Length);
            currentColor = currentShape;
            currentCells.Clear();

            var shape = Shapes[currentShape];
            int offsetX = Cols / 2 - 1;

            for (int i = 0; i < shape.GetLength(0); i++)
            {
                currentCells.Add(new Point(shape[i, 0] + offsetX, shape[i, 1]));
            }

            if (!IsValidPosition(currentCells))
            {
                gameOver = true;
                gameTimer.Stop();
            }
        }

        bool IsValidPosition(List<Point> cells)
        {
            foreach (var c in cells)
            {
                if (c.X < 0 || c.X >= Cols || c.Y < 0 || c.Y >= Rows)
                    return false;
                if (board[c.Y, c.X] != 0)
                    return false;
            }
            return true;
        }

        void GameTimer_Tick(object? sender, EventArgs e)
        {
            MovePiece(0, 1);
        }

        void MovePiece(int dx, int dy)
        {
            if (gameOver) return;

            var newCells = new List<Point>();
            foreach (var c in currentCells)
                newCells.Add(new Point(c.X + dx, c.Y + dy));

            if (IsValidPosition(newCells))
            {
                currentCells = newCells;
            }
            else if (dy == 1)
            {
                LockPiece();
                ClearLines();
                SpawnPiece();
            }

            Invalidate();
        }

        void LockPiece()
        {
            foreach (var c in currentCells)
            {
                board[c.Y, c.X] = currentColor + 1;
            }
        }

        void ClearLines()
        {
            for (int row = Rows - 1; row >= 0; row--)
            {
                bool full = true;
                for (int col = 0; col < Cols; col++)
                {
                    if (board[row, col] == 0) { full = false; break; }
                }

                if (full)
                {
                    for (int r = row; r > 0; r--)
                        for (int c = 0; c < Cols; c++)
                            board[r, c] = board[r - 1, c];

                    for (int c = 0; c < Cols; c++)
                        board[0, c] = 0;

                    score += 100;
                    row++;
                }
            }
        }

        void RotatePiece()
        {
            if (gameOver) return;

            Point pivot = currentCells[0];
            var rotated = new List<Point>();

            foreach (var c in currentCells)
            {
                int relX = c.X - pivot.X;
                int relY = c.Y - pivot.Y;
                int newX = pivot.X - relY;
                int newY = pivot.Y + relX;
                rotated.Add(new Point(newX, newY));
            }

            if (IsValidPosition(rotated))
            {
                currentCells = rotated;
                Invalidate();
            }
        }

        void HardDrop()
        {
            if (gameOver) return;
            while (true)
            {
                var newCells = new List<Point>();
                foreach (var c in currentCells)
                    newCells.Add(new Point(c.X, c.Y + 1));

                if (IsValidPosition(newCells))
                    currentCells = newCells;
                else
                    break;
            }
            LockPiece();
            ClearLines();
            SpawnPiece();
            Invalidate();
        }

        void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (gameOver && e.KeyCode == Keys.R)
            {
                RestartGame();
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Left:
                    MovePiece(-1, 0);
                    break;
                case Keys.Right:
                    MovePiece(1, 0);
                    break;
                case Keys.Down:
                    MovePiece(0, 1);
                    break;
                case Keys.Up:
                    RotatePiece();
                    break;
                case Keys.Space:
                    HardDrop();
                    break;
            }
        }

        void RestartGame()
        {
            Array.Clear(board, 0, board.Length);
            score = 0;
            gameOver = false;
            SpawnPiece();
            gameTimer.Start();
            Invalidate();
        }

        void Form1_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (board[row, col] != 0)
                    {
                        DrawCell(g, col, row, Colors[board[row, col] - 1]);
                    }
                }
            }

            if (!gameOver)
            {
                foreach (var c in currentCells)
                {
                    DrawCell(g, c.X, c.Y, Colors[currentColor]);
                }
            }

            g.DrawRectangle(Pens.White, 0, 0, Cols * CellSize, Rows * CellSize);

            int panelX = Cols * CellSize + 15;
            using (var font = new Font("Arial", 12, FontStyle.Bold))
            {
                g.DrawString("Puntaje:", font, Brushes.White, panelX, 20);
                g.DrawString(score.ToString(), font, Brushes.Yellow, panelX, 45);
                g.DrawString("Controles:", font, Brushes.White, panelX, 100);
            }
            using (var font = new Font("Arial", 9))
            {
                g.DrawString("← → : mover", font, Brushes.LightGray, panelX, 130);
                g.DrawString("↑ : rotar", font, Brushes.LightGray, panelX, 150);
                g.DrawString("↓ : bajar", font, Brushes.LightGray, panelX, 170);
                g.DrawString("Espacio: caída rápida", font, Brushes.LightGray, panelX, 190);
                g.DrawString("R : reiniciar", font, Brushes.LightGray, panelX, 210);
            }

            if (gameOver)
            {
                using (var font = new Font("Arial", 20, FontStyle.Bold))
                {
                    g.DrawString("GAME OVER", font, Brushes.Red, 20, Rows * CellSize / 2 - 20);
                }
                using (var font = new Font("Arial", 11))
                {
                    g.DrawString("Presiona R para reiniciar", font, Brushes.White, 20, Rows * CellSize / 2 + 20);
                }
            }
        }

        void DrawCell(Graphics g, int col, int row, Color color)
        {
            var rect = new Rectangle(col * CellSize, row * CellSize, CellSize, CellSize);
            using (var brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, rect);
            }
            g.DrawRectangle(Pens.Black, rect);
        }
    }
}
