using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb.Models;

public class BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}