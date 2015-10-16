using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public sealed class GetSessionModel
    {
        [Required]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}