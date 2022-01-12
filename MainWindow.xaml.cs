using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace игра
{
   public class NotifyPropertyChanged : INotifyPropertyChanged
   {
      /*public event PropertyChangedEventHandler PropertyChanged;
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      */
      public void OnPropertyChanged([CallerMemberName] string prop = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
      public event PropertyChangedEventHandler PropertyChanged;
   }

   public class RelayCommand : ICommand
   {
      private readonly Action<object> _execute;
      private readonly Func<object, bool> _canExecute;

      public event EventHandler CanExecuteChanged
      {
         add { CommandManager.RequerySuggested += value; }
         remove { CommandManager.RequerySuggested -= value; }
      }

      public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
      {
         _execute = execute;
         _canExecute = canExecute;
      }

      public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
      public void Execute(object parameter) => _execute(parameter);
   }

   public enum State
   {
      UnknownCell = 0,  // неизвестная ячейка ?
      Empty,       // пусто
      ExtraMove,   // доп ход
      Death,       // минус жизнь
      FirstGame,   // первая игра
      SecondGame,  // вторая игра
      ThirdGame,    // третья игра
      EndGame
   }

   public class Cell : NotifyPropertyChanged
   {
      private State _state;
      private bool _active;
      private bool _noway = true;
      private int _col, _row;
      public int Row
      {
         get => _row;
         set
         {
            _row = value;
         }
      }
      public int Col
      {
         get => _col;
         set
         {
            _col = value;
         }
      }

      public State State
      {
         get => _state;
         set
         {
            _state = value;
            OnPropertyChanged(); // сообщить интерфейсу, что значение поменялось, чтобы интефейс перерисовался в этом месте
         }
      }
      public bool Active // это будет показывать, что игрок тут
      {
         get => _active;
         set
         {
            _active = value;
            OnPropertyChanged();
         }
      }
      public bool Noway // это будет показывать, что к клетке еще нет пути
      {
         get => _noway;
         set
         {
            _noway = value;
         }
      }

      public Cell(int i, int j)
      {
         _row = i;
         _col = j;
         if (i == 0 && j == 0)
         {
            _state = State.Empty;
            _active = true;
            _noway = false;
         }
         else if (i + j == 1)
         {
            _state = State.UnknownCell;
            _noway = false;
         }
         else if (i == 9 && j == 9)
         {
            _state = State.EndGame;
         }
         else
            _state = State.UnknownCell;
      }
   }

   public class Board : IEnumerable<Cell>
   {
      private readonly Cell[,] _area;
      
      public State this[int row, int column]
      {
         get => _area[row, column].State;
         set => _area[row, column].State = value;
      }

      public Board()
      {
         _area = new Cell[10, 10];
         for (int i = 0; i < _area.GetLength(0); i++)
            for (int j = 0; j < _area.GetLength(1); j++)
            {
               _area[i, j] = new Cell(i, j);
            }
      }

      public IEnumerator<Cell> GetEnumerator()
          => _area.Cast<Cell>().GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator()
          => _area.GetEnumerator();
   }

   public class MainViewModel : NotifyPropertyChanged
   {
      private Board _board = new Board();
      private ICommand _clearCommand;
      private ICommand _cellCommand;
      private ICommand _throwdiceCommand;

      private Random r = new Random();
      private int _step = 0;
      private int _lives = 3;
      private int _score = 0;
      
      public int Score
      {
         get => _score;
         set
         {
            _score = value;
            OnPropertyChanged();
         }
      }
      public int Lives
      {
         get => _lives;
         set
         {
            _lives = value;
            OnPropertyChanged();
         }
      }
      public int Step
      {
         get => _step;
         set
         {
            _step = value;
            OnPropertyChanged();
            
         }
      }
      public Board Board
      {
         get => _board;
         set
         {
            _board = value;
            OnPropertyChanged();
         }
      }

      public ICommand ClearCommand => _clearCommand ??= new RelayCommand(parameter =>
      {
         Board = new Board();
         Step = 0;
         Lives = 3;
         SetupBoard();
      });
      public ICommand ThrowDice => _throwdiceCommand ??= new RelayCommand(parameter =>
      {
         
         if (_step == 0)
         {
            Step = r.Next(1, 7);
         }
         else
            MessageBox.Show("Сначала потратьте имеющиеся ходы");
      });

      private void LookAround(Cell cell)
      {
         Cell openCell;
         int ii, jj;
         ii = cell.Row + 1;
         jj = cell.Col;
         if (ii < 10)
         {
            if (_board[ii, jj] == State.UnknownCell)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               _board[ii, jj] = (State)r.Next(1, 7);
               openCell.State = _board[ii, jj];
               openCell.Noway = false;
            }
            else if (_board[ii, jj] == State.EndGame)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               openCell.Noway = false;
            }
         }
         ii = cell.Row - 1;
         jj = cell.Col;
         if (ii > -1)
         {
            if (_board[ii, jj] == State.UnknownCell)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               _board[ii, jj] = (State)r.Next(1, 7);
               openCell.State = _board[ii, jj];
               openCell.Noway = false;
            }
            else if (_board[ii, jj] == State.EndGame)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               openCell.Noway = false;
            }
         }
         ii = cell.Row;
         jj = cell.Col + 1;
         if (jj < 10)
         {
            if (_board[ii, jj] == State.UnknownCell)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               _board[ii, jj] = (State)r.Next(1, 7);
               openCell.State = _board[ii, jj];
               openCell.Noway = false;
            }
            else if (_board[ii, jj] == State.EndGame)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               openCell.Noway = false;
            }
         }
         ii = cell.Row;
         jj = cell.Col - 1;
         if (jj > -1)
         {
            if (_board[ii, jj] == State.UnknownCell)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               _board[ii, jj] = (State)r.Next(1, 7);
               openCell.State = _board[ii, jj];
               openCell.Noway = false;
            }
            else if (_board[ii, jj] == State.EndGame)
            {
               openCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               openCell.Noway = false;
            }
         }
      }
      public ICommand CellCommand => _cellCommand ??= new RelayCommand(async parameter =>
      {
         Cell cell = (Cell)parameter;
         Cell gamerCell = Board.FirstOrDefault(x => x.Active);
         int distance = Math.Abs(cell.Row - gamerCell.Row) + Math.Abs(cell.Col - gamerCell.Col);
         if (distance > _step)
         {
            // нельзя так далеко
            MessageBox.Show("Слишком далеко!", "Не хватает ходов", MessageBoxButton.OK, MessageBoxImage.Hand);
         }
         else
         {
            int ii, jj;
            ii = gamerCell.Row;
            while (cell.Col != gamerCell.Col)
            {
               jj = gamerCell.Col + Math.Sign(cell.Col - gamerCell.Col);
               gamerCell.Active = false;
               gamerCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               gamerCell.Active = true;
               LookAround(gamerCell);
               Step -= 1;
               await Task.Delay(500);
            }
            jj = gamerCell.Col;
            while (cell.Row != gamerCell.Row)
            {
               ii = gamerCell.Row + Math.Sign(cell.Row - gamerCell.Row);
               gamerCell.Active = false;
               gamerCell = Board.FirstOrDefault(x => x.Row == ii && x.Col == jj);
               gamerCell.Active = true;
               LookAround(gamerCell);
               Step -= 1;
               await Task.Delay(500);
            }

            if (cell.State == State.EndGame)
            {
               MessageBox.Show("Вы выиграли! Ваш счет: " + _score, "Вы выиграли!", MessageBoxButton.OK, MessageBoxImage.Exclamation);

               Board = new Board();
               Step = 0;
               Lives = 3;
               SetupBoard();
               return;
            }
            else if (0 == _step)
            {
               // выбор игры
               int t = 0;
               switch (cell.State)
               {
                  case State.ExtraMove:
                     Step += 3;
                     t = 300;
                     MessageBox.Show("Бонус! +3 хода, +300 очков", "Бонус", MessageBoxButton.OK, MessageBoxImage.Hand);
                     break;
                  case State.Death:
                     t = -1;
                     MessageBox.Show("Ловушка! -1 жизнь", "Ловушка", MessageBoxButton.OK, MessageBoxImage.Hand);
                     break;
                  case State.FirstGame:
                     FirstGame firstWindow = new FirstGame();
                     firstWindow.ShowDialog();
                     try
                     {
                        t = (int)firstWindow.ScoreLable.Content;
                     }
                     catch
                     {
                        MessageBox.Show("Вы не завершили мини-игру.", "Мини-игра не окончена", MessageBoxButton.OK, MessageBoxImage.Error);
                     }
                     break;
                  case State.SecondGame:
                     SecondGame secondWindow = new SecondGame();
                     secondWindow.ShowDialog();
                     try
                     {
                        t = (int)secondWindow.ScoreLable.Content;
                     }
                     catch
                     {
                        MessageBox.Show("Вы не завершили мини-игру.", "Мини-игра не окончена", MessageBoxButton.OK, MessageBoxImage.Error);
                     }
                     break;
                  case State.ThirdGame:
                     ThirdGame thirdWindow = new ThirdGame();
                     thirdWindow.ShowDialog();
                     try
                     {
                        t = (int)thirdWindow.ScoreLable.Content;
                     }
                     catch
                     {
                        MessageBox.Show("Вы не завершили мини-игру.", "Мини-игра не окончена", MessageBoxButton.OK, MessageBoxImage.Error);
                     }
                     break;
                  default:
                     break;
               }
               if (t < 0)
               {
                  Lives -= 1;
                  if (_lives < 1)
                  {
                     MessageBox.Show("Игра окончена. Ваш счет: " + _score, "Конец игры", MessageBoxButton.OK, MessageBoxImage.Information);

                     Board = new Board();
                     Step = 0;
                     Lives = 3;
                     SetupBoard();
                     return;
                  }
               }
               else
               {
                  Score += t;
               }
               cell.State = State.Empty;
            }
           
            
         }
      }, parameter => parameter is Cell cell && cell.Noway == false && cell.Active == false);

      private void SetupBoard()
      {
         Board board = new Board();
         for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
            {
               board[i, j] = State.UnknownCell;
            }
         board[0, 0] = State.Empty;
         
         board[0, 1] = (State)r.Next(1, 7);
         board[1, 0] = (State)r.Next(1, 7);

         board[9, 9] = State.EndGame;
         Board = board;
      }

      public MainViewModel()
      {
         SetupBoard();
         Step = 0;
         Lives = 3;
         //OnPropertyChanged("Lives");
         //OnPropertyChanged("Step");
      }
   }

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   ///

   public partial class MainWindow : Window
   {
      MainViewModel vm;
      private Random r = new Random();
      public MainWindow()
      {
         InitializeComponent();
         vm = new MainViewModel();
         DataContext = vm;
      }

      
      private void ThrowDice(object sender, RoutedEventArgs e)
      {
         int s = vm.Step;
      }
   }
}
