ToDo_List_FullStack

A complete full-stack task management system built using ASP.NET Core Web API, Entity Framework Core, SQL Server, and a custom JavaScript + Bootstrap dashboard interface.

This project provides a fully functional task management environment where users can:

Create personal profiles.

Create multiple todo lists for each person.

Add categorized items inside each list.

Assign due dates, mark completion, and track progress.

Manage categories and apply them to tasks.

Upload and attach multiple files (images / PDFs) to each item.

Interact with a clean, responsive front-end UI.

🚀 Features
🔹 Backend (ASP.NET Core Web API)

Full CRUD operations for:

Persons

Todo Lists

List Items

Categories

List Item Files / Attachments

Clear DTO structure separating API models from database entities.

Entity Framework Core with proper relationships:

Person → TodoLists (1:N)

TodoList → ListItems (1:N)

Category → ListItems (optional 1:N)

ListItem → ListItemFiles (1:N)

Delete behavior:

SetNull when deleting a category.

Automatic timestamps for entity creation.

Swagger UI integration for testing all endpoints.

🔹 Frontend (HTML + JavaScript + Bootstrap)

A single-page dashboard built with:

HTML5

Bootstrap 5.3

JavaScript (Fetch API)

Bootstrap Icons

Includes:

Person selection and creation panel

Todo list creation and management

Categories panel with color-coded labels

Items panel with due dates and toggle for completion

Multi-file upload per item with preview and download

A dynamic summary bar showing:

Selected person

Selected list

Total items

Completed items

Progress percentage

All actions dynamically update the UI without page refresh.

📁 Project Structure

ToDo_List_FullStack/
│
├── Controllers/
│ ├── CategoriesController.cs
│ ├── ListItemsController.cs
│ ├── ListItemFilesController.cs
│ ├── Persons.cs
│ └── TodoListsController.cs
│
├── Data/
│ └── AppDbContext.cs
│
├── Models/
│ ├── Category.cs
│ ├── ListItem.cs
│ ├── ListItemFile.cs
│ ├── Person.cs
│ └── TodoList.cs
│
├── Dtos/
│ ├── CategoryDto.cs
│ ├── ListItemDto.cs
│ ├── ListItemFileDto.cs
│ ├── PersonDto.cs
│ └── TodoListDto.cs
│
├── Migrations/
│
├── wwwroot/
│ └── index.html ← Full interactive dashboard UI
│
├── Program.cs
├── appsettings.json
└── ToDo_List.csproj

🔧 Technologies Used
Backend

ASP.NET Core Web API

Entity Framework Core

SQL Server

Frontend

HTML5

Bootstrap 5

JavaScript (Fetch API)

Bootstrap Icons

Tools

Visual Studio / VS Code

Swagger UI

SQL Server Management Studio

📡 API Endpoints Overview
Persons

GET /api/Persons
POST /api/Persons

TodoLists

GET /api/TodoLists/by-person/{personId}
POST /api/TodoLists
DELETE /api/TodoLists/{id}

Categories

GET /api/Categories
GET /api/Categories/{id}
POST /api/Categories
PUT /api/Categories/{id}
DELETE /api/Categories/{id}

ListItems

GET /api/ListItems/by-list/{listId}
POST /api/ListItems
PUT /api/ListItems/{id}
POST /api/ListItems/{id}/toggle
DELETE /api/ListItems/{id}

List Item Files

POST /api/list-items/{itemId}/files
GET /api/list-items/{itemId}/files/{fileId}/download

🖥 How to Run the Project
1. Configure Database

Edit the appsettings.json connection string to match your SQL Server instance.

2. Apply Migrations

Run:
update-database

3. Run the API

dotnet run

📌 Future Improvements

User authentication (JWT)

Drag & drop task sorting

Mobile-friendly layout

Export tasks to PDF / CSV

Dark mode theme

📄 License

This project is for educational and training purposes.

✨ Author

Omar Kukhun
Back-End Developer
GitHub: https://github.com/omar83kn
