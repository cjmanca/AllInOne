using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace AllInOnePlugin
{
    public class RelativePanel : Panel
    {
        public readonly static DependencyProperty DockYProperty;

        public readonly static DependencyProperty DockXProperty;

        public readonly static DependencyProperty RelativeXProperty;

        public readonly static DependencyProperty RelativeYProperty;

        public readonly static DependencyProperty RelativeWidthProperty;

        public readonly static DependencyProperty RelativeHeightProperty;

        static RelativePanel()
        {
            RelativePanel.DockYProperty = DependencyProperty.RegisterAttached("DockY", typeof(VerticalAlignment), typeof(RelativePanel), new FrameworkPropertyMetadata((object)VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));
            RelativePanel.DockXProperty = DependencyProperty.RegisterAttached("DockX", typeof(HorizontalAlignment), typeof(RelativePanel), new FrameworkPropertyMetadata((object)HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));
            RelativePanel.RelativeXProperty = DependencyProperty.RegisterAttached("RelativeX", typeof(double), typeof(RelativePanel), new FrameworkPropertyMetadata((object)0.5, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));
            RelativePanel.RelativeYProperty = DependencyProperty.RegisterAttached("RelativeY", typeof(double), typeof(RelativePanel), new FrameworkPropertyMetadata((object)0.5, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));
            RelativePanel.RelativeWidthProperty = DependencyProperty.RegisterAttached("RelativeWidth", typeof(double?), typeof(RelativePanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));
            RelativePanel.RelativeHeightProperty = DependencyProperty.RegisterAttached("RelativeHeight", typeof(double?), typeof(RelativePanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));
        }

        public RelativePanel()
        {
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (base.Children == null || base.Children.Count == 0)
            {
                return finalSize;
            }
            foreach (UIElement child in base.Children)
            {
                double relativeX = RelativePanel.GetRelativeX(child);
                double relativeY = RelativePanel.GetRelativeY(child);
                double? relativeWidth = RelativePanel.GetRelativeWidth(child);
                double? relativeHeight = RelativePanel.GetRelativeHeight(child);
                double num = 0;
                double num1 = 0;
                num = (!relativeWidth.HasValue ? child.DesiredSize.Width : relativeWidth.Value * finalSize.Width);
                num1 = (!relativeHeight.HasValue ? child.DesiredSize.Height : relativeHeight.Value * finalSize.Height);
                double width = finalSize.Width * relativeX;
                double height = finalSize.Height * relativeY;
                double num2 = 0;
                double num3 = 0;
                switch (RelativePanel.GetDockX(child))
                {
                    case HorizontalAlignment.Left:
                        {
                            num2 = width;
                            break;
                        }
                    case HorizontalAlignment.Center:
                        {
                            num2 = width - num / 2;
                            break;
                        }
                    case HorizontalAlignment.Right:
                        {
                            num2 = width - num;
                            break;
                        }
                }
                switch (RelativePanel.GetDockY(child))
                {
                    case VerticalAlignment.Top:
                        {
                            num3 = height;
                            break;
                        }
                    case VerticalAlignment.Center:
                        {
                            num3 = height - num1 / 2;
                            break;
                        }
                    case VerticalAlignment.Bottom:
                        {
                            num3 = height - num1;
                            break;
                        }
                }
                child.Arrange(new Rect(num2, num3, num, num1));
            }
            return finalSize;
        }

        public static HorizontalAlignment GetDockX(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (HorizontalAlignment)element.GetValue(RelativePanel.DockXProperty);
        }

        public static VerticalAlignment GetDockY(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (VerticalAlignment)element.GetValue(RelativePanel.DockYProperty);
        }

        public static double? GetRelativeHeight(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (double?)element.GetValue(RelativePanel.RelativeHeightProperty);
        }

        public static double? GetRelativeWidth(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (double?)element.GetValue(RelativePanel.RelativeWidthProperty);
        }

        public static double GetRelativeX(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return Convert.ToDouble(element.GetValue(RelativePanel.RelativeXProperty));
        }

        public static double GetRelativeY(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return Convert.ToDouble(element.GetValue(RelativePanel.RelativeYProperty));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (object child in base.Children)
            {
                ((UIElement)child).Measure(availableSize);
            }
            if (!double.IsInfinity(availableSize.Height) && !double.IsInfinity(availableSize.Width))
            {
                return availableSize;
            }
            Size size = new Size(0, 0);
            foreach (UIElement uIElement in base.Children)
            {
                double width = size.Width;
                Size desiredSize = uIElement.DesiredSize;
                size.Width = Math.Max(width, desiredSize.Width);
                double height = size.Height;
                desiredSize = uIElement.DesiredSize;
                size.Height = Math.Max(height, desiredSize.Height);
            }
            return size;
        }

        public static void SetDockX(DependencyObject element, HorizontalAlignment value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RelativePanel.DockXProperty, value);
        }

        public static void SetDockY(DependencyObject element, VerticalAlignment value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RelativePanel.DockYProperty, value);
        }

        public static void SetRelativeHeight(DependencyObject element, double? value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RelativePanel.RelativeHeightProperty, value);
        }

        public static void SetRelativeWidth(DependencyObject element, double? value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RelativePanel.RelativeWidthProperty, value);
        }

        public static void SetRelativeX(DependencyObject element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RelativePanel.RelativeXProperty, value);
        }

        public static void SetRelativeY(DependencyObject element, double value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RelativePanel.RelativeYProperty, value);
        }
    }
}
