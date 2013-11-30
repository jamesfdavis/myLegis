using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace myLegis.Spider.Models
{
    //XML DTO objects
    #region DTO Objects

    [Serializable]
    public class GitBill
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Title { get; set; }
        [XmlElement]
        public string ShortTitle { get; set; }
        [XmlElement]
        public string StatusDate { get; set; }
        [XmlElement]
        public string CurrentStatus { get; set; }
        [XmlElement]
        public string Sponsors { get; set; }
        [XmlElement]
        public List<Activity> Activity { get; set; }
        [XmlElement]
        public List<History> History { get; set; }
        [XmlElement]
        public List<Revision> Revisions { get; set; }
        [XmlIgnore]
        public string Copy { get; set; }
        [XmlElement("CDataElement")]
        public XmlCDataSection Content
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                return doc.CreateCDataSection(Copy);
            }
            set
            {
                Copy = value.Value;
            }
        }
        [XmlElement]
        public List<Minute> Minutes { get; set; }
    }

    [Serializable]
    public class Activity
    {
        [XmlElement("Description")]
        public string Description { get; set; }
        [XmlElement("Date")]
        public DateTime Date { get; set; }
        [XmlElement("Journal")]
        public string Journal { get; set; }
    }

    [Serializable]
    public class History
    {

        [XmlElement("Offered")]
        public string Offered { get; set; }
        [XmlElement("OfferDate")]
        public DateTime? OfferDate { get; set; }
        [XmlElement("PassedHouse")]
        public DateTime? PassedHouse { get; set; }
        [XmlElement("PassSenate")]
        public DateTime? PassedSenate { get; set; }
        [XmlElement("Version")]
        public string Version { get; set; }
        [XmlElement("Title")]
        public string Title { get; set; }

    }

    [Serializable]
    public class Revision
    {
        public string Version { get; set; }
        public string Offer { get; set; }
        public DateTime? Date { get; set; }
        public string BlobId { get; set; } //GitHub Sha ID
    }

    [Serializable]
    public class Minute
    {
        public DateTime DateTime { get; set; }
        public string Committee_Name { get; set; }
        public string Audio_Url { get; set; }
        public string Minutes_Url { get; set; }
        public string Begin_Line { get; set; }
        public string End_Line { get; set; }
        public string Committee { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        [XmlIgnore]
        public string Note { get; set; }
        [XmlElement("CDataElement")]
        public XmlCDataSection NoteData
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                string val = Note;
                val = val.Replace(@"", @" ");
                val = val.Replace(@"�", @" ");
                val = val.Replace(@"", @" ");
                return doc.CreateCDataSection(val);
            }
            set
            {
                string val = value.Value;
                val = val.Replace(@"", @" ");
                val = val.Replace(@"�", @" ");
                Note = val;
            }
        }
    }

    #endregion

    //Git Objects
    #region Git Objects

    public interface IVersionHistory
    {
        String Message { get; set; }
        DateTimeOffset When { get; set; }
        String Author { get; set; }
        String Sha { get; set; }
    }

    public class GitCommit : IVersionHistory
    {
        public String Message { get; set; }
        public DateTimeOffset When { get; set; }
        public String Author { get; set; }
        public String Sha { get; set; }
    }

    #endregion

}
