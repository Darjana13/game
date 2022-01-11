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
   public enum FigureType
   {
      White = 0, 
      Red,       
      Blue,      
      Green,     
      Yellow
   }
   public class SecondViewModel : NotifyPropertyChanged
   {
      List<FigureType> _true = new List<FigureType>();
      private int size = 6;
      private ICommand _clickCommand;
      private ICommand _createCommand;

      private Random r = new Random();

      public ICommand ClickCommand => _clickCommand ??= new RelayCommand(parameter =>
      {
         
      });
      public ICommand CreateCommand => _createCommand ??= new RelayCommand(parameter =>
      {
         for(int i = 0; i < size; i++)
         {
            _true.Add((FigureType)r.Next(0, 6));
         }
      });
   }

   /// <summary>
   /// Логика взаимодействия для SecondGame.xaml
   /// </summary>
   public partial class SecondGame : Window
   {
      public SecondGame()
      {
         InitializeComponent();
      }
   }
}
