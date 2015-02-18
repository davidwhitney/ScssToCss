namespace ClassLibrary1
{
    public static class SassFileConvetionExtensions
    {
        public static bool IsScssPartialFileName(this string fileName)
        {
            return fileName.StartsWith("_");
        }
    }
}