// ****************************************************************************
// <copyright file="DesignerLibrary.cs" company="GalaSoft Laurent Bugnion">
// Copyright © GalaSoft Laurent Bugnion 2009-2016
// </copyright>
// ****************************************************************************
// <author>Laurent Bugnion</author>
// <email>laurent@galasoft.ch</email>
// <date>30.09.2014</date>
// <project>GalaSoft.MvvmLight</project>
// <web>http://www.mvvmlight.net</web>
// <license>
// See license.txt in this solution or http://www.galasoft.ch/license_MIT.txt
// </license>
// ****************************************************************************


namespace PnFDesktop.Classes
{
    public static class DesignerLibrary
    {
        private static bool? _isInDesignMode;

        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    var prop = System.ComponentModel.DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)System.ComponentModel.DependencyPropertyDescriptor
                            .FromProperty(prop, typeof(System.Windows.FrameworkElement))
                            .Metadata.DefaultValue;
                }

                return _isInDesignMode.Value;
            }
        }

    }
}
