﻿

/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/


using CSHTML5.Internal;
using CSHTML5.Internals.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if MIGRATION
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
#else
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#endif

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    /// <summary>
    /// Represents a selection control that combines a non-editable text box and
    /// a drop-down list box that allows users to select an item from a list.
    /// </summary>
    /// <example>
    /// <code lang="XAML" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    /// <ComboBox x:Name="ComboBox1" DisplayMemberPath="Name" SelectedValuePath="ImagePath" VerticalAlignment="Top"/>
    /// </code>
    /// <code lang="C#">
    /// ComboBox1.ItemsSource = Planet.GetListOfPlanets();
    /// public partial class Planet
    ///{
    ///    public string Name { get; set; }
    ///    public string ImagePath { get; set; }
    ///
    ///
    ///    public static ObservableCollection&lt;Planet&gt; GetListOfPlanets()
    ///    {
    ///        return new ObservableCollection&lt;Planet&gt;()
    ///        {
    ///            new Planet() { Name = "Mercury", ImagePath = "ms-appx:/Planets/Mercury.png" },
    ///            new Planet() { Name = "Venus", ImagePath = "ms-appx:/Planets/Venus.png" },
    ///            new Planet() { Name = "Earth", ImagePath = "ms-appx:/Planets/Earth.png" },
    ///            new Planet() { Name = "Mars", ImagePath = "ms-appx:/Planets/Mars.png" },
    ///            new Planet() { Name = "Jupiter", ImagePath = "ms-appx:/Planets/Jupiter.png" },
    ///            new Planet() { Name = "Saturn", ImagePath = "ms-appx:/Planets/Saturn.png" },
    ///            new Planet() { Name = "Uranus", ImagePath = "ms-appx:/Planets/Uranus.png" },
    ///            new Planet() { Name = "Neptune", ImagePath = "ms-appx:/Planets/Neptune.png" },
    ///            new Planet() { Name = "Pluto", ImagePath = "ms-appx:/Planets/Pluto.png" }
    ///        };
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ComboBox : Selector
    {
        Popup _popup;
        ToggleButton _dropDownToggle;
        ContentPresenter _contentPresenter;
        UIElement _selectedContent;

#if false
        dynamic _nativeComboBoxDomElement;
        private bool _useNativeComboBox;
#endif

        //todo: synchronize html/c# selected value/item/index

#if false
        public bool UseNativeComboBox
        {
            get { return _useNativeComboBox; }
            set
            {
                _useNativeComboBox = value;

                // In case of native combo box, we do not want to have the ControlTemplate of the ComboBox applied:
                base.INTERNAL_DoNotApplyControlTemplate = _useNativeComboBox;
                base.INTERNAL_DoNotApplyStyle = _useNativeComboBox;

                // Set the default Style: //todo: ensure that the user can set another style (what happens if the user style was set before this line? The test below "this.Style == null" may be enough)
                if (!_useNativeComboBox && this.Style == null)
                {
                    //todo-perfs:
                    this.DefaultStyleKey = typeof(ComboBox);
                }
            }
        }
#else
        [Obsolete]
        public bool UseNativeComboBox
        {
            get { return false; }
            set { }
        }
#endif


        /// <summary>
        /// Initializes a new instance of the ComboBox class.
        /// </summary>
        public ComboBox()
        {
            this.DefaultStyleKey = typeof(ComboBox);
#if false
            UseSystemFocusVisuals = true;
            // Call the "setter" of the UseNativeComboBox property, so that it initializes the right state:
            this.UseNativeComboBox = true; // Default value
#endif
        }

#if false
        public override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren)
        {
            if (_useNativeComboBox)
            {
                var select = INTERNAL_HtmlDomManager.CreateDomElementAndAppendIt("select", parentRef, this);
                domElementWhereToPlaceChildren = select;
                _nativeComboBoxDomElement = select;

                INTERNAL_EventsHelper.AttachToDomEvents("change", select, (Action<object>)(e =>
                {
                    DomSelectionChanged(select);
                }));

                INTERNAL_HtmlDomManager.SetDomElementStyleProperty(select, new List<string>() { "fontSize" }, "inherit");

                // Add an empty element that will make it easier to have nothing selected when items are added to the ComboBox: // See: http://stackoverflow.com/questions/8605516/default-select-option-as-blank
                var emptyOption = INTERNAL_HtmlDomManager.AddOptionToNativeComboBox(_nativeComboBoxDomElement, "");
                CSHTML5.Interop.ExecuteJavaScriptAsync("$0.disabled = true", emptyOption);
                CSHTML5.Interop.ExecuteJavaScriptAsync("$0.selected = true", emptyOption);
                CSHTML5.Interop.ExecuteJavaScriptAsync("$0.style.display = 'hidden'", emptyOption);

                // Set the mark saying that the pointer events must be "absorbed" by the ComboBox:
                INTERNAL_HtmlDomManager.SetDomElementProperty(select, "data-absorb-events", true);

                return select;
            }
            else
            {
#if !BRIDGE
                return base.CreateDomElement(parentRef, out domElementWhereToPlaceChildren);
#else
                return CreateDomElement_WorkaroundBridgeInheritanceBug(parentRef, out domElementWhereToPlaceChildren);
#endif
            }
        }
