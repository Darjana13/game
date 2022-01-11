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

namespace игра
{
   using System.Windows.Threading;

   public partial class FirstGame : Window
   {
      private List<Point> bonusPoints = new List<Point>(); // бонусные объекты 
      private List<Point> snakePoints = new List<Point>(); // тело змеи
      int endTime = 60; // время на миниигру
      private Brush snakeColor = Brushes.Green; // цвет змеи
      private enum MOVINGDIRECTION // направление движения
      {
         UPWARDS = 8,
         DOWNWARDS = 2,
         TOLEFT = 4,
         TORIGHT = 6
      };

      private Point startingPoint = new Point(100, 100); // точка начала
      private Point currentPosition = new Point();       // координаты головы

      private int direction = 0; // направление (0 - нет движения)
      private int previousDirection = 0; // чтобы не уехать в саму змею

      private int headSize = 10; // толщина земли
      private bool play_flag = false; // начало игры
      private int length = 7; // длина (кол-во кругов радиуса headSize в теле змеи)
      private int score = 0;  // очки
      private Random rnd = new Random();

      DispatcherTimer canvasTimer; // таймер перерисовки
      DispatcherTimer oneSecond;   // таймер секундомера

      public FirstGame()
      {
         InitializeComponent();
         Restart(); // обнуление данных
         
         this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
         
         for (int n = 0; n < 10; n++)
         {
            paintBonus(n); // нарисовать бонусы
         }

         startingPoint = new Point(100, 100);
         paintSnake(startingPoint); // нарисовать змею
         currentPosition = startingPoint;
      }

      private void Restart()
      {
         length = 20;
         score = 0;
         endTime = 60;
         direction = 0;

         canvasTimer = new DispatcherTimer();
         canvasTimer.Tick += new EventHandler(timer_Tick);
         canvasTimer.Interval = new TimeSpan(10000);
         canvasTimer.Start();

      }

      private void paintSnake(Point currentposition)
      {
         Ellipse newEllipse = new Ellipse();
         newEllipse.Fill = snakeColor;
         newEllipse.Width = headSize;
         newEllipse.Height = headSize;

         Canvas.SetTop(newEllipse, currentposition.Y);
         Canvas.SetLeft(newEllipse, currentposition.X);

         int count = paintCanvas.Children.Count - 10; // кол-во точек без учета бонусных объектов
         paintCanvas.Children.Add(newEllipse);
         snakePoints.Add(currentposition);

         // удалить последний элемент змеи, если уже достигнута длина змеи
         if (count > length)
         {
            paintCanvas.Children.RemoveAt(10);
            snakePoints.RemoveAt(0);
         }
      }

      private void paintBonus(int index)
      {
         Point bonusPoint = new Point(rnd.Next(5, 445 - headSize), rnd.Next(5, 395 - headSize));

         Ellipse newEllipse = new Ellipse();
         newEllipse.Fill = Brushes.Red;
         newEllipse.Width = headSize;
         newEllipse.Height = headSize;

         Canvas.SetTop(newEllipse, bonusPoint.Y);
         Canvas.SetLeft(newEllipse, bonusPoint.X);
         paintCanvas.Children.Insert(index, newEllipse);
         bonusPoints.Insert(index, bonusPoint);
      }

      // отрисовка кадра
      private void timer_Tick(object sender, EventArgs e)
      {
         
         switch (direction)
         {
            case (int)MOVINGDIRECTION.DOWNWARDS:
               currentPosition.Y += headSize / 4;
               paintSnake(currentPosition);
               break;
            case (int)MOVINGDIRECTION.UPWARDS:
               currentPosition.Y -= headSize / 4;
               paintSnake(currentPosition);
               break;
            case (int)MOVINGDIRECTION.TOLEFT:
               currentPosition.X -= headSize / 4;
               paintSnake(currentPosition);
               break;
            case (int)MOVINGDIRECTION.TORIGHT:
               currentPosition.X += headSize / 4;
               paintSnake(currentPosition);
               break;
         }

         // если граница области
         if ((currentPosition.X < 0) || (currentPosition.X > 450 - headSize + 1) ||
             (currentPosition.Y < 0) || (currentPosition.Y > 400 - headSize + 1))
            GameOver();

         // если съедена еда
         int n = 0;
         foreach (Point point in bonusPoints)
         {
            if ((Math.Abs(point.X - currentPosition.X) < headSize) &&
                (Math.Abs(point.Y - currentPosition.Y) < headSize))
            {
               // увеличить длину и очки
               length += 10;
               score += 10;
               ScoreLable.Content = score;

               // убрать съеденный объект
               bonusPoints.RemoveAt(n);
               paintCanvas.Children.RemoveAt(n);
               paintBonus(n);
               break;
            }
            n++;
         }

         // проверить пересечение с головой
         for (int q = 0; q < (snakePoints.Count - headSize * 2); q++)
         {
            Point point = new Point(snakePoints[q].X, snakePoints[q].Y);
            if ((Math.Abs(point.X - currentPosition.X) < (headSize)) &&
                 (Math.Abs(point.Y - currentPosition.Y) < (headSize)))
            {
               GameOver();
               break;
            }
         }
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
            play_flag = true;
         }
         // менять направление, если оно не прямо противоположно текущему
         switch (e.Key)
         {
            case Key.Down:
               if (previousDirection != (int)MOVINGDIRECTION.UPWARDS)
                  direction = (int)MOVINGDIRECTION.DOWNWARDS;
               break;
            case Key.Up:
               if (previousDirection != (int)MOVINGDIRECTION.DOWNWARDS)
                  direction = (int)MOVINGDIRECTION.UPWARDS;
               break;
            case Key.Left:
               if (previousDirection != (int)MOVINGDIRECTION.TORIGHT)
                  direction = (int)MOVINGDIRECTION.TOLEFT;
               break;
            case Key.Right:
               if (previousDirection != (int)MOVINGDIRECTION.TOLEFT)
                  direction = (int)MOVINGDIRECTION.TORIGHT;
               break;

         }
         previousDirection = direction;
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
