using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnFData.Model;
using PnFDesktop.Controls;
using PnFDesktop.ViewCharts;

namespace PnFDesktop.Classes.Messaging
{
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
}