#endif

#if false
        #region Native ComboBox implementation

        void DomSelectionChanged(dynamic element)
        {
            if (_useNativeComboBox)
            {
                if (!ChangingSelectionProgrammatically)
                {
#if WORKINPROGRESS
                    IList oldItems = GetListOfSelectedItemsInCSharp();
                    IList newItems = GetListOfNewlySelectedItems_InCaseOfNativeComboBox(element);
#else
                    IList<object> oldItems = GetListOfSelectedItemsInCSharp();
                    IList<object> newItems = GetListOfNewlySelectedItems_InCaseOfNativeComboBox(element);
#endif
                    //todo: remove the items present in both oldItems and newItems ?
                    var eventArgs = new SelectionChangedEventArgs(oldItems, newItems)
                    {
                        OriginalSource = this,
                    };
                    ChangingSelectionInHtml = true;
                    int selectedIndexInNativeHtmlDom = GetSelectedIndexInNativeHtmlDom();

                    SetCurrentValue(SelectedIndexProperty, selectedIndexInNativeHtmlDom); //we call SetLocalvalue directly to avoid replacing the BindingExpression that could be here on Mode = TwoWay
                    //SelectedIndex = GetSelectedIndex(element);
                    ChangingSelectionInHtml = false;
                    OnSelectionChanged(eventArgs);
                }
            }
        }

        IList<object> GetListOfNewlySelectedItems_InCaseOfNativeComboBox(dynamic e)
        {
            List<object> newlySelectedItems = new List<object>();
            //newlySelectedItems.Add(_nativeComboBoxDomElement.selectedIndex);
            int selectedIndexInNativeHtmlDom = GetSelectedIndexInNativeHtmlDom();
            newlySelectedItems.Add(selectedIndexInNativeHtmlDom);
            return newlySelectedItems;
        }

        public void SetSelectedIndexInNativeHtmlDom(int value)
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && _nativeComboBoxDomElement != null)
            {
                int selectedIndexInHtmlDom = value;

                // Compensate for the fact that the ComboBox contains an empty element at the beginning (see note in the CreateDomElement method):
                if (selectedIndexInHtmlDom >= 0)
                    selectedIndexInHtmlDom += 1;

                INTERNAL_HtmlDomManager.SetDomElementProperty(_nativeComboBoxDomElement, "selectedIndex", selectedIndexInHtmlDom, forceSimulatorExecuteImmediately: true);
            }
        }

        public int GetSelectedIndexInNativeHtmlDom()
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && _nativeComboBoxDomElement != null)
            {
                int selectedIndexInHtmlDom = (int)INTERNAL_HtmlDomManager.GetDomElementAttribute(_nativeComboBoxDomElement, "selectedIndex");

                // Compensate for the fact that the ComboBox contains an empty element at the beginning (see note in the CreateDomElement method):
                if (selectedIndexInHtmlDom >= 0)
                    selectedIndexInHtmlDom -= 1;

                return selectedIndexInHtmlDom;
            }
            else
                return -1;
        }

        #endregion
#endif

        #region Non-Native ComboBox implementation

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            ComboBoxItem container = element as ComboBoxItem;
            if (container != null)
            {
                container.INTERNAL_CorrespondingItem = item;
                container.INTERNAL_ParentSelectorControl = this;
                container.Click += ComboBoxItem_Click;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ComboBoxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is ComboBoxItem);
        }

        protected override SelectorItem INTERNAL_GenerateContainer(object item)
        {
            return (SelectorItem)this.GetContainerFromItem(item);
        }

        protected override DependencyObject GetContainerFromItem(object item)
        {
            ComboBoxItem comboBoxItem = item as ComboBoxItem ?? new ComboBoxItem();
            comboBoxItem.INTERNAL_CorrespondingItem = item;
            comboBoxItem.INTERNAL_ParentSelectorControl = this;
            comboBoxItem.Click += ComboBoxItem_Click;
            return comboBoxItem;
        }

        void ComboBoxItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedContainer = (SelectorItem)sender;
            _selectedContent = sender as UIElement;

            // Select the item:
            this.SelectedItem = selectedContainer.INTERNAL_CorrespondingItem;

            // Close the popup (note: this has other effects as well: see the "IsDropDownOpen_Changed" method):
            if (_dropDownToggle != null)
                _dropDownToggle.IsChecked = false;
        }

        #endregion

