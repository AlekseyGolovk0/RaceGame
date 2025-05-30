namespace Race
{
    public partial class RaceGame : Form
    {
        private const int _carMoveStep = 9;
        private const int _maxCarSpeed = 21;
        private const int _minCarSpeed = 0;
        private const int _continueCost = 1;

        private readonly Label[] _lanesOne;
        private readonly Label[] _lanesTwo;
        private readonly Label[] _lanesMenuOne;
        private readonly Label[] _lanesMenuTwo;
        private readonly Random _random = new Random();

        private int _score = 0;
        private int _coins = 0;
        private int _carSpeed = 2;
        public RaceGame()
        {
            InitializeComponent();

            this.Controls.Add(panelMenu);
            this.Controls.Add(panelGame);
            this.Controls.Add(panelPause);

            panelMenu.BringToFront();
            panelPause.SendToBack();
            panelGame.SendToBack();

            _lanesOne = new[] { LaneOne1, LaneOne2, LaneOne3, LaneOne4, LaneOne5 };
            _lanesTwo = new[] { LaneTwo1, LaneTwo2, LaneTwo3, LaneTwo4, LaneTwo5 };
            _lanesMenuOne = new[] { MenuOneLane1, MenuOneLane2, MenuOneLane3, MenuOneLane4, MenuOneLane5 };
            _lanesMenuTwo = new[] { MenuTwoLane1, MenuTwoLane2, MenuTwoLane3, MenuTwoLane4, MenuTwoLane5 };

            timerRoad.Interval = 10;
            timerTowardCars.Interval = 10;
            timerMenu.Interval = 15;

            panelMenu.Size = this.ClientSize;
            panelGame.Size = this.ClientSize;
            panelPause.Size = this.ClientSize;

            RestartGameState();
        }

        private void RestartGameState()
        {
            _score = 0;
            _coins = 0;
            _carSpeed = 2;

            timerRoad.Stop();
            timerTowardCars.Stop();

            panelMenu.Show();
            panelGame.Hide();
            panelPause.Hide();
        }

        private void RoadMove_Tick(object sender, EventArgs e)
        {
            labelScore.Text = $@"Score: {_score / 10}";

            UpdateRoadElements();
            CollectCoins();

            if (_carSpeed != _minCarSpeed) _score++;
        }
        private void UpdateRoadElements()
        {
            MoveLanes(_lanesOne);
            MoveLanes(_lanesTwo);

            MoveGameObject(Coin1);
            MoveGameObject(Coin2);
            MoveGameObject(Coin3);
        }
        private void MoveLanes(Label[] lanes)
        {
            foreach (var lane in lanes)
            {
                if (lane == null) continue;

                lane.Top += _carSpeed;
                if (lane.Top >= Height)
                {
                    lane.Top = -lane.Height;
                }
            }
        }

        private void MoveGameObject(Control gameObject)
        {
            if (gameObject == null) return;

            gameObject.Top += _carSpeed;
            if (gameObject.Top <= Height) return;

            gameObject.Top = -gameObject.Height;
            gameObject.Left = _random.Next(0, Math.Max(1, Width - gameObject.Width));
        }

        void CollectCoins()
        {
            TryCollectCoin(Coin1, 0, 120);
            TryCollectCoin(Coin2, 120, 240);
            TryCollectCoin(Coin3, 240, 300);
        }
        private void TryCollectCoin(PictureBox coin, int minX, int maxX)
        {
            if (mainCar.Bounds.IntersectsWith(coin.Bounds))
            {
                _coins++;
                labelCoins.Text = $"Coins: {_coins}";
                coin.Top = -coin.Height;
                coin.Left = _random.Next(minX, maxX);
            }
        }

        private void RaceGame_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right when mainCar.Right < Width && _carSpeed != _minCarSpeed:
                    mainCar.Left += _carMoveStep;
                    break;

                case Keys.Left when mainCar.Left > 0 && _carSpeed != _minCarSpeed:
                    mainCar.Left -= _carMoveStep;
                    break;

                case Keys.Up when _carSpeed < _maxCarSpeed:
                    _carSpeed++;
                    break;

                case Keys.Down when _carSpeed > _minCarSpeed:
                    _carSpeed--;
                    break;

                case Keys.Escape:
                    PauseGame();
                    break;
            }
        }

        private void PauseGame()
        {
            timerRoad.Stop();
            timerTowardCars.Stop();
            panelPause.Show();
            panelPause.BringToFront();
        }

        private void timerTowardCars_Tick(object sender, EventArgs e)
        {
            MoveCar(towardCar1, _carSpeed + 4);
            MoveCar(towardCar2, _carSpeed + 2);
            MoveCar(towardCar3, _carSpeed + 3);

            CheckCollisions();
        }
        private void MoveCar(PictureBox car, int speed)
        {
            car.Top += speed;
            if (car.Top > Height)
            {
                car.Top = -car.Height;
                car.Left = _random.Next(0, Width - car.Width);
            }
        }

        private void CheckCollisions()
        {
            if (mainCar.Bounds.IntersectsWith(towardCar1.Bounds) ||
                mainCar.Bounds.IntersectsWith(towardCar2.Bounds) ||
                mainCar.Bounds.IntersectsWith(towardCar3.Bounds))
            {
                GameOver();
            }
        }
        private void GameOver()
        {
            timerRoad.Stop();
            timerTowardCars.Stop();

            if (_coins < _continueCost)
            {
                ShowGameOver();
            }
            else
            {
                OfferContinueGame();
            }
        }
        private void ShowGameOver()
        {
            MessageBox.Show(@"Game Over!", @"Приехали!");
            RestartGameState();
        }

        private void OfferContinueGame()
        {
            timerRoad.Stop();
            timerTowardCars.Stop();
            if (_coins >= _continueCost)
            {
                var result = MessageBox.Show(@$"Продолжить? ({_continueCost} coins)", @"Приехали!", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    ContinueGame();
                }
                else
                {
                    ShowGameOver();
                }
            }
        }

        private void ContinueGame()
        {
            _carSpeed = 2;
            _coins -= _continueCost;
            labelCoins.Text = $"Coins: {_coins}";

            ResetCarPosition(towardCar1);
            ResetCarPosition(towardCar2);
            ResetCarPosition(towardCar3);

            timerRoad.Start();
            timerTowardCars.Start();
        }

        private void StartGame()
        {
            _carSpeed = 2;
            InitializeGameElements();

            timerRoad.Start();
            timerTowardCars.Start();

            panelPause.Hide();
            panelMenu.Hide();
            panelGame.Show();
            panelGame.BringToFront();
        }

        private void InitializeGameElements()
        {
            _score = 0;
            _carSpeed = 2;

            ResetCarPosition(towardCar1);
            ResetCarPosition(towardCar2);
            ResetCarPosition(towardCar3);
        }
        private void ResetCarPosition(PictureBox car)
        {
            if (car == null) return;

            car.Top = -car.Height;
            car.Left = _random.Next(0, Math.Max(1, Width - car.Width));
        }

        private void TimerCarMove_Tick(object sender, EventArgs e)
        {
            AnimateMenuElements();
        }

        private void AnimateMenuElements()
        {
            MoveLanes(_lanesMenuOne);
            MoveLanes(_lanesMenuTwo);

            AnimateMenuCar(CarMenu1, 5);
            AnimateMenuCar(CarMenu2, 3);
            AnimateMenuCar(CarMenu3, 4);
        }

        private void AnimateMenuCar(PictureBox car, int speed)
        {
            if (car == null) return;

            car.Top += speed;
            if (car.Top <= Height) return;

            car.Top = -car.Height;
            car.Left = _random.Next(0, Math.Max(1, Width - car.Width));
        }

        private void ResumeButton(object sender, EventArgs e)
        {
            timerRoad.Enabled = true;
            timerTowardCars.Enabled = true;

            panelPause.Hide();
            panelGame.BringToFront();
        }

        private void ExitButton(object sender, EventArgs e)
        {
            panelMenu.Show();
            panelMenu.BringToFront();
            panelPause.Hide();
            panelGame.Hide();
        }

        private void HelpButton(object sender, EventArgs e)
        {
            try
            {
                Help.ShowHelp(this, @"help.chm", HelpNavigator.TableOfContents);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть справку: {ex.Message}");
            }
        }

        private void StartButton(object sender, EventArgs e)
        {
            StartGame();
        }

        private void MenuExitButton(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PauseButton(object sender, EventArgs e)
        {
            PauseGame();
        }

        private void RaceGame_Shown(object sender, EventArgs e)
        {
            RestartGameState();
        }
    }
}

