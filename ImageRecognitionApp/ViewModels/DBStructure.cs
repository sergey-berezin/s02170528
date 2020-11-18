using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ImageRecognitionApp.ViewModels
{
    class DBStructure
    {
        [Key]
        public int ImageId { get; set; }
        public string ImageName { get; set; }
        public int ImageLabel { get; set; }
        public ICollection<ImageDetailDB> AdditionalInfo { get; set; }
    }
    
    class ImageDetailDB
    {
        [Key]
        public int ImageDetailId { get; set; }

        //byte sequence represents colored image
        public byte[] ByteImage { get; set; }
        public ICollection<DBStructure> PrimaryInfo { get; set; } 
    }

    class MyContext : DbContext
    {
        public DbSet<DBStructure> ProcessedImages { get; set; }
        public DbSet<ImageDetailDB> ImageDetails { get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder o)
            => o.UseSqlite("Data Source=../ModelView/DataBase.db");
    }
}