#if false
#if WORKINPROGRESS
        IList GetListOfSelectedItemsInCSharp()
#else
        IList<object> GetListOfSelectedItemsInCSharp()
#endif
        {
            List<object> selectedItems = new List<object>();
            selectedItems.Add(SelectedItem);
            return selectedItems;
        }
#endif

#if false
        protected override void AddChildItemToVisualTree(object newItem)
        {
            if (_useNativeComboBox)
            {
                object value = PropertyPathHelper.AccessValueByApplyingPropertyPathIfAny(newItem, DisplayMemberPath);

                //if (value is UIElement)
                //{
                //    SwitchToNonNativeComboBox(); // If the value is a UI element, we should switch to a non-native combo box:
                //}
                //else
                //{
                if (value != null)
                {
                    if (_nativeComboBoxDomElement != null)
                    {
                        var optionDomElement = INTERNAL_HtmlDomManager.AddOptionToNativeComboBox(_nativeComboBoxDomElement, value.ToString());
                        ItemContainerGenerator.INTERNAL_RegisterContainer(optionDomElement, newItem);
                    }
                }
                //todo: else --> ?
                //}
            }
            else
            {
                base.AddChildItemToVisualTree(newItem);
            }
        }
#endif

#if false
        protected override bool TryRemoveChildItemFromVisualTree(object item)
        {
            if (_useNativeComboBox)
            {
                if (_nativeComboBoxDomElement != null)
                {
                    var optionDomElement = ItemContainerGenerator.INTERNAL_ContainerFromItem(item);
                    if (optionDomElement != null)
                    {
                        INTERNAL_HtmlDomManager.RemoveOptionFromNativeComboBox(optionDomElement, _nativeComboBoxDomElement);

                        return ItemContainerGenerator.INTERNAL_TryUnregisterContainer(optionDomElement, item);
                    }
                }
                return false;
            }
            else
            {
                return base.TryRemoveChildItemFromVisualTree(item);
            }
        }
#endif

        protected override void ApplySelectedIndex(int index)
        {
            base.ApplySelectedIndex(index);

#if false
            if (_useNativeComboBox)
            {
                SetSelectedIndexInNativeHtmlDom(this.SelectedIndex);
            }
            else
            {
#endif
            if (this.ItemsHost == null)
            {
                return;
            }

            UIElement newSelectedContent;

            if (index == -1)
            {
                if (_contentPresenter != null)
                {
                    _contentPresenter.Content = null;
                }

                newSelectedContent = null;
            }
            else if (index < this.ItemsHost.Children.Count)
            {
                ComboBoxItem container = this.ItemsHost.Children[index] as ComboBoxItem;
                newSelectedContent = container;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }

            _selectedContent = newSelectedContent;
#if false
            }
#endif
        }

#if MIGRATION
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();

            if (_popup != null)
                _popup.ClosedDueToOutsideClick -= Popup_ClosedDueToOutsideClick; // Note: we do this here rather than at "OnDetached" because it may happen that the popup is closed after the ComboBox has been removed from the visual tree (in which case, when putting it back into the visual tree, we want the drop down to be in its initial closed state).

            _popup = GetTemplateChild("Popup") as Popup;
            _dropDownToggle = GetTemplateChild("DropDownToggle") as ToggleButton;
            _contentPresenter = GetTemplateChild("ContentPresenter") as ContentPresenter;

            if (_dropDownToggle != null)
            {
                _dropDownToggle.Checked += DropDownToggle_Checked;
                _dropDownToggle.Unchecked += DropDownToggle_Unchecked;
            }

            // Make sure the popup gets closed when the user clicks outside the combo box, and listen to the Closed event in order to update the drop-down toggle:
            if (_popup != null)
            {
                _popup.StayOpen = false;
                _popup.ClosedDueToOutsideClick += Popup_ClosedDueToOutsideClick;
            }

            ApplySelectedIndex(SelectedIndex);

            // Put the selected item into the ContentPresenter if the popup is closed
            if (this._contentPresenter != null)
            {
                this._contentPresenter.Content = this._selectedContent;
            }
        }

