using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace игра
{
   public enum FigureType
   {
      White = 0, 
      Red,       
      Blue,      
      Green,     
      Yellow
   }
   public enum GameState
   {
      Process,
      Win,
      Lose
   }
   public class SGame : NotifyPropertyChanged
   {
      List<FigureType> _true = new List<FigureType>();
      List<FigureType> _view = new List<FigureType>();

      private int _size = 5;
      private int _cur = 0;
      private int _score = 0;

      private Random r = new Random();

      public List<FigureType> True
      {
         get => _true;
         set
         {
            _true = value;
            OnPropertyChanged();
         }
      }
      public int Score
      {
         get => _score;
         set
         {
            _score = value;
         }
      }
      public int Size
      {
         get => _size;
         set
         {
            _size = value;
         }
      }
      
      public void CreateTrue()
      {
         _cur = 0;
         _score = 0;

         _true.Add((FigureType)r.Next(0, 5));

         for (int i = 1; i < _size; i++)
         {
            FigureType temp;
            do { temp = (FigureType)r.Next(0, 5); } while (temp == _true[i - 1]);
            _true.Add(temp);
         }

      }
      public int Cheak(int selected)
      {
         FigureType selectedType = (FigureType)selected;
         FigureType trueType = _true[_cur];
         if(trueType != selectedType)
         {
            _score += 30;
            return 1;
         }
         else
         {
            _cur++;
            if (_cur == _size)
            {
               return 2;
            }
         }
         _score += 30;
         return 0;
      }


   }

   /// <summary>
   /// Логика взаимодействия для SecondGame.xaml
   /// </summary>
   public partial class SecondGame : Window
   {
      int endTime; // время на миниигру
      DispatcherTimer oneSecond;   // таймер секундомера

      SGame myGame = new SGame(); 
      public SecondGame()
      {
         InitializeComponent();
         endTime = 60;
      }

      private void GameOver()
      {
         oneSecond.Stop();
         MessageBox.Show("Вы проиграли! Вы теряете 1 ОЗ.", "Проигрыш", MessageBoxButton.OK, MessageBoxImage.Hand);
         int t = -1; // изменения для того, чтобы показать, что потеряно ОЗ
         ScoreLable.Content = t;
         this.Close();
      }
      // конец игры по счетчику времени
      private void EndGame()
      {
         oneSecond.Stop();
         MessageBox.Show("Вы выиграли! Ваш счет " + 150, "Победа", MessageBoxButton.OK, MessageBoxImage.Hand);
         this.Close();
      }

      private async void Button_Click(object sender, RoutedEventArgs e)
      {
         var selected = int.Parse((String)((Button)sender).Tag);
         int res = myGame.Cheak(selected);
         ScoreLable.Content = myGame.Score;
         if (res == 1)
            GameOver();
         else if (res == 2)
            EndGame();
         ShowColor((FigureType)selected);
         await Task.Delay(500);
         Answer.Visibility = Visibility.Hidden;
      }
      private async void StartGame(object sender, RoutedEventArgs e)
      {
         ((Button)sender).IsEnabled = false;

         myGame.CreateTrue();

         for(int i = 0; i < myGame.Size; i++)
         {
            ShowColor(myGame.True[i]);
            await Task.Delay(1000);
            Answer.Visibility = Visibility.Hidden;
            await Task.Delay(200);
         }

         oneSecond = new DispatcherTimer();
         oneSecond.Tick += new EventHandler(oneSecond_Tick);
         oneSecond.Interval = new TimeSpan(0, 0, 1);
         oneSecond.Start();

         ControlButtons.IsEnabled = true;
      }
      private void ShowColor(FigureType ft)
      {
         switch(ft)
         {
            case FigureType.Blue:
               Answer.Fill = Brushes.Blue;
               break;
            case FigureType.Green:
               Answer.Fill = Brushes.Green;
               break;
            case FigureType.Red:
               Answer.Fill = Brushes.Red;
               break;
            case FigureType.White:
               Answer.Fill = Brushes.White;
               break;
            case FigureType.Yellow:
               Answer.Fill = Brushes.Yellow;
               break;
         }
         Answer.Visibility = Visibility.Visible;
      }
      private void oneSecond_Tick(object sender, EventArgs e)
      {
         // счетчик времени игры
         endTime -= 1;
         if (endTime < 0)
            GameOver();
         TimerLable.Content = endTime;
      }
   }
}
