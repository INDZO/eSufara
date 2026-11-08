using eSufara.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace eSufara.View.Pages
{
    public partial class ExerciseView : Page
    {
        private const double QuestionTimeSec = 10.0;

        private readonly int _lessonNumber;
        private readonly int _userId;
        private readonly bool _initialDoneParam;
        private readonly string _baseDir;
        private readonly Random _rng = new();
        private readonly ProgressService _progressService;

        private readonly Dictionary<int, string> _answers = new();
        private Dictionary<int, string> _audioByIndex = new();
        private List<int> _questionOrder = new();
        private int _qPos;
        private int _score;
        private int _currentN = -1;
        private double _timeLeftSec;

        private DispatcherTimer? _timer;
        private System.Windows.Media.MediaPlayer? _player;

        private bool _locked;
        private bool _answerResolved;
        private bool _initializing = true;

        public ExerciseView(int lessonNumber, int userId, bool isDone)
        {
            InitializeComponent();

            _lessonNumber = lessonNumber;
            _userId = userId;
            _initialDoneParam = isDone;
            _baseDir = Path.Combine(AppContext.BaseDirectory, "Content", "Vezbe", _lessonNumber.ToString());
            _progressService = new ProgressService(new Data.eSufaraDbContext());

            txtTitle.Text = $"Vežba - lekcija {_lessonNumber}";
            btnFinish.IsEnabled = false;
            SetCompletedSilently(_initialDoneParam);
            _initializing = false;

            Unloaded += ExerciseView_Unloaded;

            if (!LoadDataOrWarn())
            {
                DisableAll();
                return;
            }

            SetupTimer();
            StartQuestion(0);
        }

        private bool LoadDataOrWarn()
        {
            if (!Directory.Exists(_baseDir))
            {
                MessageBox.Show("Sadržaj za ovu vežbu nije pronađen.", "Vežba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            foreach (var path in Directory.GetFiles(_baseDir, "*.txt"))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (!int.TryParse(name, out int n))
                    continue;

                try
                {
                    var answer = File.ReadAllText(path).Trim();
                    if (!string.IsNullOrWhiteSpace(answer))
                        _answers[n] = answer;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            var audioMap = Directory.GetFiles(_baseDir)
                .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .Select(f => new { Path = f, Name = Path.GetFileNameWithoutExtension(f) })
                .Where(x => int.TryParse(x.Name, out _))
                .GroupBy(x => int.Parse(x.Name))
                .ToDictionary(g => g.Key, g => g.First().Path);

            var candidates = audioMap.Keys
                .Intersect(_answers.Keys)
                .OrderBy(_ => _rng.Next())
                .Take(3)
                .ToList();

            if (_answers.Count < 3 || candidates.Count < 3)
            {
                MessageBox.Show("Ova vežba nema dovoljno validnih pitanja za pokretanje.", "Vežba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            _audioByIndex = audioMap;
            _questionOrder = candidates.OrderBy(_ => _rng.Next()).ToList();
            return true;
        }

        private void DisableAll()
        {
            btnOpt1.IsEnabled = btnOpt2.IsEnabled = btnOpt3.IsEnabled = false;
            btnReplay.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnFinish.IsEnabled = false;
            txtQuestionIndex.Text = "0/0";
            txtTimeLeft.Text = "-";
            timerBar.Value = 0;
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            _timer.Tick += async (s, e) =>
            {
                if (_answerResolved)
                    return;

                _timeLeftSec -= 0.1;
                if (_timeLeftSec < 0)
                    _timeLeftSec = 0;

                timerBar.Value = (_timeLeftSec / QuestionTimeSec) * 100.0;
                txtTimeLeft.Text = $"{_timeLeftSec:0.0}s";

                if (_timeLeftSec <= 0.0)
                {
                    _timer?.Stop();
                    await ResolveAnswerAsync(correct: false, clickedButton: null);
                }
            };
        }

        private void StartTimer()
        {
            _timeLeftSec = QuestionTimeSec;
            timerBar.Value = 100;
            txtTimeLeft.Text = $"{_timeLeftSec:0.0}s";
            _timer?.Start();
        }

        private void StopTimer() => _timer?.Stop();

        private void StartQuestion(int pos)
        {
            if (pos < 0 || pos >= _questionOrder.Count)
            {
                DisableAll();
                return;
            }

            _qPos = pos;
            _currentN = _questionOrder[_qPos];
            _answerResolved = false;

            txtQuestionIndex.Text = $"{_qPos + 1}/{_questionOrder.Count}";

            var optionKeys = _answers.Keys
                .Where(k => k != _currentN)
                .OrderBy(_ => _rng.Next())
                .Take(2)
                .Append(_currentN)
                .OrderBy(_ => _rng.Next())
                .ToList();

            AssignButton(btnOpt1, optionKeys[0]);
            AssignButton(btnOpt2, optionKeys[1]);
            AssignButton(btnOpt3, optionKeys[2]);

            btnNext.IsEnabled = false;
            btnFinish.IsEnabled = false;

            PlayCurrentAudio();
            StartTimer();
        }

        private void AssignButton(Button button, int key)
        {
            button.Content = _answers.TryGetValue(key, out var text) ? text : key.ToString();
            button.Tag = key;
            button.Background = Brushes.White;
            button.Foreground = Brushes.Black;
            button.IsEnabled = true;
        }

        private void PlayCurrentAudio()
        {
            if (_audioByIndex.TryGetValue(_currentN, out var path) && File.Exists(path))
            {
                _player ??= new System.Windows.Media.MediaPlayer();
                _player.Open(new Uri(path));
                _player.Volume = 1.0;
                _player.Play();
            }
        }

        private async void Option_Click(object sender, RoutedEventArgs e)
        {
            if (_locked || _answerResolved)
                return;

            if (sender is Button button && button.Tag is int chosen)
            {
                bool correct = chosen == _currentN;
                await ResolveAnswerAsync(correct, button);
            }
        }

        private async Task ResolveAnswerAsync(bool correct, Button? clickedButton)
        {
            if (_answerResolved)
                return;

            _answerResolved = true;
            _locked = true;
            StopTimer();
            btnOpt1.IsEnabled = btnOpt2.IsEnabled = btnOpt3.IsEnabled = false;

            var old = ExercisePage.Background;
            ExercisePage.Background = correct ? Brushes.LightGreen : Brushes.IndianRed;
            await Task.Delay(900);
            ExercisePage.Background = old;

            if (correct)
                _score++;

            txtScore.Text = _score.ToString();

            if (_qPos == _questionOrder.Count - 1)
                btnFinish.IsEnabled = true;
            else
                btnNext.IsEnabled = true;

            _locked = false;
        }

        private void btnReplay_Click(object sender, RoutedEventArgs e) => PlayCurrentAudio();

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_qPos < _questionOrder.Count - 1)
                StartQuestion(_qPos + 1);
        }

        private async void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            btnFinish.IsEnabled = false;

            bool passed = _score >= 2;
            try
            {
                await _progressService.SaveExerciseAttemptAsync(_userId, _lessonNumber, passed);

                if (passed)
                {
                    MessageBox.Show("Čestitamo! Uspešno ste završili vežbu.", "Bravo", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetCompletedSilently(true);
                }
                else
                {
                    MessageBox.Show("Nažalost, niste položili. Potrebna su najmanje 2 tačna odgovora. Pokušajte ponovo.", "Pokušaj ponovo", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Rezultat trenutno ne može da se sačuva.", "Vežba", MessageBoxButton.OK, MessageBoxImage.Warning);
                btnFinish.IsEnabled = true;
            }
        }

        private async void chkCompleted_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;

            chkCompleted.IsEnabled = false;
            try
            {
                await _progressService.MarkExerciseCompletedAsync(_userId, _lessonNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Vežba trenutno ne može da se označi kao završena.", "Vežba", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetCompletedSilently(false);
            }
            finally
            {
                chkCompleted.IsEnabled = true;
            }
        }

        private async void chkCompleted_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;

            var result = MessageBox.Show(
                "Da li želite da uklonite oznaku da je vežba završena?",
                "Potvrda",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                SetCompletedSilently(true);
                return;
            }

            chkCompleted.IsEnabled = false;
            try
            {
                await _progressService.UnmarkExerciseCompletedAsync(_userId, _lessonNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Oznaka vežbe trenutno ne može da se ukloni.", "Vežba", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetCompletedSilently(true);
            }
            finally
            {
                chkCompleted.IsEnabled = true;
            }
        }

        private void ExerciseView_Unloaded(object sender, RoutedEventArgs e)
        {
            try { _player?.Stop(); } catch { }
            _timer?.Stop();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new View.Pages.ExercisesView(_userId));
        }

        private void SetCompletedSilently(bool isDone)
        {
            _initializing = true;
            chkCompleted.IsChecked = isDone;
            _initializing = false;
        }
    }
}
