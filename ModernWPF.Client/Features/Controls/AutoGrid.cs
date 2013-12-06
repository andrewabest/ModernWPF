using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ModernWPF.Client.Features.Controls
{
    /// <summary>
    /// A grid control that automatically lays out children so you don't need to specify grid.row and grid.column
    /// You can specify row and or column definitions as normal or using a string like "auto,auto,10*,50,10*".
    /// If no column definitions are given you can set NumColumns and that many will be auto-created.
    /// You can also set default row height and column width.
    /// Children are then inserted into cells one row at a time from left to right. New rows are created as required.
    /// Handles colspan and rowspanned children.
    /// </summary>
    public class AutoGrid : Grid
    {
        private bool _hasLoaded;

        public override void EndInit()
        {
            Initialize();
            base.EndInit();
        }

        private void Initialize()
        {
            if (!_hasLoaded && !String.IsNullOrWhiteSpace(Rows))
            {
                var heights = GridLengthParser.FromString(Rows);
                foreach (var height in heights)
                {
                    RowDefinitions.Add(new RowDefinition { Height = height });
                }
            }

            if (!_hasLoaded && !String.IsNullOrWhiteSpace(Columns))
            {
                var widths = GridLengthParser.FromString(Columns);
                foreach (var width in widths)
                {
                    ColumnDefinitions.Add(new ColumnDefinition { Width = width });
                }
            }

            _hasLoaded = true;

            for (int i = ColumnDefinitions.Count; i < NumColumns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = ColumnWidth });
            }

            if (RowDefinitions.Count == 0)
            {
                RowDefinitions.Add(new RowDefinition { Height = RowHeight });
            }

            var currentColumn = 0;
            var currentRow = 0;
            var columnCount = ColumnDefinitions.Count == 0 ? 1 : ColumnDefinitions.Count; //if we have no col defs we have one col.

            foreach (var child in Children)
            {
                if (!GetAutoplace((UIElement)child)) continue;

                var childColumnSpan = GetColumnSpan((FrameworkElement)child);
                var childRowSpan = GetRowSpan((FrameworkElement)child);
                var placed = false;

                while (!placed)
                {
                    var cellsLeft = columnCount - currentColumn;

                    if (currentColumn != 0 && cellsLeft < childColumnSpan) //can't fit this row
                    {
                        currentRow = GetNextRowAndAddIfRequired(currentRow);
                        currentColumn = 0;
                        continue;
                    }

                    if (IsFull(currentRow, currentColumn, childColumnSpan))
                    {
                        currentColumn++;
                        continue;
                    }

                    SetColumn((FrameworkElement)child, currentColumn);
                    SetRow((FrameworkElement)child, currentRow);
                    AddSpannedCells(currentRow, currentColumn, childRowSpan, childColumnSpan);
                    currentColumn += childColumnSpan;
                    placed = true;
                }
            }

            if (StretchLastRow)
            {
                RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private bool IsFull(int row, int col, int childColumnSpan)
        {

            for (var i = col; i < col + childColumnSpan; i++)
            {
                if (_spannedCells.Contains(new Tuple<int, int>(row, i))) return true;
            }
            return false;
        }

        private void AddSpannedCells(int currentRow, int currentColumn, int rowSpan, int columnSpan)
        {
            for (var i = currentRow; i < currentRow + rowSpan; i++)
            {
                for (var j = currentColumn; j < currentColumn + columnSpan; j++)
                {
                    _spannedCells.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        private readonly HashSet<Tuple<int, int>> _spannedCells = new HashSet<Tuple<int, int>>();

        private int GetNextRowAndAddIfRequired(int currentRow)
        {
            if (RowDefinitions.Count <= currentRow + 1)
            {
                RowDefinitions.Add(new RowDefinition { Height = RowHeight });
                return RowDefinitions.Count - 1;
            }
            return currentRow + 1;
        }

        #region Dependency Properties

        public static readonly DependencyProperty NumColumnsProperty =
            DependencyProperty.Register("NumColumns", typeof(int), typeof(AutoGrid), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty ColumnWidthProperty =
            DependencyProperty.Register("ColumnWidth", typeof(GridLength), typeof(AutoGrid), new PropertyMetadata(default(GridLength)));

        public static readonly DependencyProperty RowHeightProperty =
            DependencyProperty.Register("RowHeight", typeof(GridLength), typeof(AutoGrid), new PropertyMetadata(default(GridLength)));

        public static readonly DependencyProperty StretchLastRowProperty =
            DependencyProperty.Register("StretchLastRow", typeof(bool), typeof(AutoGrid), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(string), typeof(AutoGrid), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(string), typeof(AutoGrid), new PropertyMetadata(default(string)));

        #endregion

        /// <summary>
        /// Set the height of the last row *
        /// </summary>
        public bool StretchLastRow
        {
            get { return (bool)GetValue(StretchLastRowProperty); }
            set { SetValue(StretchLastRowProperty, value); }
        }

        /// <summary>
        /// The number of columns in the grid
        /// </summary>
        public int NumColumns
        {
            get { return (int)GetValue(NumColumnsProperty); }
            set { SetValue(NumColumnsProperty, value); }
        }

        /// <summary>
        /// The width of each column, can be *, auto or a pixel number
        /// </summary>
        public GridLength ColumnWidth
        {
            get { return (GridLength)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        /// <summary>
        /// The height of each row, can be *, auto or a pixel number
        /// </summary>
        public GridLength RowHeight
        {
            get { return (GridLength)GetValue(RowHeightProperty); }
            set { SetValue(RowHeightProperty, value); }
        }

        /// <summary>
        /// Used to manually define rows using a string like "auto,auto,*,10"
        /// </summary>
        public string Rows
        {
            get { return (string)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Used to manually define columns using a string like "auto,auto,*,10"
        /// </summary>
        public string Columns
        {
            get { return (string)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Use to grid.row and grid.column from being ignored to manually layout a child element. (set autogrid.autoplace="false" on the child)
        /// </summary>
        public static readonly DependencyProperty AutoplaceProperty =
            DependencyProperty.RegisterAttached("Autoplace", typeof(bool), typeof(AutoGrid), new PropertyMetadata(true));

        public static void SetAutoplace(UIElement element, bool value)
        {
            element.SetValue(AutoplaceProperty, value);
        }

        public static bool GetAutoplace(UIElement element)
        {
            return (bool)element.GetValue(AutoplaceProperty);
        }
    }

    public static class GridLengthParser
    {
        private static readonly Regex Star = new Regex(@"(\d*)(\*)");
        private static readonly Regex Pixel = new Regex(@"(\d*)");

        public static IEnumerable<GridLength> FromString(string lengths)
        {
            var l = lengths.Split(',');
            var gl = new List<GridLength>();
            foreach (var ss in l)
            {
                var s = ss.Trim().ToLower();
                if (s == "auto")
                {
                    gl.Add(new GridLength(1, GridUnitType.Auto));
                    continue;
                }
                var star = Star.Match(s);
                if (star.Success)
                {
                    var n = star.Groups[1].Value;
                    gl.Add(String.IsNullOrWhiteSpace(n)
                        ? new GridLength(1, GridUnitType.Star)
                        : new GridLength(Convert.ToDouble(n), GridUnitType.Star));
                    continue;
                }
                var pixel = Pixel.Match(s);
                if (pixel.Success)
                {
                    gl.Add(new GridLength(Convert.ToDouble(s), GridUnitType.Pixel));
                    continue;
                }
                throw new ArgumentException("Invalid grid length in autogrid xaml");
            }
            return gl;
        }
    }
}