public class FileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(string storedFileName, string filePath)> SaveFileAsync(IFormFile file, long applicationId)
    {
        if (file == null || file.Length == 0)
            return (null, null);

        string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "applications", applicationId.ToString());
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        string ext = Path.GetExtension(file.FileName);
        string storedName = Guid.NewGuid().ToString("N") + ext;
        string fullPath = Path.Combine(uploadDir, storedName);
        await using (FileStream stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string relativePath = "/uploads/applications/" + applicationId + "/" + storedName;
        return (storedName, relativePath);
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;
        string fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}