using System;
using System.Collections.Generic;
using System.Text;
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
   /// <summary>
   /// Логика взаимодействия для ThirdGame.xaml
   /// </summary>
   public partial class ThirdGame : Window
   {
      private List<Point> bonusPoints = new List<Point>(); // бонусные объекты 
      private List<Point> dangerPoints = new List<Point>(); // бонусные объекты 
      //private List<Point> gamerPoints = new List<Point>(); // тело змеи
      int endTime = 60; // время на миниигру
      private Brush bonusColor = Brushes.Blue; 
      private Brush dangerColor = Brushes.Red; 
      private Brush gamerColor = Brushes.Green;

      private enum MOVINGDIRECTION // направление движения
      {
         TOLEFT = 4,
         TORIGHT = 6
      };

      private Point currentPosition = new Point(); 

      private int direction = 0; // направление (0 - нет движения)

      private int gamerSize = 30;
      private int bonusSize = 20;
      private int dangerSize = 40;

      private bool play_flag = false; // начало игры
      private int dangerCount = 7;
      private int bonusCount = 2;

      private int score = 0;  // очки
      private Random rnd = new Random();

      DispatcherTimer canvasTimer; // таймер перерисовки
      DispatcherTimer oneSecond;   // таймер секундомера

      public ThirdGame()
      {
         InitializeComponent();
         Restart(); // обнуление данных

         this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
         this.KeyUp += new KeyEventHandler(OnButtonKeyUp);


         for (int n = 0; n < bonusCount; n++)
         {
            paintBonus(n);
         }
         for (int n = 0; n < dangerCount; n++)
         {
            paintDanger(n + bonusCount);
         }

         currentPosition = new Point(100, 350);
         paintGamer(currentPosition);
      }

      private void Restart()
      {
         score = 0;
         endTime = 60;
         direction = 0;
      }

      private void paintGamer(Point currentposition)
      {

         Rectangle gamer = new Rectangle();
         gamer.Fill = gamerColor;
         gamer.Width = gamerSize;
         gamer.Height = gamerSize;

         Canvas.SetTop(gamer, currentposition.Y);
         Canvas.SetLeft(gamer, currentposition.X);

         if(paintCanvas.Children.Count == bonusCount + dangerCount + 1)
             paintCanvas.Children.RemoveAt(bonusCount + dangerCount);
         paintCanvas.Children.Add(gamer);
      }

      private void paintBonus(int index)
      {
         Point bonusPoint = new Point(rnd.Next(5, 445 - bonusSize), rnd.Next(-200, 0));

         Ellipse newEllipse = new Ellipse();
         newEllipse.Fill = bonusColor;
         newEllipse.Width = bonusSize;
         newEllipse.Height = bonusSize;

         Canvas.SetTop(newEllipse, bonusPoint.Y);
         Canvas.SetLeft(newEllipse, bonusPoint.X);
         paintCanvas.Children.Insert(index, newEllipse);
         bonusPoints.Insert(index, bonusPoint);
      }
      private void rePaintBonus(int index)
      {
         Point bonusPoint = bonusPoints[index];
         bonusPoint.Y += 1;

         bonusPoints.RemoveAt(index);
         paintCanvas.Children.RemoveAt(index);

         Ellipse newEllipse = new Ellipse();
         newEllipse.Fill = bonusColor;
         newEllipse.Width = bonusSize;
         newEllipse.Height = bonusSize;

         Canvas.SetTop(newEllipse, bonusPoint.Y);
         Canvas.SetLeft(newEllipse, bonusPoint.X);
         paintCanvas.Children.Insert(index, newEllipse);
         bonusPoints.Insert(index, bonusPoint);
      }

      private void rePaintDanger(int index)
      {
         Point dangerPoint = dangerPoints[index - bonusCount];
         dangerPoint.Y += 1;

         dangerPoints.RemoveAt(index - bonusCount);
         paintCanvas.Children.RemoveAt(index);

         Rectangle danger = new Rectangle();
         danger.Fill = dangerColor;
         danger.Width = dangerSize;
         danger.Height = dangerSize - 10;
         danger.Stroke = Brushes.Black;

         Canvas.SetTop(danger, dangerPoint.Y);
         Canvas.SetLeft(danger, dangerPoint.X);
         paintCanvas.Children.Insert(index, danger);
         dangerPoints.Insert(index - bonusCount, dangerPoint);
      }

      private void paintDanger(int index)
      {
         Point dangerPoint = new Point(rnd.Next(5, 445 - dangerSize), rnd.Next(-200, 0));
         Rectangle danger = new Rectangle();
         danger.Fill = dangerColor;
         danger.Width = dangerSize;
         danger.Height = dangerSize - 10;

         Canvas.SetTop(danger, dangerPoint.Y);
         Canvas.SetLeft(danger, dangerPoint.X);
         paintCanvas.Children.Insert(index, danger);
         dangerPoints.Insert(index - bonusCount, dangerPoint);
      }

      // отрисовка кадра
      private void timer_Tick(object sender, EventArgs e)
      {
         switch (direction)
         {
            case (int)MOVINGDIRECTION.TOLEFT:
               if (currentPosition.X - 5 > 0)
               {
                  currentPosition.X -= 5;
                  paintGamer(currentPosition);
               }
               break;
            case (int)MOVINGDIRECTION.TORIGHT:
               if (currentPosition.X + 5 < 450 - gamerSize)
               {
                  currentPosition.X += 5;
                  paintGamer(currentPosition);
               }
               break;
            default:
               paintGamer(currentPosition);
               break;
         }

         // если съедена еда
         int n = 0;
         //foreach (Point point in bonusPoints)
         for(n = 0; n < bonusCount; n++)
         {
            Point point = bonusPoints[n];
            if ((Math.Abs(point.X - currentPosition.X) < gamerSize) &&
                (Math.Abs(point.Y - currentPosition.Y) < bonusSize))
            {
               score += 20;
               ScoreLable.Content = score;

               // убрать съеденный объект
               bonusPoints.RemoveAt(n);
               paintCanvas.Children.RemoveAt(n);
               paintBonus(n);
            }
            else if (380 + gamerSize - point.Y < 0)
            {
               // убрать съеденный объект
               bonusPoints.RemoveAt(n);
               paintCanvas.Children.RemoveAt(n);
               paintBonus(n);
               break;
            }
            else
            {
               rePaintBonus(n);
            }
         }
         //foreach (Point point in dangerPoints)
         for (n = bonusCount; n < dangerCount + bonusCount; n++)
         {
            Point point = dangerPoints[n - bonusCount];

            if (((point.X - currentPosition.X > 0 && Math.Abs(point.X - currentPosition.X) < gamerSize)
             || (point.X - currentPosition.X < 0 && Math.Abs(point.X - currentPosition.X) < dangerSize)) &&
                (Math.Abs(point.Y - currentPosition.Y) < dangerSize - 10))
            {
               score += 20;
               ScoreLable.Content = score;
               GameOver();
               break;
            }
            else if (380 + gamerSize - point.Y < 0)
            { 
               // убрать съеденный объект
               dangerPoints.RemoveAt(n - bonusCount);
               paintCanvas.Children.RemoveAt(n);
               paintDanger(n);
               break;
            }
            else 
            { 
               rePaintDanger(n); 
            }
         }
      }

      private void OnButtonKeyUp(object sender, KeyEventArgs e)
      {
         direction = 0;
      }
      private void OnButtonKeyDown(object sender, KeyEventArgs e)
      {
         // запустить таймер, если началась игра
         if (!play_flag)
         {
            oneSecond = new DispatcherTimer();
            oneSecond.Tick += new EventHandler(oneSecond_Tick);
            oneSecond.Interval = new TimeSpan(0, 0, 1);
            oneSecond.Start();

            canvasTimer = new DispatcherTimer();
            canvasTimer.Tick += new EventHandler(timer_Tick);
            canvasTimer.Interval = new TimeSpan(50000);
            canvasTimer.Start();

            play_flag = true;
         }
         // менять направление
         switch (e.Key)
         {
            case Key.Left:
                  direction = (int)MOVINGDIRECTION.TOLEFT;
               break;
            case Key.Right:
                  direction = (int)MOVINGDIRECTION.TORIGHT;
               break;
         }
      }

      // проигрыш
      private void GameOver()
      {
         // остановка таймеров
         canvasTimer.Stop();
         oneSecond.Stop();
         MessageBox.Show("Вы проиграли! Вы теряете 1 ОЗ.", "Проигрыш", MessageBoxButton.OK, MessageBoxImage.Hand);
         int t = -1; // изменения для того, чтобы показать, что потеряно ОЗ
         ScoreLable.Content = t;
         this.Close();
      }
      // конец игры по счетчику времени
      private void EndGame()
      {
         canvasTimer.Stop();
         oneSecond.Stop();
         MessageBox.Show("Время вышло! Ваш счет " + score.ToString(), "Время вышло", MessageBoxButton.OK, MessageBoxImage.Hand);
         this.Close();
      }
      private void oneSecond_Tick(object sender, EventArgs e)
      {
         // счетчик времени игры
         endTime -= 1;
         if (endTime < 0)
            EndGame();
         TimerLable.Content = endTime;
      }
   }
}