#if false
        protected internal override void INTERNAL_OnAttachedToVisualTree()
        {
            base.INTERNAL_OnAttachedToVisualTree();

            // We update the ItemsPanel only if we use the native ComboBox. Otherwise, it will be done in the "OnApplyTemplate" method.
            if (_useNativeComboBox)
            {
                UpdateChildrenInVisualTree(Items, Items, forceUpdateAllChildren: true);
            }
        }

        protected internal override void INTERNAL_OnDetachedFromVisualTree()
        {
            base.INTERNAL_OnDetachedFromVisualTree();

            if (_useNativeComboBox)
            {
                ItemContainerGenerator.INTERNAL_Clear();
            }
        }
#endif

        void DropDownToggle_Checked(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = true;
        }

        void DropDownToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;
        }


        /// <summary>
        /// Invoked when the DropDownClosed event is raised.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
#if MIGRATION
        protected virtual void OnDropDownClosed(EventArgs e)
#else
        protected virtual void OnDropDownClosed(RoutedEventArgs e)
#endif
        {
            if (DropDownClosed != null)
                DropDownClosed(this, e);
        }

        /// <summary>
        /// Invoked when the DropDownOpened event is raised.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
#if MIGRATION
        protected virtual void OnDropDownOpened(EventArgs e)
#else
        protected virtual void OnDropDownOpened(RoutedEventArgs e)
#endif
        {
            if (DropDownOpened != null)
                DropDownOpened(this, e);
        }

        /// <summary>
        /// Occurs when the drop-down portion of the ComboBox closes.
        /// </summary>
#if MIGRATION
        public event EventHandler DropDownClosed;
#else
        public event RoutedEventHandler DropDownClosed;
#endif

        /// <summary>
        /// Occurs when the drop-down portion of the ComboBox opens.
        /// </summary>
#if MIGRATION
        public event EventHandler DropDownOpened;
#else
        public event RoutedEventHandler DropDownOpened;
#endif

#if false
        protected override void UpdateItemsPanel(ItemsPanelTemplate newTemplate)
        {
            if (_useNativeComboBox)
            {
                //this is to keep the implementation of UpdateItemsPanel from ItemsControl from interfering with this class' logic.
            }
            else
                base.UpdateItemsPanel(newTemplate);
        }
#endif

        /// <summary>
        /// Gets or sets a value that indicates whether the drop-down portion of the
        /// ComboBox is currently open.
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }
        /// <summary>
        /// Identifies the IsDropDownOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(ComboBox), new PropertyMetadata(false, IsDropDownOpen_Changed)
            { CallPropertyChangedWhenLoadedIntoVisualTree = WhenToCallPropertyChangedEnum.IfPropertyIsSet });

        private static void IsDropDownOpen_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = (ComboBox)d;
#if false
            if (!comboBox._useNativeComboBox)
            {
#endif
            // IMPORTANT: we must NOT require that the element be in the visual tree because the DropDown may be closed even when the ComboBox is not in the visual tree (for example, after it has been removed from the visual tree)
            if (e.NewValue is bool)
            {
                bool isDropDownOpen = (bool)e.NewValue;

                if (isDropDownOpen)
                {
                    //-----------------------------
                    // Show the Popup
                    //-----------------------------

                    // Empty the ContentPresenter so that, in case it is needed, the same item can be placed in the popup:
                    if (comboBox._contentPresenter != null)
                        comboBox._contentPresenter.Content = null;

                    // Show the popup:
                    if (comboBox._popup != null)
                    {
                        // add removed 

                        comboBox._popup.IsOpen = true;

                        // Make sure the Width of the popup is the same as the popup:
                        double actualWidth = comboBox._popup.ActualWidth;
                        if (!double.IsNaN(actualWidth) && comboBox._popup.Child is FrameworkElement)
                            ((FrameworkElement)comboBox._popup.Child).Width = actualWidth;
                    }

                    // Ensure that the toggle button is checked:
                    if (comboBox._dropDownToggle != null
                        && comboBox._dropDownToggle.IsChecked == false)
                    {
                        comboBox._dropDownToggle.IsChecked = true;
                    }

                    // Raise the Opened event:
#if MIGRATION
                    comboBox.OnDropDownOpened(new EventArgs());
#else
                    comboBox.OnDropDownOpened(new RoutedEventArgs());
#endif
                }
                else
                {
                    //-----------------------------
                    // Hide the Popup
                    //-----------------------------

                    // Close the popup:
                    if (comboBox._popup != null)
                        comboBox._popup.IsOpen = false;

                    // Put the selected item back into the ContentPresenter if it was removed when the ToggleButton was checked:
                    if (comboBox._contentPresenter != null && comboBox._contentPresenter.Content == null)
                    {
                        comboBox._contentPresenter.Content = comboBox._selectedContent;
                    }

                    //// Workaround for the fact that IsHitTestVisible does not propagate properly //todo: fix the propagation (which comes from the ComboBox style), and remove this block of code. Current issue when removing this code: if we click multiple times on the ComboBox (selecting and deselecting items), at one point the item in the ContentPresenter does not inherit "transparency to click", meaning that it is no longer possible to click the underlying ToggleButton when clicking on the item.
                    //if (comboBox._contentPresenter != null)
                    //{
                    //    comboBox._contentPresenter.IsHitTestVisible = true;
                    //    comboBox._contentPresenter.IsHitTestVisible = false;
                    //}

                    // Ensure that the toggle button is unchecked:
                    if (comboBox._dropDownToggle != null && comboBox._dropDownToggle.IsChecked == true)
                    {
                        comboBox._dropDownToggle.IsChecked = false;
                    }

                    // Raise the Closed event:
#if MIGRATION
                    comboBox.OnDropDownClosed(new EventArgs());
#else
                    comboBox.OnDropDownClosed(new RoutedEventArgs());
#endif
                }
            }
#if false
        }
