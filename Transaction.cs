using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Inventory_monitor.Models
{
    public class Transaction : INotifyPropertyChanged
    {
        private const string IN_SMALL = "../../Icons/in_small.png";
        private const string OUT_SMALL = "../../Icons/out_small.png";
        private const string IN = "../../Icons/in.png";
        private const string OUT = "../../Icons/out.png";
        private const string IN_BIG = "../../Icons/in_big.png";
        private const string OUT_BIG = "../../Icons/out_big.png";
        public long Id { get; set; }
        public long ResourceId { get; set; }
        public bool IsRemoval { get; set; }
        public int NumberTransacted { get; set; }
        public Resource Parent { get; set; }
        public int SizeBefore { get; set; }
        public int SizeAfter { get; set; }
        private string _Comment;
        public string Comment {
            get { return _Comment; }
            set
            {
                _Comment = value;
                NotifyPropertyChanged();
                GetComment = "";
            }
        }
        public string TransactionDate { get; set; }
        public string ShortTransactionDate { get; set; }
        public int ParentInitialSize { get; set; }

        public Transaction(long id, long resourceId, bool isRemoval, int numberTransacted, Resource parent, string comment, 
            string transactionDate, string shortTransactionDate)
        {
            Id = id;
            ResourceId = resourceId;
            IsRemoval = isRemoval;
            NumberTransacted = numberTransacted;
            Parent = parent;
            SizeBefore = parent.Size;
            int newSize = isRemoval ? SizeBefore - numberTransacted : SizeBefore + numberTransacted;
            parent.Size = newSize;
            SizeAfter = newSize;
            Comment = comment;
            TransactionDate = transactionDate;
            ShortTransactionDate = shortTransactionDate;
            ParentInitialSize = parent.InitialSize;
        }

        public string SmallTransImage => IsRemoval ? OUT : IN;
        public string BigTransImage => IsRemoval ? OUT_BIG : IN_BIG;
        public string GetComment
        {
            get { return string.IsNullOrEmpty(Comment) ? "* Click edit button to enter comment *" : Comment; }
            set { NotifyPropertyChanged(); }
        }
        public string GetTime => TransactionDate.Split(',')[0];
        public string GetBottomDate => TransactionDate.Split(',')[1].TrimStart();
        public string GetNTString => IsRemoval ? "-" + NumberTransacted : "+" + NumberTransacted;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}