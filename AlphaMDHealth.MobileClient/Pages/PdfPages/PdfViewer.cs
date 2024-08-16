using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaMDHealth.MobileClient.PdfPages
{
    public class PdfViewer : INotifyPropertyChanged
    {
        private MemoryStream m_pdfDocumentStream;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the memory stream containing the PDF document.
        /// </summary>
        public MemoryStream PdfDocumentStream
        {
            get => m_pdfDocumentStream;
            set
            {
                m_pdfDocumentStream = value;
                OnPropertyChanged("PdfDocumentStream");
            }
        }


        /// <summary>
        /// Initializes a new instance of the PdfViewer class with a PDF document stream.
        /// </summary>
        public PdfViewer(MemoryStream stream)
        {
            m_pdfDocumentStream = stream;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
