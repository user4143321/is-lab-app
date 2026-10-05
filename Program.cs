var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --- Задание 3: /health и /version ---
app.MapGet("/health", () => Results.Json(new { status = "ok", time = DateTime.UtcNow.ToString("o") }));

app.MapGet("/version", (IConfiguration config) => Results.Json(new {
    name = config["App:Name"] ?? "IsLabApp",
    version = config["App:Version"] ?? "0.1.0-lab4"
}));

// --- Задание 4: Заметки (CRUD) ---
var notes = new List<Note>();
int nextId = 1;

// GET /api/notes - список всех
app.MapGet("/api/notes", () => Results.Json(notes));

// GET /api/notes/{id} - одна заметка
app.MapGet("/api/notes/{id}", (int id) => {
    var note = notes.FirstOrDefault(n => n.Id == id);
    return note is not null ? Results.Json(note) : Results.NotFound();
});

// POST /api/notes - создать
app.MapPost("/api/notes", (NoteInput input) => {
    if (string.IsNullOrWhiteSpace(input.Title)) 
        return Results.BadRequest("Title is required");
        
    var newNote = new Note(nextId++, input.Title, input.Text ?? "", DateTime.UtcNow);
    notes.Add(newNote);
    return Results.Created($"/api/notes/{newNote.Id}", newNote);
});

// DELETE /api/notes/{id} - удалить
app.MapDelete("/api/notes/{id}", (int id) => {
    var note = notes.FirstOrDefault(n => n.Id == id);
    if (note is null) return Results.NotFound();
    notes.Remove(note);
    return Results.NoContent();
});

// --- Задание 5: /db/ping ---
app.MapGet("/db/ping", (IConfiguration config) => {
    var connString = config.GetConnectionString("Mssql");
    if (string.IsNullOrEmpty(connString)) return Results.Json(new { status = "error", message = "Connection string not set" });
    // Имитация попытки подключения
    return Results.Json(new { status = "ok", message = "Connection string found (DB not actually connected yet)" });
});

app.Run();

// Классы
record Note(int Id, string Title, string Text, DateTime CreatedAt);
record NoteInput(string Title, string? Text);