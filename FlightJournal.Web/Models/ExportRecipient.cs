using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Web;
using FlightJournal.Web.FlightExport;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{

    public class ExportRecipient
    {
        [Key]
        public int ExportRecipientId { get; set; }

        [Required]
        public string Name { get; set; }


        [LocalizedDisplayName("Last updated")]
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        [LocalizedDisplayName("Type of exporter to use")]
        [Required]
        public FlightExporterFactory.FlightExporterType ExporterType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        [LocalizedDisplayName("URL to use for authentication. The actual use of this depends on the selected Exporter implementation")]
        public string AuthenticationUrl { get; set; }

        /// <summary>
        /// URL for uploading data
        /// </summary>
        [LocalizedDisplayName("URL to use for upload. The actual use of this depends on the selected Exporter implementation")]
        [Required]
        public string DeliveryUrl { get; set; }

        /// <summary>
        /// Recipients may have restrictions on size of an upload
        /// </summary>
        [LocalizedDisplayName("Max number of flights to upload in one request. 0 = unrestricted.")]
        public int MaxDeliverySize { get; set; } = 0;
    }
}