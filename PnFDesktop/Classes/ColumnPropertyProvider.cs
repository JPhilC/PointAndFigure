using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnFData.Model;

namespace PnFDesktop.Classes
{
    public enum ColumnPropertyEnum
    {
        Tooltip
    }

    public class ColumnPropertyProvider
    {
        private static ColumnPropertyProvider _Instance = null;

        private static ColumnPropertyProvider Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ColumnPropertyProvider();
                }

                return _Instance;
            }

        }

        public static object GetProperty(PnFColumn column, ColumnPropertyEnum property)
        {
            return Instance.GetPropertyInternal(column, property);
        }

        private object GetPropertyInternal(PnFColumn node, ColumnPropertyEnum property)
        {
            string tooltip = "";
            StringBuilder sb = new StringBuilder();
            
            // TODO: Build proper tooltip
            
            sb.Append("Column tooltip");

            // What we return depends on the property requested
            object propertyValue = null;
            switch (property)
            {
                case ColumnPropertyEnum.Tooltip:
                    propertyValue = tooltip;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("property", "Unrecognised ColumnPropertyEnum value.");
            }

            return propertyValue;
        }
    }
}
