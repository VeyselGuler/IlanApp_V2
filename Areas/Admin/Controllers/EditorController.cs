using System.Net;
using System.Net.Mail;
using Dapper;
using IlanAppv2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace IlanAppv2.Areas.Admin.Controllers;

[Area("Admin")]
public class EditorController : Controller
{
    string connectionString =
        "Server=;Initial Catalog=;User Id=;Password=;TrustServerCertificate=True";


    public IActionResult Index()
    {
        var ilanModel = new IlanModel();
        using (var connection = new SqlConnection(connectionString))
        {
            var sql =
                "SELECT ads.Id ,ads.Name, categories.Name as CategoryName, Price, UpdatedTime, EMail, ImgUrl FROM ads LEFT JOIN categories ON ads.CategoryId = categories.Id WHERE IsApproved = 1";
            var ilans = connection.Query<Ilan>(sql).ToList();
            ilanModel.Ilans = ilans;
        }

        using (var connection = new SqlConnection(connectionString))
        {
            var sql = "SELECT * FROM categories";
            var categories = connection.Query<Category>(sql).ToList();
            ilanModel.Categories = categories;
        }

        return View(ilanModel);
    }

    public IActionResult DeleteCategory(int id)
    {
        using var connection = new SqlConnection(connectionString);
        var sql = "DELETE FROM categories WHERE id = @id";
        var rowEffected = connection.Execute(sql, new { Id = id });

        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "Kategori başarılı bir şekilde silindi.";

        return View("Message");
    }

    [HttpGet]
    public IActionResult UpdateCategory(int id)
    {
        using var connection = new SqlConnection(connectionString);
        var category =
            connection.QueryFirstOrDefault<Category>("SELECT * FROM categories WHERE id = @id", new { Id = id });


        return View(category);
    }

    [HttpPost]
    public IActionResult UpdateCategory(Category model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.MessageCssClass = "alert-danger";
            ViewBag.Message = "Eksik veya hatalı işlem yaptın.";
            return View("Message");
        }

        using var connection = new SqlConnection(connectionString);
        var sql = "UPDATE categories SET name = @name WHERE id = @Id";

        var param = new
        {
            model.Id,
            model.Name
        };

        var rowAffected = connection.Execute(sql, param);

        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "Kategori başarılı bir şekilde güncellendi.";

