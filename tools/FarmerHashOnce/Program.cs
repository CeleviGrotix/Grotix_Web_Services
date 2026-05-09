var hash = BCrypt.Net.BCrypt.HashPassword("Farmer123$");
var outPath = @"C:\Users\alext\proyectos\Grotix-backend\farmer-seed.hash.txt";
File.WriteAllText(outPath, hash);
