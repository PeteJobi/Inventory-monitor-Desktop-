using Inventory_monitor.Controls;
using Inventory_monitor.Models;
using Inventory_monitor.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;

namespace Inventory_monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/InventoryMonitor/";
        public static string backupFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/InventoryMonitorBackups/";
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _CurrentGroupTitle;
        public string CurrentGroupTitle
        {
            get { return _CurrentGroupTitle; }
            set
            {
                _CurrentGroupTitle = value;
                NotifyPropertyChanged();
                new Thread(UpdateGroupTitle).Start();
            }
        }
        private string _CurrentGroupStock;
        public string CurrentGroupStock
        {
            get { return _CurrentGroupStock; }
            set
            {
                _CurrentGroupStock = value;
                NotifyPropertyChanged();
            }
        }
        void UpdateGroupTitle()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            this.Dispatcher.Invoke(new Action(() => {
                titl.MaxWidth =Math.Max(0, ((0.45f / 1.45f) * this.ActualWidth) - 100 - stockT.ActualWidth);
            }));
        }
        private bool _InMoveMode;
        public bool InMoveMode
        {
            get { return _InMoveMode; }
            set
            {
                _InMoveMode = value;
                multiSelectBut.IsEnabled = !value;
                addResourceBut.IsEnabled = !value;
                if(!value)
                    foreach (Resource item in toMove)
                        item.IsMoving = false;
                toMove.Clear();
                movePar = null;
                NotifyPropertyChanged();
            }
        }

        public bool _InMultiSelectMode;
        public bool InMultiSelectMode
        {
            get { return _InMultiSelectMode; }
            set
            {
                _InMultiSelectMode = value;
                if (!value)
                {
                    foreach (Resource res in selected)
                        res.IsSelected = false;
                    selected.Clear();
                    lastSelected.IsSelected = true;
                }
                else
                {
                    selected.Add(lastSelected);
                }
                NotifyPropertyChanged();
            }
        }

        Resource parent;
        public ObservableCollection<Resource> resources;
        ObservableCollection<Resource> topResources;
        ObservableCollection<Resource> selected;
        public MasterPanel masterPanel;
        Resource lastSelected;
        Stack<MasterPanel> backStack;
        Stack<MasterPanel> forwardStack;
        Resource movePar;
        ObservableCollection<Resource> toMove;
        string aa;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            if (!File.Exists(DatabaseHandler.databaseFile))
                DatabaseHandler.CreateNewDatabase();
            masterPanel = new MasterPanel(this, GetResources());
            masterFrame.Content = masterPanel;
            masterFrame.Navigating += MasterFrame_Navigating;
            masterGrid.DataContext = this;
            backStack = new Stack<MasterPanel>();
            forwardStack = new Stack<MasterPanel>();
            UpdateNavButs();
            selected = new ObservableCollection<Resource>();
            toMove = new ObservableCollection<Resource>();
        }

        private void MasterFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            ResetUI();
            masterPanel.lastSelected = lastSelected;
            if (e.NavigationMode == NavigationMode.Forward)
            {
                if (forwardStack.Count == 0)
                {
                    e.Cancel = true;
                    return;
                }
                backStack.Push(masterPanel);
                masterPanel = forwardStack.Pop();
            }
            else if(e.NavigationMode == NavigationMode.Back)
            {
                forwardStack.Push(masterPanel);
                masterPanel = backStack.Pop();
            }else if(e.NavigationMode == NavigationMode.New)
            {
                lastSelected = masterPanel.resources[0];
                masterPanel.lastSelected = lastSelected;
                Select(masterPanel.resources[0]);
            }
            UpdateNavButs();
            RefreshParent(masterPanel);
        }

        ObservableCollection<Resource> GetResources()
        {
            Resource grr = new Resource(0, 0, "Some group", "This is the group for my tools", 0, false, 0, null, true, "13/15/13", this);
            grr.AddResource(new Resource(0, 0, "Laceses", "This is the laces I will be using for Abbot work", 80, false, 0, null, false, "13/13/13",this));
            grr.AddResource(new Resource(0, 0, "Miscal", "This is the laces I will be using for Abbot work", 34, true, 0, null, false, "13/13/13", this));
            Resource re = new Resource(0, 0, "Done good with this", "This one's got transactions!", 100, false, 0, null, false, "9:11, 9 November, 2016", this);
            re.AddTransaction(new Transaction(0, 0, true, 20, re, "Took these for your dad..", "13/12/11", "hahahah"));
            re.AddTransaction(new Transaction(0, 0, false, 5, re, "Took these for your mum..", "15/12/11", "hahahah"));
            re.AddTransaction(new Transaction(0, 0, true, 10, re, "Took these for your brother..", "13/12/11", "hahahah"));
            re.AddTransaction(new Transaction(0, 0, false, 8, re, "Took these for your sister..", "13/12/11", "hahahah"));
            ObservableCollection<Resource> resourcess = new ObservableCollection<Resource>
            {
                new Resource(0,0,"Laces","This is the laces I will be using for Abbot work",100,false,0,null,false,"13/13/13", this),
                grr,
                new Resource(0,0,"Threads","This is for sewing",100,false,0,null,false,"13/13/13",this),
                re
            };
            DatabaseHandler db = new DatabaseHandler(this);
            resources = db.GetResources();
            db.Close();
            topResources = resources;
            if (resources.Count == 0)
                NoResources(true);
            else
                NoResources(false);
            return resources;
        }
        public void Select(Resource selected)
        {
            if (InMultiSelectMode && selected.IsSelected)
            {
                if (this.selected.Count == 1) return;
                selected.IsSelected = false;
                this.selected.Remove(selected);
                return;
            }
            if (selected.IsSelected && selected.IsGroup)
            {
                GoToGroup(selected);
                return;
            }
            if (InMultiSelectMode)
                this.selected.Add(selected);
            else
                if (lastSelected != null) lastSelected.IsSelected = false;
            lastSelected = selected;
            selected.IsSelected = true;
            detailGrid.DataContext = selected;
            new Thread(UpdateDetail).Start();
        }
        public void GoToGroup(Resource selected)
        {
            if (InMoveMode && (toMove.Contains(selected))) return;
            lastSelected.IsSelected = false;
            selected.IsSelected = true;
            lastSelected = selected;
            
            masterPanel.lastSelected = lastSelected;
            backStack.Push(masterPanel);
            MasterPanel mp;
            while(forwardStack.Count > 0 && (mp = forwardStack.Pop()) != null)
            {
                mp.lastSelected.IsSelected = false;
            }
            masterPanel = new MasterPanel(this, selected);
            masterFrame.NavigationService.Navigate(masterPanel);
        }
        void ResetUI()
        {
            InMultiSelectMode = false;
            editSaveName.IsChecked = false;
            editSaveDesc.IsChecked = false;
        }
        void RefreshParent(MasterPanel mp)
        {
            parent = mp.parent;
            resources = mp.resources;
            lastSelected = mp.lastSelected;
            lastSelected.IsSelected = false;
            Select(lastSelected);
            CurrentGroupTitle = parent == null ? "All resources" : parent.Title;
            CurrentGroupStock = parent == null ? "" : "(" + parent.Size + "/" + parent.InitialSize + ")";
        }
        void UpdateNavButs()
        {
            backBut.IsEnabled = backStack.Count > 0;
            forwardBut.IsEnabled = forwardStack.Count > 0;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDetail();
            titl.MaxWidth = ((0.45f / 1.45f) * this.ActualWidth) - 100 - stockT.ActualWidth;
        }
        void UpdateDetail()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    resourceNameText.MaxWidth = resNameRow.ActualWidth - 70 - itemsCell.ActualWidth;
                    resourceNameBlock.MaxWidth = resourceNameText.MaxWidth;
                    resourceNameText.Select(resourceNameText.Text.Length, 0);

                    resourceDescText.MaxWidth = resNameRow.ActualWidth - 60;
                    resourceDescBlock.MaxWidth = resourceDescText.MaxWidth;
                    resourceDescText.Select(resourceDescText.Text.Length, 0);
                }
                catch
                {

                }
            }));
        }
        public void AddResource(string title, string description, int stock, string image)
        {
            DatabaseHandler db = new DatabaseHandler(this);
            db.Open();
            Resource res = db.AddResource(title, description, stock, image, parent);
            db.Close();
            if (parent != null)
                parent.AddResource(res);
            else resources.Add(res);
            CurrentGroupStock = parent == null ? "" : "(" + parent.Size + "/" + parent.InitialSize + ")";
            if (resources.Count == 1) NoResources(false);
        }
        public void DeleteResource(Resource resource, bool isDeleting)
        {
            int index = resources.IndexOf(resource);
            if (resource.IsGroup && forwardStack.Count > 0 && forwardStack.Peek().parent == resource)
            {
                forwardStack.Clear();
                UpdateNavButs();
            }
            if (parent != null) parent.RemoveResource(resource, isDeleting);
            else
            {
                resources.Remove(resource);
                if (resource.IsGroup)
                {
                    for (int i = 0; i < resource.Resources.Count; i++)
                    {
                        resource.RemoveResource(resource.Resources[i], true);
                    }
                }
                else
                {
                    resource.RemoveTransactions(0);
                    if (resource.ResourceImagePath != Resource.RES_DEFAULT_IMAGE && File.Exists(resource.ResourceImagePath))
                        File.Delete(resource.ResourceImagePath);
                }
                DatabaseHandler db = new DatabaseHandler(this);
                db.Open();
                db.DeleteResource(resource);
                db.Close();
            }
            if (!InMultiSelectMode)
            {
                int newInd = Math.Min(index, resources.Count - 1);
                if (newInd < 0)
                {
                    GoBack(parent);
                    return;
                }
                lastSelected = resources[newInd];
                Select(lastSelected);
            }
            CurrentGroupStock = parent == null ? "" : "(" + parent.Size + "/" + parent.InitialSize + ")";
        }
        public void AddTransaction(int amount, bool isRemoval, string comment)
        {
            DatabaseHandler db = new DatabaseHandler(this);
            db.Open();
            lastSelected.AddTransaction(db.AddTransaction(isRemoval, amount, comment, lastSelected));
            db.Close();
        }
        public void DeleteTransactions(Transaction transaction)
        {
            int ind = lastSelected.Transactions.IndexOf(transaction);
            string title = ind == lastSelected.Transactions.Count - 1 ? "Delete transaction?" : "Delete transactions?";
            string mess = ind == lastSelected.Transactions.Count - 1 ? "Are you sure you want to delete this transaction?" :
                "Are you sure you want to delete this transaction? Deleting this transaction automatically deletes the ones that" +
                " come after it.";
            MessageBoxResult result = MessageBox.Show(mess, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
                lastSelected.RemoveTransactions(ind);
        }
        public void EditTransaction(Transaction transaction, string comment)
        {
            transaction.Comment = comment;
            DatabaseHandler db = new DatabaseHandler(this);
            db.Open();
            db.UpdateComment(transaction, comment);
            db.Close();
        }
        public void CreateGroup(string name, string description)
        {
            ObservableCollection<Resource> reses = selected.Count > 0 ? selected : new ObservableCollection<Resource> { lastSelected };
            DatabaseHandler db = new DatabaseHandler(this);
            db.Open();
            Resource group = db.AddGroup(name, description, parent, reses);
            if (parent == null) resources.Add(group);
            else parent.AddResource(group);
            if (selected.Count > 0)
            {
                foreach (Resource res in selected)
                {
                    if (res.Group == null)
                        resources.Remove(res);
                    else res.Group.RemoveResource(res, false);
                    group.AddResource(res);
                    res.Group = group;
                }
            }
            else
            {
                if (lastSelected.Group == null)
                    resources.Remove(lastSelected);
                else lastSelected.Group.RemoveResource(lastSelected, false);
                group.AddResource(lastSelected);
                lastSelected.Group = group;
            }
            InMultiSelectMode = false;
            Select(group);
        }
        void GoBack(Resource emptyGroup)
        {
            if (backStack.Count == 0)
            {
                NoResources(true);
                return;
            }
            masterFrame.NavigationService.GoBack();
            if(parent != null)
            {
                parent.RemoveResource(emptyGroup, true);
                if (parent.Resources.Count == 0) GoBack(parent);
            }
            else
            {
                //DatabaseHandler db = new DatabaseHandler(this);
                //db.Open();
                //db.DeleteResource(emptyGroup);
                //db.Close();
                resources.Remove(emptyGroup);
            }
            forwardStack.Clear();
            UpdateNavButs();
            if (!resources.Contains(lastSelected)) Select(resources[0]);
        }
        void NoResources(bool none)
        {
            multiSelectBut.IsEnabled = !none;
            makeGroupBut.IsEnabled = !none;
            pinBut.IsEnabled = !none;
            moveBut.IsEnabled = !none;
            delBut.IsEnabled = !none;

            noReseources.Visibility = none ? Visibility.Visible : Visibility.Collapsed;
            detailGrid.Visibility = !none ? Visibility.Visible : Visibility.Collapsed;
            CurrentGroupTitle = none ? "No resources" : "All resources";

            if (none)
                lastSelected = null;
            else
                Select(resources[0]);
        }
        private void MoveBut_Click(object sender, RoutedEventArgs e)
        {
            InMoveMode = true;
            if (selected.Count > 0)
            {
                foreach (Resource item in selected)
                {
                    toMove.Add(item);
                    item.IsMoving = true;
                }
                InMultiSelectMode = false;
            }
            else { toMove.Add(lastSelected); lastSelected.IsMoving = true; }
            movePar = parent;
        }

        private void ConfirmMoveBut_Click(object sender, RoutedEventArgs e)
        {
            if (parent != movePar)
            {
                DatabaseHandler db = new DatabaseHandler(this);
                db.Open();
                foreach (Resource item in toMove)
                {
                    if (parent == null)
                    {
                        resources.Add(item);
                        item.Group = null;
                    }
                    else
                    {
                        parent.AddResource(item);
                    }
                    db.UpdateResParent(item, parent);
                    if (movePar == null)
                        topResources.Remove(item);
                    else
                    {
                        movePar.RemoveResource(item, false);
                    }
                    item.IsSelected = false;
                }
                Resource par = movePar;
                while (par != null && par.Resources.Count == 0)
                {
                    if (resources.Contains(par))
                    {
                        resources.Remove(par);
                        forwardStack.Clear(); UpdateNavButs();
                        if (!resources.Contains(lastSelected)) Select(toMove[0]);
                        break;
                    }
                    par = par.Group;
                }
                db.Close();
            }
            InMoveMode = false;
        }

        private void CancelMoveBut_Click(object sender, RoutedEventArgs e)
        {
            InMoveMode = false;
        }

        private void BackBut_Click(object sender, RoutedEventArgs e)
        {
            masterFrame.NavigationService.GoBack();
        }

        private void ForwardBut_Click(object sender, RoutedEventArgs e)
        {
            masterFrame.NavigationService.GoForward();
        }

        private void AddResourceBut_Click(object sender, RoutedEventArgs e)
        {
            AddResourceWindow addResourceWindow = new AddResourceWindow(this);
            addResourceWindow.ShowDialog();
        }

        private void DelBut_Click(object sender, RoutedEventArgs e)
        {
            string title = selected.Count > 0 ? "Delete resources?" : lastSelected.IsGroup ? "Delete group?" : "Delete resource?";
            string mess = selected.Count > 0 ? "Are you sure you want to delete the selected resources?" :
                lastSelected.IsGroup ? "Are you sure you want to delete this group? All resources contained in this group will be lost." : 
            "Are you sure you want to delete this resource?";
            MessageBoxResult result = MessageBox.Show(mess, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if(result == MessageBoxResult.OK)
            {
                if (selected.Count > 0)
                {
                    int index = resources.IndexOf(selected[selected.Count - 1]);
                    foreach (Resource res in selected)
                    {
                        DeleteResource(res, true);
                    }
                    int newInd = Math.Min(index, resources.Count - 1);
                    if (newInd < 0)
                    {
                        GoBack(parent);
                        return;
                    }
                    lastSelected = resources[newInd];
                    InMultiSelectMode = false;
                }
                else
                {
                    DeleteResource(lastSelected, true);
                }
            }
        }

        private void AddItemBut_Click(object sender, RoutedEventArgs e)
        {
            TransactionWindow transactionWindow = new TransactionWindow(this);
            transactionWindow.DataContext = false;
            transactionWindow.ShowDialog();
        }

        private void RemoveItemBut_Click(object sender, RoutedEventArgs e)
        {
            if (lastSelected.Size == 0)
            {
                MessageBox.Show("There are no more items in this resource to remove");
                return;
            }
            TransactionWindow transactionWindow = new TransactionWindow(lastSelected.Size, this);
            transactionWindow.DataContext = true;
            transactionWindow.ShowDialog();
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (lastSelected.ResourceImagePath != Resource.RES_DEFAULT_IMAGE && File.Exists(lastSelected.ResourceImagePath))
                File.Delete(lastSelected.ResourceImagePath);
            lastSelected.ResourceImagePath = null;
            DatabaseHandler db = new DatabaseHandler(this);
            db.Open();
            db.UpdatePicture(lastSelected, null);
            db.Close();
        }

        private void EditImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select picture";
            ofd.Filter = "Image files (*.png, *.jpg)|*.png;*.jpg";
            bool? success = ofd.ShowDialog();
            if (success == true)
            {
                if (lastSelected.ResourceImagePath != Resource.RES_DEFAULT_IMAGE && File.Exists(lastSelected.ResourceImagePath))
                    File.Delete(lastSelected.ResourceImagePath);
                string name = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString() + Path.GetExtension(ofd.FileName);
                string dest = dataFolder + name;
                File.Copy(ofd.FileName, dest);
                lastSelected.ResourceImagePath = dest;
                DatabaseHandler db = new DatabaseHandler(this);
                db.Open();
                db.UpdatePicture(lastSelected, dest);
                db.Close();
            }
            else
            {
                MessageBox.Show("Failed to open File Dialog");
            }
        }

        private void EditSaveName_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked == false)
            {
                if (lastSelected.Title == resourceNameText.Text) return;
                lastSelected.Title = resourceNameText.Text;
                DatabaseHandler db = new DatabaseHandler(this);
                db.Open();
                db.UpdateTitle(lastSelected, resourceNameText.Text);
                db.Close();
            }
        }

        private void EditSaveDesc_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked == false)
            {
                if (lastSelected.Description == resourceDescText.Text) return;
                lastSelected.Description = resourceDescText.Text;
                DatabaseHandler db = new DatabaseHandler(this);
                db.Open();
                db.UpdateDescription(lastSelected, resourceDescText.Text);
                db.Close();
            }
        }

        private void ViewImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(lastSelected.ResourceImagePath) || lastSelected.ResourceImagePath == Resource.RES_DEFAULT_IMAGE) return;
            ViewPicture viewPicture = new ViewPicture();
            viewPicture.DataContext = lastSelected.Image;
            viewPicture.Show();
        }

        private void DelGroupBut_Click(object sender, RoutedEventArgs e)
        {
            string title = "Delete group?";
            string mess = "Are you sure you want to delete this group? All resources contained in this group will be lost.";
            MessageBoxResult result = MessageBox.Show(mess, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
                DeleteResource(lastSelected, true);
        }

        private void MakeGroupBut_Click(object sender, RoutedEventArgs e)
        {
            CreateGroup createGroup;
            if (selected.Count > 0)
                createGroup = new CreateGroup(selected, this);
            else
                createGroup = new CreateGroup(lastSelected, this);
            createGroup.ShowDialog();
        }

        private void PinBut_Click(object sender, RoutedEventArgs e)
        {
            if (selected.Count == 0) selected.Add(lastSelected);
            int modCount = 1;
            int prevI = -1;
            Resource prevRes = null;
            DatabaseHandler db = new DatabaseHandler(this);
            db.Open();

            foreach (Resource res in selected)
            {
                int i = resources.IndexOf(res);
                if (prevI > -1 && prevRes.IsPinned)
                {
                    if (i < prevI)
                    {
                        i += modCount;
                        prevI = i;
                        prevRes = res;
                        modCount++;
                    }
                    else
                    {
                        i = Math.Min(resources.Count - 1, i + modCount - 1);
                        prevI = i;
                        prevRes = res;
                    }
                }
                else if (prevI > -1 && !prevRes.IsPinned)
                {
                    if (i > prevI)
                    {
                        prevI = i;
                        i -= modCount; prevRes = res;
                        modCount++;
                    }
                    else
                    {
                        i = Math.Max(0, i - modCount - 1);
                        prevI = i;
                        prevRes = res;
                    }
                }
                else
                {
                    prevI = i;
                    prevRes = res;
                }
                if (res.IsPinned)
                {
                    res.IsPinned = false;
                    resources.Remove(res);
                    int pos = -1;
                    for(int j = 0; j < resources.Count; j++)
                    {
                        if (j == resources.Count - 1) pos = j + 1;
                        else if(resources[j].Id<res.Id && !resources[j+1].IsPinned && resources[j + 1].Id > res.Id){
                            pos = j + 1;
                            break;
                        }else if(!resources[j].IsPinned && resources[j].Id > res.Id)
                        {
                            pos = j;
                            break;
                        }
                    }
                    resources.Insert(Math.Max(0, pos), res);
                    db.SetPinned(res, false);
                }
                else
                {
                    res.IsPinned = true;
                    resources.Remove(res);
                    resources.Insert(0, res);
                    db.SetPinned(res, true);
                }
            }
            db.Close();
            InMultiSelectMode = false;
        }
    }
}
