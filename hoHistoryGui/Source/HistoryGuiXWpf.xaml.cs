using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using hoReverse.HistoryList;

namespace hoReverse.History
{
    /// <summary>
    /// Interactions logic for UserControlWpf.xaml
    /// </summary>
    public partial class HistoryActiveXWpf : UserControl
    {
        private EA.Repository _repository;
        private EaHistoryList _historyList;
              
        public HistoryActiveXWpf()
        {
            InitializeComponent();
        }
        public void SetRepository(EA.Repository rep)
        {
            _repository = rep;
        }
        public void SetHistory(EaHistoryList history)
        {
            _historyList = history;
            if (history is EaHistory)
            {
                // disable bookmark name and type
                this.listBoxDiagrams.Columns[1].Visibility = Visibility.Collapsed;
                // disable add/remove bookmark buttons
                this.btnBookmarkAdd.Visibility = Visibility.Collapsed;
                this.btnBookmarkRemove.Visibility = Visibility.Collapsed;
                

            }
            this.listBoxDiagrams.Columns[2].Visibility = Visibility.Collapsed;
            this.listBoxDiagrams.Columns[3].Visibility = Visibility.Collapsed;

        }
        public void Show()
        {
            this.listBoxDiagrams.ItemsSource = _historyList.GetAll();
        }


        private void ShowInBrowser(EaHistoryListEntry entry)
        {
            string guid = entry.Guid;

            switch (entry.EaObjectTypeName)
            {
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
        private void DiagramsMenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDiagrams.SelectedItem != null)
            {
                ShowDiagram(listBoxDiagrams.SelectedItem as EaHistoryListEntry); 

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


        private void DiagramsMenuItemGoto_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDiagrams.SelectedItem != null)
            {
                ShowInBrowser(listBoxDiagrams.SelectedItem as EaHistoryListEntry);

            }

        }


        public void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // save all bookmarks
            this.Save();
        }
        private void Save()
        {
            _historyList?.SetHistory("history", _repository.ProjectGUID, _historyList.GetAll());
        }


        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
           e.CanExecute = true;
        }
        private void SaveExecute(object sender, ExecutedRoutedEventArgs e)
        {
           this.Save();
        }
        private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            
        }
        private void NewExecute(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        //------- Bookmark ----------------------------//

        
        void btnBookmarkAdd_Click(object sender, EventArgs e)
        {
            EA.ObjectType ot = _repository.GetContextItemType();
            string guid;
            switch (ot)
            {
                case EA.ObjectType.otDiagram:
                    EA.Diagram dia = (EA.Diagram)_repository.GetContextObject();
                    guid = dia.DiagramGUID;
                    _historyList.Add(ot, guid);
                    break;
                case EA.ObjectType.otPackage:
                    EA.Package pkg = (EA.Package)_repository.GetContextObject();
                    guid = pkg.PackageGUID;
                    _historyList.Add(ot, guid);
                    break;
                case EA.ObjectType.otElement:
                    EA.Element el = (EA.Element)_repository.GetContextObject();
                    guid = el.ElementGUID;
                    _historyList.Add(ot, guid);
                    break;
            }


        }
        void btnBookmarkRemove_Click(object sender, EventArgs e)
        {
            EA.ObjectType ot = _repository.GetContextItemType();
            string guid = "";
            if (ot.Equals(EA.ObjectType.otDiagram))
            {
                EA.Diagram dia = (EA.Diagram)_repository.GetContextObject();
                guid = dia.DiagramGUID;
            }
            _historyList.Remove(ot, guid);
        }

        private void listBoxDiagrams_Drop(object sender, DragEventArgs e)
        {
            MessageBox.Show("Drag End");
            
        }

        private void listBoxDiagrams_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") ||
        sender == e.Source)
            {
                //e.Effects = DragDropEffects.None;
            }
           
        }
      }
}
