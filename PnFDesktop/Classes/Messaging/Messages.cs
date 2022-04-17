using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnFData.Model;
using PnFDesktop.Controls;
using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;

namespace PnFDesktop.Classes.Messaging
{

    public enum ActiveDocumentType
    {
        PandFChart,
        MarketSummary
    }

    public class ActiveDocumentChangedMessage
    {
        public Object Sender { get; set; }
        public Guid DocumentId { get; set; }

        public ActiveDocumentType DocumentType { get; set; }

        public ActiveDocumentChangedMessage(object sender, ActiveDocumentType documentType, Guid documentId)
        {
            this.Sender = sender;
            this.DocumentType = documentType;
            this.DocumentId = documentId;
        }

    }

    public class ActivePointAndFigureChartChangedMessage
    {
        public Object Sender { get; set; }
        public PnFChart Chart { get; set; }

        public ActivePointAndFigureChartChangedMessage(object sender, PnFChart activeChart)
        {
            this.Sender = sender;
            this.Chart = activeChart;
        }
    }

    /// <summary>
    /// A message envelope to signal that a P & F chart should be printed
    /// </summary>
    public class PrintPointAndFigureChartMessage
    {
        public Object Sender { get; set; }
        public Guid ChartId { get; set; }

        public PrintPointAndFigureChartMessage(object sender, Guid chartId)
        {
            this.Sender = sender;
            this.ChartId = chartId;
        }
    }

    /// <summary>
    /// A message envelope to signal that a P & F chart should be saved as an image
    /// </summary>
    public class SavePointAndFigureChartAsImageMessage
    {
        public Object Sender { get; set; }
        public Guid ChartId { get; set; }

        public string ImageFilename { get; set; }
        public SavePointAndFigureChartAsImageMessage(object sender, Guid chartId, string imageFilename)
        {
            this.Sender = sender;
            this.ChartId = chartId;
            this.ImageFilename = imageFilename;
        }
    }


    public class PointAndFigureChartClosedMessage
    {
        public Object Sender { get; set; }
        public PointAndFigureChartViewModel ViewModel { get; set; }

        public PointAndFigureChartClosedMessage(object sender, PointAndFigureChartViewModel viewModel)
        {
            this.Sender = sender;
            this.ViewModel = viewModel;
        }
    }
    public class DocumentClosedMessage
    {
        public Object Sender { get; set; }

        public ActiveDocumentType DocumentType { get; set; }
        public Guid ProcedureId { get; set; }

        public string DocumentId { get; set; }
        public DocumentClosedMessage(object sender, Guid procedureId, string documentId)
        {
            this.Sender = sender;
            this.DocumentType = ActiveDocumentType.PandFChart;
            this.ProcedureId = procedureId;
            this.DocumentId = documentId;
        }

        public DocumentClosedMessage(object sender, ActiveDocumentType documentType, string documentId)
        {
            this.Sender = sender;
            this.DocumentType = documentType;
            this.DocumentId = documentId;
        }
    }

    public class ActivePointAndFigureColumnChangedMessage
    {
        public Object Sender { get; set; }
        public PnFColumn Column { get; set; }

        public ActivePointAndFigureColumnChangedMessage(object sender, PnFColumn activeColumn)
        {
            this.Sender = sender;
            this.Column = activeColumn;
        }
    }

}