#endif
        }

        /// <summary>
        /// Gets or sets the maximum height for a combo box drop-down.
        /// </summary>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the MaxDropDownHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(ComboBox), new PropertyMetadata(200d)
            { CallPropertyChangedWhenLoadedIntoVisualTree = WhenToCallPropertyChangedEnum.IfPropertyIsSet });

        void Popup_ClosedDueToOutsideClick(object sender, EventArgs e)
        {
            //------------------
            // The user clicked outside the combo box, so the Popup closed itself. We now need to reflect this on the ComboBox appearance (drop-down toggle...).
            //------------------

            if (this._dropDownToggle != null)
            {
#if MIGRATION
                // See comment below
                if (this._dropDownToggle.IsMouseCaptured)
                {
                    this._dropDownToggle.ReleaseMouseCapture();
                }

#else
                // In case the pointer is captured by the toggle button, we need to release it because the Click event would be triggered right after the popup was closed, 
                // resulting in the popup to reopen right away.
                // To reproduce the issue that happens if we remove the "if" block of code below: create a ComboBox with items, click the ToggleButton of the ComboBox to
                // open the drop -down popup, then click it again to close the drop-down. Expected result: the drop-down is closed. Actual result: the popup closes and re-opens.
                // The issue was due to the fact that, when we clicked on the ToggleButton to close the drop-down, the toggle button became Unchecked due to the
                // "Popup.ClosedDueToOutsideClick" event (resulting in the popup being successfully closed), but then it reopened because the "PointerReleased" event of the
                // ToggleButton was raised, which re-checked the unchecked ToggleButton. By releasing the capture, we prevent the "PointerReleased" event of the ToggleButton
                // to be raised.
                if (this._dropDownToggle.IsPointerCaptured)
                {
                    this._dropDownToggle.ReleasePointerCapture();
                }
#endif
                this._dropDownToggle.IsChecked = false; // Note: this has other effects as well: see the "IsDropDownOpen_Changed" method
            }
        }

        ////
        //// Summary:
        ////     Gets a value that indicates whether the user can edit text in the text box
        ////     portion of the ComboBox. This property always returns false.
        ////
        //// Returns:
        ////     False in all cases.
        //public bool IsEditable { get { return false; } }

        //// Returns:
        ////     True if the SelectionBoxItem is highlighted; otherwise, false. The default
        ////     is true.
        ///// <summary>
        ///// Gets a value that indicates whether the SelectionBoxItem component is highlighted.
        ///// </summary>
        //public bool IsSelectionBoxHighlighted { get; }

#if WORKINPROGRESS
        //
        // Summary:
        //     Gets the item displayed in the selection box.
        //
        // Returns:
        //     The item displayed in the selection box.
        public object SelectionBoxItem
        {
            get { return null; }
        }
#endif

        ////
        //// Summary:
        ////     Gets the template applied to the selection box content.
        ////
        //// Returns:
        ////     The template applied to the selection box content.
        //public DataTemplate SelectionBoxItemTemplate { get; }

        ////
        //// Summary:
        ////     Gets an object that provides calculated values that can be referenced as
        ////     TemplateBinding sources when defining templates for a ComboBox control.
        ////
        //// Returns:
        ////     An object that provides calculated values for templates.
        //public ComboBoxTemplateSettings TemplateSettings { get; }
    }
}