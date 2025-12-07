using Microsoft.EntityFrameworkCore;
using System;
using ToDo_List.Models;

namespace ToDo_List.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ListItem> ListItems => Set<ListItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Person>()
            .HasMany(p => p.TodoLists)
            .WithOne(t => t.Person)
            .HasForeignKey(t => t.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<TodoList>()
            .HasMany(t => t.Items)
            .WithOne(i => i.TodoList)
            .HasForeignKey(i => i.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Category>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        b.Entity<ListItem>()
            .HasIndex(i => new { i.TodoListId, i.SortOrder });
    }
}
