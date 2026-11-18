namespace QuietSpot;

public static class PhotoService
{
    // Let the user choose a picture, then copy it into the app folder
    public static async Task<string> PickPhotoAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Choose a photo",
            FileTypes = FilePickerFileType.Images
        });

        if (file == null)
            return "";

        string newPath = Path.Combine(FileSystem.AppDataDirectory,
            Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

        using (Stream source = await file.OpenReadAsync())
        using (FileStream target = File.Create(newPath))
        {
            await source.CopyToAsync(target);
        }

        return newPath;
    }
}
