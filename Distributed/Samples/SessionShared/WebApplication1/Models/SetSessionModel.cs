using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public sealed class SetSessionModel
    {
        [Required]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}