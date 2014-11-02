namespace VisualizerService
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public partial class LineViewer : UserControl
    {
        private ViewModel viewModel;

        public LineViewer()
        {
            InitializeComponent();
        }

        public void Init( IWatchObjectsSource source )
        {
            viewModel = new ViewModel(source);
            this.DataContext = viewModel;

            viewport.Children.Add( viewModel._lineModel );
        }

        private void OnObjectVisible( object sender, RoutedEventArgs e )
        {
            viewModel.SetVisibility( ( sender as CheckBox ).Content.ToString(), true );
        }

        private void OnObjectInvisible( object sender, RoutedEventArgs e )
        {
            viewModel.SetVisibility( ( sender as CheckBox ).Content.ToString(), false );
        }

        private void ReloadAllClick( object sender, RoutedEventArgs e )
        {
            viewModel.ReloadModels();
        }

        private void DeleteAllClick( object sender, RoutedEventArgs e )
        {
            viewModel.RemoveAllModels();
        }
    }
}
