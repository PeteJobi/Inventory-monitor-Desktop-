using Inventory_monitor.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Inventory_monitor.Models
{
    public class Resource: INotifyPropertyChanged
    {
        public const string RES_DEFAULT_IMAGE = "Icons/resource.png";
        public const string GROUP_DEFAULT_IMAGE = "Icons/group.png";
        public long Id { get; set; }
        public long ParentId { get; set; }
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                NotifyPropertyChanged();
            }
        }
        private string _Description;
        public string Description {
            get { return _Description; }
            set
            {
                _Description = value;
                NotifyPropertyChanged();
            }
        }
        public int InitialSize { get; set; }
        private int _size;
        public int Size {
            get { return _size; }
            set
            {
                _size = value;
                NotifyPropertyChanged();
                GetStock = "";
                GetResourcesContained = "";
            }
        }
        public bool IsPinned { get; set; }
        public long PinnedDate { get; set; }
        private string _ResourceImagePath;
        public string ResourceImagePath {
            get { return _ResourceImagePath; }
            set
            {
                _ResourceImagePath = value != null ? value : IsGroup ? GROUP_DEFAULT_IMAGE : RES_DEFAULT_IMAGE;
                NotifyPropertyChanged();
                Image = null;
                HasImage = false;
            }
        }
        public BitmapImage Image
        {
            set {NotifyPropertyChanged();}
            get
            {
                BitmapImage image = new BitmapImage();
                using (var stream = File.OpenRead(ResourceImagePath))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                return image;
            }
        }
        public ObservableCollection<Transaction> Transactions { get; set; }
        public bool IsGroup { get; set; }
        public ObservableCollection<Resource> Resources { get; set; }
        public Resource Group { get; set; }
        public string DateCreated { get; set; }

        public Resource(long id, long parentId, string title, string description, int initialSize, bool isPinned, long pinnedDate, 
            string resourceImagePath, bool isGroup, string dateCreated, MainWindow mainWindow)
        {
            Id = id;
            ParentId = parentId;
            Title = title;
            Description = description;
            InitialSize = initialSize;
            Size = InitialSize;
            IsPinned = isPinned;
            PinnedDate = pinnedDate;
            IsGroup = isGroup;
            ResourceImagePath = resourceImagePath;
            DateCreated = dateCreated;
            MainWindow = mainWindow;

            if (!IsGroup)
                Transactions = new ObservableCollection<Transaction>();
            else
                Resources = new ObservableCollection<Resource>();
        }

        public void SetGroup(Resource group)
        {
            Group = group;
            ParentId = group == null ? 0 : Id;
        }
        public void SetMainWindow(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }
        public MainWindow GetMainWindow()
        {
            return MainWindow;
        }
        public void AddResource(Resource resource)
        {
            Resources.Add(resource);
            resource.SetGroup(this);
            Size += resource.Size;
            InitialSize += resource.InitialSize;
            Resource par = Group;
            while(par != null)
            {
                par.Size += resource.Size;
                par.InitialSize += resource.InitialSize;
                par = par.Group;
            }
        }
        public void RemoveResource(Resource resource, bool isDeleting)
        {
            DatabaseHandler db = new DatabaseHandler(MainWindow);
            db.Open();
            if (isDeleting)
            {
                if (resource.IsGroup)
                {
                    foreach (Resource res in resource.Resources)
                    {
                        RemoveResource(res, true);
                    }
                }
                else
                {
                    if (resource.ResourceImagePath != RES_DEFAULT_IMAGE && File.Exists(resource.ResourceImagePath))
                        File.Delete(resource.ResourceImagePath);
                    resource.RemoveTransactions(0);
                }
                db.DeleteResource(resource);
            }
            Resources.Remove(resource);
            Size -= resource.Size;
            InitialSize -= resource.InitialSize;
            Resource par = Group;
            while (par != null)
            {
                par.Size -= resource.Size;
                par.InitialSize -= resource.InitialSize;
                par = par.Group;
            }
            if (Resources.Count == 0)
            {
                db.DeleteResource(this);
                if (Group != null) Group.RemoveResource(this, isDeleting);
            }
            db.Close();
        }
        public void AddTransaction(Transaction transaction)
        {
            Transactions.Add(transaction);
            HasTransactions = false;
            GetLastTransaction = null;
        }
        public void RemoveTransactions(int index)
        {
            if (Transactions.Count == 0) return;
            DatabaseHandler db = new DatabaseHandler(MainWindow);
            db.Open();
            while (Transactions.Count() > index)
            {
                Size += (Transactions[index].IsRemoval ? 1 : -1) * Transactions[index].NumberTransacted;
                db.DeleteTransaction(Transactions[index]);
                Transactions.RemoveAt(index);
            }
            db.Close();
            HasTransactions = false;
            GetLastTransaction = null;
        }

        public Transaction GetLastTransaction
        {
            get {
                return Transactions != null && Transactions.Count > 0 ? Transactions[Transactions.Count - 1] : null;
            }
            set { NotifyPropertyChanged(); }
        }
        public string GetResourcesContained
        {
            set { NotifyPropertyChanged(); }
            get
            {
                if (Size == 0) return "";
                string cont = "";
                foreach (Resource res in Resources)
                {
                    cont += res.Title + ", ";
                }
                return cont.Substring(0, cont.Length - 2);
            }
        }
        public bool HasTransactions
        {
            get
            {
                return Transactions?.Count > 0;
            }
            set { NotifyPropertyChanged(); }
        }
        public string GetStock
        {
            set { NotifyPropertyChanged(); }
            get { return "(" + Size + "/" + InitialSize + ")";}
        }
        public bool HasImage
        {
            set { NotifyPropertyChanged(); }
            get { return ResourceImagePath != RES_DEFAULT_IMAGE; }
        }
        public int ResNum
        {
            get
            {
                if (Resources == null) return 0;
                int i = 0;
                foreach (Resource res in Resources)
                    if (!res.IsGroup) i++;
                return i;
            }
        }
        public int GroupNum
        {
            get
            {
                if (Resources == null) return 0;
                int i = 0;
                foreach (Resource res in Resources)
                    if (res.IsGroup) i++;
                return i;
            }
        }
        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value;
                NotifyPropertyChanged();
            }
        }
        private bool _IsMoving;
        public bool IsMoving
        {
            get { return _IsMoving; }
            set
            {
                _IsMoving = value;
                NotifyPropertyChanged();
            }
        }
        private MainWindow MainWindow;

        public static string ThisDate() {
            return "aa";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
