﻿using System;
using System.Diagnostics;
using System.Linq;

namespace SpecFlow.ExternalData.SpecFlowPlugin.DataSource
{
    public class ExternalDataSpecification
    {
        public DataValue DataSource { get; }

        public ExternalDataSpecification(DataValue dataSource)
        {
            DataSource = dataSource;
        }

        public DataTable GetExampleRecords(string[] examplesHeaderNames)
        {
            //TODO: handle different data sources
            //TODO: handle data sets
            //TODO: handle data fields
            Debug.Assert(DataSource.IsDataTable);
            var dataTable = DataSource.AsDataTable;

            var headerNames = examplesHeaderNames ?? dataTable.Header;

            //TODO: handle not provided columns
            var result = new DataTable(headerNames);
            foreach (var dataRecord in dataTable.Items)
            {
                result.Items.Add(new DataRecord(dataRecord.Fields.Where(f => headerNames.Contains(f.Key))));
            }
            return result;
        }
    }
}
