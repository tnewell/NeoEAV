using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using NeoEAV.Data.DataClasses;

using Attribute = NeoEAV.Data.DataClasses.Attribute;


namespace NeoEAV.Web.UI
{
    internal enum ControlType { None, Label, TextBox, DropDown }

    #region Custom Event Argument Classes
    public partial class FixedInstanceEventArgs : EventArgs
    {
        public string ContainerName { get; set; }
        public int InstanceCount { get; set; }
        public string SequenceAttributeName { get; set; }
        public IEnumerable<string> SequenceAttributeValues { get; set; }
    }

    public partial class FixedValueEventArgs : EventArgs
    {
        public string AttributeName { get; set; }
        public int InstanceIndex { get; set; }
        public IEnumerable<string> Values { get; set; }
    }

    public partial class ValueRepeaterColumnsEventArgs : EventArgs
    {
        public string ContainerName { get; set; }
        public int ColumnCount { get; set; }
    }

    public partial class ContainerHeaderEventArgs : EventArgs
    {
        public string ContainerName { get; set; }
        public bool UseHeaders { get; set; }
    }
    #endregion

    #region Value Control Classes
    [Serializable]
    internal class ValueControlState
    {
        public ValueControlState(Attribute attribute)
        {
            ValueControlType = (attribute.HasFixedValues && attribute.FixedValues.Count() == 1) ? ControlType.Label : (attribute.HasFixedValues || attribute.DataTypeID == EAVDataType.Boolean) ? ControlType.DropDown : ControlType.TextBox;
            UnitsControlType = !attribute.HasVariableUnits && !attribute.Units.Any() ? ControlType.None : attribute.HasVariableUnits ? ControlType.TextBox : attribute.Units.Count() > 1 ? ControlType.DropDown : ControlType.Label;
        }

        public ControlType ValueControlType { get; set; }
        public ControlType UnitsControlType { get; set; }
    }

