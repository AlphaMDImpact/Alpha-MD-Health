using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaMDHealth.Model;

public class PatientScanVitalDTO : BaseDTO
{
    public PatientScanVital PatientScanVital { get; set; }

    public List<OptionModel> Posture { get; set; }
    public List<OptionModel> ScanType { get; set; }

    public string PostureID { get; set; }

    public string ScanTypeID { get; set; }
}
