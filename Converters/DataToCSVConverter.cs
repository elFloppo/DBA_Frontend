using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;
using DBA_Frontend.Models.ViewModels;
using System.Text;

namespace DBA_Frontend.Controllers
{
    public static class DataToCSVConverter
    {
        public static string ConvertDataListToCSV<T>(IEnumerable<T> dataList)
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Join(",", GetPropertyNames(typeof(AbonentVM))));

            foreach (var data in dataList)
            {
                var line = string.Join(",", GetDataLineList(data));
                builder.AppendLine(line);
            }

            return builder.ToString();           
        }

        private static List<object> GetDataLineList<T>(T data)
        {
            var dataLineList = new List<object>();

            var properties = data.GetType().GetProperties();
            foreach (var property in properties)
            {
                dataLineList.Add(property.GetValue(data));
            }

            return dataLineList;
        }

        private static List<object> GetPropertyNames(Type type)
        {
            var propertyNames = new List<object>();

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (attributes.Length < 1 || attributes.First() == null)
                    continue;

                var attribute = (DisplayNameAttribute)attributes.First();
                propertyNames.Add(attribute.DisplayName);
            }

            return propertyNames;
        }
    }
}