    public partial class NeoEAVValueControl : CompositeControl, ITextControl
    {
        #region Properties
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return (HtmlTextWriterTag.Table);
            }
        }

        // Cast of Parent property
        public ValueRepeaterDataItem MyParent { get { return (Parent as ValueRepeaterDataItem); } }

        // Faux DataSource property
        public AttributeValuePair MyDataSource { get { return (MyParent.MyDataItem); } }

        public string Caption
        {
            get
            {
                return (((ITextControl)FindControl("ctlCaption")).Text);
            }
            set
            {
                ((ITextControl)FindControl("ctlCaption")).Text = value;
            }
        }

        public string Text
        {
            get
            {
                return (((ITextControl)FindControl("ctlValue")).Text);
            }
            set
            {
                ((ITextControl)FindControl("ctlValue")).Text = value;
            }
        }

        public string Units
        {
            get
            {
                ITextControl ctl = FindControl("ctlUnits") as ITextControl;
                return (ctl != null && !String.IsNullOrWhiteSpace(ctl.Text) ? ctl.Text : null);
            }
            set
            {
                ITextControl ctl = FindControl("ctlUnits") as ITextControl;
                if (ctl != null)
                    ctl.Text = String.IsNullOrWhiteSpace(value) ? null : value;
            }
        }
        #endregion

        public bool CaptionsAsHeaders { get; set; }

        internal ValueControlState ValueControlState { get; set; }

        protected override void CreateChildControls()
        {
            Controls.Add(new LiteralControl("<tr>"));

            // Caption
            Controls.Add(new LiteralControl(String.Format("<td class='{0}'>", CaptionsAsHeaders ? "neoEAVValueEmptyCaption" : "neoEAVValueCaption")));
            Controls.Add(new Label() { ID = "ctlCaption", CssClass = "neoEAVValueCaptionLabel" });
            Controls.Add(new LiteralControl("</td>"));

            // Value
            Controls.Add(new LiteralControl(String.Format("<td class='{0}'>", CaptionsAsHeaders ? "neoEAVValueValueNoCaption" : "neoEAVValueValue")));

            switch (ValueControlState.ValueControlType)
            {
                case ControlType.DropDown:
                    Controls.Add(new DropDownList() { ID = "ctlValue", CssClass = "neoEAVValueValueList", Enabled = this.Enabled });
                    break;
                case ControlType.Label:
                    Controls.Add(new Label { ID = "ctlValue", CssClass = "neoEAVValueValueLabel" });
                    break;
                case ControlType.TextBox:
                    Controls.Add(new TextBox() { ID = "ctlValue", CssClass = "neoEAVValueValueText", Enabled = this.Enabled });
                    break;
                default:
                    throw (new InvalidOperationException("Type for value control is not specified, cannot create."));
            }

            Controls.Add(new LiteralControl("</td>"));

            // Units
            Controls.Add(new LiteralControl("<td class='neoEAVValueUnits'>"));

            switch (ValueControlState.UnitsControlType)
            {
                case ControlType.DropDown:
                    Controls.Add(new DropDownList() { ID = "ctlUnits", CssClass = "neoEAVValueUnitsList", Enabled = this.Enabled });
                    break;
                case ControlType.Label:
                    Controls.Add(new Label() { ID = "ctlUnits", CssClass = "neoEAVValueUnitsLabel" });
                    break;
                case ControlType.None:
                    Controls.Add(new LiteralControl("&nbsp;"));
                    break;
                case ControlType.TextBox:
                    Controls.Add(new TextBox() { ID = "ctlUnits", CssClass = "neoEAVValueUnitsText", Enabled = this.Enabled });
                    break;
            }

            Controls.Add(new LiteralControl("</td>"));

            Controls.Add(new LiteralControl("</tr>"));
        }

        private void BindCaption()
        {
            Caption = CaptionsAsHeaders ? String.Empty : MyDataSource.Attribute.DisplayName;
        }

        private void BindValues()
        {
            DropDownList ctlValue = FindControl("ctlValue") as DropDownList;
            if (ctlValue != null)
            {
                if (MyDataSource.Attribute.HasFixedValues)
                {
                    ctlValue.DataSource = MyDataSource.Attribute.FixedValues;
                }
                else if (MyDataSource.Attribute.DataTypeID == EAVDataType.Boolean)
                {
                    ctlValue.DataSource = new string[] { String.Empty, Boolean.FalseString, Boolean.TrueString };
                }
            }

            if (MyDataSource.Value != null)
                Text = MyDataSource.Value.RawValue;
            else
                Text = MyDataSource.Attribute.FixedValues.FirstOrDefault();
        }

        private void BindUnits()
        {
            DropDownList ctlUnits = FindControl("ctlUnits") as DropDownList;
            if (ctlUnits != null)
            {
                ctlUnits.DataSource = MyDataSource.Attribute.Units.Select(it => it.Symbol);
            }

            if (MyDataSource.Value != null)
                Units = MyDataSource.Value.Units;
        }

        protected override void OnDataBinding(EventArgs e)
        {
            BindCaption();
            BindValues();
            BindUnits();
        }
    }
    #endregion

    #region Value Repeater Classes
    public partial class AttributeValuePair
    {
        public Attribute Attribute { get; set; }
        public Value Value { get; set; }
    }

    public partial class ValueItemTemplate : ITemplate
    {
        public void InstantiateIn(Control container)
        {
            ValueRepeaterDataItem dataItem = container as ValueRepeaterDataItem;

            string containerTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "div" : "table";
            string rowTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "div" : "tr";
            string cellTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "span" : "td";

            switch (dataItem.ItemType)
            {
                case ListItemType.Header:
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0} class='neoEAVValueSet'>", containerTag)));
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0}><{1} class='{2}'{3}></{1}></{0}>", rowTag, cellTag, "neoEAVValueHeader", dataItem.TagKey == HtmlTextWriterTag.Div ? String.Empty : String.Format(" colspan='{0}'", dataItem.ColumnCount))));
                    break;
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                    if ((dataItem.ItemIndex % dataItem.ColumnCount) == 0)
                        dataItem.Controls.Add(new LiteralControl(String.Format("<{0}>", rowTag)));

                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0} class='{1}'>", cellTag, "neoEAVValueItem")));
                    dataItem.Controls.Add(new NeoEAVValueControl()
                    {
                        ID = ValueRepeaterDataItem.ValueControlID,
                        CssClass = "neoEAVValue",
                        CaptionsAsHeaders = dataItem.CaptionsAsHeaders,
                        ValueControlState = dataItem.ValueControlState,
                        Enabled = dataItem.Enabled,
                    });
                    dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", cellTag)));

                    if (dataItem.ItemIndex == dataItem.ColumnCount - 1)
                        dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", rowTag)));

                    break;
                case ListItemType.Separator:
                    break;
                case ListItemType.Footer:
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0}><{1} class='{2}'{3}></{1}></{0}>", rowTag, cellTag, "neoEAVValueFooter", dataItem.TagKey == HtmlTextWriterTag.Div ? String.Empty : String.Format(" colspan='{0}'", dataItem.ColumnCount))));
                    dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", containerTag)));
                    break;
            }
        }
    }

    public partial class ValueRepeaterDataItem : RepeaterItem
    {
        internal const string ValueControlID = "ctlValueControl";

        #region Properties
        // Cast of Parent property
        public ValueRepeater MyParent { get { return (Parent as ValueRepeater); } }

        // Cast of DataItem property
        public AttributeValuePair MyDataItem { get { return (DataItem as AttributeValuePair); } }

        // Primary control
        public NeoEAVValueControl ValueControl { get { return (FindControl(ValueControlID) as NeoEAVValueControl); } }
        #endregion

        #region Passthrough Properties
        public int DataItemCount { get; set; }

        public int ColumnCount { get; set; }

        internal HtmlTextWriterTag TagKey { get; set; }

        internal bool Enabled { get; set; }

        internal bool CaptionsAsHeaders { get; set; }

        internal ValueControlState ValueControlState { get; set; }
        #endregion

        public ValueRepeaterDataItem(int itemIndex, ListItemType itemType) : base(itemIndex, itemType) { }
    }

    public partial class ValueRepeater : Repeater
    {
        #region Recast Properties
        // Cast of Parent property
        public ContainerInstanceRepeaterDataItem MyParent { get { return (Parent as ContainerInstanceRepeaterDataItem); } }

        // Cast of DataSource property
        public IEnumerable<AttributeValuePair> MyDataSource
        {
            get { return (DataSource as IEnumerable<AttributeValuePair>); }
            set { DataSource = value; }
        }

        // Cast of Items property
        public IEnumerable<NeoEAVValueControl> Values
        {
            get
            {
                return (Items.OfType<ValueRepeaterDataItem>().Select(it => it.ValueControl));
            }
        }
        #endregion

        public HtmlTextWriterTag TagKey
        {
            get
            {
                if (ViewState["TagKey"] == null)
                    TagKey = HtmlTextWriterTag.Table;

                return ((HtmlTextWriterTag)ViewState["TagKey"]);
            }
            set
            {
                if (value != HtmlTextWriterTag.Table && value != HtmlTextWriterTag.Div)
                    throw (new ArgumentException("Legal values are 'Table' or 'Div'."));

                ViewState["TagKey"] = value;
            }
        }

        public bool Enabled { get { return (((bool?)ViewState["Enabled"]).GetValueOrDefault()); } set { ViewState["Enabled"] = value; } }

        #region Base Method Overrides
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (HeaderTemplate == null)
                HeaderTemplate = new ValueItemTemplate();

            if (ItemTemplate == null)
                ItemTemplate = new ValueItemTemplate();

            if (AlternatingItemTemplate == null)
                AlternatingItemTemplate = new ValueItemTemplate();

            if (SeparatorTemplate == null)
                SeparatorTemplate = new ValueItemTemplate();

            if (FooterTemplate == null)
                FooterTemplate = new ValueItemTemplate();
        }

        protected override RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
        {
            ValueControlState state = itemType == ListItemType.Item || itemType == ListItemType.AlternatingItem ? MyParent.MyParent.MyParent.ValueControlStateList.ElementAt(itemIndex) : null;

            return (new ValueRepeaterDataItem(itemIndex, itemType)
            {
                DataItemCount = MyParent.MyParent.MyParent.ValueDataItemCount,
                ColumnCount = Math.Max(Math.Min(MyParent.MyParent.MyParent.ValueColumnCount, MyParent.MyParent.MyParent.ValueDataItemCount), 1),
                CaptionsAsHeaders = MyParent.Headers.Any(),
                ValueControlState = state,
                Enabled = this.Enabled,
                TagKey = this.TagKey,
            });
        }
        #endregion

        internal void ExtractData(IEnumerable<AttributeValuePair> data, ContainerInstance anInstance)
        {
            var dataPairs = this.Values.Zip(data, (ctl, obj) => new Tuple<NeoEAVValueControl, AttributeValuePair>(ctl, obj));

            var inserts = dataPairs.Where(it => !String.IsNullOrWhiteSpace(it.Item1.Text) && it.Item2.Value == null);
            var updates = dataPairs.Where(it => !String.IsNullOrWhiteSpace(it.Item1.Text) && it.Item2.Value != null && !String.Equals(it.Item1.Text, it.Item2.Value.RawValue));
            var deletes = dataPairs.Where(it => String.IsNullOrWhiteSpace(it.Item1.Text) && it.Item2.Value != null && !String.IsNullOrWhiteSpace(it.Item2.Value.RawValue));
            var unchanged_values = dataPairs.Where(it => !String.IsNullOrWhiteSpace(it.Item1.Text) && it.Item2.Value != null && String.Equals(it.Item1.Text, it.Item2.Value.RawValue));

            if (anInstance.Container.HasFixedInstances && !updates.Any() && !inserts.Any() && unchanged_values.All(it => it.Item2.Attribute.HasFixedValues && it.Item2.Attribute.FixedValues.Count() == 1))
            {
                // Do deletes
                foreach (var dataPair in deletes)
                {
                    dataPair.Item2.Value.RawValue = null;
                    dataPair.Item2.Value.Units = null;
                }

                // Special delete for singleton fixed value fields
                foreach (var dataPair in unchanged_values)
                {
                    dataPair.Item2.Value.RawValue = null;
                }
            }
            else if (!anInstance.Container.HasFixedInstances || inserts.Any(it => !it.Item2.Attribute.HasFixedValues || it.Item2.Attribute.FixedValues.Count() != 1) || updates.Any() || deletes.Any())
            {
                foreach (var dataPair in deletes)
                {
                    dataPair.Item2.Value.RawValue = null;
                    dataPair.Item2.Value.Units = null;
                }

                foreach (var dataPair in updates)
                {
                    dataPair.Item2.Value.RawValue = dataPair.Item1.Text;

                    if (!String.Equals(dataPair.Item1.Units, dataPair.Item2.Value.Units))
                        dataPair.Item2.Value.Units = dataPair.Item1.Units;
                }

                foreach (var dataPair in inserts)
                {
                    //Value aValue = dataPair.Item2.Attribute.CreateNewValue();
                    Value aValue = new Value()
                        {
                            ContainerInstance = anInstance,
                            Attribute = dataPair.Item2.Attribute,
                            RawValue = dataPair.Item1.Text,
                            Units = dataPair.Item1.Units
                        };

                    //aValue.RawValue = dataPair.Item1.Text;
                    //aValue.Units = dataPair.Item1.Units;

                    dataPair.Item2.Value = aValue;

                    //anInstance.Values.Add(aValue);
                    //anInstance.AddValue(aValue);
                }

                foreach (var dataPair in unchanged_values)
                {
                    if (!String.Equals(dataPair.Item1.Units, dataPair.Item2.Value.Units))
                        dataPair.Item2.Value.Units = dataPair.Item1.Units;
                }
            }
        }
    }
    #endregion

    #region Container Instance Repeater Classes
    public partial class ContainerInstancePair
    {
        public Container Container { get; set; }
        public ContainerInstance Instance { get; set; }

        public IEnumerable<AttributeValuePair> AttributeValuePairs
        {
            get
            {
                var values = Instance != null ? Instance.Values : Enumerable.Empty<Value>();

                return (Container.Attributes.GroupJoin(values, at => at.AttributeID, val => val.Attribute.AttributeID, (at, val) => new AttributeValuePair() { Attribute = at, Value = val.FirstOrDefault() }));
            }
        }

        public IEnumerable<ContainerInstanceSet> ChildContainerInstanceSets
        {
            get
            {
                var instances = Instance != null ? Instance.ChildContainerInstances : Enumerable.Empty<ContainerInstance>();

                return (Container.ChildContainers.GroupJoin(instances, cont => cont.ContainerID, inst => inst.Container.ContainerID, (cont, inst) => new ContainerInstanceSet() { Container = cont, Instances = inst.ToList() }));
            }
        }
    }

    public partial class ContainerInstanceItemTemplate : ITemplate
    {
        public void InstantiateIn(Control container)
        {
            ContainerInstanceRepeaterDataItem dataItem = container as ContainerInstanceRepeaterDataItem;
            int nHeaders = dataItem.Headers.Count();

            string containerTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "div" : "table";
            string rowTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "div" : "tr";
            string cellTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "span" : "td";

            switch (dataItem.ItemType)
            {
                case ListItemType.Header:
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0} class='neoEAVInstanceSet'>", containerTag)));
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0}><{1} class='{2}'{3}>Instance Repeater (Header)</{1}></{0}>", rowTag, cellTag, "neoEAVInstanceHeader", dataItem.TagKey == HtmlTextWriterTag.Div ? String.Empty : String.Format(" colspan='{0}'", dataItem.ColumnCount * Math.Max(nHeaders, 1)))));

                    if (dataItem.Headers.Any())
                    {
                        dataItem.Controls.Add(new LiteralControl(String.Format("<{0}>", rowTag)));

                        int width = 100 / nHeaders;
                        foreach (string header in dataItem.Headers)
                        {
                            dataItem.Controls.Add(new LiteralControl(String.Format("<{0} class='neoEAVValueHeaderCaption' width='{2}%'>{1}</{0}>", cellTag, header, width)));
                        }

                        dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", rowTag)));
                    }

                    break;
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                    if ((dataItem.ItemIndex % dataItem.ColumnCount) == 0)
                        dataItem.Controls.Add(new LiteralControl(String.Format("<{0}>", rowTag)));

                    string cssClass = dataItem.ItemType == ListItemType.Item ? "neoEAVInstanceItem" : "neoEAVInstanceAlternatingItem";
                    if (dataItem.ItemIndex == 0) cssClass += " neoEAVInstanceFirstItem";
                    if (dataItem.ItemIndex == dataItem.DataItemCount - 1) cssClass += " neoEAVInstanceLastItem";

                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0} class='{1}'{2}>", cellTag, cssClass, dataItem.TagKey == HtmlTextWriterTag.Div ? String.Empty : String.Format(" colspan='{0}'", dataItem.ColumnCount * Math.Max(nHeaders, 1)))));
                    dataItem.Controls.Add(new ValueRepeater() { ID = ContainerInstanceRepeaterDataItem.ValueRepeaterID, TagKey = dataItem.TagKey, Enabled = dataItem.Enabled });
                    dataItem.Controls.Add(new ContainerRepeater() { ID = ContainerInstanceRepeaterDataItem.ContainerRepeaterID, TagKey = dataItem.TagKey, Enabled = dataItem.Enabled });
                    dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", cellTag)));

                    if (dataItem.ItemIndex == dataItem.ColumnCount - 1)
                        dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", rowTag)));
                    break;
                case ListItemType.Separator:
                    break;
                case ListItemType.Footer:
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0}><{1} class='{2}'{3}>Instance Repeater (Footer)</{1}></{0}>", rowTag, cellTag, "neoEAVInstanceFooter", dataItem.TagKey == HtmlTextWriterTag.Div ? String.Empty : String.Format(" colspan='{0}'", dataItem.ColumnCount * Math.Max(nHeaders, 1)))));
                    dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", containerTag)));
                    break;
            }
        }
    }

    public partial class ContainerInstanceRepeaterDataItem : RepeaterItem
    {
        internal const string ValueRepeaterID = "ctlValueRepeater";
        internal const string ContainerRepeaterID = "ctlContainerRepeater";

        #region Recast Properties
        // Cast of Parent property
        public ContainerInstanceRepeater MyParent { get { return (Parent as ContainerInstanceRepeater); } }

        // Cast of DataItem property
        public ContainerInstancePair MyDataItem { get { return (DataItem as ContainerInstancePair); } }

        // Primary control
        public ValueRepeater ValueRepeater { get { return (FindControl(ValueRepeaterID) as ValueRepeater); } }

        // Primary control
        public ContainerRepeater ContainerRepeater { get { return (FindControl(ContainerRepeaterID) as ContainerRepeater); } }
        #endregion

        #region Passthrough Properties
        public int DataItemCount { get; set; }

        public int ColumnCount { get; set; }

        internal HtmlTextWriterTag TagKey { get; set; }

        internal bool Enabled { get; set; }

        internal IEnumerable<string> Headers { get; set; }
        #endregion

        public ContainerInstanceRepeaterDataItem(int itemIndex, ListItemType itemType) : base(itemIndex, itemType) { }

        #region Base Method Overrides
        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            if (MyDataItem != null)
            {
                // Set the fixed values for this instance
                foreach (Attribute anAttribute in MyDataItem.Container.Attributes.Where(it => it.HasFixedValues))
                {
                    MyParent.MyParent.GetFixedValues(anAttribute, this.ItemIndex);
                }

                // Values
                if (MyDataItem.AttributeValuePairs.Any())
                {
                    ValueRepeater.MyDataSource = MyDataItem.AttributeValuePairs;
                }

                // Containers
                if (MyDataItem.ChildContainerInstanceSets.Any())
                {
                    ContainerRepeater.MyDataSource = MyDataItem.ChildContainerInstanceSets;
                }
            }
        }
        #endregion
    }

    public partial class ContainerInstanceRepeater : Repeater
    {
        #region Recast Properties
        // Cast of Parent property
        public ContainerRepeaterDataItem MyParent { get { return (Parent as ContainerRepeaterDataItem); } }

        // Cast of DataSource property
        public IEnumerable<ContainerInstancePair> MyDataSource
        {
            get { return (DataSource as IEnumerable<ContainerInstancePair>); }
            set { DataSource = value; }
        }

        // Cast of Items property for values
        public IEnumerable<ValueRepeater> Values
        {
            get
            {
                return (Items.OfType<ContainerInstanceRepeaterDataItem>().Select(it => it.ValueRepeater));
            }
        }

        // Cast of Items property for containers
        public IEnumerable<ContainerRepeater> Containers
        {
            get
            {
                return (Items.OfType<ContainerInstanceRepeaterDataItem>().Select(it => it.ContainerRepeater));
            }
        }
        #endregion

        public HtmlTextWriterTag TagKey
        {
            get
            {
                if (ViewState["TagKey"] == null)
                    TagKey = HtmlTextWriterTag.Table;

                return ((HtmlTextWriterTag)ViewState["TagKey"]);
            }
            set
            {
                if (value != HtmlTextWriterTag.Table && value != HtmlTextWriterTag.Div)
                    throw (new ArgumentException("Legal values are 'Table' or 'Div'."));

                ViewState["TagKey"] = value;
            }
        }

        public bool Enabled { get { return (((bool?)ViewState["Enabled"]).GetValueOrDefault()); } set { ViewState["Enabled"] = value; } }

        #region Base Method Overrides
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (HeaderTemplate == null)
                HeaderTemplate = new ContainerInstanceItemTemplate();

            if (ItemTemplate == null)
                ItemTemplate = new ContainerInstanceItemTemplate();

            if (AlternatingItemTemplate == null)
                AlternatingItemTemplate = new ContainerInstanceItemTemplate();

            if (SeparatorTemplate == null)
                SeparatorTemplate = new ContainerInstanceItemTemplate();

            if (FooterTemplate == null)
                FooterTemplate = new ContainerInstanceItemTemplate();
        }

        protected override RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
        {
            Container aContainer = MyDataSource != null && MyDataSource.Any() ? MyDataSource.First().Container : null;

            return (new ContainerInstanceRepeaterDataItem(itemIndex, itemType)
            {
                DataItemCount = MyDataSource != null ? MyDataSource.Count() : 0,
                ColumnCount = 1,
                Headers = MyParent.Headers,
                Enabled = this.Enabled,
                TagKey = this.TagKey,
            });
        }
        #endregion

        internal void ExtractData(IEnumerable<ContainerInstancePair> data, ContainerInstance anInstance)
        {
            foreach (var dataPair in this.Values.Zip(data, (ctl, obj) => new Tuple<ValueRepeater, ContainerInstancePair>(ctl, obj)))
            {
                if (dataPair.Item2.Instance == null) // Insert
                {
                    //ContainerInstance newInstance = dataPair.Item2.Container.CreateNewContainerInstance();
                    ContainerInstance newInstance = new ContainerInstance()
                        {
                            Container = dataPair.Item2.Container,
                            ParentContainerInstance = anInstance,
                            //Subject = null,
                        };
                    
                    dataPair.Item2.Instance = newInstance;

                    //if (anInstance != null)
                        //anInstance.AddChildContainer(newInstance);
                }

                dataPair.Item1.ExtractData(dataPair.Item2.AttributeValuePairs, dataPair.Item2.Instance);
            }

            foreach (var dataPair in this.Containers.Zip(data, (ctl, obj) => new Tuple<ContainerRepeater, ContainerInstancePair>(ctl, obj)))
            {
                dataPair.Item1.ExtractData(dataPair.Item2.ChildContainerInstanceSets, dataPair.Item2.Instance);
            }
        }
    }
    #endregion

    #region Container Repeater Classes
    public partial class ContainerInstanceSet
    {
        public Container Container { get; set; }
        public IList<ContainerInstance> Instances { get; set; }

        public IEnumerable<ContainerInstancePair> ContainerInstancePairs
        {
            get
            {
                List<ContainerInstancePair> pairs = new List<ContainerInstancePair>();

                foreach (ContainerInstance anInstance in Instances != null ? Instances : Enumerable.Empty<ContainerInstance>())
                    pairs.Add(new ContainerInstancePair() { Container = this.Container, Instance = anInstance });

                return (pairs);
            }
        }
    }

    public partial class ContainerItemTemplate : ITemplate
    {
        public void InstantiateIn(Control container)
        {
            ContainerRepeaterDataItem dataItem = container as ContainerRepeaterDataItem;

            string containerTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "div" : "table";
            string rowTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "div" : "tr";
            string cellTag = dataItem.TagKey == HtmlTextWriterTag.Div ? "span" : "td";

            switch (dataItem.ItemType)
            {
                case ListItemType.Header:
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0} class='neoEAVContainerSet'>", containerTag)));
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0}><{1} class='{2}'{3}></{1}></{0}>", rowTag, cellTag, "neoEAVContainerHeader", dataItem.TagKey == HtmlTextWriterTag.Div ? String.Empty : String.Format(" colspan='{0}'", dataItem.ColumnCount))));
                    break;
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                    if ((dataItem.ItemIndex % dataItem.ColumnCount) == 0)
                        dataItem.Controls.Add(new LiteralControl(String.Format("<{0}>", rowTag)));

                    string cssClass = dataItem.ItemType == ListItemType.Item ? "neoEAVContainerItem" : "neoEAVContainerAlternateItem";
                    if (dataItem.ItemIndex == 0) cssClass += " neoEAVContainerFirstItem";
                    if (dataItem.ItemIndex == dataItem.DataItemCount - 1) cssClass += " neoEAVContainerLastItem";

                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0} class='{1}'>", cellTag, cssClass)));
                    dataItem.Controls.Add(new ContainerInstanceRepeater() { ID = ContainerRepeaterDataItem.InstanceRepeaterID, TagKey = dataItem.TagKey, Enabled = dataItem.Enabled });
                    dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", cellTag)));

                    if (dataItem.ItemIndex == dataItem.ColumnCount - 1)
                        dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", rowTag)));
                    break;
                case ListItemType.Separator:
                    break;
                case ListItemType.Footer:
                    dataItem.Controls.Add(new LiteralControl(String.Format("<{0}><{1} class='{2}'{3}></{1}></{0}>", rowTag, cellTag, "neoEAVContainerFooter", dataItem.TagKey == HtmlTextWriterTag.Div ? String.Empty : String.Format(" colspan='{0}'", dataItem.ColumnCount))));
                    dataItem.Controls.Add(new LiteralControl(String.Format("</{0}>", containerTag)));
                    break;
            }
        }
    }

    public partial class ContainerRepeaterDataItem : RepeaterItem
    {
        internal const string InstanceRepeaterID = "ctlContainerInstanceRepeater";

        #region Recast Properties
        // Cast of Parent property
        public ContainerRepeater MyParent { get { return (Parent as ContainerRepeater); } }

        // Cast of DataItem property
        public ContainerInstanceSet MyDataItem { get { return (DataItem as ContainerInstanceSet); } }

        // Primary control
        public ContainerInstanceRepeater InstanceRepeater { get { return (FindControl(InstanceRepeaterID) as ContainerInstanceRepeater); } }
        #endregion

        #region Passthrough Properties
        public int DataItemCount { get; set; }

        public int ColumnCount { get; set; }

        internal HtmlTextWriterTag TagKey { get; set; }

        internal bool Enabled { get; set; }
        #endregion

        // State
        internal int ValueDataItemCount { get { return ((int)ViewState["ValueDataItemCount"]); } set { ViewState["ValueDataItemCount"] = value; } }
        internal int ValueColumnCount { get { return ((int)ViewState["ValueColumnCount"]); } set { ViewState["ValueColumnCount"] = value; } }
        internal IEnumerable<ValueControlState> ValueControlStateList { get { return (ViewState["ValueControlStateList"] as IEnumerable<ValueControlState>); } set { ViewState["ValueControlStateList"] = value; } }
        internal IEnumerable<string> Headers { get { return (ViewState["Headers"] as IEnumerable<string>); } set { ViewState["Headers"] = value; } }

        public ContainerRepeaterDataItem(int itemIndex, ListItemType itemType) : base(itemIndex, itemType) { }

        #region Base Method Overrides
        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            if (MyDataItem != null)
            {
                // We're gathering all this data and caching it away because it influences the way in which the UI
                // is generated and we'll need it on postback to properly reconstruct the control tree. If we don't
                // ASP will lose it's everlovin' digital mind.

                // TODO: The count should be used to set up labels which should be set in DataBind

                // Are we rendering captions as headers and if so, collect those values for later.
                GetHeaderInfo(MyDataItem.Container);

                // Count of attributes
                ValueDataItemCount = MyDataItem.Container.Attributes.Count();

                // Figure out how many columns the user wants for the value repeater control.
                GetValueColumnCount();

                // Set fixed values for our attributes so we can determine what controls should represent those attributes.
                foreach (Attribute anAttribute in MyDataItem.Container.Attributes.Where(it => it.HasFixedValues))
                {
                    // Note that we only need to do this once instead of for each instance, as we'll do later,
                    // because all we really need to know is the number of fixed values, not their actual value.
                    GetFixedValues(anAttribute, 0);
                }

                ValueControlStateList = MyDataItem.Container.Attributes.Select(it => new ValueControlState(it)).ToList();

                // TODO: Render Container Header Text

                // Figure out what instances are associated with this container
                // so we can set up the instance repeater's data source.
                MyParent.GetInstances(MyDataItem);

                InstanceRepeater.MyDataSource = MyDataItem.ContainerInstancePairs;
            }
        }
        #endregion

        internal void GetValueColumnCount()
        {
            ValueRepeaterColumnsEventArgs args = new ValueRepeaterColumnsEventArgs() { ColumnCount = 1, ContainerName = MyDataItem.Container.Name };

            RaiseBubbleEvent(this, args);

            ValueColumnCount = args.ColumnCount;
        }

        internal void GetHeaderInfo(Container aContainer)
        {
            ContainerHeaderEventArgs args = new ContainerHeaderEventArgs() { ContainerName = aContainer.Name };

            RaiseBubbleEvent(this, args);

            Headers = args.UseHeaders ? aContainer.Attributes.Select(it => it.DisplayName).ToList() : Enumerable.Empty<string>();
        }

        internal void GetFixedValues(Attribute anAttribute, int instanceIndex)
        {
            FixedValueEventArgs args = new FixedValueEventArgs() { AttributeName = anAttribute.Name, InstanceIndex = instanceIndex };

            RaiseBubbleEvent(this, args);

            anAttribute.FixedValues = args.Values.ToList();
        }
    }

    public partial class ContainerRepeater : Repeater
    {
        #region Recast Properties
        // Cast of Parent property
        public ContainerInstanceRepeaterDataItem MyParent { get { return (Parent as ContainerInstanceRepeaterDataItem); } }

        // Cast of DataSource property
        public IEnumerable<ContainerInstanceSet> MyDataSource
        {
            get { return (DataSource as IEnumerable<ContainerInstanceSet>); }
            set { DataSource = value; }
        }

        // Cast of Items property
        public IEnumerable<ContainerInstanceRepeater> ContainerInstances
        {
            get
            {
                return (Items.OfType<ContainerRepeaterDataItem>().Select(it => it.InstanceRepeater));
            }
        }
        #endregion

        public HtmlTextWriterTag TagKey
        {
            get
            {
                if (ViewState["TagKey"] == null)
                    TagKey = HtmlTextWriterTag.Table;

                return ((HtmlTextWriterTag)ViewState["TagKey"]);
            }
            set
            {
                if (value != HtmlTextWriterTag.Table && value != HtmlTextWriterTag.Div)
                    throw (new ArgumentException("Legal values are 'Table' or 'Div'."));

                ViewState["TagKey"] = value;
            }
        }

        public bool Enabled { get { return (((bool?)ViewState["Enabled"]).GetValueOrDefault()); } set { ViewState["Enabled"] = value; } }

        #region Base Method Overrides
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (HeaderTemplate == null)
                HeaderTemplate = new ContainerItemTemplate();

            if (ItemTemplate == null)
                ItemTemplate = new ContainerItemTemplate();

            if (AlternatingItemTemplate == null)
                AlternatingItemTemplate = new ContainerItemTemplate();

            if (SeparatorTemplate == null)
                SeparatorTemplate = new ContainerItemTemplate();

            if (FooterTemplate == null)
                FooterTemplate = new ContainerItemTemplate();
        }

        protected override RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
        {
            return (new ContainerRepeaterDataItem(itemIndex, itemType)
            {
                DataItemCount = MyDataSource != null ? MyDataSource.Count() : 0,
                ColumnCount = 1,
                Enabled = this.Enabled,
                TagKey = this.TagKey,
            });
        }
        #endregion

        internal void GetInstances(ContainerInstanceSet data)
        {
            if (data != null)
            {
                if (data.Container.HasFixedInstances)
                {
                    FixedInstanceEventArgs args = new FixedInstanceEventArgs() { ContainerName = data.Container.Name };

                    RaiseBubbleEvent(this, args);

                    data.Instances = args.SequenceAttributeValues.GroupJoin(data.Instances, at => at, inst => inst.Values.Single(it => it.Attribute.Name == args.SequenceAttributeName).RawValue, (at, inst) => inst.FirstOrDefault()).ToList();
                }
                else
                {
                    if (!data.Instances.Any() || data.Container.IsRepeating)
                        data.Instances.Add(null);
                }
            }
        }

        internal void ExtractData(IEnumerable<ContainerInstanceSet> data, ContainerInstance anInstance)
        {
            for (int index = 0; index < data.Count(); ++index)
            {
                ContainerInstanceSet dataSet = data.ElementAt(index);

                GetInstances(dataSet);

                var instancePairs = dataSet.ContainerInstancePairs;

                ContainerInstances.ElementAt(index).ExtractData(instancePairs, anInstance);

                dataSet.Instances = instancePairs.Select(it => it.Instance).ToList();
            }
        }

        public ContainerInstance ExtractData()
        {
            if (MyDataSource != null && MyDataSource.Any())
            {
                ExtractData(MyDataSource, null);

                return (MyDataSource.Single().ContainerInstancePairs.Single().Instance);
            }

            return (null);
        }
    }
    #endregion
}

