using System.ComponentModel.DataAnnotations;

namespace IlanAppv2.Models;

public class Ilan
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Detail { get; set; }
    [Required]
    public int Price { get; set; }
    [Required]
    public IFormFile Img { get; set; }
    public string? ImgUrl { get; set; }
    [Required]
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
    public bool IsApproved { get; set; }
    [Required]
    public string EMail { get; set; }

}

public class Category
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
}

public class IlanModel
{
    public List<Ilan> Ilans { get; set; }
    public List<Category> Categories { get; set; }
}

public class UpdateIlan
{
    public Ilan Ilan { get; set; }
    public List<Category> Categories { get; set; }
}
