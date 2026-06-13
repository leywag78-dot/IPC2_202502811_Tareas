using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

List<NodoAVL> arbol = new List<NodoAVL> 
{ 
    new NodoAVL { Id = 30, Etiqueta = "Raíz FE:-2", Altura = 3 },
    new NodoAVL { Id = 10, Etiqueta = "Hijo Izq FE:+1", Altura = 2 } 
};

bool rotado = false;

app.MapGet("/api/arbol", () => 
{
    return Results.Ok(new 
    { 
        estado = rotado ? "balanceado" : "desbalanceado (zig-zag)",
        nodos = arbol,
        total = arbol.Count 
    });
});

app.MapPost("/api/arbol/insertar", (NodoAVL nodo) =>
{
    if (nodo.Id <= 0)
        return Results.BadRequest(new { error = "ID debe ser mayor a 0" });
    
    if (arbol.Any(n => n.Id == nodo.Id))
        return Results.Conflict(new { error = $"El nodo {nodo.Id} ya existe" });
    
    if (nodo.Id == 20 && !rotado)
    {
        arbol = new List<NodoAVL>
        {
            new NodoAVL { Id = 20, Etiqueta = "Raíz (después de RID) - FE:0", Altura = 2 },
            new NodoAVL { Id = 10, Etiqueta = "Hijo Izquierdo - FE:0", Altura = 1 },
            new NodoAVL { Id = 30, Etiqueta = "Hijo Derecho - FE:0", Altura = 1 }
        };
        rotado = true;
        
        return Results.Created("/api/arbol", new 
        { 
            mensaje = "Rotación Doble Izquierda-Derecha (RID) aplicada exitosamente",
            arbol 
        });
    }
    
    arbol.Add(nodo);
    return Results.Created($"/api/arbol/{nodo.Id}", new 
    { 
        mensaje = $"Nodo {nodo.Id} insertado (sin rotación)",
        nodo 
    });
});

app.MapPost("/api/arbol/reset", () =>
{
    arbol = new List<NodoAVL> 
    { 
        new NodoAVL { Id = 30, Etiqueta = "Raíz FE:-2", Altura = 3 },
        new NodoAVL { Id = 10, Etiqueta = "Hijo Izq FE:+1", Altura = 2 } 
    };
    rotado = false;
    
    return Results.Ok(new 
    { 
        mensaje = "Árbol reiniciado al estado inicial",
        arbol 
    });
});

app.Run();

public class NodoAVL
{
    public int Id { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public int Altura { get; set; } = 1;
}