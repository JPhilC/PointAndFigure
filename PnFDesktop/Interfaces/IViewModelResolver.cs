using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PnFDesktop.Interfaces
{
    public interface IViewModelResolver
    {
        ObservableObject? ContentViewModelFromID(string content_id);
    }
}
