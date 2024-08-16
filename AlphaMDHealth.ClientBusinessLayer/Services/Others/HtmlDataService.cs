using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.ComponentModel;
using System.Text;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class HtmlDataService : BaseService
    {
        public HtmlDataService(IEssentials essentials) : base(essentials)
        {
            
        }
        /// <summary>
        /// This method creats HTML data 
        /// </summary>
        /// <param name="FileName">Name of file</param>
        /// <param name="OrgDetail">Organisation Data</param>
        /// <param name="Header">List of Headers</param>
        /// <param name="tableData">Data Table</param>
        /// <returns>Return HMTL data</returns>
        public string CreateHtml(string FileName, List<string> OrgDetail, Dictionary<string, string> Header, List<TableModel> tableData)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><title>" + FileName + "</title><style></style></head>");
            html.Append("<body>");
            html.Append("<p><br></p>");

            //Organisation Details
            html.Append("<table style='width: 100%;'>");
            html.Append("<tbody>");
            html.Append("<tr>");
            if (!string.IsNullOrWhiteSpace(OrgDetail[0]))
            {
                html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                    <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center  ltr'>
                        <div class='row px-0 mx-0 d-flex justify-content-center'>
                                <img src='" + OrgDetail[0] + @"' class='avatar-circle' style='height:60px; width:60px; border-radius:8px;'>
                        </div>
                    </div>
                </div></td>");
            }
            else
            {
                string OrgInitial = OrgDetail[1].Substring(0, 1);
                html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center ltr'>
                    <div class='row px-0 mx-0 d-flex justify-content-center'>
                        <div class='avatar-circle d-flex' style='border: 1px solid;height:60px; width:60px; border-radius:8px;'>
                            <span class='w-available' style='padding-left: 10px;width: 100%;font-size: 50px;'>" + OrgInitial + @"</span>
                        </div>
                    </div>
                </div>
            </div></td>");
            }
            html.Append("<td style='font-weight: bold;width: 90%;'>" + OrgDetail[1] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + OrgDetail[2] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + OrgDetail[3] + "</td>");
            html.Append("</tr>");
            html.Append("</tbody>");
            html.Append("</table>");
            html.Append("<br>");

            //Header Section
            html.Append("<table style='width: 100%;'>");
            html.Append("<tbody>");
            for (var i = 0; i < Header.Count; i += 2)
            {
                html.Append("<tr>");
                html.Append("<td style='width: 49.9567%;'><label>" + Header.ElementAt(i).Key + "</label><br><label style='font-weight: bold;'>" + Header.ElementAt(i).Value + "</label></td>");
                html.Append("<td style='width: 49.9567%;'><label>" + Header.ElementAt(i + 1).Key + "</label><br><label style='font-weight: bold;'>" + Header.ElementAt(i + 1).Value + "</label></td>");
                html.Append("</tr>");
            }
            html.Append("</tbody>");
            html.Append("</table>");
            html.Append("<br>");
            //Table Section
            html.Append(CreateTable(tableData));
            html.Append("</body></html>");
            return html.ToString();
        }

        private string CreateTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            StringBuilder sb = new StringBuilder();
            sb.Append("<table style='width: 100%;'>");
            sb.Append("<tbody>");
            sb.Append("<tr>");
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                sb.Append("<td style='border-bottom: 0.5px dotted #afafaf;'><div style='text-align: left;'>" + prop.Name + "</div></td>");
            }
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("<tbody>");
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                sb.Append("<tr>");
                for (int i = 0; i < values.Length; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("<td style='width: 50%;border-bottom: 0.5px dotted #afafaf;'>" + props[i].GetValue(item) + "</td>");
                    }
                    else
                    {
                        sb.Append("<td style='font-weight: bold;width: 50%;border-bottom: 0.5px dotted #afafaf;'>" + props[i].GetValue(item) + "</td>");
                    }
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            return sb.ToString();
        }

        public string CreateHtml(string FileName, List<string> OrgDetail, List<string> PatientAndCaregiverDetail, PatientMedicationDTO medicationData)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><title>" + FileName + "</title>");
            html.Append("<style>");
            html.Append(".progressbar li {");
            html.Append("list-style: none;");
            html.Append("display: inline-table;");
            html.Append("width: 9.33%;");
            html.Append("position: relative;");
            html.Append("}");

            html.Append(".progressbar li:before {");
            html.Append("content: \"\";");
            html.Append("counter-increment: step;");
            html.Append("width: 30px;");
            html.Append("height: 30px;");
            html.Append("line-height: 10px;");
            html.Append("border: 1px solid #020202;");
            html.Append("border-radius: 100%;");
            html.Append("display: block;");
            html.Append("text-align: center;");
            html.Append("margin: 0 auto 10px -1px;");
            html.Append("background-color: #fff;");
            html.Append("}");

            html.Append(".progressbar li:after {");
            html.Append("content: \"\";");
            html.Append("position: absolute;");
            html.Append("width: 100%;");
            html.Append("height: 1px;");
            html.Append("background-color: #020202;");
            html.Append("top: 15px;");
            html.Append("left: -100%;");
            html.Append("z-index: -1;");
            html.Append("}");

            html.Append(".progressbar li:first-child:after {");
            html.Append("content: none;");
            html.Append("}");
            html.Append("</style></head>");

            html.Append("<body>");
            html.Append("<p><br></p>");

            //Organisation Details
            html.Append("<table style='width: 100%;'>");
            html.Append("<tbody>");
            html.Append("<tr>");
            if (!string.IsNullOrWhiteSpace(OrgDetail[0]))
            {
                html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                    <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center  ltr'>
                        <div class='row px-0 mx-0 d-flex justify-content-center'>
                                <img src='" + OrgDetail[0] + @"' class='avatar-circle' style='height:85px; width:180px; border-radius:8px;'>
                        </div>
                    </div>
                </div></td>");
            }
            else
            {
                string OrgInitial = OrgDetail[1].Substring(0, 1);
                html.Append(@"<td style='width: 10%;' rowspan='3'><div class='col margin-vertical-xs px-0'>
                <div class='lbl-secondary-text-body-medium-regular d-flex justify-content-center ltr'>
                    <div class='row px-0 mx-0 d-flex justify-content-center'>
                        <div class='avatar-circle d-flex' style='border: 1px solid;height:60px; width:60px; border-radius:8px;'>
                            <span class='w-available' style='padding-left: 10px;width: 100%;font-size: 50px;'>" + OrgInitial + @"</span>
                        </div>
                    </div>
                </div>
            </div></td>");
            }
            html.Append("<td style='font-weight: bold;width: 90%; font-size= " + LabelType.PrimaryMediumBoldLeft + "'>" + OrgDetail[1] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + OrgDetail[2] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + OrgDetail[3] + "</td>");
            html.Append("</tr>");
            html.Append("</tbody>");
            html.Append("</table>");
            html.Append("<br>");

            //Header Section
            html.Append("<table style='width: 100%;'>");
            html.Append("<tbody>");
            html.Append("<td style='font-weight: bold;width: 90%; font-size= " + LabelType.PrimaryMediumBoldLeft + "'>" + PatientAndCaregiverDetail[0] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + PatientAndCaregiverDetail[1] + "</td>");
            html.Append("</tr>");
            html.Append("<tr>");
            html.Append("<td style='width: 90%;'>" + PatientAndCaregiverDetail[2] + "</td>");
            html.Append("</tr>");
            html.Append("</tbody>");
            html.Append("</table>");
            html.Append("<br>");
            html.Append("<br>");

            html.Append("<table style='width: 100%;'>");
            html.Append("<tbody>");
            foreach (var items in medicationData.Medications)
            {
                html.Append("<tr>");
                html.Append("<td style='font-weight: bold;'>" + items.ShortName + "</td>");
                html.Append("</tr>");
                html.Append("<tr>");
                html.Append("<td>" + items.FormattedDate + "</td>");
                html.Append("</tr>");
                html.Append("<tr>");
                html.Append("<td>" + items.HowOftenString + "</td>");
                html.Append("</tr>");
                html.Append("<tr>");
                html.Append("<td>");
                html.Append("<ul class='progressbar' style='padding-left:5px;'>");
                foreach (var frequency in items.FrequencyTypeOptions)
                {
                    if (frequency.IsSelected)
                    {
                        html.Append("<li>" + frequency.OptionText + "</li>"); // Add a step for selected options
                    }
                    else
                    {
                        html.Append("<li></li>"); // Add an empty step for non-selected options
                    }
                }
                html.Append("</ul>");
                html.Append("</td>");
                html.Append("</tr>");
                int checkboxCount = 0;
                StringBuilder rowHtml = new StringBuilder();

                foreach (var notes in items.AdditionalNotesOptions)
                {
                    if (notes.IsSelected)
                    {
                        if (checkboxCount % 3 == 0)
                        {
                            // Start a new row
                            rowHtml.Append("<tr style='display:block;'>");
                        }

                        rowHtml.Append("<td><input type='checkbox' name='notesOption' value='" + notes.OptionText + "' checked/>" + notes.OptionText + "</td>");

                        if (checkboxCount % 3 == 2)
                        {
                            // End the row after three checkboxes
                            rowHtml.Append("</tr>");
                            html.Append(rowHtml.ToString());
                            rowHtml.Clear();
                        }

                        checkboxCount++;
                    }
                }

                // If the last row is not filled with three checkboxes, close the row.
                if (checkboxCount % 3 != 0)
                {
                    rowHtml.Append("</tr>");
                    html.Append(rowHtml.ToString());
                }
                html.Append("<tr><td colspan='3'><hr/></td></tr>");

            }

            html.Append("</tbody>");
            html.Append("</table>");
            html.Append("<br>");

            //Table Section
            //html.Append(CreateTable(tableData));
            html.Append("</body></html>");
            return html.ToString();
        }

    }
}
