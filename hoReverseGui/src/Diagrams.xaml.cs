using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using hoReverse.HistoryList;

namespace WpfDiagram
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class Diagram : Window
    {
        private readonly EA.Repository _repository;
        private readonly EaHistoryList _history;
        private readonly EaHistoryList _bookmark;

        
        public Diagram(EA.Repository rep, EaHistoryList history, EaHistoryList bookmark)
        {
            _repository = rep;
            _history = history;
            _bookmark = bookmark;
            InitializeComponent();

                          
            this.listBoxDiagrams.ItemsSource = _history.GetAll();
            this.listBoxBookmarks.ItemsSource = _bookmark.GetAll();

        }

        private void ShowInBrowser(EaHistoryListEntry entry)
        {
            string guid = entry.Guid;
            
            switch (entry.EaObjectTypeName) {
                case "Diagram":
                    EA.Diagram dia = (EA.Diagram)_repository.GetDiagramByGuid(guid);
                    _repository.ShowInProjectView(dia);
                    break;
                case "Element":
                    EA.Element el = (EA.Element)_repository.GetDiagramByGuid(guid);
                    _repository.ShowInProjectView(el);
                    break;
                case "Package":
                    EA.Package pkg = _repository.GetPackageByGuid(guid);
                    _repository.ShowInProjectView(pkg);
                    break;
            }
            
        }
        private void ShowDiagram(EaHistoryListEntry entry)
        {
            string guid = entry.Guid;

            switch (entry.EaObjectTypeName)
            {
                case "Diagram":
                    EA.Diagram dia = (EA.Diagram)_repository.GetDiagramByGuid(guid);
                    _repository.OpenDiagram(dia.DiagramID);
                    break;
                case "Element":
                    EA.Element el = _repository.GetElementByGuid(guid);
                    _repository.ShowInProjectView(el);
                    break;
                case "Package":
                    EA.Package pkg = _repository.GetPackageByGuid(guid);
                    _repository.ShowInProjectView(pkg);
                    break;
            }

        }
        private void listBoxDiagrams_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // get the GUID
            if (listBoxDiagrams.SelectedItem != null)
            {
                ShowDiagram(listBoxDiagrams.SelectedItem as EaHistoryListEntry);
  
            }
        }

        private void listBoxBookmarks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listBoxBookmarks.SelectedItem != null)
            {
                // get the GUID
                ShowDiagram(listBoxBookmarks.SelectedItem as EaHistoryListEntry);
            }
        }

        private void DiagramsMenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDiagrams.SelectedItem != null)
            {
                ShowDiagram(listBoxDiagrams.SelectedItem as EaHistoryListEntry); 
         
            }
        }
        private void DiagramsMenuItemGoto_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDiagrams.SelectedItem != null)
            {
                ShowInBrowser(listBoxDiagrams.SelectedItem as EaHistoryListEntry);
              
            }

        }
        
        private void BookmarksMenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxBookmarks.SelectedItem != null)
            {
                ShowDiagram(listBoxBookmarks.SelectedItem as EaHistoryListEntry); 
                
            }
        }
        private void BookmarksMenuItemGoto_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxBookmarks.SelectedItem != null)
            {
                ShowInBrowser(listBoxBookmarks.SelectedItem as EaHistoryListEntry);
            }

        }

        public void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e){
            e.CanExecute = true;
        }
        private void CloseExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            // save all bookmarks
            this.Save();
        }
        private void Save()
        {
            if (_bookmark != null)
            {
                _bookmark.SetHistory("bookmark", _repository.ProjectGUID, _bookmark.GetAll());
                _history.SetHistory("history", _repository.ProjectGUID, _history.GetAll());
            }
        }
        private void listBoxBookmarks_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //DataRowView rowView = (e.Row.Item as DataRowView);
            //DataRow dataRow = rowView.Row;
            //dataRow.
            //if (listBoxBookmarks.SelectedItem != null)
            //{
            //    // set the history
            //    EaHistoryListEntry entry = listBoxBookmarks.SelectedItem as EaHistoryListEntry;
            //    string bookmarkName = entry.bookmarkName;
            //    m_bookmark.setHistory("bookmark", m_repository.ProjectGUID, m_bookmark.getAll());
                
            //}
        }

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void SaveExecute( object sender, ExecutedRoutedEventArgs e ) 
        
        {
           
            this.Save();
        }
        private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            
        }
        private void NewExecute(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Save();
        }

    }
}
