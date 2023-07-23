using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Threading;

namespace ArtemisManagerUI
{
    public static class DragHelper
    {
        public static readonly DependencyProperty DragScopeIdProperty =
          DependencyProperty.RegisterAttached("DragScopeId",
          typeof(string), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void SetDragScopeId(DependencyObject? element, string value)
        {
            element?.SetValue(DragScopeIdProperty, value);
        }
        public static string GetDragScopeId(this DependencyObject? element)
        {
            string retVal = string.Empty;
            if (element != null)
            {
                retVal = (string)element.GetValue(DragScopeIdProperty);
            }
            return retVal;
        }




        public static readonly DependencyProperty IsDraggingProperty =
          DependencyProperty.RegisterAttached("IsDragging",
          typeof(bool), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void SetIsDragging(DependencyObject? element, bool value)
        {
            element?.SetValue(IsDraggingProperty, value);
        }
        public static bool GetIsDragging(DependencyObject? element)
        {
            bool retVal = false;
            if (element != null)
            {
                retVal = (bool)element.GetValue(IsDraggingProperty);
            }
            return retVal;
        }

        public static readonly DependencyProperty DragHasLeftScopeProperty =
          DependencyProperty.RegisterAttached("DragHasLeftScope",
          typeof(bool), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void SetDragHasLeftScope(DependencyObject? element, bool value)
        {
            element?.SetValue(DragHasLeftScopeProperty, value);
        }
        public static bool GetDragHasLeftScope(DependencyObject element)
        {

            bool retVal = false;
            if (element != null)
            {
                retVal = (bool)element.GetValue(DragHasLeftScopeProperty);
            }
            return retVal;
        }

        public static readonly DependencyProperty StartPointProperty =
           DependencyProperty.RegisterAttached("StartPoint",
           typeof(Point), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void SetStartPoint(DependencyObject? element, Point value)
        {
            element?.SetValue(StartPointProperty, value);
        }
        public static Point GetStartPoint(DependencyObject? element)
        {
            Point retVal;
            if (element != null)
            {
                retVal = (Point)element.GetValue(StartPointProperty);
            }
            else
            {
                retVal = new Point();
            }
            return retVal;
        }
        public static readonly DependencyProperty DragAdornerProperty =
           DependencyProperty.RegisterAttached("DragAdorner",
           typeof(DragAdorner), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void SetDragAdorner(DependencyObject? element, DragAdorner? value)
        {
            element?.SetValue(DragAdornerProperty, value);
        }
        public static DragAdorner? GetDragAdorner(DependencyObject? element)
        {
            DragAdorner? retVal = null;
            if (element != null)
            {
                retVal = (DragAdorner)element.GetValue(DragAdornerProperty);
            }
            return retVal;
        }

        public static readonly DependencyProperty RelativeMousePointProperty =
           DependencyProperty.RegisterAttached("RelativeMousePoint",
           typeof(Point), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void SetRelativeMousePoint(DependencyObject? element, Point value)
        {
            element?.SetValue(RelativeMousePointProperty, value);
        }
        public static Point GetRelativeMousePoint(DependencyObject? element)
        {
            Point retVal;
            if (element != null)
            {
                retVal = (Point)element.GetValue(RelativeMousePointProperty);
            }
            else
            {
                retVal = new Point();
            }
            return retVal;
        }

        public static readonly DependencyProperty HasDroppedProperty =
         DependencyProperty.RegisterAttached("HasDropped",
         typeof(bool), typeof(DragHelper), new FrameworkPropertyMetadata());

        /// <summary>
        /// Sets the has dropped.  Useful if multiple Drop handlers are tripped.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetHasDropped(DependencyObject? element, bool value)
        {
            element?.Dispatcher.Invoke(
                  new Action(() =>
                  {
                      element.SetValue(HasDroppedProperty, value);
                  }));
        }
        public static bool GetHasDropped(DependencyObject? element)
        {
            bool retVal = false;
            if (element != null)
            {
                retVal = element.Dispatcher.Invoke(
                  new Func<bool>(() =>
                  {
                      return (bool)element.GetValue(HasDroppedProperty);
                  }));
            }
            return retVal;
        }

        public static readonly DependencyProperty DragScopeProperty =
         DependencyProperty.RegisterAttached("DragScope",
         typeof(FrameworkElement), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void SetDragScope(DependencyObject? element, FrameworkElement? value)
        {
            element?.Dispatcher.Invoke(
                   new Action(() =>
                   {
                       element.SetValue(DragScopeProperty, value);
                   }));
        }
        public static FrameworkElement? GetDragScope(DependencyObject? element)
        {
            FrameworkElement? retVal = null;
            if (element != null)
            {
                retVal = element.Dispatcher.Invoke(
                   new Func<FrameworkElement>(() =>
                   {
                       return (FrameworkElement)element.GetValue(DragScopeProperty);
                   }));

            }
            return retVal;
        }
        public static void SetDragTypes(this UIElement source, params Type[]? types)
        {
            List<Type>? ValidTypes = source.Dispatcher.Invoke(
                   new Func<List<Type>?>(() =>
                   {
                       return (List<Type>?)source.GetValue(ValidDragTypesProperty);
                   }));
            if (ValidTypes != null)
            {
                if (types != null)
                {
                    foreach (Type t in types)
                    {
                        ValidTypes.Add(t);
                    }
                }
            }
        }

        public static void SetInvalidDragTypes(this UIElement source, params Type[]? types)
        {
            List<Type>? InValidDragTypes = source.Dispatcher.Invoke(
                    new Func<List<Type>?>(() =>
                    {
                        return (List<Type>?)source.GetValue(InValidDragTypesProperty);
                    }));
            if (InValidDragTypes != null)
            {
                if (types != null)
                {
                    foreach (Type t in types)
                    {
                        InValidDragTypes.Add(t);
                    }
                }
            }
        }

        public static readonly DependencyProperty InValidDragTypesProperty =
            DependencyProperty.RegisterAttached("InValidDragTypes",
            typeof(List<Type>), typeof(DragHelper), new FrameworkPropertyMetadata());


        public static readonly DependencyProperty ValidDragTypesProperty =
         DependencyProperty.RegisterAttached("ValidDragTypes",
         typeof(List<Type>), typeof(DragHelper), new FrameworkPropertyMetadata());

        public static void InitializeDragging(this UIElement? source, FrameworkElement? scope)
        {
            if (source != null)
            {
                source.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(source_PreviewMouseLeftButtonDown);
                source.PreviewMouseMove += new System.Windows.Input.MouseEventHandler(Source_PreviewMouseMove);

                source.Dispatcher.Invoke(new Action(() =>
                {
                    source.SetValue(ValidDragTypesProperty, new List<Type>());
                    source.SetValue(InValidDragTypesProperty, new List<Type>());
                }));
                SetDragScope(source, scope);
            }
        }
        public static void InitializeDragging(this UIElement source, string TagNameForScope)
        {
            FrameworkElement? scope = null;
            DependencyObject? obj = source;
            while (scope == null && obj != null)
            {
                obj = obj.GetParent();

                if (obj is FrameworkElement item)
                {
                    if (item.GetDragScopeId() == TagNameForScope)
                    {
                        scope = item;
                    }
                }
            }
            scope ??= source as FrameworkElement;
            source.InitializeDragging(scope);
        }
        public static T? FindAncestor<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            if (obj != null)
            {
                var dependObj = obj;
                do
                {
                    dependObj = dependObj.GetParent();
                    if (dependObj is T)
                        return dependObj as T;
                }
                while (dependObj != null);
            }

            return null;
        }

        public static DependencyObject? GetParent(this DependencyObject obj)
        {
            if (obj == null)
                return null;
            if (obj is ContentElement)
            {
                var parent = ContentOperations.GetParent(obj as ContentElement);
                if (parent != null)
                    return parent;
                if (obj is FrameworkContentElement p)
                    return p.Parent;
                return null;
            }

            return VisualTreeHelper.GetParent(obj);
        }
        public static void InitializeDragging(this UIElement source)
        {
            
            source.InitializeDragging(source as FrameworkElement);
        }

        static void Source_GiveFeedback(object? sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            e.Handled = true;
        }
        public static event EventHandler<ArtemisManagerUI.DragStartedEventArgs>? PreviewDragStarted;


        static void Source_PreviewMouseMove(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is DependencyObject obj)
            {
                if (e.LeftButton == MouseButtonState.Pressed && !GetIsDragging(obj))
                {
                    Point position = e.GetPosition(null);
                    Point startPoint = GetStartPoint(obj);

                    if (Math.Abs(position.X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(position.Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        DependencyObject? dragElement = FindAncestor<UIElement>((DependencyObject)e.OriginalSource, obj);
                        if (dragElement != null)
                        {
                            StartDragInProcAdorner(obj, dragElement);
                        }

                    }
                }
            }
        }
        private static DependencyObject? FindAncestor<T>(DependencyObject? current, DependencyObject source)
            where T : DependencyObject
        {
            do
            {

                List<Type> validTypes = source.Dispatcher.Invoke(
                    new Func<List<Type>>(() =>
                    {
                        return (List<Type>)source.GetValue(ValidDragTypesProperty);
                    }));

                List<Type> invalidTypes = source.Dispatcher.Invoke(new Func<List<Type>>(() =>
                {
                    return (List<Type>)source.GetValue(InValidDragTypesProperty);
                }));

                if (current != null)
                {
                    T? wrk = current as T;

                    if (wrk != null && invalidTypes != null && invalidTypes.Count > 0 && invalidTypes.Contains(current.GetType()))
                    {
                        return null;
                    }

                    if ((wrk != null && validTypes.Count == 0) || (validTypes.Count > 0 && validTypes.Contains(current.GetType())))
                    {
                        return current;
                    }
                    if (current is Visual || current is Visual3D)
                    {
                        current = VisualTreeHelper.GetParent(current);
                    }
                    else
                    {
                        current = null;
                    }
                }
            } while (current != null);
            return null;
        }


        private static void StartDragInProcAdorner(DependencyObject source, DependencyObject dragElement)
        {
            // Let's define our DragScope .. In this case it is every thing inside our main window .. 
            FrameworkElement? DragScope = GetDragScope(source); // source as FrameworkElement;// Application.Current.MainWindow.Content as FrameworkElement;

            SetHasDropped(dragElement, false);

            // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
            if (DragScope != null)
            {
                bool previousDrop = DragScope.AllowDrop;
                DragScope.AllowDrop = true;

                // Let's wire our usual events.. 
                // GiveFeedback just tells it to use no standard cursors..  

                GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(Source_GiveFeedback);
                DragScope.GiveFeedback += feedbackhandler;

                // The DragOver event ... 
                DragEventHandler draghandler = new DragEventHandler(DragOver);
                DragScope.PreviewDragOver += draghandler;

                // Drag Leave is optional, but write up explains why I like it .. 
                DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                DragScope.DragLeave += dragleavehandler;

                // QueryContinue Drag goes with drag leave... 


                //Here we create our adorner.. 
                DragAdorner? adorn = null;
                AdornerLayer? layr = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                if (layr != null)
                {
                    adorn = new(DragScope, (UIElement)dragElement, true, 1);

                    SetDragAdorner(DragScope, adorn);

                    layr.Add(adorn);
                    adorn.BringIntoView();
                }

                SetIsDragging(source, true);

                SetDragHasLeftScope(source, false);
                //Finally lets drag drop 
                DataObject data = new(dragElement);

                if (PreviewDragStarted != null)
                {
                    DragStartedEventArgs eArg = new(dragElement);
                    PreviewDragStarted(DragScope, eArg);
                }
                DragDrop.DoDragDrop(DragScope, data, DragDropEffects.Move | DragDropEffects.Copy);

                // Clean up our mess :) 
                DragScope.AllowDrop = previousDrop;

                layr?.Remove(adorn);

                SetDragAdorner(source, null);

                DragScope.GiveFeedback -= feedbackhandler;
                DragScope.DragLeave -= dragleavehandler;
                //DragScope.QueryContinueDrag -= queryhandler;
                DragScope.PreviewDragOver -= draghandler;

                SetIsDragging(source, false);
            }
        }


        static void DragScope_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (GetDragHasLeftScope((DependencyObject)sender))
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }

        static void DragScope_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is DependencyObject me)
            {
                FrameworkElement? drgScope = GetDragScope(me);
                if (e.OriginalSource == drgScope)
                {
                    Point p = e.GetPosition(drgScope);
                    Rect r = VisualTreeHelper.GetContentBounds(drgScope);
                    if (!r.Contains(p))
                    {
                        SetDragHasLeftScope(me, true);
                        e.Handled = true;
                    }
                }
            }
        }
        static void DragOver(object sender, DragEventArgs args)
        {
            if (sender is DependencyObject me)
            {
                DragAdorner? adorn = GetDragAdorner(me);
                if (adorn != null)
                {
                    Point pos = args.GetPosition((IInputElement)me);
                    adorn.LeftOffset = pos.X;

                    adorn.TopOffset = pos.Y;
                }
            }
        }


        static void source_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetStartPoint((DependencyObject)sender, e.GetPosition(null));
        }


        public static void DisposeDragging(this UIElement? source)
        {
            if (source != null)
            {
                source.PreviewMouseLeftButtonDown -= new System.Windows.Input.MouseButtonEventHandler(source_PreviewMouseLeftButtonDown);
                source.PreviewMouseMove -= new System.Windows.Input.MouseEventHandler(Source_PreviewMouseMove);
            }
        }
    }
}
