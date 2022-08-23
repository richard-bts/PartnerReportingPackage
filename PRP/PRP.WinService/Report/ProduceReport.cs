﻿using Newtonsoft.Json;
using PRP.Domain.Models;
using PRP.Domain.Models.Dto;
using PRP.WinService.ApiServices;
using PRP.WinService.Email; 
using PRP.WinService.Report;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRP.WinService.Report
{
    public class ProduceReport: IProduceReport
    {
        #region Constructor and member variables
        private readonly ILogger<ProduceReport> _logger;
        private readonly IPRPService _PRPService;
        private readonly IEmailService _EmailService;
        private readonly IConfiguration _Configuration;

        public ProduceReport(IPRPService PRPService, IEmailService EmailService, ILogger<ProduceReport> logger)
        {
            _logger = logger;

            _PRPService = PRPService;
            _EmailService = EmailService;

            _Configuration = (IConfiguration)new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                .AddEnvironmentVariables()
                                .Build();
        }
        #endregion

        #region Public Methods


        public bool GenerateAllReportsFor(GetPartnerDto PartnersList)
        {
            try
            {
                string dir = _Configuration.GetSection("EmailSettings:FileLocation").Value;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                else
                {
                    CleanUpTempDir();
                }

                foreach (var PartnerRaport in PartnersList.partner_report_types)
                {
                    if((PartnerRaport.report_name == "POD Report") & (PartnerRaport.active == 1))
                    {
                        GeneratePODReport(PartnersList);
                        
                    }
                    if ((PartnerRaport.report_name == "Exception Report") & (PartnerRaport.active == 1))
                    {
                        GenerateExceptionReport(PartnersList);
                        
                    }
                    if ((PartnerRaport.report_name == "Scan Report") & (PartnerRaport.active == 1))
                    {
                        GenerateScanReport(PartnersList);
                        
                    }
                }

                //List<PartnerDetailDto>? PartnersList = new ();

                //var response = _PRPService.GetPartners();

                //string? content = String.Empty;

                //if (response != null && response.Result != null && response.Result.Result != null)
                //    content = Convert.ToString(response.Result.Result);

                //if (response != null && response.Result != null && !string.IsNullOrEmpty(content))
                //{
                //    PartnersList = JsonConvert.DeserializeObject<List<PartnerDetailDto>>(content);
                //}
                //string dir = _Configuration.GetSection("EmailSettings:FileLocation").Value;
                //if (!Directory.Exists(dir))
                //{
                //    Directory.CreateDirectory(dir);
                //}
                //else
                //{
                //    CleanUpTempDir();
                //}


                //if (Partner != null)
                //{

                //foreach (PartnerDetailDto pd in PartnersList)
                //{
                // switch (PartnersList.report_name)
                // {
                //     case "POD":
                //         GeneratePODReport(PartnersList);
                //         break;
                //     case "EXCEPTION":
                //         GenerateExceptionReport(PartnersList);
                //         break;
                //     case "SCAN":
                //         GenerateScanReport(PartnersList);
                //         break;
                //// }
                // }
                //    }
                return true;
            }
            catch (Exception)
            {
                //    return false;
            }
            return true;
            }
            //public bool GenerateAllReports()
            //{
            //    try
            //    {

            //        List<PartnerDetailDto>? PartnersList = new();

            //        var response = _PRPService.GetPartners();

            //        string? content = String.Empty;

            //        if (response != null && response.Result != null && response.Result.Result != null)
            //            content = Convert.ToString(response.Result.Result);

            //        if (response != null && response.Result != null && !string.IsNullOrEmpty(content))
            //        {
            //            PartnersList = JsonConvert.DeserializeObject<List<PartnerDetailDto>>(content);
            //        }
            //        


            //        if (!Directory.Exists(dir))
            //        {
            //            Directory.CreateDirectory(dir);
            //        }
            //        else { 
            //            CleanUpTempDir();
            //        }



            //        if (PartnersList != null)
            //        {

            //            foreach (PartnerDetailDto pd in PartnersList)
            //            {
            //                switch (pd.ReportName)
            //                {
            //                    case "POD":
            //                        GeneratePODReport(pd);
            //                        break;
            //                    case "EXCEPTION":
            //                        GenerateExceptionReport(pd);
            //                        break;
            //                    case "SCAN":
            //                        GenerateScanReport(pd);
            //                        break;
            //                }
            //            }
            //        }                
            //    }
            //    catch (Exception)
            //    {
            //        return false;
            //    }
            //    return true;
            //}
            public bool GeneratePODReport(GetPartnerDto pd)
            {
                List<PODReportDto>? list = new();
            DateTime dateraport = DateTime.Now.AddDays(-1);
                var response = _PRPService.GetPODReport(dateraport, pd.client_id);;

                string? content = String.Empty;

                if (response != null && response.Result != null && response.Result.Result != null)
                    content = Convert.ToString(response.Result.Result);

                if (response != null && response.Result != null && !string.IsNullOrEmpty(content))
                {
                    list = JsonConvert.DeserializeObject<List<PODReportDto>>(content);
                if (list.Count == 0) { return false; }
                if (list != null && CreatePodCSVFileAndNotifyByEmail(list, pd, dateraport))
                    {
                        Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}",
                            $"POD Report: Client {content.Substring(1, 20)}........{content.Substring(content.Length - 20, 20)}");
                        return true;
                    }
                }
                else
                {
                    return false;
                }

                return false;
            }
        public bool GenerateScanReport(GetPartnerDto pd)
        {
            try
            {
                List<ScanReportDto>? list = new();
                DateTime dateraport = DateTime.Now.AddDays(-1);
                var response = _PRPService.GetScanReport(dateraport, pd.client_id);
                string? content = String.Empty;
                if (response != null && response.Result != null && response.Result.Result != null)
                    content = Convert.ToString(response.Result.Result);

                if (response != null && response.Result != null && !string.IsNullOrEmpty(content) )
                {



                    list = JsonConvert.DeserializeObject<List<ScanReportDto>>(content);
                    if(list.Count == 0) { return false; }
                   if (list != null && CreateScanCSVFileAndNotifyByEmail(list, pd, dateraport) )
                    {
                        Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}",
                            $"SCAN Report: Client {content.Substring(1, 20)}........{content.Substring(content.Length - 20, 20)}");
                        return true;
                    }

                }
                else
                {
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Logger.ForContext("Component", "PRP.WinService").Error("{Message}", ex);
                throw;
            }
        }
        public bool GenerateExceptionReport(GetPartnerDto pd)
        {
            try
            {
                List<ExceptionReportDto>? list = new();
                DateTime dateraport = DateTime.Now.AddDays(-1);
                var response = _PRPService.GetExceptionReport(dateraport, pd.client_id);
                if (response != null && response.Result != null)
                {
                    string? content = Convert.ToString(response.Result.Result);

                    if (!string.IsNullOrEmpty(content))
                    {
                        list = JsonConvert.DeserializeObject<List<ExceptionReportDto>>(content);
                        if (list.Count == 0) { return false; }
                        if (list != null && CreateExceptionCSVFileAndNotifyByEmail(list, pd, dateraport))
                        {
                            Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}",
                                $"EXCEPTION Report: Client {content.Substring(1, 20)}........{content.Substring(content.Length - 20, 20)}");
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region PrivateMethods
        private bool CreatePodCSVFileAndNotifyByEmail(List<PODReportDto> list, GetPartnerDto Partner, DateTime dateraport)
            {
                try
                {
                if (list.Count == 0)
                { return false; }
                DateTime timeNow = dateraport;

                    string hour = timeNow.Hour.ToString();
                    string minute = timeNow.Minute.ToString();
                    string second = timeNow.Second.ToString();
                    string millisecond = timeNow.Millisecond.ToString();

                    string filename = $"{Partner.client_id}_POD Report_{hour}_{minute}_{second}_{millisecond}.csv";
                    string dir = _Configuration.GetSection("EmailSettings:FileLocation").Value;

                    string fullpath = $"{dir}{filename}";

                    using (StreamWriter sw = new StreamWriter(fullpath))
                    {
                        string header = "OrderTrackingID,ClientRefNo,RefNo,DCoName,DContact,DStreet,DStreet2,DCity,DState,DZip,PODcompletionTime";

                        sw.WriteLine(header);

                        foreach (PODReportDto dto in list)
                        {
                            string row = $"{dto.OrderTrackingID},{dto.ClientRefNo},{dto.Barcode},{dto.DCoName},{dto.DContact}," +
                                         $"{dto.DStreet},{dto.DStreet2},{dto.DCity},{dto.DState},{dto.DZip},{dto.PODcompletionTime}";
                            sw.WriteLine(row);
                        }
                    }

                    if (!_EmailService.SendEmail(fullpath, "POD Report", Partner, dateraport))
                    {
                        Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}", $"Email is not sent");
                    }
                    else
                    {
                        Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}", $"Email sent sucessfully.");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Log.Logger.ForContext("Component", "PRP.WinService").Warning("{Message}", e);
                    throw;
                }

            }
        private bool CreateScanCSVFileAndNotifyByEmail(List<ScanReportDto> list, GetPartnerDto Partner, DateTime dateraport)
        {
            try
            {
                if (list.Count == 0)
                { return false; }
                DateTime timeNow = dateraport;

                string hour = timeNow.Hour.ToString();
                string minute = timeNow.Minute.ToString();
                string second = timeNow.Second.ToString();
                string millisecond = timeNow.Millisecond.ToString();

                string filename = $"{Partner.client_id}_SCAN Report_{hour}_{minute}_{second}_{millisecond}.csv";
                string dir = _Configuration.GetSection("EmailSettings:FileLocation").Value;

                string fullpath = $"{dir}{filename}";

                using (StreamWriter sw = new StreamWriter(fullpath))
                {
                    string header = "OrderTrackingID,ClientRefNo,SCANcode,SCANlocation,aTimeStamp";

                    sw.WriteLine(header);

                    foreach (ScanReportDto dto in list)
                    {
                        string row = $"{dto.OrderTrackingID},{dto.ClientRefNo},{dto.SCANcode},{dto.SCANlocation},{dto.aTimeStamp}";
                        sw.WriteLine(row);
                    }
                }

                if (!_EmailService.SendEmail(fullpath, "SCAN Report", Partner, dateraport))
                {
                    Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}", $"Email is not sent");
                }
                else
                {
                    Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}", $"Email sent sucessfully.");
                }

                return true;

            }
           catch (Exception e)
            {
                Console.WriteLine($"Error with send email {e}");
                Log.Logger.ForContext("Component", "PRP.WinService").Warning("{Message}", e);
                throw;
            }
        }
        private bool CreateExceptionCSVFileAndNotifyByEmail(List<ExceptionReportDto> list, GetPartnerDto Partner, DateTime dateraport)
        {
            try
            {
                if (list.Count == 0)
                { return false; }
                DateTime timeNow = dateraport;

                string hour = timeNow.Hour.ToString();
                string minute = timeNow.Minute.ToString();
                string second = timeNow.Second.ToString();
                string millisecond = timeNow.Millisecond.ToString();

                string filename = $"{Partner.client_id}_EXCEPTION Report_{hour}_{minute}_{second}_{millisecond}.csv";
                string dir = _Configuration.GetSection("EmailSettings:FileLocation").Value;

                string fullpath = $"{dir}{filename}";

                using (StreamWriter sw = new StreamWriter(fullpath))
                {
                    string header = "OrderNumber,TrackingNumber,Exception,ExceptionDetails,EventTimestamp,Ship Date,EDD,City,State,Facility Code";

                    sw.WriteLine(header);

                    foreach (ExceptionReportDto dto in list)
                    {
                        string row = $"{dto.OrderNumber},{dto.TrackingNumber},{dto.Exception},{dto.ExceptionDetails},{dto.EventTimestamp}," +
                            $"{dto.ShipDate},{dto.EDD},{dto.City},{dto.State},{dto.FacilityCode}";
                        sw.WriteLine(row);
                    }
                }

                if (!_EmailService.SendEmail(fullpath, "EXCEPTION Report", Partner, dateraport))
                {
                    Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}", $"Email is not sent");
                }
                else
                {
                    Log.Logger.ForContext("Component", "PRP.WinService").Information("{Message}", $"Email sent sucessfully.");
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }
        private void CleanUpTempDir()
        {
            try
            {
                string fullpath = _Configuration.GetSection("EmailSettings:FileLocation").Value;

                foreach (FileInfo fi in new DirectoryInfo(fullpath).GetFiles())
                {
                    fi.Delete();
                }
            }
            catch (Exception e)
            {
                Log.Logger.ForContext("Component", "PRP.WinService").Warning("{Message}", e);
            }

        }
        #endregion
    }
}
