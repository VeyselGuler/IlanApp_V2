using System.Diagnostics;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using IlanAppv2.Models;
using Microsoft.Data.SqlClient;

namespace IlanAppv2.Controllers;

public class HomeController : Controller
{
    string connectionString =
        "Server=;Initial Catalog=;User Id=;Password=;TrustServerCertificate=True";

    public IActionResult Index()
    {
        var IlanModel = new IlanModel();

        using (var connection = new SqlConnection(connectionString))
        {
            var sql = "SELECT TOP 8 ads.Name, categories.Name as CategoryName, Price, UpdatedTime, ImgUrl FROM ads LEFT JOIN categories ON ads.CategoryId = categories.Id WHERE IsApproved = 1 ORDER BY updatedtime DESC ";
            var ilans = connection.Query<Ilan>(sql).ToList();
            IlanModel.Ilans = ilans;
        }

        using (var connection = new SqlConnection(connectionString))
        {
            var sql = "SELECT * FROM Categories";
            var categories = connection.Query<Category>(sql).ToList();
            IlanModel.Categories = categories;
        }


        return View(IlanModel);
    }
}