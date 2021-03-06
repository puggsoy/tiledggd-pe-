using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TiledGGD.BindingTools
{
    class FilterSet : IFilter, IEnumerable<IFilter>
    {
        #region Fields & Porperties

        #region FilterMethod
        /// <summary>
        /// The FilterMethod this FilterSet uses
        /// </summary>
        private FilterMethod filterMethod;
        /// <summary>
        /// The FilterMethod this FilterSet uses
        /// </summary>
        public FilterMethod FilterMethod { get { return this.filterMethod; } private set { this.filterMethod = value; } }
        #endregion

        #region Filters
        /// <summary>
        /// The Filters in this FilterSet
        /// </summary>
        private List<IFilter> filters;
        #endregion

        #region IsValid
        /// <summary>
        /// If this FilterSet is valid (ie: had been initialzed with a valid XmlNode)
        /// </summary>
        private bool isValid = false;
        /// <summary>
        /// If this FilterSet is valid (ie: had been initialzed with a valid XmlNode)
        /// </summary>
        public bool IsValid { get { return this.isValid; } private set { this.isValid = value; } }
        #endregion

        #endregion

        #region Constructors
        /// <summary>
        /// Create a new FilterSet from an XmlNode
        /// </summary>
        /// <param name="filtersetnode">The XmlNode to create a FilterSet from</param>
        public FilterSet(XmlNode filtersetnode)
        {
            if (filtersetnode == null|| filtersetnode.Name != "FilterSet")
            {
                MainWindow.ShowError("Unable to make a Filterset out of an empty or non-FilterSet XmlNode");
                return;
            }

            this.filters = new List<IFilter>();


            string mstr = filtersetnode.Attributes["method"].Value;
            switch (mstr.ToUpper())
            {
                case "OR": this.FilterMethod = FilterMethod.OR; break;
                case "AND": this.FilterMethod = FilterMethod.AND; break;
                case "XOR": this.FilterMethod = FilterMethod.XOR; break;
                case "NAND": this.FilterMethod = FilterMethod.NAND; break;
                default: MainWindow.ShowError("Invalid FilterSet: invalid FilterMethod " + mstr); return;
            }
            foreach (XmlNode fnode in filtersetnode.SelectNodes("Filter"))
                this.filters.Add(new Filter(fnode));
            foreach (XmlNode fsnode in filtersetnode.SelectNodes("FilterSet"))
                this.filters.Add(new FilterSet(fsnode));

            if (filters.Count == 0)
                MainWindow.ShowError("Invalid FilterSet: a FilterSet should have at least one sub-Filter or sub-filterSet. The FilterSet will now pass anything.");

            this.IsValid = true;
        }
        /// <summary>
        /// Create a new Filterset
        /// </summary>
        /// <param name="filters">The filters this FilterSet has</param>
        /// <param name="method">The method this FilterSet filters with</param>
        public FilterSet(IFilter[] filters, FilterMethod method)
        {
            this.FilterMethod = method;
            this.filters = new List<IFilter>(filters);
            this.IsValid = true;
        }
        #endregion

        #region Filters-accessor
        /// <summary>
        /// Get a filter from this FilterSet
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IFilter this[int index]
        {
            get
            {
                return this.filters[index];
            }
        }
        #endregion

        #region Filter-Count
        /// <summary>
        /// The amount of direct sub-IFilters in this FilterSet
        /// </summary>
        public int Count { get { return this.filters.Count; } }
        #endregion

        #region method: Passes(string)
        /// <summary>
        /// Checks if a file passes this set of filters
        /// </summary>
        /// <param name="filename">The filename (or path) of the file to check</param>
        /// <returns><code>true</code> iff the file passes this set of filters</returns>
        public bool Passes(string filename)
        {
            if (!IsValid)
                return false;

            if (filters.Count == 0)
                return true;
            if (filters.Count == 1)
                return filters[0].Passes(filename);

            switch (this.FilterMethod)
            {
                case FilterMethod.AND:
                    foreach (Filter f in this)
                        if (!f.Passes(filename))
                            return false;
                    return true;
                case FilterMethod.NAND:
                    bool val = this[0].Passes(filename);
                    for (int i = 1; i < this.filters.Count; i++)
                        if (this[i].Passes(filename) != val)
                            return true;
                    return !val;
                case FilterMethod.OR:
                    foreach (Filter f in this)
                        if (f.Passes(filename))
                            return true;
                    return false;
                case FilterMethod.NOR:
                    foreach (Filter f in this)
                        if (f.Passes(filename))
                            return false;
                    return true;
                case FilterMethod.XOR:
                    int passes = 0;
                    foreach (Filter f in this)
                        if ((passes += f.Passes(filename) ? 1 : 0) > 1)
                            return false;
                    return passes == 1;
                default:
                    throw new Exception("Unhandled FilterMethod: " + this.FilterMethod);
            }
        }
        #endregion

        #region IEnumerable<IFilter> Members

        public IEnumerator<IFilter> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            foreach (IFilter f in this.filters)
                f.Dispose();
            this.filters.Clear();
            this.filters = null;
        }

        #endregion
    }
    public enum FilterMethod
    {
        /// <summary>
        /// Pass if all filters pass
        /// </summary>
        AND,
        /// <summary>
        /// Always pass, unless all filters pass
        /// </summary>
        NAND,
        /// <summary>
        /// Pass if at least one of the filters passes
        /// </summary>
        OR,
        /// <summary>
        /// Pass if no filter passes
        /// </summary>
        NOR,
        /// <summary>
        /// Pass if only one filter passes
        /// </summary>
        XOR
    }
}
