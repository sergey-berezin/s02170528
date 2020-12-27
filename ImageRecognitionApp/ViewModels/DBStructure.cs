using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ImageRecognitionApp.ViewModels
{
    public class ImageDetails
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
    }
    public class ImageData
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public float Confidence { get; set; }
        virtual public ImageDetails Details { get; set; }
        public int count { get; set; }
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<ImageData> Images { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder o)
            => o.UseSqlite("Data Source=/Users/maximkurkin/Downloads/Lab1/s02170258/ImageRecognitionApp/ViewModels/DataBase.db");
    }
}