        return View("Message");
    }

    [HttpPost]
    public IActionResult AddCategory(Category model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.MessageCssClass = "alert-danger";
            ViewBag.Message = "Eksik veya hatalı işlem yaptın.";
            return View("Message");
        }

        using var connection = new SqlConnection(connectionString);
        var sql = "INSERT INTO categories (name) VALUES (@Name)";
        var data = new
        {
            model.Id,
            model.Name
        };

        var rowAffected = connection.Execute(sql, data);

        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "Kategori başarılı bir şekilde Eklendi.";

        return View("Message");
    }

    [HttpPost]
    public IActionResult AddIlan(Ilan model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.MessageCssClass = "alert-danger";
            ViewBag.Message = "Eksik veya hatalı işlem yaptın.";
            return View("Message");
        }

        model.CreatedTime = DateTime.Now;
        model.UpdatedTime = DateTime.Now;

        using var connection = new SqlConnection(connectionString);
        var sql =
            "INSERT INTO ads (name, detail, price, imgurl, categoryid, createdtime, updatedtime, email) VALUES (@Name, @Detail, @Price, @ImgUrl, @CategoryId, @CreatedTime, @UpdatedTime, @Email)";
        var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);

        using var stream = new FileStream(path, FileMode.Create);
        model.Img.CopyTo(stream);
        model.ImgUrl = $"/uploads/{imageName}";

        var data = new
        {
            model.Name,
            model.Detail,
            model.Price,
            model.ImgUrl,
            model.CategoryId,
            model.CreatedTime,
            model.UpdatedTime,
            model.EMail
        };

        var rowAffected = connection.Execute(sql, data);

        // var mailsql =
        //     "SELECT categories.Name as CategoryName FROM ads LEFT JOIN categories ON ads.CategoryId = categories.Id  ";
        // var x = connection.QueryFirstOrDefault<Ilan>(mailsql);


        using var reader = new StreamReader("wwwroot/mailTemp/index.html");
        var template = reader.ReadToEnd();
        var mailBody = template;

        var client = new SmtpClient("smtp.eu.mailgun.org", 587)
        {
            Credentials = new NetworkCredential(),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("no-reply@bildirim.veyselguler.com", "Veysel Pazarlama"),
            Subject = "İlanınız",
            Body = mailBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(model.EMail, "Ilan sahibine"));

        client.Send(mailMessage);

        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "ilanınız onaylandıktan sonra yayınalanacaktır.";

        return View("Message");
    }

    public IActionResult PendingAds()
    {
        using var conneciton = new SqlConnection(connectionString);
        var sql =
            "SELECT ads.Id ,ads.Name, categories.Name as CategoryName, Price, CreatedTime, EMail, ImgUrl, IsApproved FROM ads LEFT JOIN categories ON ads.CategoryId = categories.Id";
        var ads = conneciton.Query<Ilan>(sql).ToList();

        return View(ads);
    }

    public IActionResult ConfirmAd(int id)
    {
        var posta = new Ilan();
        using (var connection = new SqlConnection(connectionString))
        {
            var sql = "UPDATE ads SET IsApproved = 1 WHERE id = @Id";
            var rowAffected = connection.Execute(sql, new { Id = id });
        }

        using (var connection = new SqlConnection(connectionString))
        {
            var mailsql = "SELECT EMail FROM ads WHERE id = @Id";
            var mail = connection.QuerySingleOrDefault<Ilan>(mailsql, new { id });
            posta = mail;
        }


        using var reader = new StreamReader("wwwroot/mailTemp/onay.html");
        var template = reader.ReadToEnd();
        var mailBody = template;

        var client = new SmtpClient("smtp.eu.mailgun.org", 587)
        {
            Credentials = new NetworkCredential(),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("no-reply@bildirim.veysel.com", "Veysel Pazarlama"),
            Subject = "İlanınız",
            Body = mailBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(posta.EMail, "Ilan sahibine"));

        client.Send(mailMessage);

        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "İlan Onaylandı.";
        return View("Message");
    }

    public IActionResult RejectAd(int id)
    {
        var posta = new Ilan();

        using (var connection = new SqlConnection(connectionString))
        {
            var mailsql = "SELECT EMail FROM ads WHERE id = @Id";
            var mail = connection.QuerySingleOrDefault<Ilan>(mailsql, new { id });
            posta = mail;
        }


        using var reader = new StreamReader("wwwroot/mailTemp/red.html");
        var template = reader.ReadToEnd();
        var mailBody = template;

        var client = new SmtpClient("smtp.eu.mailgun.org", 587)
        {
            Credentials = new NetworkCredential(),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("no-reply@bildirim.veysel.com", "Veysel Pazarlama"),
            Subject = "İlanınız",
            Body = mailBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(posta.EMail, "Ilan sahibine"));

        client.Send(mailMessage);

        using (var connection = new SqlConnection(connectionString))
        {
            var sql = "DELETE FROM ads WHERE id = @Id";
            var rowAffected = connection.Execute(sql, new { Id = id });
        }

        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "İlan Silindi.";
        return View("Message");
    }

    [HttpGet]
    public IActionResult UpdateAd(int id)
    {
        using var connection = new SqlConnection(connectionString);
        var ad = connection.QuerySingleOrDefault<Ilan>("SELECT * FROM ads WHERE id = @Id", new {id});


        return View(ad);
    }

    [HttpPost]
    public IActionResult UpdateAd(Ilan model)
    {
        model.UpdatedTime = DateTime.Now;


        using var connection = new SqlConnection(connectionString);
        var sql =
            "UPDATE ads SET name = @Name, detail = @Detail, price = @Price, imgurl = @ImgUrl, updatedtime=@UpdatedTime WHERE id = @Id";
        var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);

        using var stream = new FileStream(path, FileMode.Create);
        model.Img.CopyTo(stream);
        model.ImgUrl = $"/uploads/{imageName}";

        var data = new
        {
            model.Name,
            model.Detail,
            model.Price,
            model.ImgUrl,
            model.CategoryId,
            model.UpdatedTime,
            model.EMail,
            model.Id
        };

        var rowAffected = connection.Execute(sql, data);

        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "İlanınız başarılı bir şekilde güncellendi.";

        return View("Message");
    }

    public IActionResult DeleteAd(int id)
    {
        using var connection = new SqlConnection(connectionString);
        var sql = "DELETE FROM ads WHERE id = @Id";
        var rowAffected = connection.Execute(sql, new {Id = id});
        
        ViewBag.MessageCssClass = "alert-success";
        ViewBag.Message = "İlan başarılı bir şekilde silindi.";

        return View("Message");
    }
}