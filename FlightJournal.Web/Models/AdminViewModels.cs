using FlightJournal.Web.Translations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace FlightJournal.Web.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [LocalizedDisplayName("Email")]
        [EmailAddress]
        public string Email { get; set; }

        [LocalizedDisplayName("Email confirmed")]
        public bool EmailConfirmed { get; set; }

        [LocalizedDisplayName("Roles")]
        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}