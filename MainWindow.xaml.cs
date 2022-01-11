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
      public bool Noway // это будет показывать, что игрок тут
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
         _step = 0;
         _lives = 3;
         OnPropertyChanged("Lives");
         OnPropertyChanged("Step");
         SetupBoard();
      });
      public ICommand ThrowDice => _throwdiceCommand ??= new RelayCommand(parameter =>
      {
         // <Setter Property="Content" Value="<Image Source="Resources/1.jpg" Width="60" Height="60"></Image>"/>
         
         if (_step == 0)
         {
            _step = r.Next(1, 7);
            OnPropertyChanged("Step");
         }
         else
            MessageBox.Show("Сначала потратьте имеющиеся ходы");
      });

      public ICommand CellCommand => _cellCommand ??= new RelayCommand(parameter =>
      {
         Cell cell = (Cell)parameter;
         Cell gamerCell = Board.FirstOrDefault(x => x.Active);
         Cell openCell;
         int distance = Math.Abs(cell.Row - gamerCell.Row) + Math.Abs(cell.Col - gamerCell.Col);
         if (distance > _step)
         {
            // нельзя так далеко
            MessageBox.Show("Слишком далеко!");
         }
         else
         {

            if (cell.State == State.EndGame)
            {
               MessageBox.Show("Вы выиграли! Ваш счет: " + _score);
               Board = new Board();
               _step = 0;
               _lives = 3;
               OnPropertyChanged("Lives");
               OnPropertyChanged("Step");
               SetupBoard();
               return;
            }

            else if (distance == _step)
            {
               //selectGame
               int t = 0;
               switch (cell.State)
               {
                  case State.ExtraMove:
                     _step += 3;
                     OnPropertyChanged("Step");
                     t = 300;
                     break;
                  case State.Death:
                     t = -1;
                     break;
                  case State.FirstGame:
                  case State.SecondGame:
                  case State.ThirdGame:
                     FirstGame firstWindow = new FirstGame();
                     firstWindow.ShowDialog();
                     try
                     {
                        t = (int)firstWindow.ScoreLable.Content;
                     }
                     catch
                     {
                        MessageBox.Show("Вы не завершили мини-игру.");
                     }
                     break;
                  default:
                     break;
               }
               if (t < 0)
               {
                  _lives -= 1;
                  if (_lives < 1)
                  {
                     MessageBox.Show("Игра окончена. Ваш счет: " + _score);
                     Board = new Board();
                     _step = 0;
                     _lives = 3;
                     OnPropertyChanged("Lives");
                     OnPropertyChanged("Step");
                     SetupBoard();
                     return;
                  }
                  OnPropertyChanged("Lives");
               }
               else
               {
                  _score += t;
                  OnPropertyChanged("Score");
               }
               cell.State = State.Empty;
            }
 
            
            cell.Active = true;
            gamerCell.Active = false;

            //--------------
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
            //-------------

            
            _step -= distance;
            OnPropertyChanged("Step");
         }
         /* if (cell.State != State.Gamer)
         {
            if (!cell.Active && activeCell != null)
               activeCell.Active = false;
            cell.Active = !cell.Active;
         }
         else if (activeCell != null)
         {
            activeCell.Active = false;
            cell.State = activeCell.State;
            activeCell.State = State.Empty;
         }*/
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
         _step = 0;
         _lives = 3;
         OnPropertyChanged("Lives");
         OnPropertyChanged("Step");
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
