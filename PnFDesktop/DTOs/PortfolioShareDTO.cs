using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.DTOs
{
    public class PortfolioShareDTO : ObservableObject, IEditableObject
    {
        struct PSData
        {
            internal Guid id;
            internal string tidm;
            internal string name;
            internal double? holding;
            internal string? remarks;
        }

        private bool inTxn = false;
        private PSData backupData;

        public Guid Id { get; set; }

        public string Tidm { get; set; }

        public string Name { get; set; }


        public double? Holding { get; set;}

        public string? Remarks { get;set;}

        public void BeginEdit()
        {
            if (!inTxn)
            {
                this.backupData = new PSData()
                {
                    id = this.Id,
                    tidm = this.Tidm,
                    name = this.Name,
                    holding = this.Holding,
                    remarks = this.Remarks
                };
                inTxn = true;
            }
        }

        public void CancelEdit()
        {
            if (inTxn)
            {
                this.Id = backupData.id;
                this.Tidm = backupData.tidm;
                this.Name = backupData.name;
                this.Holding = backupData.holding;
                this.Remarks = backupData.remarks;
                inTxn = false;
            }
        }

        public void EndEdit()
        {
            if (inTxn)
            {
                inTxn = false;
                OnPropertyChanged("RecordUpdated");
            }
        }
    }
}
