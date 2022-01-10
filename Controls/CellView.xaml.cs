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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace игра
{
   /// <summary>
   /// Логика взаимодействия для CellView.xaml
   /// </summary>
   public partial class CellView : UserControl
   {

      public static readonly DependencyProperty PieceProperty = DependencyProperty.Register("Piece", typeof(State), typeof(CellView));

      public State Piece
      {
         get => (State)GetValue(PieceProperty);
         set => SetValue(PieceProperty, value);
      }
      public CellView()
      {
         InitializeComponent();
      }
   }
}
