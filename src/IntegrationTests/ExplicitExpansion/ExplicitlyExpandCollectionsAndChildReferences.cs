﻿using AutoMapper.UnitTests;
using Shouldly;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
    public class ExplicitlyExpandCollectionsAndChildReferences : AutoMapperSpecBase
    {
        TrainingCourseDto _course;

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateProjection<Category, CategoryDto>();
            cfg.CreateProjection<TrainingCourse, TrainingCourseDto>().ForMember(c => c.Content, o => o.ExplicitExpansion());
            cfg.CreateProjection<TrainingContent, TrainingContentDto>().ForMember(c => c.Category, o => o.ExplicitExpansion());
        });

        protected override void Because_of()
        {
            using (var context = new ClientContext())
            {
                context.Database.Log = s => Trace.WriteLine(s);
                _course = ProjectTo<TrainingCourseDto>(context.TrainingCourses, null, c => c.Content.Select(co => co.Category)).FirstOrDefault(n => n.CourseName == "Course 1");
            }
        }


        [Fact]
        public void Should_expand_collections_items()
        {
            _course.Content[0].Category.CategoryName.ShouldBe("Category 1");
        }

        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                var category = new Category { CategoryName = "Category 1" };
                var course = new TrainingCourse { CourseName = "Course 1" };
                context.TrainingCourses.Add(course);
                var content = new TrainingContent { ContentName = "Content 1", Category = category };
                context.TrainingContents.Add(content);
                course.Content.Add(content);
            }
        }

        class ClientContext : DbContext
        {
            public ClientContext()
            {
                Database.SetInitializer(new Initializer());
            }
            public DbSet<Category> Categories { get; set; }
            public DbSet<TrainingCourse> TrainingCourses { get; set; }
            public DbSet<TrainingContent> TrainingContents { get; set; }
        }

        public class TrainingCourse
        {
            [Key]
            public int CourseId { get; set; }

            public string CourseName { get; set; }

            public virtual IList<TrainingContent> Content { get; set; } = new List<TrainingContent>();
        }

        public class TrainingContent
        {
            [Key]
            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public Category Category { get; set; }
        }

        public class Category
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
        }


        public class TrainingCourseDto
        {
            public int CourseId { get; set; }

            public string CourseName { get; set; }

            public virtual IList<TrainingContentDto> Content { get; set; }
        }

        public class CategoryDto
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
        }

        public class TrainingContentDto
        {
            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public CategoryDto Category { get; set; }
        }
    }
}
