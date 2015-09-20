using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public class IgnorwAnalysisPropertyAttribute : Attribute
    { }

    public class AnalysisPropertyConverter
    {
        public virtual String ConvertToString(Object Object) { throw new NotImplementedException(); }
        public virtual Object ConvertFromString(String String) { throw new NotImplementedException(); }
    }

    public class GenericConverter : AnalysisPropertyConverter
    {
        private Type DestinationType;
        public GenericConverter(Type DestinationType)
        {
            this.DestinationType = DestinationType;
        }

        public override string ConvertToString(object Object)
        {
            return Object.ToString();
        }

        public override object ConvertFromString(string String)
        {
            return Convert.ChangeType(String, DestinationType);
        }
    }

    public class AnalysisPropertyConverterAttribute : Attribute
    {
        public Type ConverterType;
        public AnalysisPropertyConverterAttribute(Type ConverterType)
        {
            this.ConverterType = ConverterType;
        }

        public AnalysisPropertyConverter MakeConverter()
            {
                return Activator.CreateInstance(ConverterType) as AnalysisPropertyConverter;
            }
    }

    public class DateConverter : AnalysisPropertyConverter
    {
        public override string ConvertToString(object Object)
        {
            return (Object as DateTime?).Value.ToShortDateString();
        }

        public override object ConvertFromString(string String)
        {
            return DateTime.Parse(String);
        }
    }

    public class LeaveAnalysis
    {
        [IgnorwAnalysisProperty]
        public String ReportName { get; protected set; }

        public String Summary;

        [AnalysisPropertyConverter(typeof(DateConverter))]
        public DateTime StartDate { get; set; }

        [AnalysisPropertyConverter(typeof(DateConverter))]
        public DateTime EndDate { get; set; }

        public LeaveAnalysis(LeaveChart Chart)
        {
            this.ReportName = "Unimplemented Report";
            this.Summary = "This report lacks a summary.";
            StartDate = Chart.Weeks[0].SaturdayDate;
            EndDate = DateTime.Now.Date;
        }

        public virtual LeaveAnalysisResults Analyze(LeaveChart Data)
        {
            throw new NotImplementedException();
        }
    }
}
