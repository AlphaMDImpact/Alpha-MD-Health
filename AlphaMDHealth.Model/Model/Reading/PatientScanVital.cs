using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaMDHealth.Model;

public class PatientScanVital
{
    public DateTimeOffset? DateOfBirth { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public List<OptionModel> Gender { get; set; }
    public string GenderID { get; set; }

}
